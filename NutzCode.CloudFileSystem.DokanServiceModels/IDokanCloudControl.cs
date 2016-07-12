using System.ServiceModel;
using System.Threading.Tasks;

namespace NutzCode.CloudFileSystem.DokanServiceModels
{
    [ServiceContract]
    public interface IDokanCloudControl
    {
        [OperationContract]
        Task<ServiceResult<Settings>> GetActiveSettings();

        [OperationContract]
        Task<ServiceResult> Mount(Settings s);

        [OperationContract]
        Task<ServiceResult> Unmount();

        [OperationContract]
        Task<ServiceResult> ReportSyncProgress(bool report);
    }

}
