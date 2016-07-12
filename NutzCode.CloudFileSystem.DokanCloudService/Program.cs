using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanCloudService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new DokanCloudService() 
            };
            ServiceBase.Run(ServicesToRun);
#else
            DokanCloudService svc = new DokanCloudService();
            svc.Start();
            do
            {
                Thread.Sleep(1000);
            } while (true);
            svc.Stop();
#endif

        }
    }
}
