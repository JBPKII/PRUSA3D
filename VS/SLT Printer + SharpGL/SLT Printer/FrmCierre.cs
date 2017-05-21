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
    public partial class FrmCierre : Form
    {
        public FrmCierre()
        {
            InitializeComponent();
            LblDescripcion.Text = "Detención de la impresión en curso. Espere.";
        }
    }
}
