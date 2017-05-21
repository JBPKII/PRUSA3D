namespace SLT_Printer
{
    partial class FrmCierre
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
            this.PGB = new System.Windows.Forms.ProgressBar();
            this.LblDescripcion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PGB
            // 
            this.PGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PGB.Location = new System.Drawing.Point(12, 40);
            this.PGB.Name = "PGB";
            this.PGB.Size = new System.Drawing.Size(312, 23);
            this.PGB.TabIndex = 0;
            this.PGB.UseWaitCursor = true;
            // 
            // LblDescripcion
            // 
            this.LblDescripcion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LblDescripcion.Location = new System.Drawing.Point(12, 9);
            this.LblDescripcion.Name = "LblDescripcion";
            this.LblDescripcion.Size = new System.Drawing.Size(312, 28);
            this.LblDescripcion.TabIndex = 1;
            this.LblDescripcion.Text = "label1";
            this.LblDescripcion.UseWaitCursor = true;
            // 
            // FrmCierre
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 75);
            this.Controls.Add(this.LblDescripcion);
            this.Controls.Add(this.PGB);
            this.Name = "FrmCierre";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Impresión:";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LblDescripcion;
        public System.Windows.Forms.ProgressBar PGB;
    }
}