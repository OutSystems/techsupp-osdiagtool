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
    public partial class PopUpForm : Form {
        public PopUpForm(string testConnectionResult) {
            InitializeComponent();

            if (testConnectionResult.ToLower().Contains("success")) {
                System.Drawing.Icon icon = System.Drawing.SystemIcons.Information;
                this.Icon = icon;
            } else {
                System.Drawing.Icon icon = System.Drawing.SystemIcons.Error;
                this.Icon = icon;
            }

            this.lbl_connectionTest.Text = this.lbl_connectionTest.Text + testConnectionResult;
        }

        private void bt_CloseFormPopUp_Click(object sender, EventArgs e) {
            this.Dispose();
            this.Close();
        }
    }
}
