using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public static class Extensions
    {
        public static void InvokeUI(this Control c, Action<Control> act)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new Action(() => act(c)));
            }
            else
            {
                act(c);
            }
        }
    }
}
