using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public static class UserDataPath
    {
        public static string AppPath { get; set; } = null;

        public static string Get()
        {
            if (AppPath == null)
                return null;
            string basepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppPath);
            if (!Directory.Exists(basepath))
                Directory.CreateDirectory(basepath);
            return basepath;
        }
    }
}
