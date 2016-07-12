using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class SettingsManager
    {
        public Settings ActiveSettings = null;
        private Settings _oldSettings = null;
        public const string SettingsName = "settings.json";
        public const string SettingsPath = "CloudFileSystemDokan";

        public SettingsManager()
        {
            UserDataPath.AppPath = SettingsPath;
        }
        public void Load()
        {
            
            try
            {
                _oldSettings = null;
                string path = Path.Combine(UserDataPath.Get(), SettingsName);
                if (System.IO.File.Exists(path))
                    ActiveSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                ClientServiceProxy.Instance.ShowError("Settings Error", "Unable to load mount point settings", false);
            }
        }

        public void Save()
        {
            string path = Path.Combine(UserDataPath.Get(), SettingsName);
            try
            {
                if (ActiveSettings != null)
                {
                    string settings = JsonConvert.SerializeObject(ActiveSettings);
                    File.WriteAllText(path, settings);
                }
            }
            catch (Exception)
            {
                ClientServiceProxy.Instance.ShowError("Settings Error", "Unable to save mount point settings", false);
            }
        }


    }


}
