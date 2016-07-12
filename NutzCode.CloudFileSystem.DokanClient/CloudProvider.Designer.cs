namespace NutzCode.CloudFileSystem.DokanClient
{
    partial class CloudProvider
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.butKill = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pic = new System.Windows.Forms.PictureBox();
            this.labName = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.butKill);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(562, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(60, 48);
            this.panel1.TabIndex = 3;
            // 
            // butKill
            // 
            this.butKill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butKill.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butKill.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(156)))), ((int)(((byte)(255)))));
            this.butKill.Image = global::NutzCode.CloudFileSystem.DokanClient.Properties.Resources.clear;
            this.butKill.Location = new System.Drawing.Point(10, 4);
            this.butKill.Name = "butKill";
            this.butKill.Size = new System.Drawing.Size(46, 40);
            this.butKill.TabIndex = 3;
            this.butKill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butKill.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pic);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(64, 48);
            this.panel2.TabIndex = 4;
            // 
            // pic
            // 
            this.pic.Location = new System.Drawing.Point(5, 0);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(48, 48);
            this.pic.TabIndex = 1;
            this.pic.TabStop = false;
            // 
            // labName
            // 
            this.labName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labName.Location = new System.Drawing.Point(64, 0);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(498, 48);
            this.labName.TabIndex = 5;
            this.labName.Text = "Google Amazon One Drive Cloud";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CloudProvider
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.labName);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CloudProvider";
            this.Size = new System.Drawing.Size(622, 48);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.PictureBox pic;
        private System.Windows.Forms.Button butKill;
    }
}
