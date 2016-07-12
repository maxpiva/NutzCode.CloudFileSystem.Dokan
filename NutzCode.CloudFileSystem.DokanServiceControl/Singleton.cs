using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class Singleton<T>
    {
        static Singleton()
        {
        }

        public static T Instance { get; } = default(T);
    }
}
