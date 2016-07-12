using System;
using System.ServiceModel;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public class Hosting
    {
        private static ServiceHost shost = null;

        public static void Start()
        {
            End();
            shost = new ServiceHost(typeof (DokanCloudClient), new Uri[] {new Uri("net.pipe://localhost/")});
            shost.AddServiceEndpoint(typeof (IDokanCloudClient), new NetNamedPipeBinding(), PipeNames.CloudClientName);
            shost.Open();
        }

        public static void End()
        {
            shost?.Close();
        }
    }
}
