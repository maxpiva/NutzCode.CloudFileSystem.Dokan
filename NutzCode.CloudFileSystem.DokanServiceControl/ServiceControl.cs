using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class ServiceControl
    {
        public static DokanCloudControl Control=new DokanCloudControl();

        public void Start()
        {
            Hosting.Start();
            Control.Init().RunSynchronously();
        }

        public void Stop()
        {
            Hosting.End();
        }

    }
}
