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
