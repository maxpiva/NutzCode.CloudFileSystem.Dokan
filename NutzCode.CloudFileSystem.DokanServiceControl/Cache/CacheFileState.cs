namespace NutzCode.CloudFileSystem.DokanServiceControl.Cache
{
    public enum CacheFileState
    {
        InUse,
        Cached,
        Waiting,
        FinishDownload,
        Upload,
        Error,
        Canceled,
        None
    }
}
