﻿namespace OSDiagTool.OSDiagToolForm {
    partial class puf_popUpForm {
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
            this.lbl_message = new System.Windows.Forms.Label();
            this.bt_CloseFormPopUp = new System.Windows.Forms.Button();
            this.pb_progressBar = new System.Windows.Forms.ProgressBar();
            this.lb_ProgressFeedback = new System.Windows.Forms.ListBox();
            this.bt_CancelOsDiagTool = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_message
            // 
            this.lbl_message.AutoSize = true;
            this.lbl_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_message.Location = new System.Drawing.Point(12, 25);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(0, 13);
            this.lbl_message.TabIndex = 0;
            // 
            // bt_CloseFormPopUp
            // 
            this.bt_CloseFormPopUp.Location = new System.Drawing.Point(76, 79);
            this.bt_CloseFormPopUp.Name = "bt_CloseFormPopUp";
            this.bt_CloseFormPopUp.Size = new System.Drawing.Size(75, 23);
            this.bt_CloseFormPopUp.TabIndex = 1;
            this.bt_CloseFormPopUp.Text = "Close";
            this.bt_CloseFormPopUp.UseVisualStyleBackColor = true;
            this.bt_CloseFormPopUp.Click += new System.EventHandler(this.bt_CloseFormPopUp_Click);
            // 
            // pb_progressBar
            // 
            this.pb_progressBar.Location = new System.Drawing.Point(12, 50);
            this.pb_progressBar.Name = "pb_progressBar";
            this.pb_progressBar.Size = new System.Drawing.Size(205, 23);
            this.pb_progressBar.TabIndex = 2;
            this.pb_progressBar.Visible = false;
            // 
            // lb_ProgressFeedback
            // 
            this.lb_ProgressFeedback.Enabled = false;
            this.lb_ProgressFeedback.FormattingEnabled = true;
            this.lb_ProgressFeedback.Location = new System.Drawing.Point(12, 13);
            this.lb_ProgressFeedback.Name = "lb_ProgressFeedback";
            this.lb_ProgressFeedback.Size = new System.Drawing.Size(205, 30);
            this.lb_ProgressFeedback.TabIndex = 3;
            this.lb_ProgressFeedback.TabStop = false;
            this.lb_ProgressFeedback.Visible = false;
            // 
            // bt_CancelOsDiagTool
            // 
            this.bt_CancelOsDiagTool.Location = new System.Drawing.Point(-5, 79);
            this.bt_CancelOsDiagTool.Name = "bt_CancelOsDiagTool";
            this.bt_CancelOsDiagTool.Size = new System.Drawing.Size(75, 23);
            this.bt_CancelOsDiagTool.TabIndex = 4;
            this.bt_CancelOsDiagTool.Text = "Cancel";
            this.bt_CancelOsDiagTool.UseVisualStyleBackColor = true;
            this.bt_CancelOsDiagTool.Visible = false;
            this.bt_CancelOsDiagTool.Click += new System.EventHandler(this.bt_CancelOsDiagTool_Click);
            // 
            // puf_popUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(225, 114);
            this.Controls.Add(this.bt_CancelOsDiagTool);
            this.Controls.Add(this.lb_ProgressFeedback);
            this.Controls.Add(this.pb_progressBar);
            this.Controls.Add(this.bt_CloseFormPopUp);
            this.Controls.Add(this.lbl_message);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "puf_popUpForm";
            this.ShowIcon = false;
            this.Text = "OSDiagTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Button bt_CloseFormPopUp;
        public System.Windows.Forms.ProgressBar pb_progressBar;
        public System.Windows.Forms.ListBox lb_ProgressFeedback;
        private System.Windows.Forms.Button bt_CancelOsDiagTool;
    }
}