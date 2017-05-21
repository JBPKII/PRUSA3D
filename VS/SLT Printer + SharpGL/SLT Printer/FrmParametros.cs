using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SLT_Printer
{
    public partial class FrmParametros : Form
    {
        Main FrmMain;

        public FrmParametros()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            this.Close();
        }

        private void FrmParametros_Load(object sender, EventArgs e)
        {
            FrmMain=this.Owner as Main;
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
