using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using DokanNet;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NutzCode.CloudFileSystem.DokanClient.Cache;
using NutzCode.CloudFileSystem.DokanServiceControl.Streams;
using FileAccess = System.IO.FileAccess;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Cache
{
    public class CacheFile : FileNameInfo
    {
        public const string ExplorerCacheDirectory = "Explorer";
        public const string IOCacheDirectory = "IO";
        public static string[] ExplorerCachedFiles = { "Thumbs.db", "desktop.ini"};

        public CacheFileState State { get; set; }
        public CacheFileState DesiredState { get; set; }

        public string Key { get; set; }
        public string CacheFullName => System.IO.Path.Combine(CacheDirectory, Key);
        public string CacheDirectory { get; set; }



        public DateTime Timestamp { get; set; }
        public long Size { get; set; }
        public int Retries { get; set; } = 0;
        public Ranges CachedRanges { get; set; } = new Ranges();
        public bool WasWritten { get; set; } = false;
        public long Length { get; internal set; } = 0;
        public string ErrorType { get; set; } = null;



        internal int UseCount = 0;

        public delegate void VoidHandler();
        public delegate void ProgressHandler(CacheFile file, FileProgress p);

        public event VoidHandler OnClose;
        public event ProgressHandler OnProgress;

        public CancellationTokenSource WorkerSource { get; set; } = null;
        internal AsyncReaderWriterLock statLock = new AsyncReaderWriterLock();
        internal AsyncReaderWriterLock countLock = new AsyncReaderWriterLock();
        internal AsyncReaderWriterLock saveinfoLock = new AsyncReaderWriterLock();
        internal AsyncReaderWriterLock rangelock = new AsyncReaderWriterLock();

        internal void DoProgress(FileProgress p)
        {
            OnProgress?.Invoke(this, p);
        }

        public void ChangeState(Action changestate)
        {
            using (statLock.WriterLock())
                changestate();
        }

        public bool IsComplete()
        {
            using (rangelock.WriterLock())
            {
                return CachedRanges.InCache(0, Length).All(a => a.Found);
            }

        }

        internal async Task UploadFileTask(CancellationToken token)
        {
            try
            {
                Progress<FileProgress> fp = new Progress<FileProgress>();
                fp.ProgressChanged += (a, b) =>
                {
                    DoProgress(b);
                };
                IObject file = Resolve();
                if (file != null && file is IFile)
                {
                    IFile f = (IFile) file;
                    //TODO Add Extra properties (upload status?)
                    FileSystemResult fr =
                        await
                            f.OverwriteFile(
                                File.Open(CacheFullName, FileMode.Open, FileAccess.Read, FileShare.Read), token, fp,
                                new Dictionary<string, object>());
                    if (!fr.IsOk)
                    {
                        ChangeState(() =>
                        {
                            DesiredState = State;
                            ErrorType = "Error uploading file";
                            Retries++;
                            State = Retries == Properties.Settings.Default.MaxRetries
                                ? CacheFileState.Error
                                : CacheFileState.Waiting;
                        });
                        return;
                    }
                }
                else if (file is IDirectory)
                {
                    ChangeState(() =>
                    {
                        DesiredState = State;
                        State = CacheFileState.Error;
                        ErrorType = "Unable to upload a File with a Directory with the same name";
                    });
                    return;
                }
                FileNameInfo fn=new FileNameInfo(PathWithoutAltStream);
                IObject dir = fn.Resolve();
                if (dir == null || !(dir is IDirectory))
                {
                    ChangeState(() =>
                    {
                        DesiredState = State;
                        State = CacheFileState.Error;
                        ErrorType = "Directory not found in provider cloud";
                    });
                    return;
                }
                IDirectory d = (IDirectory) dir;
                //TODO Add Extra properties (upload status?)
                FileSystemResult<IFile> fr2 =
                    await
                        d.CreateFile(NamePart, File.Open(CacheFullName, FileMode.Open, FileAccess.Read, FileShare.Read),
                            WorkerSource.Token, fp, new Dictionary<string, object>());
                if (!fr2.IsOk)
                {
                    ChangeState(() =>
                    {
                        DesiredState = State;
                        ErrorType = "Error uploading file";
                        Retries++;
                        State = Retries == Properties.Settings.Default.MaxRetries
                            ? CacheFileState.Error
                            : CacheFileState.Waiting;
                    });
                    return;
                }
                ChangeState(() =>
                {
                    State = CacheFileState.Cached;
                    DesiredState = CacheFileState.None;
                });
            }
            catch (OperationCanceledException e)
            {
                ChangeState(() =>
                {
                    DesiredState = State;
                    State = CacheFileState.Canceled;
                });
            }
            catch (Exception e)
            {
                ChangeState(() =>
                {
                    DesiredState = State;
                    ErrorType = "Error uploading file: "+e;
                    Retries++;
                    State = Retries == Properties.Settings.Default.MaxRetries
                        ? CacheFileState.Error
                        : CacheFileState.Waiting;
                });
            }
        }




        public static CacheFile ExplorerFileRequest(string fname)
        {
            CacheFile cf=new CacheFile(fname);
            if (ExplorerCachedFiles.Any(c => cf.NamePart.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
                return Create(fname, ExplorerCacheDirectory);
            return null;
        }

        public static CacheFile CacheFileRequest(string fname)
        {
            return Create(fname, IOCacheDirectory);
        }
        public static CacheFile FileRequest(string metadata)
        {
            CacheFile c=new CacheFile();
            JsonConvert.PopulateObject(metadata, c);
            return c;
        }
        public virtual void SaveMetadata()
        {
            using (saveinfoLock.WriterLock())
            {
                string s = JsonConvert.SerializeObject(this);
                File.WriteAllText(CacheFullName + ".meta", s);
            }
        }


        private static CacheFile Create(string filename, string cachedir)
        {
            CacheFile ecf = new CacheFile(filename);
            ecf.Key = Guid.NewGuid().ToString().Replace("-", string.Empty);
            string path = UserDataPath.Get();
            ecf.CacheDirectory = System.IO.Path.Combine(path, cachedir);
            return ecf;
        }

        public BackedCacheFile UpgradeToBackedCacheFile(Stream inputstream)
        {
            BackedCacheFile ch = new BackedCacheFile();
            this.CopyTo(ch);
            ch._inputStream = inputstream;
            ch._outputStream = File.Open(CacheFullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            ch.Length = inputstream.Length;
            return ch;
        }


        public virtual Stream Open(FileMode mode, FileAccess access, FileShare share)
        {              
            try { System.IO.Directory.CreateDirectory(CacheDirectory); } catch (Exception) { }
            using (countLock.WriterLock())
            {
                UseCount++;
                ReportOnCloseStream s = new ReportOnCloseStream(new FileStream(CacheFullName, mode, access, share,65536,true));
                s.OnClose += DecrementUsage;
                return s;
            }
            
        }

        public void CancelTransfer()
        {
            if (WorkerSource != null)
            {
                WorkerSource.Cancel();
                while (WorkerSource != null)
                    Thread.Sleep(50);
            }
        }
        public virtual void Close()
        {
        }
        internal void DecrementUsage()
        {
            using (countLock.WriterLock())
            {
                UseCount--;
                if (UseCount == 0)
                {
                    Timestamp = DateTime.Now;
                    OnClose?.Invoke();
                }
            }
        }
        public bool Exists()
        {
            return File.Exists(CacheFullName);
        }

        public void Delete()
        {
            File.Delete(CacheFullName);
        }

        public FileInformation GetFileInformation()
        {
            FileInformation f = new FileInformation();
            FileInfo finfo = new FileInfo(CacheFullName);
            f.FileName = NamePart;
            f.CreationTime = finfo.CreationTime;
            f.Length = finfo.Length;
            f.LastAccessTime = finfo.LastAccessTime;
            f.LastWriteTime = finfo.LastWriteTime;
            f.Attributes = finfo.Attributes;
            return f;
        }

        public void SetFileAttributes(FileAttributes attributes)
        {
            attributes &= ~FileAttributes.Hidden;
            File.SetAttributes(CacheFullName, attributes);
        }

        public void SetFileTime(DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            if (creationTime!=null)
                File.SetCreationTime(CacheFullName, creationTime.Value);
            if (lastAccessTime!=null)
                File.SetLastAccessTime(CacheFullName, lastAccessTime.Value);
            if (lastWriteTime!=null)
                File.SetLastWriteTime(CacheFullName, lastWriteTime.Value);
        }

        public FileSystemSecurity GetFileSecurity(AccessControlSections sections)
        {
            return File.GetAccessControl(CacheFullName);
        }

        public void SetFileSecurity(FileSystemSecurity security, AccessControlSections sections)
        {
            File.SetAccessControl(CacheFullName, (FileSecurity)security);
        }

        internal CacheFile(string fileName) : base(fileName)
        {
        }

        internal CacheFile()
        {
            
        }
    }
}
