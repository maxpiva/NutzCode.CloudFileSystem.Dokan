using System.Collections.Generic;

namespace NutzCode.CloudFileSystem.DokanServiceModels
{
    public class Account
    {
        public string Name { get; set; }
        public string PluginName { get; set; }
    }


    public enum ReportType
    {
        Trace,
        Debug,
        Info,
        Error,
        Warn
    }

    public enum SyncType
    {
        Waiting,
        Downloading,
        Uploading,
        Error
    }
    public class Settings
    {
        public string MountPoint { get; set; }
        public long DokanOptions { get; set; }
        public bool Persist { get; set; }
        public bool IsMounted { get; set; }
        public List<Account> Accounts { get; set; }
    }
    public class ServiceResult<T> : ServiceResult
    {
        public T Result { get; set; }
    }

    public class ServiceResult
    {
        public bool IsOk { get; set; }
        public string Error { get; set; }
    }
    
}
