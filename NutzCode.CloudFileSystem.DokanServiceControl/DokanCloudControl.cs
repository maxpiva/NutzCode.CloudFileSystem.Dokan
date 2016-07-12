using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DokanNet;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.DokanServiceModels;
using NutzCode.CloudFileSystem.Plugins.GoogleDrive;
using File = System.IO.File;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class DokanCloudControl : IDokanCloudControl
    {
        public const string SettingsName = "settings.json";
        public const string SettingsPath = "CloudFileSystemDokan";
        public const int MaxDokanThreads = 20;

        public ReaderWriterLockSlim _fslock=new ReaderWriterLockSlim();
        public IFileSystem GetFileSystem(string name)
        {
            try
            {
                _fslock.EnterReadLock();
                if (LoadedFileSystems.ContainsKey(name))
                    return LoadedFileSystems[name];
                return null;
            }
            finally
            {
                _fslock.ExitReadLock();                
            }
        }

        public List<IFileSystem> GetAllFileSystems()
        {
            try
            {
                _fslock.EnterReadLock();
                return LoadedFileSystems.Values.ToList();
            }
            finally
            {
                _fslock.ExitReadLock();
            }
        }
        public void AddFileSystem(string name, IFileSystem fs)
        {
            try
            {
                _fslock.EnterWriteLock();
                LoadedFileSystems[name] = fs;
            }
            finally
            {
                _fslock.ExitWriteLock();
            }
        }
        public void RemoveFileSystem(string name)
        {
            try
            {
                _fslock.EnterWriteLock();
                if (LoadedFileSystems.ContainsKey(name))
                    LoadedFileSystems.Remove(name);
            }
            finally
            {
                _fslock.ExitWriteLock();
            }
        }
        public void RemoveAllFileSystems()
        {
            try
            {
                _fslock.EnterWriteLock();
                LoadedFileSystems.Clear();
            }
            finally
            {
                _fslock.ExitWriteLock();
            }
        }
        private Dictionary<string, IFileSystem> LoadedFileSystems { get; } = new Dictionary<string, IFileSystem>();
        private Settings ActiveSettings = null;
        private CloudFileSystemOperations ops = new CloudFileSystemOperations();


        public DokanCloudControl()
        {
            UserDataPath.AppPath = SettingsPath;
        }

        public async Task Init()
        {
            ActiveSettings = await LoadSettings();
            if (ActiveSettings.Persist)
                await Mount(ActiveSettings);
        }


        public async Task<ServiceResult> Unmount()
        {
            if (ActiveSettings != null && ActiveSettings.IsMounted)
            {
                Dokan.RemoveMountPoint(ActiveSettings.MountPoint);
                RemoveAllFileSystems();
                LoadedFileSystems.Clear();
                ActiveSettings.IsMounted = false;
            }
            return await Task.FromResult(new ServiceResult { IsOk = true});
        }

        public Task<ServiceResult> ReportSyncProgress(bool report)
        {
            throw new NotImplementedException();
        }


        public async Task<ServiceResult> Mount(Settings set)
        {
            if (set == null)
                return new ServiceResult {IsOk = false, Error = "Empty Settings"};
            try
            {
                List<Account> logaccounts = ActiveSettings != null
               ? set.Accounts.Where(
                   ac =>
                       !ActiveSettings.Accounts.Any(
                           a =>
                               a.Name == ac.Name &&
                               a.PluginName == ac.PluginName)).ToList()
               : set.Accounts;
                foreach (Account ac in logaccounts)
                {
                    string auth = LoadAccountAuthorization(ac.Name);
                    ICloudPlugin plugin = CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == ac.PluginName.ToString());
                    if (plugin == null)
                    {
                        await ClientServiceProxy.Instance.ReportError("Mount Error", "Unable to find plugin type '" + ac.PluginName.ToString() + "'.",ReportType.Warn, DateTime.Now);
                        continue;
                    }
                    OAuthProxy oauthproxyprovider = new OAuthProxy(Properties.Settings.Default.OAuthGUIProvider);
                    Dictionary<string,object> secrets=new Dictionary<string, object>();
                    if (PluginsSecrets.List.ContainsKey(ac.PluginName))
                        secrets = PluginsSecrets.List[ac.PluginName];
                    FileSystemResult<IFileSystem> fs = await plugin.Init(ac.Name, oauthproxyprovider, secrets, auth);
                    if (fs == null || !fs.IsOk)
                    {
                        if (fs != null)
                            await ClientServiceProxy.Instance.ReportError("Mount Error", "Unable to mount '" + ac.Name + "'. Error: " + fs.Error,ReportType.Warn, DateTime.Now);
                        else
                            await ClientServiceProxy.Instance.ReportError("Mount Error", "Unable to mount '" + ac.Name + "'.", ReportType.Warn, DateTime.Now);
                        continue;
                    }
                    AddFileSystem(ac.Name,fs.Result);
                    SaveAccountAuthorization(ac.Name, fs.Result.GetUserAuthorization());
                }
                bool mount = true;
                if (ActiveSettings != null)
                {
                    if (set.MountPoint != ActiveSettings.MountPoint)
                        Dokan.RemoveMountPoint(ActiveSettings.MountPoint);
                    else
                        mount = false;
                }

                if (ops == null)
                    ops = new CloudFileSystemOperations();
                if (mount)
                    ops.Mount(set.MountPoint, (DokanOptions)set.DokanOptions, MaxDokanThreads);
                HashSet<string> actives = new HashSet<string>(set.Accounts.Select(a => a.Name + "_" + a.PluginName));
                foreach (string n in LoadedFileSystems.Keys.ToList())
                {
                    if (!actives.Contains(n))
                    {
                        RemoveFileSystem(n);
                    }
                }
                ActiveSettings = set;
                set.IsMounted = true;
                await SaveSettings(ActiveSettings);
                return new ServiceResult {IsOk = true};
            }
            catch (Exception e)
            {
                await ClientServiceProxy.Instance.ReportError("Mount Error", "Unable to mount filesystem. Error: " + e, ReportType.Error, DateTime.Now);
                return new ServiceResult {IsOk = false, Error = e.ToString()};
            }
        }

        private static string LoadAccountAuthorization(string s)
        {
            string path = Path.Combine(UserDataPath.Get(), s + ".json");
            if (System.IO.File.Exists(path))
                return System.IO.File.ReadAllText(path);
            return null;
        }

        private static void SaveAccountAuthorization(string d, string s)
        {
            string path = Path.Combine(UserDataPath.Get(), d + ".json");
            File.WriteAllText(path, s);
        }

        private async Task<Settings> LoadSettings()
        {

            try
            {
                string path = Path.Combine(UserDataPath.Get(), SettingsName);
                if (System.IO.File.Exists(path))
                    return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                await ClientServiceProxy.Instance.ReportError("Settings Error", "Unable to load mount point settings", ReportType.Warn, DateTime.Now);
            }
            return null;
        }

        private async Task SaveSettings(Settings set)
        {
            string path = Path.Combine(UserDataPath.Get(), SettingsName);
            try
            {
                if (set != null)
                {
                    string settings = JsonConvert.SerializeObject(set);
                    File.WriteAllText(path, settings);
                }
            }
            catch (Exception)
            {
                await ClientServiceProxy.Instance.ReportError("Settings Error", "Unable to save mount point settings", ReportType.Warn, DateTime.Now);
            }
        }

        public async Task<ServiceResult<Settings>> GetActiveSettings()
        {
            return await Task.FromResult(new ServiceResult<Settings>() {IsOk = true, Result = ActiveSettings});
        }
    }
}
