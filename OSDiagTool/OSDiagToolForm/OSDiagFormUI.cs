using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSDiagTool.OSDiagToolConf;
using System.Threading;
using OSDiagTool.Platform.ConfigFiles;
using System.IO;

namespace OSDiagTool.OSDiagToolForm {
    public partial class OsDiagForm : Form {

        private static string _helpLink = "https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/OSDiagTool_-_OutSystems_Support_Diagnostics_Tool";
        private static string _failedConnectionTest = "Test Connection: Failed";
        private static string _successConnectionTest = "Test Connection: Successful";
        public static string _waitMessage = "OSDiagTool running. Please wait...";
        private static string _doneMessage = "OSDiagTool has finished!";
        private static string _operationCancelled = "Operation was cancelled.";
        public static string _tdIis = "Threads IIS";
        public static string _tdOsServices = "Threads OS Services";
        public static string _mdIis = "Memory IIS";
        public static string _osRequirements = "Platform Requirements";
        public static string _mdOsServices = "Memory OS Services";
        public static string _slEvt = "Event Viewer Logs";
        public static string _slIisLogs = "IIS Access Logs";
        public static string _diMetamodel = "Platform Metamodel";
        public static string _diDbTroubleshoot = "Database Troubleshoot";
        public static string _plPlatformLogs = "Platform Logs";
        public static string _plPlatformAndServerFiles = "Platform and Server Configuration files";
        // new check box items must be added to dictHelper dictionary

        public SortedDictionary<int, string> FeedbackSteps = new SortedDictionary<int, string> {
            { 1, "Collecting IIS thread dumps" },
            { 2, "Collecting OutSystems Services thread dumps" },
            { 3, "Collecting IIS memory dumps" },
            { 4, "Collecting OutSystems Services memory dumps" },
            { 5, "Exporting Event Viewer and Server logs" },
            { 6, "Exporting IIS Access logs" },
            { 7, "Exporting Platform logs" },
            { 8, "Exporting Platform and Server Configuration files" },
            { 9, "Exporting Platform metamodel" },
            { 10, "Performing Database Troubleshoot" },
            { 11, "Checking the OutSystems Platform Requirements" },
            { 12, "Zipping file..." },
            { 13, "" }, // Last step for closing the pop up
        };

        public OsDiagForm(OSDiagToolConf.ConfModel.strConfModel configurations, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null) {

            InitializeComponent();

            this.cb_iisThreads.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_iisW3wp]; // Iis thread dumps
            this.cb_osServicesThreads.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_osServices]; // OS services thread dumps

            this.cb_iisMemDumps.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_iisW3wp];
            this.cb_osMemDumps.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_osServices];

            this.cb_EvtViewerLogs.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_evtAndServer];
            this.cb_iisAccessLogs.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_iisLogs];
            this.nud_iisLogsNrDays.Value = configurations.IISLogsNrDays; // Number of days of IIS logs

            this.cb_platformLogs.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_platform][OSDiagToolConfReader._l3_platformLogs];
            this.nud_topLogs.Value = configurations.osLogTopRecords;
            this.cb_platformAndServerFiles.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_platform][OSDiagToolConfReader._l3_platformAndServerConfigFiles];

            this.cb_dbPlatformMetamodel.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_databaseOperations][OSDiagToolConfReader._l3_platformMetamodel];
            this.cb_dbTroubleshoot.Checked = configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_databaseOperations][OSDiagToolConfReader._l3_databaseTroubleshoot];

            this.lb_metamodelTables.Items.AddRange(configurations.tableNames.ToArray()); // add Platform Metamodel tables to list box

            bt_TestSaConnection.Click += delegate (object sender, EventArgs e) { bt_TestSaConnection_Click(sender, e, dbms, SQLConnectionString, OracleConnectionString); };
            bt_runOsDiagTool.Click += delegate (object sender, EventArgs e) { bt_runOsDiagTool_Click(sender, e, configurations); };

            //bt_iisMonitRun.Click += delegate (object sender, EventArgs e) { bt_iisMonitRun_Click(sender, e, ); };
        }

        private void bt_TestSaConnection_Click(object sender, EventArgs e, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null) {

            string testConnectionResult = null;

            if (dbms.ToLower().Equals("sqlserver")) {
                SQLConnectionString.userId = this.tb_iptSaUsername.Text;
                SQLConnectionString.pwd = this.tb_iptSaPwd.Text;

                try {
                    var connector = new DBConnector.SLQDBConnector();
                    SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);
                    connector.SQLCloseConnection(connection);
                    testConnectionResult = _successConnectionTest;
                } catch {
                    testConnectionResult = _failedConnectionTest;
                }


            } else if (dbms.ToLower().Equals("oracle")) {
                OracleConnectionString.userId = this.tb_iptSaUsername.Text;
                OracleConnectionString.pwd = this.tb_iptSaPwd.Text;

                try {
                    var connector = new DBConnector.OracleDBConnector();
                    OracleConnection connection = connector.OracleOpenConnection(OracleConnectionString);
                    connector.OracleCloseConnection(connection);
                    testConnectionResult = _successConnectionTest;
                } catch {
                    testConnectionResult = _failedConnectionTest;
                }

            }

            puf_popUpForm popup = new puf_popUpForm(puf_popUpForm._feedbackTestConnectionType ,testConnectionResult);
            DialogResult dg = popup.ShowDialog();
        }

        private void bt_runOsDiagTool_Click(object sender, EventArgs e, OSDiagToolConf.ConfModel.strConfModel configurations) {
            
            var formConfigurations = new OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel();
            List<string> tableNameHelper = new List<string>();

            if(tb_iptSaUsername.Text.Equals("") || tb_iptSaPwd.Text.Equals("")) { // if no input is provided in user or pwd, then DBTroubleshoot doesn't run
                cb_dbTroubleshoot.Checked = false;
            }

            formConfigurations.saUser = tb_iptSaUsername.Text.ToString();
            formConfigurations.saPwd = tb_iptSaPwd.Text.ToString();
            formConfigurations.iisLogsNrDays = Convert.ToInt32(nud_iisLogsNrDays.Value);
            formConfigurations.osLogTopRecords = Convert.ToInt32(nud_topLogs.Value);
            foreach (object item in lb_metamodelTables.Items) {
                tableNameHelper.Add(item.ToString());
            }

            formConfigurations.metamodelTables = tableNameHelper;

            Dictionary<string, bool> dictHelper = new Dictionary<string, bool>() {
                { _tdIis, cb_iisThreads.Checked},
                { _tdOsServices, cb_osServicesThreads.Checked},
                { _mdIis, cb_iisMemDumps.Checked},
                { _osRequirements, cb_osRequirements.Checked},
                { _mdOsServices, cb_osMemDumps.Checked},
                { _slEvt, cb_EvtViewerLogs.Checked},
                { _slIisLogs, cb_iisAccessLogs.Checked},
                { _diMetamodel, cb_dbPlatformMetamodel.Checked},
                { _diDbTroubleshoot, cb_dbTroubleshoot.Checked},
                { _plPlatformLogs, cb_platformLogs.Checked},
                { _plPlatformAndServerFiles, cb_platformAndServerFiles.Checked },
            };

            formConfigurations.cbConfs = dictHelper;
            
            var configurationsHelper = new DataHelperClass.strConfigurations();

            configurationsHelper.FormConfigurations = new OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel();
            configurationsHelper.FormConfigurations = formConfigurations;

            int numberOfSteps = OSDiagToolHelper.CountSteps(configurationsHelper.FormConfigurations.cbConfs);
            puf_popUpForm popup = new puf_popUpForm(puf_popUpForm._feedbackWaitType, OsDiagForm._waitMessage, totalSteps: numberOfSteps + 2); // totalSteps + 2 for the zipping and pop up close
            popup.Show();
            popup.Refresh();
            configurationsHelper.popup = popup;

            configurationsHelper.ConfigFileConfigurations = new OSDiagToolConf.ConfModel.strConfModel();
            configurationsHelper.ConfigFileConfigurations = configurations;


            backgroundWorker1.RunWorkerAsync(configurationsHelper);
            Cursor = Cursors.Arrow;

        }

        private void mstrp_Exit_Click(object sender, EventArgs e) {

            this.Dispose();
            this.Close();
        }

        private void mstrp_Help_Click(object sender, EventArgs e) {

            System.Diagnostics.Process.Start(_helpLink);
        }

        private void bt_addMetamodelTables_Click(object sender, EventArgs e) {

            MetamodelTables mtTables = new MetamodelTables();

            List<string> listBoxTableList = lb_metamodelTables.Items.Cast<string>().ToList();

            if (mtTables.ValidateMetamodelTableName(tb_inptMetamodelTables.Text.ToString().ToLower(), listBoxTableList.ConvertAll(d => d.ToLower()))) {

                string escapedTableName = mtTables.TableNameEscapeCharacters(tb_inptMetamodelTables.Text.ToString());

                this.lb_metamodelTables.Items.Add(escapedTableName);
                this.tb_inptMetamodelTables.Text = "";
            } else {
                puf_popUpForm popup = new puf_popUpForm(puf_popUpForm._feedbackErrorType, "Failed to add table: " + Environment.NewLine + "Input contains spaces, already exists or " + Environment.NewLine + "does not have prefix OSSYS or OSLTM.");
                DialogResult dg = popup.ShowDialog();
            }

        }

        private void bt_removeMetamodelTables_Click(object sender, EventArgs e) {

            string selectedItem = lb_metamodelTables.SelectedItem.ToString();
            this.lb_metamodelTables.Items.Remove(selectedItem);

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {

            /* REFACTOR */

            OSDiagToolForm.DataHelperClass.strConfigurations configurationsHelper = (OSDiagToolForm.DataHelperClass.strConfigurations)e.Argument;

            // Initialization
            Program.OSDiagToolInitialization();


            // Get Platform and Server files
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._plPlatformAndServerFiles, out bool getPSandServerConfFiles) && getPSandServerConfFiles == true) {

                    backgroundWorker1.ReportProgress(8, configurationsHelper.popup);
                    Program.GetPlatformAndServerFiles();
                }
            }
                

            // Export Event Viewer and Server logs
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slEvt, out bool getEvt) && getEvt == true) {

                    backgroundWorker1.ReportProgress(5, configurationsHelper.popup);
                    Program.ExportEventViewerAndServerLogs();
                }
            }

            // Copy IIS Access logs
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slIisLogs, out bool getIisLogs) && getIisLogs == true) {

                    backgroundWorker1.ReportProgress(6, configurationsHelper.popup);
                    Program.CopyIISAccessLogs(configurationsHelper.FormConfigurations.iisLogsNrDays);
                }
            }
                
            // Database Troubleshoot
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diDbTroubleshoot, out bool _doDbTroubleshoot) && _doDbTroubleshoot == true) {

                    backgroundWorker1.ReportProgress(10, configurationsHelper.popup);
                    Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                    Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, true, ConnectionStringDefiner, configurationsHelper.FormConfigurations.saUser, configurationsHelper.FormConfigurations.saPwd);

                    if (Program.dbEngine.Equals("sqlserver")) {
                        Program.DatabaseTroubleshootProgram(configurationsHelper.ConfigFileConfigurations, ConnStringHelper.SQLConnString);

                    } else if (Program.dbEngine.Equals("oracle")) {
                        Program.DatabaseTroubleshootProgram(configurationsHelper.ConfigFileConfigurations, null, ConnStringHelper.OracleConnString);

                    }
                }
            }
                
            // Export Platform Metamodel
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diMetamodel, out bool _getPlatformMetamodel) && _getPlatformMetamodel == true) {

                    backgroundWorker1.ReportProgress(9, configurationsHelper.popup);
                    Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                    Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, false, ConnectionStringDefiner);

                    if (Program.dbEngine.Equals("sqlserver")) {
                        Program.ExportPlatformMetamodel(Program.dbEngine, configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, ConnStringHelper.SQLConnString, null);

                    } else if (Program.dbEngine.Equals("oracle")) {
                        Program.ExportPlatformMetamodel(Program.dbEngine, configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, null, ConnStringHelper.OracleConnString);

                    }
                }
            }
                
            // Export Platform Logs
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._plPlatformLogs, out bool _getPlatformLogs) && _getPlatformLogs == true) {

                    backgroundWorker1.ReportProgress(7, configurationsHelper.popup);
                    Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                    Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, Program.separateLogCatalog, false, ConnectionStringDefiner);

                    if (Program.dbEngine.Equals("sqlserver")) {
                        Program.ExportServiceCenterLogs(Program.dbEngine, configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, ConnStringHelper.SQLConnString, null);

                    } else if (Program.dbEngine.Equals("oracle")) {
                        Program.ExportServiceCenterLogs(Program.dbEngine, configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, null, ConnStringHelper.OracleConnString, ConnStringHelper.AdminSchema);

                    }
                }
            }
                
            // IIS Thread dumps
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdIis, out bool getIisThreadDumps) && getIisThreadDumps == true) {

                    backgroundWorker1.ReportProgress(1, configurationsHelper.popup);
                    Program.CollectThreadDumpsProgram(getIisThreadDumps, false);

                }
            }
                
            // OutSystems Services Thread dumps
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdOsServices, out bool _getOsThreadDumps) && _getOsThreadDumps == true) {

                    backgroundWorker1.ReportProgress(2, configurationsHelper.popup);
                    Program.CollectThreadDumpsProgram(false, _getOsThreadDumps);

                }
            }
                
            // IIS Memory Dumps 
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdIis, out bool getIisMemDumps) && getIisMemDumps == true) {

                    backgroundWorker1.ReportProgress(3, configurationsHelper.popup);
                    Program.CollectMemoryDumpsProgram(getIisMemDumps, false);

                }
            }
                
            // OutSystems Services Memory Dumps
            if (!backgroundWorker1.CancellationPending) {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdOsServices, out bool getOsMemDumps) && getOsMemDumps == true) {

                    backgroundWorker1.ReportProgress(4, configurationsHelper.popup);
                    Program.CollectMemoryDumpsProgram(false, getOsMemDumps);

                }
            }

            // Platform Requirements
            if (!backgroundWorker1.CancellationPending)
            {
                if (configurationsHelper.FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._osRequirements, out bool getOsRequirements) && getOsRequirements == true)
                {
                    backgroundWorker1.ReportProgress(11, configurationsHelper.popup);

                    // Connecting to the database, by using the credentials located in the server.hsconf
                    Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                    Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, false, ConnectionStringDefiner);

                    if (Program.dbEngine.Equals("sqlserver"))
                    {
                        Program.PlatformRequirementsProgram(configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, ConnStringHelper.SQLConnString, null);

                    }
                    else if (Program.dbEngine.Equals("oracle"))
                    {
                        Program.PlatformRequirementsProgram(configurationsHelper.ConfigFileConfigurations, configurationsHelper.FormConfigurations, null, ConnStringHelper.OracleConnString);

                    }
                }
            }


            backgroundWorker1.ReportProgress(12, configurationsHelper.popup);
            Program.GenerateZipFile();
            backgroundWorker1.ReportProgress(13, configurationsHelper.popup); // Last step to close pop up

            /* REFACTOR */

            

            if (!backgroundWorker1.CancellationPending == true) {
                puf_popUpForm popup2 = new puf_popUpForm(puf_popUpForm._feedbackDoneType, _doneMessage + Environment.NewLine + Program._endFeedback);
                popup2.ShowDialog();
            } else {
                puf_popUpForm popup2 = new puf_popUpForm(puf_popUpForm._feedbackDoneType, _operationCancelled + Environment.NewLine + Program._endFeedback);
                popup2.ShowDialog();

            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {

            puf_popUpForm popup = (puf_popUpForm)e.UserState;
            int stepId = e.ProgressPercentage;

            if (puf_popUpForm.isBackgroundWorkerCancelled == true) {
                backgroundWorker1.CancelAsync();
                puf_popUpForm.isBackgroundWorkerCancelled = false; // reset
            } else {

                FeedbackSteps.TryGetValue(stepId, out string feedbackText);

                popup.lb_ProgressFeedback.Items.Add(feedbackText);
                popup.lb_ProgressFeedback.SelectedIndex = popup.lb_ProgressFeedback.Items.Count - 1;
                popup.lb_ProgressFeedback.SelectedIndex = -1;

                if (!(popup.pb_progressBar.Value >= popup.pb_progressBar.Maximum)) {
                    popup.pb_progressBar.PerformStep();
                }

                popup.Refresh();

            }

            if (stepId == FeedbackSteps.Keys.Last<int>()) {
                popup.Dispose();
                popup.Close();
            }
        }

        private void IISMonitorizationTabClick(object sender, EventArgs e) {

            bt_runOsDiagTool.Enabled = false;

        }

        private void GeneralDatabaseConfigurationsTabClick(object sender, EventArgs e) {

            bt_runOsDiagTool.Enabled = true;

        }

        private void bt_iisMonitRun_Click(object sender, EventArgs e) {

            lb_iisMonitStatus.Text = "IIS Monitorization currently running...";

            this.bt_iisMonitRun.Enabled = false;
            this.bt_iisMonitStop.Enabled = true;


            bw_iisMonit.RunWorkerAsync();

        }

        private void bt_iisMonitStop_Click(object sender, EventArgs e) {

            lb_iisMonitStatus.Text = "Cancelling IIS Monitorization...";
            this.bw_iisMonit.CancelAsync();

            this.bt_iisMonitRun.Enabled = true;
            this.bt_iisMonitStop.Enabled = false;

            lb_iisMonitStatus.Text = "";

        }

        private void bw_iisMonit_DoWork(object sender, DoWorkEventArgs e) {

            bool alarm = false;

            int timeStep = Convert.ToInt32(this.nud_iisTimestep.Value);
            float iisQueueThreshold = (float) this.nud_iisQueueThreshold.Value;

            while (alarm.Equals(false)) {

                alarm = WinPerfCounters.IISQueueAlarm(iisQueueThreshold);

                Thread.Sleep(timeStep);

            } 


        }

    }
}

