using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OSDiagTool.OSDiagToolForm {
    public partial class OsDiagForm : Form {

        private string _helpLink = "https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/OSDiagTool_-_OutSystems_Support_Diagnostics_Tool";
        public OsDiagForm() {
            InitializeComponent();
        }

        private void bt_TestSaConnection_Click(object sender, EventArgs e) {

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
