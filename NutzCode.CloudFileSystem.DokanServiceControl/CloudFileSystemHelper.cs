using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using Nito.AsyncEx;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public static class CloudFileSystemHelper
    {


        public static FileInformation FromObject(IObject o)
        {
            if (o == null)
                return new FileInformation();
            FileInformation f=new FileInformation();
            f.CreationTime = o.CreatedDate?? DateTime.Now;
            f.FileName = o.Name;
            f.LastAccessTime = o.ModifiedDate ?? DateTime.Now;
            f.LastWriteTime = o.LastViewed ?? DateTime.Now;
            if (o is IFile)
            {
                f.Length = ((IFile) o).Size;
                f.Attributes = FileAttributes.Archive ;
            }
            else
            {
                f.Length = 0;
                f.Attributes = FileAttributes.Directory ;
            }
            if ((o.Attributes&ObjectAttributes.Hidden)>0)
                f.Attributes|=FileAttributes.Hidden;
            return f;
        }

      
    }
}
