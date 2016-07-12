using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NutzCode.CloudFileSystem.DokanServiceModels;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public partial class Main : Form
    {
        private Settings _currentSettings = new Settings() {Accounts = new List<Account>()};

        private const long DOptions = 16 + 4 + 1;
        public Main()
        {
            InitializeComponent();
        }

        private async Task GetSettings()
        {
            ServiceResult<Settings> res= await DokanServiceProxy.Instance.GetActiveSettings();
            if (!res.IsOk)
            {
                await DokanCloudClient.Instance.ReportError(res.Error, "Please make sure Dokan Cloud Service is running", ReportType.Error, DateTime.Now);
            }
            else
            {
                if (res.Result == null)
                {
                    _currentSettings = new Settings() { Accounts = new List<Account>(),IsMounted = false,DokanOptions=DOptions};
                }
                else
                {
                    _currentSettings = res.Result;
                    panClouds.Controls.Clear();
                    foreach (Account ac in _currentSettings.Accounts)
                        await AddAccount(ac, false);
                }
                RefreshComboDisks();
                chkPersist.Checked = _currentSettings.Persist;
                SetMountButtonState(_currentSettings.IsMounted);
            }
        }

        public void SetMountButtonState(bool mounted)
        {
            if (!mounted)
            {
                butMount.Text = menuMount.Text = "Mount";
                butMount.Image = menuMount.Image = Properties.Resources.cloud;

            }
            else
            {
                butMount.Text = menuMount.Text = "Eject";
                butMount.Image = menuMount.Image = Properties.Resources.eject;
            }
        }

        private void RefreshComboDisks()
        {
            List<string> used = new List<string>();
            DriveInfo[] infos=System.IO.DriveInfo.GetDrives();
            string inuse = _currentSettings?.MountPoint;
            foreach (DriveInfo n in infos)
            {
                string letter = n.Name.Substring(0, 1).ToUpper();
                if (inuse != letter)
                {
                    used.Add(letter);
                }                
            }
            List<string> notused=new List<string>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (!used.Contains(c.ToString()))
                    notused.Add(c.ToString());
            }
            cmbDisks.Items.Clear();
            notused.ForEach(a=>cmbDisks.Items.Add(a));
            if (!string.IsNullOrEmpty(inuse))
            {
                int idex = notused.IndexOf(inuse);
                if (idex >= 0)
                    cmbDisks.SelectedIndex = idex;
            }
        }
        private async void butAdd_Click(object sender, EventArgs e)
        {
            ProviderChoser ch=new ProviderChoser();
            if (ch.ShowDialog() == DialogResult.OK)
            {
                await AddAccount(ch.Account);
            }
        }

        public async Task AddAccount(Account ac, bool updatesettings=true)
        {
            if (panClouds.Controls.Count>0)
            {
                Panel p=new Panel();
                p.Dock=DockStyle.Top;
                p.Size=new Size(1,1);
                p.BackColor = Color.Gray;
                panClouds.Controls.Add(p);
            }
            CloudProvider c=new CloudProvider();
            c.Dock = DockStyle.Top;
            c.Account = ac;
            c.DisconnectAccount += async a =>
            {
                await RemoveAccount(a.Account);
            };
            panClouds.Controls.Add(c);
            if (updatesettings)
            {
                _currentSettings.Accounts.Add(ac);
                await UpdateSettings();
            }
        }

        public async Task UpdateSettings()
        {
            if (_currentSettings.IsMounted)
            {
                await DokanServiceProxy.Instance.Mount(_currentSettings);
            }
        }
        public async Task RemoveAccount(Account ac)
        {
            foreach (Control c in panClouds.Controls)
            {
                CloudProvider cp = c as CloudProvider;
                if (cp!=null)
                {
                    int idx = panClouds.Controls.IndexOf(c);
                    if (idx!= 0) //Remove Separator
                        panClouds.Controls.RemoveAt(idx-1);
                    panClouds.Controls.Remove(c);
                    Account cc=_currentSettings.Accounts.FirstOrDefault(a=>a.Name==cp.Account.Name && a.PluginName==cp.Account.PluginName);
                    if (cc != null)
                    {
                        _currentSettings.Accounts.Remove(cc);
                        await UpdateSettings();
                        break;
                    }
                }
            }
        }

        private async void butMount_Click(object sender, EventArgs e)
        {
            if (_currentSettings.IsMounted)
            {
                ServiceResult r = await DokanServiceProxy.Instance.Unmount();
                if (r.IsOk)
                {
                    _currentSettings.IsMounted = false;
                    SetMountButtonState(_currentSettings.IsMounted);
                }
            }
            else
            {
                if (_currentSettings.Accounts.Count == 0)
                {
                    MessageBox.Show("Mount Error", "You should have a least one cloud provider to mount a disk", MessageBoxButtons.OK);
                }
                else
                {
                    ServiceResult r = await DokanServiceProxy.Instance.Mount(_currentSettings);
                    if (r.IsOk)
                    {
                        _currentSettings.IsMounted = true;
                        SetMountButtonState(_currentSettings.IsMounted);
                    }
                }
            }
        }

        private async void chkPersist_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateSettings();
        }



        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notIcon.Visible = true;
                notIcon.ShowBalloonTip(500);
                Hide();
            }

            else if (WindowState == FormWindowState.Normal)
            {
                notIcon.Visible = false;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                notIcon.Visible = true;
                notIcon.ShowBalloonTip(500);
                Hide();                
                e.Cancel = true;
            }
        }

        public void ReportFileName(string filename, long transfer, long total, SyncType type, string errormessage)
        {
            
        }
        public void AddToLog(string title, string message, ReportType type, DateTime time)
        {
            ListViewItem lv = new ListViewItem(new string[] {time.ToString("g"), type.ToString(), title, message});
            if (listLog.Items.Count == 0)
                listLog.Items.Add(lv);
            else
                listLog.Items.Insert(0, lv);
        }
        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
