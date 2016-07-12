using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class Hosting
    {
        private static ServiceHost shost = null;

        public static void Start()
        {
            End();
            shost = new ServiceHost(typeof(DokanCloudControl), new Uri[] { new Uri("net.pipe://localhost/") });
            shost.AddServiceEndpoint(typeof(IDokanCloudControl), new NetNamedPipeBinding(), PipeNames.CloudServiceName);
            shost.Open();
        }

        public static void End()
        {
            shost?.Close();
        }
    }
}
