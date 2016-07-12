using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceModels;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class ClientServiceProxy : Singleton<ClientServiceProxy>
    {

        public class ClientService : ClientBase<IDokanCloudClient>
        {
            public ClientService() : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IDokanCloudClient)), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/"+PipeNames.CloudClientName)))
            {
  
            }
            public async Task<AuthResult> Login(AuthRequest request, string authprovider)
            {
                return await Channel.Login(request, authprovider);
            }

            public async Task ReportError(string title, string error, ReportType rtype, DateTime timestamp)
            {
                await Channel.ReportError(title, error, rtype, timestamp);
            }
        }

        private ClientService _client = null;
        
        private List<Tuple<string,string, ReportType, DateTime>> _nonLoggedErrors=new List<Tuple<string, string, ReportType, DateTime>>();

        public async Task MayProcessNonLoggedErrors()
        {
            if (_nonLoggedErrors.Count > 0)
            {
                try
                {
                    foreach(Tuple<string, string, ReportType, DateTime> a in _nonLoggedErrors.ToList())
                    { 
                        await _client.ReportError(a.Item1, a.Item2, a.Item3, a.Item4);
                        _nonLoggedErrors.Remove(a);
                    };                    
                }
                catch (Exception)
                {
                }                
            }
        }

        public async Task<AuthResult> Login(AuthRequest request, string authprovider)
        {
            try
            {
                if (_client == null)
                    _client = new ClientService();
                if (_client==null)
                    throw new Exception("Unable to connect to Cloud FileSystem Dokan Client");
                AuthResult res= await _client.Login(request, authprovider);                
                await MayProcessNonLoggedErrors();
                return res;
            }
            catch (Exception)
            {
                _nonLoggedErrors.Add(new Tuple<string,string, ReportType, DateTime>("Authorization Error", "Cannont connect with the Cloud FileSystem Dokan Client to process cloud authorization",ReportType.Error, DateTime.Now));
            }
            return new AuthResult {  HasError=true, ErrorString = "Cannont connect with the Cloud FileSystem Dokan Client to process cloud authorization" };
        }
        public async Task ReportError(string title, string error, ReportType rtype, DateTime timestamp)
        {
            try
            {
                if (_client == null)
                _client = new ClientService();
                if (_client == null)
                    throw new Exception("Unable to connect to Cloud FileSystem Dokan Client");
                await MayProcessNonLoggedErrors();
                await _client.ReportError(title, error, rtype, timestamp);
            }
            catch (Exception)
            {
                _nonLoggedErrors.Add(new Tuple<string, string, ReportType, DateTime>(title, error, rtype, timestamp));
            }
        }
    }
}
