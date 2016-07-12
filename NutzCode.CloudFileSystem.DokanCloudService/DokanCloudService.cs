using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NutzCode.CloudFileSystem.DokanServiceControl;

namespace NutzCode.CloudFileSystem.DokanCloudService
{
    public partial class DokanCloudService : ServiceBase
    {
        private ServiceControl svc = new ServiceControl();

        public DokanCloudService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            svc.Start();
        }
        protected override void OnStart(string[] args)
        {
            svc.Start();
        }

        public void Stop()
        {
            svc.Stop();
        }
        protected override void OnStop()
        {
            svc.Stop();
        }
    }
}
