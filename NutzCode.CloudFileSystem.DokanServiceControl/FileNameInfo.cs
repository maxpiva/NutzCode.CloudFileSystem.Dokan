using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class FileNameInfo
    {
        public string AccountPart { get; set; }
        public string DirectoryPart { get; set; }
        public string NamePart { get; set; }
        public string AltStreamPart { get; set; }

        public string FullDirectory => string.IsNullOrEmpty(DirectoryPart) ? AccountPart : string.Join("/", AccountPart, DirectoryPart);
        public string FullPathWithoutAltStream => string.IsNullOrEmpty(NamePart) ? FullDirectory : string.Join("/", FullDirectory, NamePart);
        public string FullPath => string.IsNullOrEmpty(AltStreamPart) ? FullPathWithoutAltStream : FullPathWithoutAltStream + ":" + AltStreamPart;

        public string Directory => DirectoryPart ?? string.Empty;
        public string PathWithoutAltStream => string.IsNullOrEmpty(NamePart) ? Directory : string.Join("/",Directory,NamePart);
        public string Path => string.IsNullOrEmpty(AltStreamPart) ? PathWithoutAltStream : PathWithoutAltStream + ":" + AltStreamPart;


        public FileNameInfo()
        {
            
        }
        public FileNameInfo(string fileName)
        {
            fileName = fileName.Replace("\\", "/");
            int idx = fileName.IndexOf('/');
            AccountPart = string.Empty;
            if (idx >= 0)
            {
                AccountPart = fileName.Substring(0, idx);
                DirectoryPart = string.Empty;
                NamePart = string.Empty;
                string rdir = fileName.Substring(idx + 1);
                if (rdir.Length > 0)
                {
                    AltStreamPart = null;
                    idx = rdir.LastIndexOf("\\");
                    int idx2 = rdir.LastIndexOf(":");
                    if (idx != 0 && idx2 != 0 && idx2 > idx)
                    {
                        AltStreamPart = rdir.Substring(idx2 + 1);
                        rdir = rdir.Substring(0, idx2);
                    }
                    if (idx >= 0)
                    {
                        DirectoryPart = rdir.Substring(0, idx);
                        NamePart = rdir.Substring(idx + 1);
                    }
                }
            }
        }



        public IObject Resolve()
        {
            IFileSystem r = ServiceControl.Control.GetFileSystem(AccountPart);
            if (r != null)
            {
                return AsyncContext.Run<IObject>(async () =>
                {
                    if (string.IsNullOrEmpty(Directory))
                        return r;
                    FileSystemResult<IObject> obj = await r.FromPath(PathWithoutAltStream);
                    if (obj.IsOk)
                    {
                        if (!string.IsNullOrEmpty(AltStreamPart))
                        {
                            if ((AltStreamPart.ToLowerInvariant() == "md5") && ((obj.Result.FileSystem.Supports & SupportedFlags.MD5) > 0) && (obj is IFile))
                                return new MemoryFile(obj.Result.FullName + ":" + AltStreamPart, AltStreamPart, "text/plain",
                                    Encoding.UTF8.GetBytes(((IFile)obj).MD5));
                            if ((AltStreamPart.ToLowerInvariant() == "sha") && ((obj.Result.FileSystem.Supports & SupportedFlags.SHA1) > 0) && (obj is IFile))
                                return new MemoryFile(obj.Result.FullName + ":" + AltStreamPart, AltStreamPart, "text/plain",
                                    Encoding.UTF8.GetBytes(((IFile)obj).SHA1));
                            if (AltStreamPart.ToLowerInvariant() == "metadata")
                                return new MemoryFile(obj.Result.FullName + ":" + AltStreamPart, AltStreamPart, obj.Result.MetadataMime, Encoding.UTF8.GetBytes(obj.Metadata));
                            List<IFile> assets = obj.Result.GetAssets();
                            foreach (IFile f in assets)
                            {
                                if (f.Name.Equals(AltStreamPart, StringComparison.InvariantCultureIgnoreCase))
                                    return f;
                            }
                            return null;
                        }
                        return obj.Result;
                    }
                    return null;
                });
            }
            return null;
        }
    }
}
