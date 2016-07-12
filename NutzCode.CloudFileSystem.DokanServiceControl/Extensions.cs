using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceControl.Cache;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public static class Extensions
    {
        public static void CopyTo(this object s, object d)
        {
            foreach (PropertyInfo pis in s.GetType().GetProperties())
            {
                foreach (PropertyInfo pid in d.GetType().GetProperties())
                {
                    if (pid.Name == pis.Name)
                        (pid.GetSetMethod()).Invoke(d, new[] { pis.GetGetMethod().Invoke(s, null) });
                }
            };
        }

        public static string GetKey(this ConcurrentDictionary<string, CacheFile> dict,
            Func<KeyValuePair<string, CacheFile>, bool> predicate)
        {
            return dict.Where(predicate).Select(a=> (KeyValuePair<string, CacheFile>?)a).FirstOrDefault()?.Key;
        }
    }
}
