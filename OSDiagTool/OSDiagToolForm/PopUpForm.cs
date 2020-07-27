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
        public static bool isBackgroundWorkerCancelled = false;

        public puf_popUpForm(string feedbackType, string message, int totalSteps = 0) {
            InitializeComponent();

            if (feedbackType.Equals(_feedbackWaitType)) {

                this.Text = "Collecting information. Please wait...";
                lb_ProgressFeedback.Visible = true;
                lb_ProgressFeedback.Width = lb_ProgressFeedback.Width * 5 / 4;
                lb_ProgressFeedback.Height = lb_ProgressFeedback.Height * 4;
                this.Height = this.Height * 3 / 2;
                this.Width = this.Width * 5 / 4;

                bt_CloseFormPopUp.Visible = false;
                pb_progressBar.Style = ProgressBarStyle.Continuous;
                pb_progressBar.Width = pb_progressBar.Width * 5 / 4;
                pb_progressBar.Maximum = 100;
                pb_progressBar.Value = 0;
                pb_progressBar.Step = pb_progressBar.Maximum/totalSteps;
                pb_progressBar.PerformStep();
                pb_progressBar.Visible = true;
                pb_progressBar.Location = new Point(pb_progressBar.Location.X, pb_progressBar.Location.Y * 5/2);

                bt_CancelOsDiagTool.Visible = true;
                bt_CancelOsDiagTool.Location = new Point(bt_CloseFormPopUp.Location.X * 3/2, bt_CloseFormPopUp.Location.Y*2);

            } else if (feedbackType.Equals(_feedbackDoneType)) {
                this.Width = this.Width * 3;
                bt_CloseFormPopUp.Location = new Point(bt_CloseFormPopUp.Location.X * 4, bt_CloseFormPopUp.Location.Y);
                this.lbl_message.Text = message;

            } else if (feedbackType.Equals(_feedbackErrorType)) {
                bt_CloseFormPopUp.Visible = true;
                bt_CloseFormPopUp.Text = "Exit";
                this.lbl_message.Text = message;

            }else if (feedbackType.Equals(_feedbackTestConnectionType)) {
                this.lbl_message.Text = message;
            }

        }

        public void bt_CloseFormPopUp_Click(object sender, EventArgs e) {
            this.Dispose();
            this.Close();
        }

        public static void ChangeFeedbackLabelAndProgressBar (puf_popUpForm popUpForm, string labelText) {

            popUpForm.lb_ProgressFeedback.Items.Add(labelText);
            popUpForm.lb_ProgressFeedback.SelectedIndex = popUpForm.lb_ProgressFeedback.Items.Count - 1;
            popUpForm.lb_ProgressFeedback.SelectedIndex = -1;

            if (!(popUpForm.pb_progressBar.Value >= popUpForm.pb_progressBar.Maximum)) {
                popUpForm.pb_progressBar.PerformStep();
            }
            
            popUpForm.Refresh();

        }

        private void bt_CancelOsDiagTool_Click(object sender, EventArgs e) {

            isBackgroundWorkerCancelled = true;
            this.Text = "Cancelling. Please wait...";

        }
    }
}
