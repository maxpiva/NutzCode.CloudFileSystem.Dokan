namespace NutzCode.CloudFileSystem.DokanClient
{
    partial class Main
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkPersist = new System.Windows.Forms.CheckBox();
            this.cmbDisks = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.butMount = new System.Windows.Forms.Button();
            this.butAdd = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panClouds = new System.Windows.Forms.Panel();
            this.listLog = new System.Windows.Forms.ListView();
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLog = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.notIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuMount = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuTray.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkPersist);
            this.panel1.Controls.Add(this.cmbDisks);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.butMount);
            this.panel1.Controls.Add(this.butAdd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(759, 45);
            this.panel1.TabIndex = 2;
            // 
            // chkPersist
            // 
            this.chkPersist.AutoSize = true;
            this.chkPersist.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPersist.Location = new System.Drawing.Point(275, 8);
            this.chkPersist.Name = "chkPersist";
            this.chkPersist.Size = new System.Drawing.Size(76, 25);
            this.chkPersist.TabIndex = 4;
            this.chkPersist.Text = "Persist";
            this.chkPersist.UseVisualStyleBackColor = true;
            this.chkPersist.CheckedChanged += new System.EventHandler(this.chkPersist_CheckedChanged);
            // 
            // cmbDisks
            // 
            this.cmbDisks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisks.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDisks.FormattingEnabled = true;
            this.cmbDisks.Location = new System.Drawing.Point(190, 5);
            this.cmbDisks.Name = "cmbDisks";
            this.cmbDisks.Size = new System.Drawing.Size(69, 29);
            this.cmbDisks.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(128, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 36);
            this.label1.TabIndex = 2;
            this.label1.Text = "Drive:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butMount
            // 
            this.butMount.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butMount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(156)))), ((int)(((byte)(255)))));
            this.butMount.Image = global::NutzCode.CloudFileSystem.DokanClient.Properties.Resources.cloud;
            this.butMount.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.butMount.Location = new System.Drawing.Point(-1, -1);
            this.butMount.Name = "butMount";
            this.butMount.Size = new System.Drawing.Size(123, 40);
            this.butMount.TabIndex = 1;
            this.butMount.Text = "Mount";
            this.butMount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butMount.UseVisualStyleBackColor = true;
            this.butMount.Click += new System.EventHandler(this.butMount_Click);
            // 
            // butAdd
            // 
            this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAdd.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(156)))), ((int)(((byte)(255)))));
            this.butAdd.Image = global::NutzCode.CloudFileSystem.DokanClient.Properties.Resources.add;
            this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.butAdd.Location = new System.Drawing.Point(539, -1);
            this.butAdd.Name = "butAdd";
            this.butAdd.Size = new System.Drawing.Size(221, 40);
            this.butAdd.TabIndex = 0;
            this.butAdd.Text = "Add Cloud Provider";
            this.butAdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butAdd.UseVisualStyleBackColor = true;
            this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 50);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panClouds);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listLog);
            this.splitContainer1.Size = new System.Drawing.Size(759, 289);
            this.splitContainer1.SplitterDistance = 180;
            this.splitContainer1.TabIndex = 3;
            // 
            // panClouds
            // 
            this.panClouds.AutoScroll = true;
            this.panClouds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panClouds.Location = new System.Drawing.Point(0, 0);
            this.panClouds.Name = "panClouds";
            this.panClouds.Size = new System.Drawing.Size(759, 180);
            this.panClouds.TabIndex = 0;
            // 
            // listLog
            // 
            this.listLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDate,
            this.colType,
            this.colTitle,
            this.colLog});
            this.listLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listLog.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listLog.GridLines = true;
            this.listLog.Location = new System.Drawing.Point(0, 0);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(759, 105);
            this.listLog.TabIndex = 3;
            this.listLog.UseCompatibleStateImageBehavior = false;
            // 
            // colDate
            // 
            this.colDate.Text = "DateTime";
            // 
            // colType
            // 
            this.colType.Text = "Type";
            // 
            // colLog
            // 
            this.colLog.Text = "Message";
            this.colLog.Width = -1;
            // 
            // notIcon
            // 
            this.notIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notIcon.BalloonTipText = "Cloud File System Mounter has been minimized.";
            this.notIcon.BalloonTipTitle = "Cloud File System Mounter";
            this.notIcon.ContextMenuStrip = this.menuTray;
            this.notIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notIcon.Icon")));
            this.notIcon.Visible = true;
            // 
            // menuTray
            // 
            this.menuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuMount,
            this.toolStripSeparator1,
            this.menuExit});
            this.menuTray.Name = "menuTray";
            this.menuTray.Size = new System.Drawing.Size(153, 76);
            // 
            // menuMount
            // 
            this.menuMount.Image = global::NutzCode.CloudFileSystem.DokanClient.Properties.Resources.cloud;
            this.menuMount.Name = "menuMount";
            this.menuMount.Size = new System.Drawing.Size(152, 22);
            this.menuMount.Text = "Mount";
            this.menuMount.Click += new System.EventHandler(this.butMount_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // menuExit
            // 
            this.menuExit.Image = global::NutzCode.CloudFileSystem.DokanClient.Properties.Resources.clear;
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(152, 22);
            this.menuExit.Text = "Exit";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 344);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "Cloud File System Mounter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuTray.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butAdd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbDisks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button butMount;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listLog;
        private System.Windows.Forms.NotifyIcon notIcon;
        private System.Windows.Forms.ContextMenuStrip menuTray;
        private System.Windows.Forms.ToolStripMenuItem menuMount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colLog;
        private System.Windows.Forms.Panel panClouds;
        private System.Windows.Forms.ColumnHeader colTitle;
        private System.Windows.Forms.CheckBox chkPersist;
    }
}

