using System;
using System.Linq;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceModels;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public class DokanCloudClient : IDokanCloudClient
    {
        readonly OAuthProviderFactory _factory=new OAuthProviderFactory();

        public async Task<AuthResult> Login(AuthRequest request, string authprovider)
        {
            IOAuthProvider provider = _factory.List.FirstOrDefault(a => a.Name == authprovider);
            if (provider != null)
                return await provider.Login(request);
            AuthResult r=new AuthResult();
            r.ErrorString = "Unable to find OAuth Login Provider '" + authprovider + "'.";
            r.HasError = true;
            return r;
        }

        public Task ReportError(string title, string error, ReportType warning, DateTime timestamp)
        {
            throw new NotImplementedException();
        }
    }
}
