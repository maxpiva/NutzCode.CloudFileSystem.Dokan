using System;
using System.ServiceModel;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.OAuth2;

namespace NutzCode.CloudFileSystem.DokanServiceModels
{
    [ServiceContract]
    public interface IDokanCloudClient
    {
        [OperationContract]
        Task<AuthResult> Login(AuthRequest request, string authprovider);

        [OperationContract]
        Task ReportError(string title, string error, ReportType warning, DateTime timestamp);

        [OperationContract]
        Task ReportSyncProgress(string filename, long transfer, long total, SyncType type, string errormessage);

    }
}
