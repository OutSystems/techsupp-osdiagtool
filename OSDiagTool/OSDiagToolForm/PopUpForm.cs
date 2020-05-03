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
    public partial class puf_popUpForm : Form {

        public static string _feedbackTestConnectionType = "testConnection";
        public static string _feedbackWaitType = "wait";
        public static string _feedbackDoneType = "done";
        public static string _feedbackErrorType = "error";

        public puf_popUpForm(string feedbackType, string message, int totalSteps = 0) {
            InitializeComponent();

            if (feedbackType.Equals(_feedbackWaitType)) {
                bt_CloseFormPopUp.Visible = false;
                this.Width = this.Width * 6/5;
                pb_progressBar.Style = ProgressBarStyle.Continuous;
                pb_progressBar.Width = pb_progressBar.Width * 6 / 5;
                pb_progressBar.Maximum = 100;
                pb_progressBar.Value = 0;
                pb_progressBar.Step = pb_progressBar.Maximum/totalSteps;
                pb_progressBar.PerformStep();
                pb_progressBar.Visible = true;

            } else if (feedbackType.Equals(_feedbackDoneType)) {
                this.Width = this.Width * 3;
                bt_CloseFormPopUp.Location = new Point(bt_CloseFormPopUp.Location.X * 4, bt_CloseFormPopUp.Location.Y);
            } else if (feedbackType.Equals(_feedbackErrorType)) {
                bt_CloseFormPopUp.Visible = true;
                bt_CloseFormPopUp.Text = "Exit";
            }

            this.lbl_message.Text = message;

        }

        public void bt_CloseFormPopUp_Click(object sender, EventArgs e) {
            this.Dispose();
            this.Close();
        }

        public static void ChangeFeedbackLabelAndProgressBar (puf_popUpForm popUpForm, string labelText) {

            popUpForm.lbl_message.Text = labelText;
            if(!(popUpForm.pb_progressBar.Value >= popUpForm.pb_progressBar.Maximum)) {
                popUpForm.pb_progressBar.PerformStep();
            }
            
            popUpForm.Refresh();

        }

         
    }
}
