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

        private static DokanCloudClient _instance = null;
        public static DokanCloudClient Instance => _instance ?? (_instance = new DokanCloudClient());

        public delegate void ReportHandler(string filename, long transfer, long total, bool upload);

        public event ReportHandler OnReportSync;

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

        public async Task ReportError(string title, string error, ReportType warning, DateTime timestamp)
        {
            if (Program.MainForm!=null)
            {
                await Task.Run(() =>
                {
                    Program.MainForm.InvokeUI((a) =>
                    {
                        ((Main)a).AddToLog(title, error, warning, timestamp);
                    });
                });
            }
        }

        public async Task ReportSyncProgress(string filename, long transfer, long total, SyncType type, string errormessage)
        {
            if (Program.MainForm != null)
            {
                await Task.Run(() =>
                {
                    Program.MainForm.InvokeUI((a) =>
                    {
                        ((Main)a).ReportFileName(filename, transfer, total, type, errormessage);
                    });
                });
            }

        }
    }
}
