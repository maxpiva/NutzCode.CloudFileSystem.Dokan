using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using Nito.AsyncEx;
using NutzCode.CloudFileSystem.DokanServiceControl.Cache;

using FileAccess = DokanNet.FileAccess;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class CloudFileSystemOperations : IDokanOperations
    {


        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private string ToTrace(DokanFileInfo info)
        {
            var context = info.Context != null ? "<" + info.Context.GetType().Name + ">" : "<null>";

            return string.Format(CultureInfo.InvariantCulture, "{{{0}, {1}, {2}, {3}, {4}, #{5}, {6}, {7}}}",
                context, info.DeleteOnClose, info.IsDirectory, info.NoCache, info.PagingIo, info.ProcessId, info.SynchronousIo, info.WriteToEndOfFile);
        }


        private string ToTrace(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(CultureInfo.CurrentCulture) : "<null>";
        }

        private NtStatus Trace(string method, string fileName, DokanFileInfo info, NtStatus result, params string[] parameters)
        {
            var extraParameters = parameters != null && parameters.Length > 0 ? ", " + string.Join(", ", parameters) : string.Empty;

#if TRACE
            if (NtStatus.Success != result)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}('{1}', {2}{3}) -> {4}",
                    method, fileName, ToTrace(info), extraParameters, result));
            }
#endif

            return result;
        }

        private NtStatus Trace(string method, string fileName, DokanFileInfo info,
                                  FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
                                  NtStatus result)
        {
#if TRACE
            if (NtStatus.Success!=result)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "{0}('{1}', {2}, [{3}], [{4}], [{5}], [{6}], [{7}]) -> {8}",
                    method, fileName, ToTrace(info), access, share, mode, options, attributes, result));
            }
#endif

            return result;
        }

        private async Task<FileSystemResult<Stream>> OpenStreamFromFileName(string fileName)
        {
            IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
            IFile f = obj as IFile;
            if (f != null)
                return await f.OpenRead();
            return new FileSystemResult<Stream>("Unable to find file");
        }



        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
                                     DokanFileInfo info)
        {
            FileNameInfo finfo = new FileNameInfo(fileName);

            if (finfo.AltStreamName!=null && (mode==FileMode.Append || mode==FileMode.Create || (mode==FileMode.OpenOrCreate) || (mode==FileMode.CreateNew) || (mode==FileMode.Truncate)))
                return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.AccessDenied);

            if (info.IsDirectory && finfo.AltStreamName == null)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                            IObject iobj = CloudFileSystemHelper.ResolvePath(fileName);
                            if ((iobj==null) || (iobj is IFile))
                                Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.PathNotFound);
                            break;

                        case FileMode.CreateNew:
                            IObject iobj2 = CloudFileSystemHelper.ResolvePath(fileName);
                            if (iobj2!=null)
                                Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.FileExists);
                            iobj2= CloudFileSystemHelper.ResolvePath(finfo.Directory);
                            if (iobj2==null || iobj2 is IFile)
                                Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.PathNotFound);
                            IDirectory dir=iobj2 as IDirectory;
                            IDirectory fdir=AsyncContext.Run(async () =>
                            {
                                FileSystemResult<IDirectory> fr = await dir.CreateDirectory(finfo.FileName, new Dictionary<string, object>());
                                if (fr.IsOk)
                                    return fr.Result;
                                return null;
                            });
                            if (fdir==null)
                                return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.AccessDenied);
                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.AccessDenied);
                }
            }
            if (fileName == "/" || fileName == "" || fileName == "\\")
            {
                info.IsDirectory = true;
                info.Context = new object();
                return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.Success);
            }


            bool pathExists = true;
            bool pathIsDirectory = false;

            bool readWriteAttributes = (access & DataAccess) == 0;
            bool readAccess = (access & DataWriteAccess) == 0;


            IObject obj = CloudFileSystemHelper.ResolvePath(fileName);

            if (obj == null)
                pathExists = false;
            else
                pathIsDirectory = obj is IDirectory;

            switch (mode)
            {
                case FileMode.Open:

                    if (pathExists)
                    {
                        if (readWriteAttributes || pathIsDirectory)
                        // check if driver only wants to read attributes, security info, or open directory
                        {
                            info.IsDirectory = pathIsDirectory;
                            info.Context = new object();
                            // must set it to someting if you return DokanError.Success
                            if (finfo.AltStreamName!=null)
                            {
                                Stream s = AsyncContext.Run(async () =>
                                {
                                    FileSystemResult<Stream> ss = await OpenStreamFromFileName(fileName);
                                    if (ss.IsOk)
                                        return ss.Result;
                                    return null;
                                });
                                info.Context = s;
                            }
                            return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.Success);
                        }
                    }
                    else
                    {
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.FileNotFound);
                    }
                    break;

                case FileMode.CreateNew:
                    if (pathExists)
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.FileExists);
                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.FileNotFound);
                    break;

                default:
                    break;
            }
            if (finfo.AltStreamName != null)
            {
                Stream s = AsyncContext.Run(async () =>
                {
                    FileSystemResult<Stream> ss = await OpenStreamFromFileName(fileName);
                    if (ss.IsOk)
                        return ss.Result;
                    return null;
                });
                info.Context = s;
                return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.Success);
            }
            IObject fobj = CloudFileSystemHelper.ResolvePath(finfo.Directory);
            if (fobj is IDirectory)
            {
                try
                {
                    Stream s = AsyncContext.Run(async () => await CacheManager.Instance.OpenStreamAsync((IDirectory)fobj, finfo.FileName, mode, readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share));
                    if (s != null)
                    {
                        info.Context = s;
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.Success);
                    }
                }
                catch (Exception) // don't have access rights
                {
                }
            }

            return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, DokanResult.AccessDenied);
        }
        /*
        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            if (fileName == string.Empty || fileName == "\\" || fileName == "/")
                return Trace("OpenDirectory", fileName, info, DokanResult.Success);
            IObject ob = CloudFileSystemHelper.ResolvePath(fileName);
            if (ob !=null && ob is IDirectory)
                return Trace("OpenDirectory", fileName, info, DokanResult.Success);
            return Trace("OpenDirectory", fileName, info, DokanResult.PathNotFound);
        }
        
        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {            
            fileName = fileName.Replace("/", "\\");
            List<string> parts = fileName.Split('\\').ToList();
            string name = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            fileName = string.Join("\\", parts);
            IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
            if (obj != null && obj is IDirectory)
            {
                IDirectory dir = (IDirectory)obj;
                IDirectory o = AsyncContext.Run(async () =>
                {
                    FileSystemResult<IDirectory> d = await dir.CreateDirectory(name, new Dictionary<string, object>());
                    if (d.IsOk)
                        return d.Result;
                    return null;
                });
                if (o != null)
                    return Trace("CreateDirectory", fileName, info, DokanResult.Success);
            }
            return Trace("CreateDirectory", fileName, info, DokanResult.AccessDenied);
        }
                */

        public void Cleanup(string fileName, DokanFileInfo info)
        {
#if TRACE
            if (info.Context != null)
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}('{1}', {2} - entering",
                    "Cleanup", fileName, ToTrace(info)));
#endif

            if (info.Context != null && info.Context is Stream)
            {
                (info.Context as Stream).Close();
                (info.Context as Stream).Dispose();
            }
            info.Context = null;

            if (info.DeleteOnClose)
            {
                IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
                if (obj != null)
                {
                    AsyncContext.Run(async () =>
                    {
                        FileSystemResult ss = await obj.Delete(false);
                    });
                }
            }
            Trace("Cleanup", fileName, info, DokanResult.Success);
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
#if TRACE
            if (info.Context != null)
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}('{1}', {2} - entering",
                    "CloseFile", fileName, ToTrace(info)));
#endif

            if (info.Context != null && info.Context is Stream)
            {
                (info.Context as Stream).Close();
                (info.Context as Stream).Dispose();
            }
            info.Context = null;
            Trace("CloseFile", fileName, info, DokanResult.Success); // could recreate cleanup code here but this is not called sometimes
        }




        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            if (info.Context == null) // memory mapped read
            {
                FileNameInfo finfo = new FileNameInfo(fileName);

                bytesRead = AsyncContext.Run(async () =>
                {
                    if (finfo.AltStreamName != null)
                    {
                        FileSystemResult<Stream> ss = await OpenStreamFromFileName(fileName);
                        if (ss.IsOk)
                        {
                            using (ss.Result)
                            {
                                ss.Result.Position = offset;
                                return ss.Result.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                    else
                    {
                        IObject obj = CloudFileSystemHelper.ResolvePath(finfo.Directory);
                        if (obj != null && obj is IDirectory)
                        {
                            using (
                                Stream s =
                                    await
                                        CacheManager.Instance.OpenStreamAsync((IDirectory) obj, finfo.FileName, FileMode.Open,
                                            System.IO.FileAccess.Read, FileShare.ReadWrite))
                            {
                                if (s != null)
                                {
                                    s.Position = offset;
                                    return s.Read(buffer, 0, buffer.Length);
                                }
                            }
                        }
                    }
                    return 0;
                });
            }
            else // normal read
            {
                var stream = info.Context as Stream;
                stream.Position = offset;
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            return Trace("ReadFile", fileName, info, DokanResult.Success, "out " + bytesRead.ToString(), offset.ToString(CultureInfo.InvariantCulture));
        }
        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            bytesWritten = 0;
            if (info.Context == null) // memory mapped read
            {
                FileNameInfo finfo = new FileNameInfo(fileName);
                if (finfo.AltStreamName != null)
                {
                    return Trace("WriteFile", fileName, info, DokanResult.AccessDenied, "out " + bytesWritten.ToString(), offset.ToString(CultureInfo.InvariantCulture));
                }
                int cnt=AsyncContext.Run(async () =>
                {

                    IObject obj = CloudFileSystemHelper.ResolvePath(finfo.Directory);
                    if (obj != null && obj is IDirectory)
                    {
                        using (Stream s = await CacheManager.Instance.OpenStreamAsync((IDirectory) obj, finfo.FileName, FileMode.Open, System.IO.FileAccess.Write, FileShare.ReadWrite))
                        {
                            if (s != null)
                            {
                                s.Position = offset;
                                await s.WriteAsync(buffer, 0, buffer.Length);
                                return buffer.Length;
                            }
                        }
                    }
                    return 0;
                });
                bytesWritten = cnt;
                return Trace("WriteFile", fileName, info, DokanResult.Success, "out " + bytesWritten.ToString(), offset.ToString(CultureInfo.InvariantCulture));
            }
            else
            {

                var stream = info.Context as Stream;
                stream.Position = offset;
                stream.Write(buffer, 0, buffer.Length);
                bytesWritten = buffer.Length;
            }
            return Trace("WriteFile", fileName, info, DokanResult.Success, "out " + bytesWritten.ToString(), offset.ToString(CultureInfo.InvariantCulture));
        }
    

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            ((FileStream) info.Context)?.Flush();
            return Trace("FlushFileBuffers", fileName, info, DokanResult.Success);
        }
        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            if (fileName == string.Empty || fileName == "\\" || fileName == "/")
            {
                fileInfo=new FileInformation { Attributes = FileAttributes.Directory|FileAttributes.System|FileAttributes.Hidden, FileName = "\\", CreationTime = DateTime.Today, LastAccessTime = DateTime.Today, LastWriteTime = DateTime.Today, Length = 0};
            }
            else
            {
                CacheFile ff=CacheManager.Instance.FindFile(fileName);
                if (ff != null)
                {
                    fileInfo = ff.GetFileInformation();
                    return Trace("GetFileInformation", fileName, info, DokanResult.Success);
                }
                IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
                if (obj == null)
                {
                    fileInfo = new FileInformation();
                    return Trace("GetFileInformation", fileName, info, DokanResult.PathNotFound);
                }
                fileInfo = CloudFileSystemHelper.FromObject(obj);
            }
#if TRACE
            Console.WriteLine("FindInfo: " + fileInfo.FileName + " Length: " + fileInfo.Length + " Attr: " + NameFromAttributes(fileInfo.Attributes));
#endif
            return Trace("GetFileInformation", fileName, info, DokanResult.Success);
        }
        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {

            List<FileInformation> rfiles = new List<FileInformation>();
            List<CacheFile> cfiles = CacheManager.Instance.FindFiles(fileName);
            foreach (CacheFile cnfile in cfiles)
            {
                rfiles.Add(cnfile.GetFileInformation());
            }
            if (fileName == string.Empty || fileName == "\\" || fileName == "/")
            {
                foreach (IFileSystem f in ServiceControl.Control.GetAllFileSystems())
                {
                    rfiles.Add(new FileInformation
                    {
                        Attributes = FileAttributes.Directory,
                        CreationTime = f.CreatedDate ?? DateTime.Now,
                        FileName = f.Name,
                        LastWriteTime = f.ModifiedDate ?? DateTime.Now,
                        LastAccessTime = f.LastViewed ?? DateTime.Now,
                        Length = 0
                    });
                }
            }
            else
            {
                IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
                if (obj != null && obj is IDirectory)
                {
                    IDirectory d = (IDirectory) obj;
                    if (!d.IsPopulated)
                    {
                        AsyncContext.Run(async () =>
                        {
                            await d.Populate();
                        });
                    }
                    rfiles.AddRange(d.Directories.Select(a => CloudFileSystemHelper.FromObject(a)));
                    rfiles.AddRange(d.Files.Select(a => CloudFileSystemHelper.FromObject(a)));
                }
            }
            files = rfiles.OrderBy(a=>a.FileName).ToList();
#if TRACE
            foreach (FileInformation f in files)
            {
                Console.WriteLine("FindFiles: " + f.FileName + " Length: " + f.Length + " Attr: " + NameFromAttributes(f.Attributes));

            }
#endif
            return Trace("FindFiles", fileName, info, DokanResult.Success);
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info)
        {
            IList<FileInformation> orginfos;
            files=new List<FileInformation>();
            NtStatus stat = FindFiles(fileName, out orginfos, info);
            foreach (FileInformation n in orginfos)
            {
                if (PatternMatcher.StrictMatchPattern(searchPattern, n.FileName))
                {
                    files.Add(n);
                }

            }
            return stat;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            CacheFile cf = CacheManager.Instance.FindFile(fileName);
            cf?.SetFileAttributes(attributes);
            return Trace("SetFileAttributes", fileName, info, DokanResult.Success, attributes.ToString());
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            CacheFile cf = CacheManager.Instance.FindFile(fileName);
            cf?.SetFileTime(creationTime, lastAccessTime, lastWriteTime);
            return Trace("", fileName, info, DokanResult.Success, ToTrace(creationTime), ToTrace(lastAccessTime), ToTrace(lastWriteTime));
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            FileNameInfo finfo = new FileNameInfo(fileName);
            IObject obj = CloudFileSystemHelper.ResolvePath(finfo.Directory);
            if (obj is IDirectory)
                CacheManager.Instance.DeleteLocalFileIfExists((IDirectory) obj, finfo.FileName);
            IObject s = CloudFileSystemHelper.ResolvePath(fileName);
            if (s != null)
            {
                AsyncContext.Run(async () =>
                {
                    await s.Delete(false);
                });
            }
            return Trace("DeleteFile", fileName, info, DokanResult.Success);
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            IObject s = CloudFileSystemHelper.ResolvePath(fileName);
            if (s != null)
            {
                AsyncContext.Run(async () =>
                {
                    await s.Delete(false);
                });
            }

            return Trace("DeleteDirectory", fileName, info, DokanResult.Success);
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            FileNameInfo oldinfo = new FileNameInfo(oldName);
            FileNameInfo newinfo = new FileNameInfo(newName);

            //TODO
            return Trace("MoveFile", oldName, info, DokanResult.FileExists,newName, replace.ToString(CultureInfo.InvariantCulture));


        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return Trace("SetEndOfFile", fileName, info, DokanResult.Success, length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("SetEndOfFile", fileName, info, DokanResult.DiskFull, length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            return Trace("SetAllocationSize", fileName, info, DokanResult.DiskFull, length.ToString(CultureInfo.InvariantCulture));
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Lock(offset, length);
                return Trace("LockFile", fileName, info, DokanResult.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("LockFile", fileName, info, DokanResult.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Unlock(offset, length);
                return Trace("UnlockFile", fileName, info, DokanResult.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("UnlockFile", fileName, info, DokanResult.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            free = 0;
            total = 0;
            used = 0;
            foreach (IFileSystem f in ServiceControl.Control.GetAllFileSystems())
            {
                free += f.Sizes.AvailableSize;
                total += f.Sizes.TotalSize;
                used += f.Sizes.UsedSize;
            }
            return Trace("GetDiskFreeSpace", null, info, DokanResult.Success, "out " + free.ToString(), "out " + total.ToString(), "out " + used.ToString());
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                                out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "CloudFileSystem";
            fileSystemName = "CloudFileSystem";

            features = FileSystemFeatures.NamedStreams;

            return Trace("GetVolumeInformation", null, info, DokanResult.Success, "out " + volumeLabel, "out " + features.ToString(), "out " + fileSystemName);
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {            
            security = null;
            return Trace("GetFileSecurity", fileName, info, DokanResult.NotImplemented);
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            return Trace("SetFileSecurity", fileName, info, DokanResult.NotImplemented);
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            return Trace("Unmount", null, info, DokanResult.Success);
        }


        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
        {
            streams = new FileInformation[0];
            IObject obj = CloudFileSystemHelper.ResolvePath(fileName);
            if (obj==null)
                return Trace("EnumerateNamedStreams", fileName, info, DokanResult.FileNotFound);
            streams=new List<FileInformation>();
            FileInformation f = CloudFileSystemHelper.FromObject(obj);
            streams.Add(f);
            if ((obj.FileSystem.Supports & SupportedFlags.Assets) > 0)
            {
                List<IFile> assets = obj.GetAssets();
                if (assets.Count > 0)
                {
                    foreach (IFile fi in assets)
                    {
                        FileInformation nf = CloudFileSystemHelper.FromObject(fi);
                        nf.FileName = f.FileName + ":" + nf.FileName;
                        streams.Add(nf);
                    }
                }
            }
            if (((obj.FileSystem.Supports & SupportedFlags.MD5) > 0) && (obj is IFile))
            {
                IFile n = (IFile) obj;
                string md5 = n.MD5;
                FileInformation nf=new FileInformation();
                f.CopyTo(nf);
                nf.FileName = nf.FileName + ":md5";
                nf.Length = md5.Length;
                streams.Add(nf);
            }
            if (((obj.FileSystem.Supports & SupportedFlags.SHA1) > 0) && (obj is IFile))
            {
                IFile n = (IFile)obj;
                string sha = n.SHA1;
                FileInformation nf = new FileInformation();
                f.CopyTo(nf);
                nf.FileName = nf.FileName + ":sha";
                nf.Length = sha.Length;
                streams.Add(nf);
            }
            FileInformation nf2=new FileInformation();
            f.CopyTo(nf2);
            nf2.FileName = nf2.FileName + ":metadata";
            nf2.Length = Encoding.UTF8.GetBytes(obj.Metadata).Length;
            streams.Add(nf2);
            return Trace("EnumerateNamedStreams", fileName, info, DokanResult.Success);
        }

        public string NameFromAttributes(FileAttributes file)
        {
            List<string> opts=new List<string>();
            StringBuilder bld=new StringBuilder();
            foreach (FileAttributes f in Enum.GetValues(typeof (FileAttributes)))
            {
                if ((f & file) > 0)
                {
                    opts.Add(Enum.GetName(typeof (FileAttributes), f));
                }
            }
            return string.Join(", ", opts);
        }
        public NtStatus Mounted(DokanFileInfo info)
        {
            return Trace("Mount", null, info, DokanResult.Success);
        }

        public NtStatus Unmounted(DokanFileInfo info)
        {
            return Trace("Unmount", null, info, DokanResult.Success);
        }

    }
}
