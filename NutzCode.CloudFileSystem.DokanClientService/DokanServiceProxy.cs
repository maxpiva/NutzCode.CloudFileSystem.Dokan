using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public class DokanServiceProxy : ClientBase<IDokanCloudControl>
    {
        

        public DokanServiceProxy()
        : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IDokanCloudClient)),
            new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/"+PipeNames.CloudServiceName)))
        {

        }

        public Task<ServiceResult<Settings>> GetActiveSettings()
        {
            return Channel.GetActiveSettings();
        }

        public Task<ServiceResult> Mount(Settings s)
        {
            return Channel.Mount(s);
        }

        public Task<ServiceResult> Unmount(Settings s)
        {
            return Channel.Unmount();
        }

    }
}
