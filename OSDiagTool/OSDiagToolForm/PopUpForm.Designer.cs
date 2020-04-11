namespace OSDiagTool.OSDiagToolForm {
    partial class PopUpForm {
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
            this.lbl_connectionTest = new System.Windows.Forms.Label();
            this.bt_CloseFormPopUp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_connectionTest
            // 
            this.lbl_connectionTest.AutoSize = true;
            this.lbl_connectionTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_connectionTest.Location = new System.Drawing.Point(12, 25);
            this.lbl_connectionTest.Name = "lbl_connectionTest";
            this.lbl_connectionTest.Size = new System.Drawing.Size(0, 13);
            this.lbl_connectionTest.TabIndex = 0;
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
            // PopUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(229, 114);
            this.Controls.Add(this.bt_CloseFormPopUp);
            this.Controls.Add(this.lbl_connectionTest);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PopUpForm";
            this.ShowIcon = false;
            this.Text = "Test Connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_connectionTest;
        private System.Windows.Forms.Button bt_CloseFormPopUp;
    }
}