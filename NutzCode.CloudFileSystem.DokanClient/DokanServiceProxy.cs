using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public class DokanServiceProxy 
    {
        public class DokanService : ClientBase<IDokanCloudControl>
        {
            public DokanService() : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IDokanCloudClient)),
            new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/" + PipeNames.CloudServiceName)))
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

            public Task<ServiceResult> Unmount()
            {
                return Channel.Unmount();
            }
        }

        private DokanService _client = null;


        private static DokanServiceProxy _instance = null;
        public static DokanServiceProxy Instance => _instance ?? (_instance = new DokanServiceProxy());


        public async Task<ServiceResult<Settings>> GetActiveSettings()
        {
            try
            {
                if (_client == null)
                    _client = new DokanService();
                return await _client.GetActiveSettings();
            }
            catch (Exception)
            {
                return new ServiceResult<Settings> {Error = "Unable to get Settings", IsOk = false};
            }
        }

        public async Task<ServiceResult> Mount(Settings s)
        {
            try
            {
                if (_client == null)
                    _client = new DokanService();
                return await _client.Mount(s);
            }
            catch (Exception)
            {
                return new ServiceResult { Error = "Unable to Mount Providers", IsOk = false };
            }
        }

        public async Task<ServiceResult> Unmount()
        {
            try
            {
                if (_client == null)
                    _client = new DokanService();
                return await _client.Unmount();
            }
            catch (Exception)
            {
                return new ServiceResult { Error = "Unable to UnMount Providers", IsOk = false };
            }
        }

    }
}
