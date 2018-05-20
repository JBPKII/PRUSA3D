namespace SLT_Printer
{
    partial class FrmShowLog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TxtLog = new System.Windows.Forms.TextBox();
            this.CmBClearLog = new System.Windows.Forms.Button();
            this.ToClipBoard = new System.Windows.Forms.Button();
            this.CkAutoScroll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // TxtLog
            // 
            this.TxtLog.AcceptsReturn = true;
            this.TxtLog.AcceptsTab = true;
            this.TxtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtLog.Location = new System.Drawing.Point(12, 12);
            this.TxtLog.Multiline = true;
            this.TxtLog.Name = "TxtLog";
            this.TxtLog.ReadOnly = true;
            this.TxtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtLog.Size = new System.Drawing.Size(550, 255);
            this.TxtLog.TabIndex = 0;
            // 
            // CmBClearLog
            // 
            this.CmBClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CmBClearLog.Location = new System.Drawing.Point(487, 273);
            this.CmBClearLog.Name = "CmBClearLog";
            this.CmBClearLog.Size = new System.Drawing.Size(75, 23);
            this.CmBClearLog.TabIndex = 1;
            this.CmBClearLog.Text = "Clear";
            this.CmBClearLog.UseVisualStyleBackColor = true;
            this.CmBClearLog.Click += new System.EventHandler(this.CmBClearLog_Click);
            // 
            // ToClipBoard
            // 
            this.ToClipBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ToClipBoard.Location = new System.Drawing.Point(406, 273);
            this.ToClipBoard.Name = "ToClipBoard";
            this.ToClipBoard.Size = new System.Drawing.Size(75, 23);
            this.ToClipBoard.TabIndex = 2;
            this.ToClipBoard.Text = "Copy";
            this.ToClipBoard.UseVisualStyleBackColor = true;
            this.ToClipBoard.Click += new System.EventHandler(this.ToClipBoard_Click);
            // 
            // CkAutoScroll
            // 
            this.CkAutoScroll.AutoSize = true;
            this.CkAutoScroll.Location = new System.Drawing.Point(12, 277);
            this.CkAutoScroll.Name = "CkAutoScroll";
            this.CkAutoScroll.Size = new System.Drawing.Size(75, 17);
            this.CkAutoScroll.TabIndex = 3;
            this.CkAutoScroll.Text = "Auto scroll";
            this.CkAutoScroll.UseVisualStyleBackColor = true;
            // 
            // FrmShowLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 308);
            this.Controls.Add(this.CkAutoScroll);
            this.Controls.Add(this.ToClipBoard);
            this.Controls.Add(this.CmBClearLog);
            this.Controls.Add(this.TxtLog);
            this.Name = "FrmShowLog";
            this.Text = "FrmShowLog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtLog;
        private System.Windows.Forms.Button CmBClearLog;
        private System.Windows.Forms.Button ToClipBoard;
        private System.Windows.Forms.CheckBox CkAutoScroll;
    }
}