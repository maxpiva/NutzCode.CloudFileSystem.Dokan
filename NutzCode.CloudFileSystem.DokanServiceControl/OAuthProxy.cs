using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.DokanServiceControl
{
    public class OAuthProxy : IOAuthProvider
    {
        public string Name => "Forms Proxy";

        private readonly string _providername;

        public OAuthProxy(string providername)
        {
            _providername = providername;
        }

        public async Task<AuthResult> Login(AuthRequest request)
        {
            return await ClientServiceProxy.Instance.Login(request,_providername);
        }
    }
}
