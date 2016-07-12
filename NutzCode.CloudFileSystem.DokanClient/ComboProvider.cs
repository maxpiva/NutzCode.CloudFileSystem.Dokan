using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NutzCode.CloudFileSystem.DokanClient
{
    public class ComboProvider : ComboBox
    {
        public ComboProvider()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.Index != -1)
            {
                DropDownItem item = (DropDownItem) Items[e.Index];
                if (item.Image != null)
                {
                    Rectangle src = new Rectangle(0, 0, item.Image.Width, item.Image.Height);
                    Rectangle dst = new Rectangle(0, 0, e.Bounds.Height, e.Bounds.Height);
                    e.Graphics.DrawImage(item.Image, dst, src, GraphicsUnit.Pixel);
                    SizeF s = e.Graphics.MeasureString("qPWj", e.Font);
                    e.Graphics.DrawString(item.Value, e.Font, new SolidBrush(e.ForeColor),
                        e.Bounds.Left + e.Bounds.Height + 5, e.Bounds.Top + ((e.Bounds.Height - s.Height)/2));
                }
            }
            base.OnDrawItem(e);
        }
    }
    public class DropDownItem
    {
        public string Value { get; set; }
        public Image Image { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
