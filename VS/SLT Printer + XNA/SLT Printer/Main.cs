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
    public partial class Main : Form
    {
        Game1 Game;
        private SolidoSLT SolSLt = new SolidoSLT();

        public Main()
        {
            InitializeComponent();
            Game = new Game1();
            Game.GraphicsDevice.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport(this.GBPreview.Location.X, this.GBPreview.Location.Y, this.GBPreview.Width, this.GBPreview.Height);
        }

        private void CmbSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CmBAbrirSLT_Click(object sender, EventArgs e)
        {
            OFD.Filter = "Standard Tessellation Language (*.stl)|*.stl";
            OFD.Multiselect = false;

            OFD.ShowDialog();

            TxtFileSLT.Text = OFD.FileName;

            if (TxtFileSLT.Text == "")
            {
                CmBTestSLT.Enabled = false;
                CmBIniciar.Enabled = false;
            }
            else
            {
                SolSLt.LeeSLT(TxtFileSLT.Text);

                if (SolSLt.NumFallos > 0)
                {
                    TxtWarning.Text = "\nWRN - Carga del fichero SLT KO:";
                    foreach (string str in SolSLt.Fallos)
                    {
                        TxtWarning.Text += ("\nWRN - - " + str);
                    }
                }
                else
                {
                    TxtWarning.Text = "\nINF - Carga del fichero SLT OK:";
                }

                //Renderiza el resultado
                //Actualiza el punto de vista

                /*VistaOjo[0] = SolSLt.Centro._X + SolSLt.Ancho;
                VistaOjo[1] = SolSLt.Centro._Y;// +SolSLt.Largo;
                VistaOjo[2] = SolSLt.Centro._Z + SolSLt.Alto;

                VistaCentro[0] = SolSLt.Centro._X;
                VistaCentro[1] = SolSLt.Centro._Y;
                VistaCentro[2] = SolSLt.Centro._Z;*/

                UpdateViewPoint();

                CmBTestSLT.Enabled = true;
            }
        }

        private void CmBTestSLT_Click(object sender, EventArgs e)
        {
            TSPGB.Value = 0;
            TSPGB.Visible = true;

            this.Cursor = Cursors.WaitCursor;
            this.Refresh();

            bool Res = false;

            TSPGB.Value = 5;
            this.Refresh();

            if(SolSLt.NumFallos == 0)
            {
                Res = SolSLt.TestSLT();

                TSPGB.Value = 100;
                this.Refresh();
            }

            //Resultado OK o KO
            CmBIniciar.Enabled = Res;

            if (Res)
            {
                TxtWarning.Text += "\nINF - Test del fichero SLT OK.";

                UpdateViewPoint();
            }
            else
            {
                TxtWarning.Text += "\nWRN - Carga y Test del fichero SLT KO:";
                foreach(string str in SolSLt.Fallos)
                {
                    TxtWarning.Text += ("\nWRN - - " + str);
                }
            }

            TSPGB.Visible = false;

            this.Cursor = Cursors.Default;
        }

        #region XNA

        
      


        //private double[] VistaOjo = new double[3] { 0, 0, 0 };
        //private double[] VistaCentro = new double[3] { 0, 0, 0 };

        private void UpdateViewPoint()
        {
            //http://msdn.microsoft.com/en-us/library/bb197293%28v=xnagamestudio.31%29.aspx

            SolSLt.Renderiza(null);
        }
        #endregion
    }
}
