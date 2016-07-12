using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public partial class CloudProvider : UserControl
    {
        public CloudProvider()
        {
            InitializeComponent();
            butKill.Click += (a, b) =>
            {
                if (MessageBox.Show(
                    "Are you sure you want to disconnect the account "+_account.Name+"?",
                    "Disconnect "+_account.PluginName, 
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                    )==DialogResult.Yes)
                    DisconnectAccount?.Invoke(this);
            };
        }

        public delegate void DisconnectHandler(CloudProvider provider);
        public event DisconnectHandler DisconnectAccount;

        private Account _account;
        public Account Account
        {
            get { return _account; }
            set
            {
                _account = value;
                labName.Text = _account.Name;
                ICloudPlugin plugin=CloudFileSystemPluginFactory.Instance.List.FirstOrDefault(a=>a.Name==_account.PluginName);
                if (plugin != null)
                    pic.Image = plugin.Icon;
            }
        }

    }
}
