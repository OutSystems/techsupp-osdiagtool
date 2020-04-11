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

namespace OSDiagTool.OSDiagToolForm {
    public partial class OsDiagForm : Form {

        private string _helpLink = "https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/OSDiagTool_-_OutSystems_Support_Diagnostics_Tool";

        public OsDiagForm(OSDiagToolConf.ConfModel.strConfModel configurations, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null) {

            InitializeComponent();

            this.lb_metamodelTables.Items.AddRange(configurations.tableNames.ToArray()); // add Platform Metamodel tables to list box

            bt_TestSaConnection.Click += delegate (object sender, EventArgs e) { bt_TestSaConnection_Click(sender, e, dbms, SQLConnectionString, OracleConnectionString); };
        }

        private void bt_TestSaConnection_Click(object sender, EventArgs e, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null) {

            // TODO : test login
            

            if (dbms.ToLower().Equals("sqlserver")) {
                SQLConnectionString.userId = this.tb_iptSaPwd.Text;
                SQLConnectionString.pwd = this.tb_iptSaPwd.Text;

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

                using (connection) {

                    // test login and return state
                }


            } else if (dbms.ToLower().Equals("oracle")) {

            }

            
        }

        private void bt_runOsDiagTool_Click(object sender, EventArgs e) {

            OSDiagTool.Program.RunOsDiagTool();

        }

        private void mstrp_Exit_Click(object sender, EventArgs e) {

            this.Close();
        }

        private void mstrp_Help_Click(object sender, EventArgs e) {

            System.Diagnostics.Process.Start(_helpLink);
        }
    }
}
