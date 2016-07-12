using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public partial class ProviderChoser : Form
    {
        public ProviderChoser()
        {
            InitializeComponent();
            foreach (ICloudPlugin plugin in CloudFileSystemPluginFactory.Instance.List)
            {
                DropDownItem di=new DropDownItem();
                di.Image = plugin.Icon;
                di.Value = plugin.Name;
                cmbProviders.Items.Add(di);
            }
            if (cmbProviders.Items.Count > 0)
                cmbProviders.SelectedIndex = 0;
        }

        private Account _account;

        public Account Account
        {
            get { return _account; }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (cmbProviders.SelectedIndex >= 0)
            {
                _account = new Account();
                _account.Name = txtName.Text;
                _account.PluginName = ((DropDownItem) cmbProviders.SelectedItem).Value;
            }
        }
    }
}
