namespace OSDiagTool.OSDiagToolForm {
    partial class OsDiagForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OsDiagForm));
            this.label1 = new System.Windows.Forms.Label();
            this.DBCredentials = new System.Windows.Forms.GroupBox();
            this.bt_TestSaConnection = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.ipt_saPwd = new System.Windows.Forms.TextBox();
            this.ipt_saUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_iisThreads = new System.Windows.Forms.CheckBox();
            this.cb_osServicesThreads = new System.Windows.Forms.CheckBox();
            this.cb_osMemDumps = new System.Windows.Forms.CheckBox();
            this.cb_iisMemDumps = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cb_iisAccessLogs = new System.Windows.Forms.CheckBox();
            this.cb_EvtViewerLogs = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cb_dbTroubleshoot = new System.Windows.Forms.CheckBox();
            this.cb_dbPlatformMetamodel = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.bt_runOsDiagTool = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrp_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrp_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.DBCredentials.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(263, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select below the information you wish to collect";
            // 
            // DBCredentials
            // 
            this.DBCredentials.Controls.Add(this.bt_TestSaConnection);
            this.DBCredentials.Controls.Add(this.label6);
            this.DBCredentials.Controls.Add(this.ipt_saPwd);
            this.DBCredentials.Controls.Add(this.ipt_saUsername);
            this.DBCredentials.Controls.Add(this.label3);
            this.DBCredentials.Controls.Add(this.label2);
            this.DBCredentials.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBCredentials.Location = new System.Drawing.Point(9, 350);
            this.DBCredentials.Name = "DBCredentials";
            this.DBCredentials.Size = new System.Drawing.Size(410, 132);
            this.DBCredentials.TabIndex = 1;
            this.DBCredentials.TabStop = false;
            this.DBCredentials.Text = "Database sa credentials";
            // 
            // bt_TestSaConnection
            // 
            this.bt_TestSaConnection.Location = new System.Drawing.Point(293, 40);
            this.bt_TestSaConnection.Name = "bt_TestSaConnection";
            this.bt_TestSaConnection.Size = new System.Drawing.Size(111, 23);
            this.bt_TestSaConnection.TabIndex = 5;
            this.bt_TestSaConnection.Text = "Test Connection";
            this.bt_TestSaConnection.UseVisualStyleBackColor = true;
            this.bt_TestSaConnection.Click += new System.EventHandler(this.bt_TestSaConnection_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(329, 24);
            this.label6.TabIndex = 4;
            this.label6.Text = "* If no sa credentials are provided, Database troubleshoot will not be performed." +
    "\r\nDatabase Troubleshoot checkbox must be checked.";
            // 
            // ipt_saPwd
            // 
            this.ipt_saPwd.Location = new System.Drawing.Point(77, 55);
            this.ipt_saPwd.Name = "ipt_saPwd";
            this.ipt_saPwd.PasswordChar = '*';
            this.ipt_saPwd.Size = new System.Drawing.Size(200, 21);
            this.ipt_saPwd.TabIndex = 3;
            this.ipt_saPwd.UseSystemPasswordChar = true;
            // 
            // ipt_saUsername
            // 
            this.ipt_saUsername.Location = new System.Drawing.Point(77, 23);
            this.ipt_saUsername.Name = "ipt_saUsername";
            this.ipt_saUsername.Size = new System.Drawing.Size(200, 21);
            this.ipt_saUsername.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "User";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Thread Dumps";
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(12, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(410, 2);
            this.label5.TabIndex = 3;
            // 
            // cb_iisThreads
            // 
            this.cb_iisThreads.AutoSize = true;
            this.cb_iisThreads.Checked = true;
            this.cb_iisThreads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_iisThreads.Location = new System.Drawing.Point(18, 109);
            this.cb_iisThreads.Name = "cb_iisThreads";
            this.cb_iisThreads.Size = new System.Drawing.Size(129, 17);
            this.cb_iisThreads.TabIndex = 4;
            this.cb_iisThreads.Text = "IIS Worker Processes";
            this.cb_iisThreads.UseVisualStyleBackColor = true;
            // 
            // cb_osServicesThreads
            // 
            this.cb_osServicesThreads.AutoSize = true;
            this.cb_osServicesThreads.Checked = true;
            this.cb_osServicesThreads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_osServicesThreads.Location = new System.Drawing.Point(212, 109);
            this.cb_osServicesThreads.Name = "cb_osServicesThreads";
            this.cb_osServicesThreads.Size = new System.Drawing.Size(126, 17);
            this.cb_osServicesThreads.TabIndex = 5;
            this.cb_osServicesThreads.Text = "OutSystems Services";
            this.cb_osServicesThreads.UseVisualStyleBackColor = true;
            // 
            // cb_osMemDumps
            // 
            this.cb_osMemDumps.AutoSize = true;
            this.cb_osMemDumps.Location = new System.Drawing.Point(212, 178);
            this.cb_osMemDumps.Name = "cb_osMemDumps";
            this.cb_osMemDumps.Size = new System.Drawing.Size(126, 17);
            this.cb_osMemDumps.TabIndex = 9;
            this.cb_osMemDumps.Text = "OutSystems Services";
            this.cb_osMemDumps.UseVisualStyleBackColor = true;
            // 
            // cb_iisMemDumps
            // 
            this.cb_iisMemDumps.AutoSize = true;
            this.cb_iisMemDumps.Location = new System.Drawing.Point(18, 178);
            this.cb_iisMemDumps.Name = "cb_iisMemDumps";
            this.cb_iisMemDumps.Size = new System.Drawing.Size(129, 17);
            this.cb_iisMemDumps.TabIndex = 8;
            this.cb_iisMemDumps.Text = "IIS Worker Processes";
            this.cb_iisMemDumps.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label7.Location = new System.Drawing.Point(12, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(410, 2);
            this.label7.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(12, 139);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 15);
            this.label8.TabIndex = 6;
            this.label8.Text = "Memory Dumps";
            // 
            // cb_iisAccessLogs
            // 
            this.cb_iisAccessLogs.AutoSize = true;
            this.cb_iisAccessLogs.Checked = true;
            this.cb_iisAccessLogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_iisAccessLogs.Location = new System.Drawing.Point(212, 245);
            this.cb_iisAccessLogs.Name = "cb_iisAccessLogs";
            this.cb_iisAccessLogs.Size = new System.Drawing.Size(103, 17);
            this.cb_iisAccessLogs.TabIndex = 13;
            this.cb_iisAccessLogs.Text = "IIS Access Logs";
            this.cb_iisAccessLogs.UseVisualStyleBackColor = true;
            // 
            // cb_EvtViewerLogs
            // 
            this.cb_EvtViewerLogs.AutoSize = true;
            this.cb_EvtViewerLogs.Checked = true;
            this.cb_EvtViewerLogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_EvtViewerLogs.Location = new System.Drawing.Point(18, 245);
            this.cb_EvtViewerLogs.Name = "cb_EvtViewerLogs";
            this.cb_EvtViewerLogs.Size = new System.Drawing.Size(115, 17);
            this.cb_EvtViewerLogs.TabIndex = 12;
            this.cb_EvtViewerLogs.Text = "Event Viewer Logs";
            this.cb_EvtViewerLogs.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label9.Location = new System.Drawing.Point(12, 231);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(410, 2);
            this.label9.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(12, 207);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 15);
            this.label10.TabIndex = 10;
            this.label10.Text = "Server Logs";
            // 
            // cb_dbTroubleshoot
            // 
            this.cb_dbTroubleshoot.AutoSize = true;
            this.cb_dbTroubleshoot.Checked = true;
            this.cb_dbTroubleshoot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_dbTroubleshoot.Location = new System.Drawing.Point(212, 317);
            this.cb_dbTroubleshoot.Name = "cb_dbTroubleshoot";
            this.cb_dbTroubleshoot.Size = new System.Drawing.Size(137, 17);
            this.cb_dbTroubleshoot.TabIndex = 17;
            this.cb_dbTroubleshoot.Text = "Database Troubleshoot";
            this.cb_dbTroubleshoot.UseVisualStyleBackColor = true;
            // 
            // cb_dbPlatformMetamodel
            // 
            this.cb_dbPlatformMetamodel.AutoSize = true;
            this.cb_dbPlatformMetamodel.Checked = true;
            this.cb_dbPlatformMetamodel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_dbPlatformMetamodel.Location = new System.Drawing.Point(18, 317);
            this.cb_dbPlatformMetamodel.Name = "cb_dbPlatformMetamodel";
            this.cb_dbPlatformMetamodel.Size = new System.Drawing.Size(178, 17);
            this.cb_dbPlatformMetamodel.TabIndex = 16;
            this.cb_dbPlatformMetamodel.Text = "OutSystems Platform Metamodel";
            this.cb_dbPlatformMetamodel.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label11.Location = new System.Drawing.Point(9, 303);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(410, 2);
            this.label11.TabIndex = 15;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(9, 279);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(125, 15);
            this.label12.TabIndex = 14;
            this.label12.Text = "Database Information";
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(12, 56);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(410, 2);
            this.label13.TabIndex = 18;
            // 
            // bt_runOsDiagTool
            // 
            this.bt_runOsDiagTool.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_runOsDiagTool.Location = new System.Drawing.Point(161, 498);
            this.bt_runOsDiagTool.Name = "bt_runOsDiagTool";
            this.bt_runOsDiagTool.Size = new System.Drawing.Size(107, 23);
            this.bt_runOsDiagTool.TabIndex = 19;
            this.bt_runOsDiagTool.Text = "Run";
            this.bt_runOsDiagTool.UseVisualStyleBackColor = true;
            this.bt_runOsDiagTool.Click += new System.EventHandler(this.bt_runOsDiagTool_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(442, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mstrp_Help,
            this.mstrp_Exit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // mstrp_Help
            // 
            this.mstrp_Help.Name = "mstrp_Help";
            this.mstrp_Help.Size = new System.Drawing.Size(180, 22);
            this.mstrp_Help.Text = "Help";
            this.mstrp_Help.Click += new System.EventHandler(this.mstrp_Help_Click);
            // 
            // mstrp_Exit
            // 
            this.mstrp_Exit.Name = "mstrp_Exit";
            this.mstrp_Exit.Size = new System.Drawing.Size(180, 22);
            this.mstrp_Exit.Text = "Exit";
            this.mstrp_Exit.Click += new System.EventHandler(this.mstrp_Exit_Click);
            // 
            // OsDiagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(442, 558);
            this.Controls.Add(this.bt_runOsDiagTool);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.cb_dbTroubleshoot);
            this.Controls.Add(this.cb_dbPlatformMetamodel);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.cb_iisAccessLogs);
            this.Controls.Add(this.cb_EvtViewerLogs);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cb_osMemDumps);
            this.Controls.Add(this.cb_iisMemDumps);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cb_osServicesThreads);
            this.Controls.Add(this.cb_iisThreads);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.DBCredentials);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "OsDiagForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "OutSystems Diagnostics Tool";
            this.DBCredentials.ResumeLayout(false);
            this.DBCredentials.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox DBCredentials;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ipt_saPwd;
        private System.Windows.Forms.TextBox ipt_saUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cb_iisThreads;
        private System.Windows.Forms.CheckBox cb_osServicesThreads;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button bt_TestSaConnection;
        private System.Windows.Forms.CheckBox cb_osMemDumps;
        private System.Windows.Forms.CheckBox cb_iisMemDumps;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox cb_iisAccessLogs;
        private System.Windows.Forms.CheckBox cb_EvtViewerLogs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox cb_dbTroubleshoot;
        private System.Windows.Forms.CheckBox cb_dbPlatformMetamodel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button bt_runOsDiagTool;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mstrp_Help;
        private System.Windows.Forms.ToolStripMenuItem mstrp_Exit;
    }
}