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
            this.label13 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrp_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrp_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.tb_osDiagConf = new System.Windows.Forms.TabControl();
            this.tb_osDiagGenConf = new System.Windows.Forms.TabPage();
            this.label22 = new System.Windows.Forms.Label();
            this.cb_platformAndServerFiles = new System.Windows.Forms.CheckBox();
            this.nud_topLogs = new System.Windows.Forms.NumericUpDown();
            this.label21 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.cb_platformLogs = new System.Windows.Forms.CheckBox();
            this.nud_iisLogsNrDays = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.tb_databaseConf = new System.Windows.Forms.TabPage();
            this.bt_removeMetamodelTables = new System.Windows.Forms.Button();
            this.bt_addMetamodelTables = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.tb_inptMetamodelTables = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lb_metamodelTables = new System.Windows.Forms.ListBox();
            this.DBCredentials = new System.Windows.Forms.GroupBox();
            this.bt_TestSaConnection = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_iptSaPwd = new System.Windows.Forms.TextBox();
            this.tb_iptSaUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_dbTroubleshoot = new System.Windows.Forms.CheckBox();
            this.cb_dbPlatformMetamodel = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.bt_runOsDiagTool = new System.Windows.Forms.Button();
            this.lbl_feedbackOsDiagForm = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1.SuspendLayout();
            this.tb_osDiagConf.SuspendLayout();
            this.tb_osDiagGenConf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_topLogs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_iisLogsNrDays)).BeginInit();
            this.tb_databaseConf.SuspendLayout();
            this.DBCredentials.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(263, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select below the information you wish to collect";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(18, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Thread Dumps";
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(18, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(410, 2);
            this.label5.TabIndex = 3;
            // 
            // cb_iisThreads
            // 
            this.cb_iisThreads.AutoSize = true;
            this.cb_iisThreads.Checked = true;
            this.cb_iisThreads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_iisThreads.Location = new System.Drawing.Point(24, 92);
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
            this.cb_osServicesThreads.Location = new System.Drawing.Point(208, 92);
            this.cb_osServicesThreads.Name = "cb_osServicesThreads";
            this.cb_osServicesThreads.Size = new System.Drawing.Size(126, 17);
            this.cb_osServicesThreads.TabIndex = 5;
            this.cb_osServicesThreads.Text = "OutSystems Services";
            this.cb_osServicesThreads.UseVisualStyleBackColor = true;
            // 
            // cb_osMemDumps
            // 
            this.cb_osMemDumps.AutoSize = true;
            this.cb_osMemDumps.Location = new System.Drawing.Point(208, 161);
            this.cb_osMemDumps.Name = "cb_osMemDumps";
            this.cb_osMemDumps.Size = new System.Drawing.Size(126, 17);
            this.cb_osMemDumps.TabIndex = 9;
            this.cb_osMemDumps.Text = "OutSystems Services";
            this.cb_osMemDumps.UseVisualStyleBackColor = true;
            // 
            // cb_iisMemDumps
            // 
            this.cb_iisMemDumps.AutoSize = true;
            this.cb_iisMemDumps.Location = new System.Drawing.Point(24, 161);
            this.cb_iisMemDumps.Name = "cb_iisMemDumps";
            this.cb_iisMemDumps.Size = new System.Drawing.Size(129, 17);
            this.cb_iisMemDumps.TabIndex = 8;
            this.cb_iisMemDumps.Text = "IIS Worker Processes";
            this.cb_iisMemDumps.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label7.Location = new System.Drawing.Point(18, 146);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(410, 2);
            this.label7.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(18, 122);
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
            this.cb_iisAccessLogs.Location = new System.Drawing.Point(208, 252);
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
            this.cb_EvtViewerLogs.Location = new System.Drawing.Point(24, 252);
            this.cb_EvtViewerLogs.Name = "cb_EvtViewerLogs";
            this.cb_EvtViewerLogs.Size = new System.Drawing.Size(170, 17);
            this.cb_EvtViewerLogs.TabIndex = 12;
            this.cb_EvtViewerLogs.Text = "Event Viewer and Server Logs";
            this.cb_EvtViewerLogs.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label9.Location = new System.Drawing.Point(18, 238);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(410, 2);
            this.label9.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(18, 214);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 15);
            this.label10.TabIndex = 10;
            this.label10.Text = "Server Logs";
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(18, 39);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(410, 2);
            this.label13.TabIndex = 18;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(465, 24);
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
            this.mstrp_Help.Size = new System.Drawing.Size(99, 22);
            this.mstrp_Help.Text = "Help";
            this.mstrp_Help.Click += new System.EventHandler(this.mstrp_Help_Click);
            // 
            // mstrp_Exit
            // 
            this.mstrp_Exit.Name = "mstrp_Exit";
            this.mstrp_Exit.Size = new System.Drawing.Size(99, 22);
            this.mstrp_Exit.Text = "Exit";
            this.mstrp_Exit.Click += new System.EventHandler(this.mstrp_Exit_Click);
            // 
            // tb_osDiagConf
            // 
            this.tb_osDiagConf.Controls.Add(this.tb_osDiagGenConf);
            this.tb_osDiagConf.Controls.Add(this.tb_databaseConf);
            this.tb_osDiagConf.Location = new System.Drawing.Point(12, 36);
            this.tb_osDiagConf.Name = "tb_osDiagConf";
            this.tb_osDiagConf.SelectedIndex = 0;
            this.tb_osDiagConf.Size = new System.Drawing.Size(449, 483);
            this.tb_osDiagConf.TabIndex = 21;
            // 
            // tb_osDiagGenConf
            // 
            this.tb_osDiagGenConf.Controls.Add(this.label22);
            this.tb_osDiagGenConf.Controls.Add(this.cb_platformAndServerFiles);
            this.tb_osDiagGenConf.Controls.Add(this.nud_topLogs);
            this.tb_osDiagGenConf.Controls.Add(this.label21);
            this.tb_osDiagGenConf.Controls.Add(this.label19);
            this.tb_osDiagGenConf.Controls.Add(this.label20);
            this.tb_osDiagGenConf.Controls.Add(this.cb_platformLogs);
            this.tb_osDiagGenConf.Controls.Add(this.nud_iisLogsNrDays);
            this.tb_osDiagGenConf.Controls.Add(this.label18);
            this.tb_osDiagGenConf.Controls.Add(this.label1);
            this.tb_osDiagGenConf.Controls.Add(this.label13);
            this.tb_osDiagGenConf.Controls.Add(this.label4);
            this.tb_osDiagGenConf.Controls.Add(this.label5);
            this.tb_osDiagGenConf.Controls.Add(this.cb_iisThreads);
            this.tb_osDiagGenConf.Controls.Add(this.cb_osServicesThreads);
            this.tb_osDiagGenConf.Controls.Add(this.label8);
            this.tb_osDiagGenConf.Controls.Add(this.cb_iisAccessLogs);
            this.tb_osDiagGenConf.Controls.Add(this.label7);
            this.tb_osDiagGenConf.Controls.Add(this.cb_EvtViewerLogs);
            this.tb_osDiagGenConf.Controls.Add(this.cb_iisMemDumps);
            this.tb_osDiagGenConf.Controls.Add(this.label9);
            this.tb_osDiagGenConf.Controls.Add(this.cb_osMemDumps);
            this.tb_osDiagGenConf.Controls.Add(this.label10);
            this.tb_osDiagGenConf.Location = new System.Drawing.Point(4, 22);
            this.tb_osDiagGenConf.Name = "tb_osDiagGenConf";
            this.tb_osDiagGenConf.Padding = new System.Windows.Forms.Padding(3);
            this.tb_osDiagGenConf.Size = new System.Drawing.Size(441, 457);
            this.tb_osDiagGenConf.TabIndex = 0;
            this.tb_osDiagGenConf.Text = "General Configurations";
            this.tb_osDiagGenConf.UseVisualStyleBackColor = true;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(18, 190);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(306, 13);
            this.label22.TabIndex = 28;
            this.label22.Text = "* Retrieval of memory dumps can cause temporary unavailability";
            // 
            // cb_platformAndServerFiles
            // 
            this.cb_platformAndServerFiles.AutoSize = true;
            this.cb_platformAndServerFiles.Checked = true;
            this.cb_platformAndServerFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_platformAndServerFiles.Location = new System.Drawing.Point(24, 359);
            this.cb_platformAndServerFiles.Name = "cb_platformAndServerFiles";
            this.cb_platformAndServerFiles.Size = new System.Drawing.Size(205, 17);
            this.cb_platformAndServerFiles.TabIndex = 27;
            this.cb_platformAndServerFiles.Text = "Platform and Server Configuration files";
            this.cb_platformAndServerFiles.UseVisualStyleBackColor = true;
            // 
            // nud_topLogs
            // 
            this.nud_topLogs.Location = new System.Drawing.Point(118, 324);
            this.nud_topLogs.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nud_topLogs.Name = "nud_topLogs";
            this.nud_topLogs.Size = new System.Drawing.Size(56, 20);
            this.nud_topLogs.TabIndex = 26;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(180, 326);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(154, 13);
            this.label21.TabIndex = 25;
            this.label21.Text = "Number of Records in the Logs";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(21, 286);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(53, 15);
            this.label19.TabIndex = 22;
            this.label19.Text = "Platform";
            // 
            // label20
            // 
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label20.Location = new System.Drawing.Point(21, 310);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(410, 2);
            this.label20.TabIndex = 23;
            // 
            // cb_platformLogs
            // 
            this.cb_platformLogs.AutoSize = true;
            this.cb_platformLogs.Location = new System.Drawing.Point(24, 325);
            this.cb_platformLogs.Name = "cb_platformLogs";
            this.cb_platformLogs.Size = new System.Drawing.Size(90, 17);
            this.cb_platformLogs.TabIndex = 24;
            this.cb_platformLogs.Text = "Platform Logs";
            this.cb_platformLogs.UseVisualStyleBackColor = true;
            // 
            // nud_iisLogsNrDays
            // 
            this.nud_iisLogsNrDays.Location = new System.Drawing.Point(317, 251);
            this.nud_iisLogsNrDays.Name = "nud_iisLogsNrDays";
            this.nud_iisLogsNrDays.Size = new System.Drawing.Size(38, 20);
            this.nud_iisLogsNrDays.TabIndex = 21;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(356, 253);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(81, 13);
            this.label18.TabIndex = 20;
            this.label18.Text = "Number of days";
            // 
            // tb_databaseConf
            // 
            this.tb_databaseConf.Controls.Add(this.bt_removeMetamodelTables);
            this.tb_databaseConf.Controls.Add(this.bt_addMetamodelTables);
            this.tb_databaseConf.Controls.Add(this.label17);
            this.tb_databaseConf.Controls.Add(this.tb_inptMetamodelTables);
            this.tb_databaseConf.Controls.Add(this.label15);
            this.tb_databaseConf.Controls.Add(this.label16);
            this.tb_databaseConf.Controls.Add(this.label14);
            this.tb_databaseConf.Controls.Add(this.lb_metamodelTables);
            this.tb_databaseConf.Controls.Add(this.DBCredentials);
            this.tb_databaseConf.Controls.Add(this.cb_dbTroubleshoot);
            this.tb_databaseConf.Controls.Add(this.cb_dbPlatformMetamodel);
            this.tb_databaseConf.Controls.Add(this.label11);
            this.tb_databaseConf.Controls.Add(this.label12);
            this.tb_databaseConf.Location = new System.Drawing.Point(4, 22);
            this.tb_databaseConf.Name = "tb_databaseConf";
            this.tb_databaseConf.Padding = new System.Windows.Forms.Padding(3);
            this.tb_databaseConf.Size = new System.Drawing.Size(441, 457);
            this.tb_databaseConf.TabIndex = 1;
            this.tb_databaseConf.Text = "Database Configurations";
            this.tb_databaseConf.UseVisualStyleBackColor = true;
            // 
            // bt_removeMetamodelTables
            // 
            this.bt_removeMetamodelTables.Location = new System.Drawing.Point(296, 410);
            this.bt_removeMetamodelTables.Name = "bt_removeMetamodelTables";
            this.bt_removeMetamodelTables.Size = new System.Drawing.Size(75, 23);
            this.bt_removeMetamodelTables.TabIndex = 33;
            this.bt_removeMetamodelTables.Text = "Remove";
            this.bt_removeMetamodelTables.UseVisualStyleBackColor = true;
            this.bt_removeMetamodelTables.Click += new System.EventHandler(this.bt_removeMetamodelTables_Click);
            // 
            // bt_addMetamodelTables
            // 
            this.bt_addMetamodelTables.Location = new System.Drawing.Point(296, 342);
            this.bt_addMetamodelTables.Name = "bt_addMetamodelTables";
            this.bt_addMetamodelTables.Size = new System.Drawing.Size(75, 23);
            this.bt_addMetamodelTables.TabIndex = 32;
            this.bt_addMetamodelTables.Text = "Add";
            this.bt_addMetamodelTables.UseVisualStyleBackColor = true;
            this.bt_addMetamodelTables.Click += new System.EventHandler(this.bt_addMetamodelTables_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(250, 286);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(115, 13);
            this.label17.TabIndex = 31;
            this.label17.Text = "Add Metamodel tables:";
            // 
            // tb_inptMetamodelTables
            // 
            this.tb_inptMetamodelTables.Location = new System.Drawing.Point(253, 310);
            this.tb_inptMetamodelTables.Name = "tb_inptMetamodelTables";
            this.tb_inptMetamodelTables.Size = new System.Drawing.Size(172, 20);
            this.tb_inptMetamodelTables.TabIndex = 30;
            // 
            // label15
            // 
            this.label15.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label15.Location = new System.Drawing.Point(15, 266);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(410, 2);
            this.label15.TabIndex = 29;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(15, 242);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(119, 15);
            this.label16.TabIndex = 28;
            this.label16.Text = "Platform Metamodel";
            // 
            // label14
            // 
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label14.Location = new System.Drawing.Point(15, 88);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(410, 2);
            this.label14.TabIndex = 27;
            // 
            // lb_metamodelTables
            // 
            this.lb_metamodelTables.FormattingEnabled = true;
            this.lb_metamodelTables.Location = new System.Drawing.Point(18, 286);
            this.lb_metamodelTables.Name = "lb_metamodelTables";
            this.lb_metamodelTables.Size = new System.Drawing.Size(216, 147);
            this.lb_metamodelTables.TabIndex = 25;
            // 
            // DBCredentials
            // 
            this.DBCredentials.Controls.Add(this.bt_TestSaConnection);
            this.DBCredentials.Controls.Add(this.label6);
            this.DBCredentials.Controls.Add(this.tb_iptSaPwd);
            this.DBCredentials.Controls.Add(this.tb_iptSaUsername);
            this.DBCredentials.Controls.Add(this.label3);
            this.DBCredentials.Controls.Add(this.label2);
            this.DBCredentials.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBCredentials.Location = new System.Drawing.Point(15, 98);
            this.DBCredentials.Name = "DBCredentials";
            this.DBCredentials.Size = new System.Drawing.Size(410, 132);
            this.DBCredentials.TabIndex = 20;
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
            // tb_iptSaPwd
            // 
            this.tb_iptSaPwd.Location = new System.Drawing.Point(77, 55);
            this.tb_iptSaPwd.Name = "tb_iptSaPwd";
            this.tb_iptSaPwd.PasswordChar = '*';
            this.tb_iptSaPwd.Size = new System.Drawing.Size(200, 21);
            this.tb_iptSaPwd.TabIndex = 3;
            this.tb_iptSaPwd.UseSystemPasswordChar = true;
            // 
            // tb_iptSaUsername
            // 
            this.tb_iptSaUsername.Location = new System.Drawing.Point(77, 23);
            this.tb_iptSaUsername.Name = "tb_iptSaUsername";
            this.tb_iptSaUsername.Size = new System.Drawing.Size(200, 21);
            this.tb_iptSaUsername.TabIndex = 2;
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
            // cb_dbTroubleshoot
            // 
            this.cb_dbTroubleshoot.AutoSize = true;
            this.cb_dbTroubleshoot.Checked = true;
            this.cb_dbTroubleshoot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_dbTroubleshoot.Location = new System.Drawing.Point(218, 52);
            this.cb_dbTroubleshoot.Name = "cb_dbTroubleshoot";
            this.cb_dbTroubleshoot.Size = new System.Drawing.Size(137, 17);
            this.cb_dbTroubleshoot.TabIndex = 24;
            this.cb_dbTroubleshoot.Text = "Database Troubleshoot";
            this.cb_dbTroubleshoot.UseVisualStyleBackColor = true;
            // 
            // cb_dbPlatformMetamodel
            // 
            this.cb_dbPlatformMetamodel.AutoSize = true;
            this.cb_dbPlatformMetamodel.Checked = true;
            this.cb_dbPlatformMetamodel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_dbPlatformMetamodel.Location = new System.Drawing.Point(24, 52);
            this.cb_dbPlatformMetamodel.Name = "cb_dbPlatformMetamodel";
            this.cb_dbPlatformMetamodel.Size = new System.Drawing.Size(178, 17);
            this.cb_dbPlatformMetamodel.TabIndex = 23;
            this.cb_dbPlatformMetamodel.Text = "OutSystems Platform Metamodel";
            this.cb_dbPlatformMetamodel.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label11.Location = new System.Drawing.Point(15, 38);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(410, 2);
            this.label11.TabIndex = 22;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(15, 14);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(125, 15);
            this.label12.TabIndex = 21;
            this.label12.Text = "Database Information";
            // 
            // bt_runOsDiagTool
            // 
            this.bt_runOsDiagTool.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_runOsDiagTool.Location = new System.Drawing.Point(175, 538);
            this.bt_runOsDiagTool.Name = "bt_runOsDiagTool";
            this.bt_runOsDiagTool.Size = new System.Drawing.Size(107, 23);
            this.bt_runOsDiagTool.TabIndex = 25;
            this.bt_runOsDiagTool.Text = "Run";
            this.bt_runOsDiagTool.UseVisualStyleBackColor = true;
            // 
            // lbl_feedbackOsDiagForm
            // 
            this.lbl_feedbackOsDiagForm.AutoSize = true;
            this.lbl_feedbackOsDiagForm.Location = new System.Drawing.Point(330, 543);
            this.lbl_feedbackOsDiagForm.Name = "lbl_feedbackOsDiagForm";
            this.lbl_feedbackOsDiagForm.Size = new System.Drawing.Size(0, 13);
            this.lbl_feedbackOsDiagForm.TabIndex = 26;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // OsDiagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(465, 573);
            this.Controls.Add(this.lbl_feedbackOsDiagForm);
            this.Controls.Add(this.bt_runOsDiagTool);
            this.Controls.Add(this.tb_osDiagConf);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "OsDiagForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "OutSystems Diagnostics Tool";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tb_osDiagConf.ResumeLayout(false);
            this.tb_osDiagGenConf.ResumeLayout(false);
            this.tb_osDiagGenConf.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_topLogs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_iisLogsNrDays)).EndInit();
            this.tb_databaseConf.ResumeLayout(false);
            this.tb_databaseConf.PerformLayout();
            this.DBCredentials.ResumeLayout(false);
            this.DBCredentials.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cb_iisThreads;
        private System.Windows.Forms.CheckBox cb_osServicesThreads;
        private System.Windows.Forms.CheckBox cb_osMemDumps;
        private System.Windows.Forms.CheckBox cb_iisMemDumps;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox cb_iisAccessLogs;
        private System.Windows.Forms.CheckBox cb_EvtViewerLogs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mstrp_Help;
        private System.Windows.Forms.ToolStripMenuItem mstrp_Exit;
        private System.Windows.Forms.TabControl tb_osDiagConf;
        private System.Windows.Forms.TabPage tb_osDiagGenConf;
        private System.Windows.Forms.TabPage tb_databaseConf;
        private System.Windows.Forms.GroupBox DBCredentials;
        private System.Windows.Forms.Button bt_TestSaConnection;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_iptSaPwd;
        private System.Windows.Forms.TextBox tb_iptSaUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_dbTroubleshoot;
        private System.Windows.Forms.CheckBox cb_dbPlatformMetamodel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button bt_runOsDiagTool;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ListBox lb_metamodelTables;
        private System.Windows.Forms.Button bt_addMetamodelTables;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tb_inptMetamodelTables;
        private System.Windows.Forms.Button bt_removeMetamodelTables;
        private System.Windows.Forms.NumericUpDown nud_iisLogsNrDays;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lbl_feedbackOsDiagForm;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox cb_platformLogs;
        private System.Windows.Forms.NumericUpDown nud_topLogs;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.CheckBox cb_platformAndServerFiles;
        private System.Windows.Forms.Label label22;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}