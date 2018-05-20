namespace SLT_Printer
{
    partial class FrmConexion
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
            this.CmBAplicar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CmBTest = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CBBaudios = new System.Windows.Forms.ComboBox();
            this.CBPuertos = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CmBAplicar
            // 
            this.CmBAplicar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CmBAplicar.Enabled = false;
            this.CmBAplicar.Location = new System.Drawing.Point(376, 189);
            this.CmBAplicar.Name = "CmBAplicar";
            this.CmBAplicar.Size = new System.Drawing.Size(75, 23);
            this.CmBAplicar.TabIndex = 4;
            this.CmBAplicar.Text = "Aplicar";
            this.CmBAplicar.UseVisualStyleBackColor = true;
            this.CmBAplicar.Click += new System.EventHandler(this.CmBAplicar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CmBTest);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.CBBaudios);
            this.groupBox1.Controls.Add(this.CBPuertos);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(207, 99);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Conexión Serial:";
            // 
            // CmBTest
            // 
            this.CmBTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CmBTest.Location = new System.Drawing.Point(161, 73);
            this.CmBTest.Name = "CmBTest";
            this.CmBTest.Size = new System.Drawing.Size(40, 23);
            this.CmBTest.TabIndex = 5;
            this.CmBTest.Text = "Test";
            this.CmBTest.UseVisualStyleBackColor = true;
            this.CmBTest.Click += new System.EventHandler(this.CmBTest_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Baudios:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Puerto COM:";
            // 
            // CBBaudios
            // 
            this.CBBaudios.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CBBaudios.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBBaudios.FormattingEnabled = true;
            this.CBBaudios.Items.AddRange(new object[] {
            "300",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200"});
            this.CBBaudios.Location = new System.Drawing.Point(80, 46);
            this.CBBaudios.Name = "CBBaudios";
            this.CBBaudios.Size = new System.Drawing.Size(121, 21);
            this.CBBaudios.TabIndex = 1;
            // 
            // CBPuertos
            // 
            this.CBPuertos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CBPuertos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBPuertos.FormattingEnabled = true;
            this.CBPuertos.Location = new System.Drawing.Point(80, 19);
            this.CBPuertos.Name = "CBPuertos";
            this.CBPuertos.Size = new System.Drawing.Size(121, 21);
            this.CBPuertos.TabIndex = 0;
            this.CBPuertos.SelectedIndexChanged += new System.EventHandler(this.CBPuertos_SelectedIndexChanged);
            // 
            // FrmConexion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 224);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CmBAplicar);
            this.Name = "FrmConexion";
            this.Text = "Parámetros:";
            this.Load += new System.EventHandler(this.FrmConexion_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CmBAplicar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button CmBTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CBBaudios;
        private System.Windows.Forms.ComboBox CBPuertos;
    }
}