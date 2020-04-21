using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSDiagTool.OSDiagToolConf;
using System.Threading;

namespace OSDiagTool.OSDiagToolForm {
    public partial class OsDiagForm : Form {

        private static string _helpLink = "https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/OSDiagTool_-_OutSystems_Support_Diagnostics_Tool";
        private static string _failedConnectionTest = "Test Connection: Failed";
        private static string _successConnectionTest = "Test Connection: Successful";
        private static string _waitMessage = "OSDiagTool running. Please wait...";
        private static string _doneMessage = "OSDiagTool has finished!";
        public static string _tdIis = "Threads IIS";
        public static string _tdOsServices = "Threads OS Services";
        public static string _mdIis = "Memory IIS";
        public static string _mdOsServices = "Memory OS Services";
        public static string _slEvt = "Event Viewer Logs";
        public static string _slIisLogs = "IIS Access Logs";
        public static string _diMetamodel = "Platform Metamodel";
        public static string _diDbTroubleshoot = "Database Troubleshoot";
        public static string _plPlatformLogs = "Platform Logs";
        public static string _plPlatformAndServerFiles = "Platform and Server Configuration files";
        // new check box items must be added to dictHelper dictionary

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

            /*BackgroundWorker backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;  //Tell the user how the process went
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;*/


            bt_TestSaConnection.Click += delegate (object sender, EventArgs e) { bt_TestSaConnection_Click(sender, e, dbms, SQLConnectionString, OracleConnectionString); };
            bt_runOsDiagTool.Click += delegate (object sender, EventArgs e) { bt_runOsDiagTool_Click(sender, e, configurations); };
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

            Cursor = Cursors.WaitCursor;
            
            puf_popUpForm popup = new puf_popUpForm(puf_popUpForm._feedbackWaitType, _waitMessage);
            popup.Show();
            popup.Refresh();

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
                { _mdOsServices, cb_osMemDumps.Checked},
                { _slEvt, cb_EvtViewerLogs.Checked},
                { _slIisLogs, cb_iisAccessLogs.Checked},
                { _diMetamodel, cb_dbPlatformMetamodel.Checked},
                { _diDbTroubleshoot, cb_dbTroubleshoot.Checked},
                { _plPlatformLogs, cb_platformLogs.Checked},
                { _plPlatformAndServerFiles, cb_platformAndServerFiles.Checked },
            };

            formConfigurations.cbConfs = dictHelper;

            /*backgroundWorker1.RunWorkerAsync();*/

            OSDiagTool.Program.RunOsDiagTool(formConfigurations, configurations);

            popup.Dispose();
            popup.Close();

            puf_popUpForm popup2 = new puf_popUpForm(puf_popUpForm._feedbackDoneType, _doneMessage + Environment.NewLine + "Location: " + Program._zipFileLocation);
            popup2.ShowDialog();

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

            if (mtTables.ValidateMetamodelTableName(tb_inptMetamodelTables.Text.ToString().ToLower())) {

                string escapedTableName = mtTables.TableNameEscapeCharacters(tb_inptMetamodelTables.Text.ToString());

                this.lb_metamodelTables.Items.Add(escapedTableName);
                this.tb_inptMetamodelTables.Text = "";
            } else {
                puf_popUpForm popup = new puf_popUpForm("errorAddTable", "Failed to add table: " + Environment.NewLine + "Cannot contain spaces and must start" + Environment.NewLine + "with prefix OSSYS or OSLTM.");
                DialogResult dg = popup.ShowDialog();
            }

        }

        private void bt_removeMetamodelTables_Click(object sender, EventArgs e) {

            string selectedItem = lb_metamodelTables.SelectedItem.ToString();
            this.lb_metamodelTables.Items.Remove(selectedItem);

        }

        /*private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            for (int i = 0; i < 100; i++) {
                Thread.Sleep(1000);
                backgroundWorker1.ReportProgress(i);

                //Check if there is a request to cancel the process
                if (backgroundWorker1.CancellationPending) {
                    e.Cancel = true;
                    backgroundWorker1.ReportProgress(0);
                    return;
                }
            }
        */
    }
}
