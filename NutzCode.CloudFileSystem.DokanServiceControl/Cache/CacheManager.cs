using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Cache
{
    public sealed class CacheManager : Singleton<CacheManager>
    {
        private ConcurrentDictionary<string, CacheFile> _cachedfiles=new ConcurrentDictionary<string, CacheFile>();        
        private ConcurrentDictionary<string, CacheFile> _explorercacheFiles=new ConcurrentDictionary<string, CacheFile>();

        private bool _abort = false;
        public bool Running { get; private set; }
        private AsyncReaderWriterLock _threadLock=new AsyncReaderWriterLock();
        private static int _threads;


        private CacheManager()
        {
            
        }
        public async Task Init()
        {
            string path = Path.Combine(UserDataPath.Get(), CacheFile.IOCacheDirectory);
            foreach (string s in Directory.GetFiles(path, "*.meta"))
            {
                CacheFile c = CacheFile.FileRequest(File.ReadAllText(s));
                if (c.State == CacheFileState.Waiting)
                {
                    if (!c.IsComplete())
                    {
                        IObject n = c.Resolve();
                        if (n is IFile)
                        {
                            FileSystemResult<Stream> fs = await ((IFile) n).OpenRead();
                            if (!fs.IsOk)
                            {
                                c.State = CacheFileState.Error;
                                c.ErrorType = "Unable to open file in the cloud";
                                c.SaveMetadata();
                            }
                            else
                                c = c.UpgradeToBackedCacheFile(fs.Result);
                        }
                        else
                        {
                            c.State = CacheFileState.Error;
                            c.ErrorType = "File do not exist on the cloud provider, and is partially downloaded in this computer";
                            c.SaveMetadata();
                        }
                    }
                }
                _cachedfiles.GetOrAdd(c.Key, c);
            }
            path = Path.Combine(UserDataPath.Get(), CacheFile.ExplorerCacheDirectory);
            foreach (string s in Directory.GetFiles(path, "*.meta"))
            {
                CacheFile c = CacheFile.FileRequest(File.ReadAllText(s));
                _explorercacheFiles.GetOrAdd(c.Key, c);
            }
        }


        private async Task ThreadRunner()
        {

            do
            {
                try
                {
                    using (await _threadLock.WriterLockAsync())
                    {
                        List<CacheFile> files =
                            _cachedfiles.Where(a => a.Value.State == CacheFileState.Waiting)
                                .OrderBy(a => a.Value.Timestamp)
                                .Take(Properties.Settings.Default.IOThreads - _threads)
                                .Select(a => a.Value)
                                .ToList();
                        foreach (CacheFile f in files)
                        {
                            await ProcessOneTransfer(f);
                        }
                    }
                }
                catch (Exception e)
                {
                    await ClientServiceProxy.Instance.ReportError("Error In Cloud Transfer Thread Runner", e.ToString(), ReportType.Error, DateTime.Now);
                }
                if (!_abort)
                    Thread.Sleep(200);
            } while (!_abort);
            _abort = false;
            Running = false;
        }

        public void Stop()
        {
            _abort = true;
            using (_threadLock.WriterLock())
            {
                Thread.Sleep(200);
                foreach (CacheFile f in _cachedfiles.Values)
                {
                    f.CancelTransfer();
                    f.SaveMetadata();
                }
            }
            while (Running)
            {
               Thread.Sleep(50); 
            }            
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                await ThreadRunner();
            });
        }
        private void DoClose(CacheFile s)
        {
            s.ChangeState(() =>
            {
                BackedCacheFile bs = s as BackedCacheFile;
                if (bs != null)
                {
                    if (bs.WasWritten && bs.IsComplete())
                    {
                        s.State = CacheFileState.Waiting;
                        s.DesiredState = CacheFileState.Upload;
                    }
                    else if (bs.WasWritten)
                    {
                        s.State = CacheFileState.Waiting;
                        s.DesiredState = CacheFileState.FinishDownload;
                    }
                    else
                    {
                        s.State = CacheFileState.Cached;
                        s.DesiredState = CacheFileState.None;
                    }
                }
                else
                {
                    if (s.DesiredState == CacheFileState.Upload)
                        s.State = CacheFileState.Waiting;
                }
            });
        }

        internal async Task ProcessOneTransfer(CacheFile c)
        {
            Func<CancellationToken, Task> process = null;
            c.ChangeState(() =>
            {
                if (c.DesiredState == CacheFileState.FinishDownload)
                {
                    c.State = c.DesiredState;
                    c.DesiredState=CacheFileState.Upload;
                    process = ((BackedCacheFile) c).FinishDownloadTask;
                }
                else if (c.DesiredState == CacheFileState.Upload)
                {
                    c.State = c.DesiredState;
                    c.DesiredState=CacheFileState.Cached;
                    process = c.UploadFileTask;
                }
            });
            if (process != null)
            {
                await Task.Factory.StartNew(async () =>
                {
                    c.WorkerSource = new CancellationTokenSource();
                    await process(c.WorkerSource.Token);
                    c.SaveMetadata();
                    c.WorkerSource = null;
                }, TaskCreationOptions.LongRunning);

            }
        }

        public List<CacheFile> FindFiles(string directory)
        {
            if (_abort)
                return new List<CacheFile>();
            List<CacheFile> rets=new List<CacheFile>();
            int idx = directory.IndexOf('/');
            if (idx >= 0)
            {
                string account = directory.Substring(0, idx);
                string rdir = directory.Substring(idx + 1);

                rets.AddRange(_cachedfiles.Values.Where(a=>a.AccountPart.Equals(account,StringComparison.InvariantCultureIgnoreCase) && a.Path.Equals(rdir, StringComparison.InvariantCultureIgnoreCase)));
                rets.AddRange(_explorercacheFiles.Values.Where(a => a.AccountPart.Equals(account, StringComparison.InvariantCultureIgnoreCase) && a.Path.Equals(rdir, StringComparison.InvariantCultureIgnoreCase)));
            }
            return rets.OrderBy(a => a).ToList();
        }

        public CacheFile FindFile(string filename)
        {
            int idx = filename.IndexOf('/');
            if (idx >= 0)
            {
                string account = filename.Substring(0, idx);
                string rdir = filename.Substring(idx + 1);
                CacheFile f = _explorercacheFiles.Values.FirstOrDefault(a => a.AccountPart.Equals(account, StringComparison.InvariantCultureIgnoreCase) && a.Path.Equals(rdir, StringComparison.InvariantCultureIgnoreCase));
                if (f != null)
                    return f;
                f = _cachedfiles.Values.FirstOrDefault(a => a.AccountPart.Equals(account, StringComparison.InvariantCultureIgnoreCase) && a.Path.Equals(rdir, StringComparison.InvariantCultureIgnoreCase));
                return f;
            }
            return null;
        } 

        public bool DeleteLocalFileIfExists(string filename)
        {
            if (_abort)
                return false;
            FileNameInfo finfo=new FileNameInfo(filename);
            string key = _explorercacheFiles.GetKey(a=>a.Value.FullPath == finfo.FullPath);
            if (key != null)
            {
                CacheFile r;
                if (_explorercacheFiles.TryGetValue(key, out r))
                {
                    r.Delete();
                    return true;
                }
                return false;
            }
            key = _cachedfiles.GetKey(a => a.Value.FullPath == finfo.FullPath);
            if (key != null)
            {
                CacheFile r;
                if (_cachedfiles.TryGetValue(key, out r))
                {
                    r.CancelTransfer();
                    r.Delete();
                    return true;
                }
            }
            return false;
        }

        public bool Move(string srcfullpath, string dstpath)
        {
            //Check for explorer files
            FileNameInfo finfo = new FileNameInfo(srcfullpath);
            string key = _explorercacheFiles.GetKey(a => a.Value.FullPath == finfo.FullPath);

            if (key != null)
            {
                CacheFile r;
                if (_explorercacheFiles.TryRemove(key,out r))
                {
                    CacheFile destination = CacheFile.ExplorerFileRequest(dstpath + "/" + r.NamePart);
                    try { Directory.CreateDirectory(destination.CacheDirectory); } catch { }
                    File.Move(r.CacheFullName, destination.CacheFullName);
                    File.Delete(r.CacheFullName+".meta");
                    _explorercacheFiles.GetOrAdd(destination.CacheFullName, destination);
                    destination.SaveMetadata();
                    return true;
                }
                return false;
            }
            //Check for cached files
            key = _cachedfiles.GetKey(a => a.Value.FullPath == finfo.FullPath);
            if (key != null)
            {
                CacheFile r;
                if (_explorercacheFiles.TryGetValue(key, out r))
                {
                    CacheFile destination = CacheFile.ExplorerFileRequest(dstpath + "/" + r.NamePart);

                }
            }
            CacheFile.CacheFileRequest(srcpath)?.CacheFullName;
        }

        public async Task<Stream> OpenStreamAsync(string filename, FileMode mode, FileAccess access, FileShare share)
        {
            if (_abort)
                return null;
            CacheFile info = CacheFile.ExplorerFileRequest(filename);
            if (info != null)
                return info.Open(mode, access, share);
            info= CacheFile.CacheFileRequest(filename);
            CacheFile s = _cachedfiles.FirstOrDefault(a => a.Value.FullPath == info.FullPath).Value;
            if (s != null)
            {
                switch (s.State)
                {
                    case CacheFileState.InUse:
                        if (mode == FileMode.Truncate || mode == FileMode.Create || mode == FileMode.CreateNew || share == FileShare.None)
                            throw new UnauthorizedAccessException();
                        break;
                    case CacheFileState.Error:
                        return info.Open(mode, access, share);
                }
                if (s.State == CacheFileState.FinishDownload ||
                    ((s.State == CacheFileState.Upload)
                     && (access == FileAccess.Write || access == FileAccess.ReadWrite)))
                {
                    s.CancelTransfer();
                }
                if (s.State == CacheFileState.FinishDownload || s.State == CacheFileState.Waiting || s.State == CacheFileState.Cached ||
                    ((s.State == CacheFileState.Upload)
                    && (access == FileAccess.Write || access == FileAccess.ReadWrite)))
                {
                    s.ChangeState(() =>
                    {
                        s.State = CacheFileState.InUse;
                    });
                }
                info.SaveMetadata();
                Stream stream =s.Open(mode,access,share);
                if (mode == FileMode.Append)
                    stream.Seek(0, SeekOrigin.End);
                return stream;
            }
            FileNameInfo finfo=new FileNameInfo(filename);
            IObject obj = finfo.Resolve();
            if (obj != null)
            {
                if (mode == FileMode.CreateNew)
                    throw new IOException("File already exists");
                if (obj is IFile)
                {
                    if (mode != FileMode.Create && mode != FileMode.Truncate)
                    {
                        FileSystemResult<Stream> fs = await ((IFile) obj).OpenRead();
                        if (!fs.IsOk)
                            throw new IOException(fs.Error);
                        if ((mode == FileMode.Open || mode == FileMode.OpenOrCreate) && (access == FileAccess.Read))
                            return fs.Result;
                        info.State = CacheFileState.InUse;
                        info.DesiredState=CacheFileState.None;
                        info = info.UpgradeToBackedCacheFile(fs.Result);
                        info.OnClose += () =>
                        {
                            DoClose(info);
                        };
                        _cachedfiles.GetOrAdd(info.Key, info);
                        info.SaveMetadata();
                        return info.Open(mode, access, share);
                    }
                }
                else
                    throw new IOException("Directory already exists");
            }
            if (mode == FileMode.Create || mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate ||
                mode == FileMode.Truncate)
            {
                if (mode == FileMode.Truncate)
                    mode = FileMode.Create;
                info.State=CacheFileState.InUse;
                info.DesiredState = CacheFileState.Upload;
                info.WasWritten = true;
                info.OnClose += () =>
                {
                    DoClose(info);
                };
                _cachedfiles.GetOrAdd(info.Key, info);
                info.SaveMetadata();
                return info.Open(mode, access, share);
            }
            throw new FileNotFoundException();
        }
    }
}
