using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using NutzCode.CloudFileSystem.DokanServiceModels;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
   
    public class Mounts
    {
        public static Dictionary<string, IFileSystem> LoadedFileSystems { get; } = new Dictionary<string, IFileSystem>();


        private static Settings ActiveSettings = null;
        static CloudFileSystemOperations ops = new CloudFileSystemOperations();

        
        public static async Task Unmount()
        {
            if (ActiveSettings != null)
            {
                Dokan.RemoveMountPoint(ActiveSettings.MountPoint);
                ops = null;
                LoadedFileSystems.Clear();
                ActiveSettings = null;
            }
            await Task.FromResult(0);
        }


        public static async Task<bool> Mount(Settings set)
        {
            if (set == null)
                return false;
            try
            {
                List<Account> logaccounts = ActiveSettings != null
               ? set.Accounts.Where(
                   ac =>
                       !ActiveSettings.Accounts.Any(
                           a =>
                               a.Name == ac.Name && a.AuthorizationData == ac.AuthorizationData &&
                               a.Plugin == ac.Plugin)).ToList()
               : set.Accounts;
                foreach (Account ac in logaccounts)
                {
                    string auth = LoadAccountAuthorization(ac.Name);
                    string tname = ac.Name + "_" + ac.Plugin.ToString();
                    ICloudPlugin plugin =
                        CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a => a.Name == ac.Plugin.ToString());
                    if (plugin == null)
                    {
                        ClientServiceProxy.Instance.ShowError("Mount Error",
                            "Unable to find plugin type '" + ac.Plugin.ToString() + "'.", false);
                        continue;
                    }
                    OAuthProxy oauthproxyprovider=new OAuthProxy(Properties.Settings.Default.OAuthProvider);
                    FileSystemResult<IFileSystem> fs = await plugin.Init(ac.Name, oauthproxyprovider, ac.AuthorizationData, auth);
                    if (fs == null || !fs.IsOk)
                    {
                        if (fs != null)
                            ClientServiceProxy.Instance.ShowError("Mount Error", "Unable to mount '" + ac.Name + "'. Error: " + fs.Error, false);
                        else
                            ClientServiceProxy.Instance.ShowError("Mount Error", "Unable to mount '" + ac.Name + "'.", false);
                        continue;
                    }
                    LoadedFileSystems[tname] = fs.Result;
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
                    ops.Mount(set.MountPoint, (DokanOptions)set.DokanOptions, 50);
                ActiveSettings = set;
                return true;
            }
            catch (Exception e)
            {
                ClientServiceProxy.Instance.ShowError("Mount Error", "Unable to mount filesystem. Error: " + e, false);
            }
            return false;
        }

        public static string LoadAccountAuthorization(string s)
        {
            string path = Path.Combine(UserDataPath.Get(), s+".json");
            if (System.IO.File.Exists(path))
                return System.IO.File.ReadAllText(path);
            return null;
        }

        public static void SaveAccountAuthorization(string d, string s)
        {
            string path = Path.Combine(UserDataPath.Get(), d + ".json");
            File.WriteAllText(path, s);
        }



    }

}
