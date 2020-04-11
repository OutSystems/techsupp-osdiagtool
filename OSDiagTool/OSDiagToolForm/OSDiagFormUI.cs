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

namespace OSDiagTool.OSDiagToolForm {
    public partial class OsDiagForm : Form {

        private static string _helpLink = "https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/OSDiagTool_-_OutSystems_Support_Diagnostics_Tool";
        private static string _failedConnectionTest = "Test Connection: Failed";
        private static string _successConnectionTest = "Test Connection: Successful";
        public static string _tdIis = "Threads IIS";
        public static string _tdOsServices = "Threads OS Services";
        public static string _mdIis = "Memory IIS";
        public static string _mdOsServices = "Memory OS Services";
        public static string _slEvt = "Event Viewer Logs";
        public static string _slIisLogs = "IIS Access Logs";
        public static string _diMetamodel = "Platform Metamodel";
        public static string _diDbTroubleshoot = "Database Troubleshoot";

        public OsDiagForm(OSDiagToolConf.ConfModel.strConfModel configurations, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null) {

            InitializeComponent();

            this.lb_metamodelTables.Items.AddRange(configurations.tableNames.ToArray()); // add Platform Metamodel tables to list box

            bt_TestSaConnection.Click += delegate (object sender, EventArgs e) { bt_TestSaConnection_Click(sender, e, dbms, SQLConnectionString, OracleConnectionString); };
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
            
            PopUpForm popup = new PopUpForm(testConnectionResult);
            DialogResult dg = popup.ShowDialog();
        }

        private void bt_runOsDiagTool_Click(object sender, EventArgs e) {

            var formConfigurations = new OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel();
            List<string> tableNameHelper = new List<string>();

            formConfigurations.saUser = tb_iptSaUsername.Text.ToString();
            formConfigurations.saPwd = tb_iptSaPwd.Text.ToString();
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
            };

            formConfigurations.cbConfs = dictHelper;

            OSDiagTool.Program.RunOsDiagTool(formConfigurations);

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
                PopUpForm popup = new PopUpForm("Failed to add table: " + Environment.NewLine + "Cannot contain spaces and must start" + Environment.NewLine + "with prefix OSSYS or OSLTM.");
                DialogResult dg = popup.ShowDialog();
            }

        }

        private void bt_removeMetamodelTables_Click(object sender, EventArgs e) {

            string selectedItem = lb_metamodelTables.SelectedItem.ToString();
            this.lb_metamodelTables.Items.Remove(selectedItem);

        }
    }
}
