using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanClient.Cache
{
    public static class Extensions
    {
        public static string GetMD5(this string str)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", string.Empty);
            }
        }
    }
}
