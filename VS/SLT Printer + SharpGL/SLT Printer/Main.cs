using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Collections;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Lighting;
using SharpGL.SceneGraph.Primitives;

namespace SLT_Printer
{
    public partial class Main : Form
    {
        //private SolidoSLT SolSLt = new SolidoSLT();
        double Aspecto;

        #region Configuraciones
        public bool MostrarBBox = false;

        #endregion

        public Main()
        {
            InitializeComponent();

            //  Create a cylinder.
            /*SharpGL.SceneGraph.Quadrics.Cylinder cylinder = new SharpGL.SceneGraph.Quadrics.Cylinder() { Name = "ModeloSLT" };
            cylinder.BaseRadius = 1.5;
            cylinder.TopRadius = 1.5;
            cylinder.Height = 2;
            cylinder.Transformation.TranslateX = -2;
            cylinder.Transformation.TranslateY = 2;

            sceneControl.Scene.SceneContainer.AddChild(cylinder);*/

            sceneControl.Cursor = Cursors.Cross;
            sceneControl.Scene.RenderBoundingVolumes = true;

            Aspecto = sceneControl.Scene.CurrentCamera.AspectRatio;

            Modelo = new Model(ref serialPort1);

            Modelo.Information += OnInformation;
            Modelo.ChangeXYZ += OnChangeXYZ;
            Modelo.Warning += OnWarning;
        }

        delegate void SetInformationCallback(string Warn);
        private void OnInformation(string Info)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TxtWarning.InvokeRequired)
            {
                SetInformationCallback d = new SetInformationCallback(OnInformation);
                this.Invoke(d, new object[] { Info });
            }
            else
            {
                string [] strInfo = Info.Split('=');

                if(strInfo.Length==2)
                {
                    switch (strInfo[0])
                    {
                        case "Temperatura Driver X ":
                            TxtTempX.Text = strInfo[1].Trim();
                            break;
                        case "Temperatura Driver Y ":
                            TxtTempY.Text = strInfo[1].Trim();
                            break;
                        case "Temperatura Driver Z ":
                            TxtTempZ.Text = strInfo[1].Trim();
                            break;
                        case "Temperatura Driver E1 ":
                            TxtTempE1.Text = strInfo[1].Trim();
                            break;
                        case "Temperatura Driver E2 ":
                            TxtTempE2.Text = strInfo[1].Trim();
                            break;
                        case "Potencia Ventilador Drivers ":
                            TxtPotVentDrivers.Text = strInfo[1].Trim();
                            break;
                        case "Temperatura Extrusor1 ":
                            TxtTempExt1.Text = strInfo[1].Trim();
                            break;
                        default:
                            this.TxtInfo.Text = (System.Environment.NewLine + "INFO:" + Info);
                            break;
                    }
                }
                else
                {
                     this.TxtInfo.Text = (System.Environment.NewLine + "INFO:" + Info);
                }
            }
        }

        delegate void SetXYZCallback(double X, double Y, double Z);
        private void OnChangeXYZ(double X, double Y, double Z)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TxtX.InvokeRequired)
            {
                SetXYZCallback d = new SetXYZCallback(OnChangeXYZ);
                this.Invoke(d, new object[] { X, Y, Z });
            }
            else
            {
                this.TxtX.Text = X.ToString("0.000");
                this.TxtY.Text = Y.ToString("0.000");
                this.TxtZ.Text = Z.ToString("0.000");
            }
        }

        delegate void SetWarningCallback(string Warn);
        private void OnWarning(string Warn)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
            if (this.TxtWarning.InvokeRequired)
			{
                SetWarningCallback d = new SetWarningCallback(OnWarning);
				this.Invoke(d, new object[] { Warn });
			}
			else
			{
                this.TxtWarning.Text += (System.Environment.NewLine + "WARN:" + Warn);
			}
		}
        void sceneControl_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            UpdateViewPoint();
        }

        void sceneControl_Resized(object sender, System.EventArgs e)
        {
            Aspecto = sceneControl.Scene.CurrentCamera.AspectRatio;
        }

        private void CmbSalir_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            try
            {
                serialPort1.DiscardOutBuffer();
                serialPort1.DiscardInBuffer();
            }
            catch (System.Exception) { }

            this.Close();
        }

        private void CmBAbrirSLT_Click(object sender, EventArgs e)
        {
            CmBIniciar.Enabled = false;

            TxtTx.Text = "0.0000";
            TxtTy.Text = "0.0000";
            TxtTz.Text = "0.0000";

            OFD.Filter = "Standard Tessellation Language (*.stl)|*.stl";
            OFD.Multiselect = false;

            OFD.ShowDialog();

            TxtFileSLT.Text = OFD.FileName;

            if (TxtFileSLT.Text == "")
            {
                CmBTestSLT.Enabled = false;
            }
            else
            {
                Modelo.Solido.LeeSLT(TxtFileSLT.Text);

                if (Modelo.Solido.NumFallos > 0)
                {
                    TxtWarning.Text = "\nWRN - Carga del fichero SLT KO:";
                    foreach (string str in Modelo.Solido.Fallos)
                    {
                        TxtWarning.Text += ("\nWRN - - " + str);
                    }
                }
                else
                {
                    TxtInfo.Text = Environment.NewLine + "INFO:Carga del fichero SLT OK:";
                }

                //Renderiza el resultado
                //Actualiza el punto de vista

                VistaOjo[0] = float.Parse((Modelo.Solido.Centro.X - Modelo.Solido.Ancho * 2).ToString());
                VistaOjo[1] = float.Parse((Modelo.Solido.Centro.Y - Modelo.Solido.Largo * 2).ToString());
                VistaOjo[2] = float.Parse((Modelo.Solido.Centro.Z + Modelo.Solido.Alto * 2).ToString());

                VistaCentro[0] = float.Parse(Modelo.Solido.Centro.X.ToString());
                VistaCentro[1] = float.Parse(Modelo.Solido.Centro.Y.ToString());
                VistaCentro[2] = float.Parse(Modelo.Solido.Centro.Z.ToString());

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

            if (Modelo.Solido.NumFallos == 0)
            {
                Res = Modelo.Solido.TestSLT();

                TSPGB.Value = 100;
                this.Refresh();
            }

            //Resultado OK o KO
            
            if (Res)
            {
                TxtWarning.Text += (Environment.NewLine + "INFO:Test del fichero SLT OK.");

                Res = serialPort1.IsOpen;
                
                UpdateViewPoint();
            }
            else
            {
                TxtWarning.Text += "\nWRN - Carga y Test del fichero SLT KO:";
                foreach (string str in Modelo.Solido.Fallos)
                {
                    TxtWarning.Text += ("\nWRN - - " + str);
                }
            }

            CmBIniciar.Enabled = Res;

            TSPGB.Visible = false;

            this.Cursor = Cursors.Default;
        }

        #region SharpGL

        private float[] VistaOjo = new float[3] { -10.0f, -10.0f, 10.0f };
        private float[] VistaCentro = new float[3] { 0.0f, 0.0f, 0.0f };

        private void UpdateViewPoint()
        {
            Modelo.Solido.Renderiza(ref sceneControl, Modelo.ZTrazado, MostrarBBox);

            LookAtCamera LAC = new LookAtCamera();

            LAC.Near = 0.1;
            LAC.Far = 6000.0;
            LAC.Position = new Vertex(VistaOjo[0], VistaOjo[1], VistaOjo[2]);
            LAC.Target = new Vertex(VistaCentro[0], VistaCentro[1], VistaCentro[2]);
            LAC.FieldOfView = 60;
            LAC.AspectRatio = Aspecto;

            //sceneControl.Scene.Draw(LAC);
            sceneControl.Scene.CurrentCamera = LAC;
            //sceneControl.DoRender();
            sceneControl.Refresh();
        }

        #endregion

        private void CmBZoomIn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                VistaOjo[i] = VistaOjo[i] * 0.8f;
            }

            UpdateViewPoint();
        }

        private void CmBZoomOut_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                VistaOjo[i] = VistaOjo[i] * 1.2f;
            }

            UpdateViewPoint();
        }

        private void sceneControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Delta>0)
            {
                CmBZoomIn_Click(sender, e);
            }
            else
            {
                CmBZoomOut_Click(sender, e);
            }
        }

        private void CmBRotateLeft_Click(object sender, EventArgs e)
        {
            double alfa = 3.14f/360.0*5.0;//Giro de la vista

            VistaOjo[0] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Cos(alfa) - (VistaOjo[1] - VistaCentro[1]) * (float)Math.Sin(alfa) + VistaCentro[0];
            VistaOjo[1] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Sin(alfa) + (VistaOjo[1] - VistaCentro[1]) * (float)Math.Cos(alfa) + VistaCentro[1];

            UpdateViewPoint();
        }

        private void CmBRotateRight_Click(object sender, EventArgs e)
        {
            double alfa = 3.14f / 360.0 * 5.0;//Giro de la vista

            VistaOjo[0] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Cos(-alfa) - (VistaOjo[1] - VistaCentro[1]) * (float)Math.Sin(-alfa) + VistaCentro[0];
            VistaOjo[1] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Sin(-alfa) + (VistaOjo[1] - VistaCentro[1]) * (float)Math.Cos(-alfa) + VistaCentro[1];

            UpdateViewPoint();
        }

        private void CmbRotateUp_Click(object sender, EventArgs e)
        {
            VistaOjo[2] = VistaOjo[2] * 1.2f;

            UpdateViewPoint();
        }

        private void CmBRotateDown_Click(object sender, EventArgs e)
        {

            VistaOjo[2] = VistaOjo[2] * 0.8f;

            UpdateViewPoint();
        }

        private int LastMousePositionX;
        private int LastMousePositionY;

        private void sceneControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
            if (MouseButtons == MouseButtons.Left)
            {
                bool ActX = false;
                bool ActY = false;

                int DeltaX = LastMousePositionX - e.X;
                if (DeltaX > 0)
                {
                    for (int i = 0; i < DeltaX/2; i++)
                    {
                        CmBRotateLeft_Click(sender, e);
                        ActX = true;
                    }
                }
                else if (DeltaX < 0)
                {
                    for (int i = 0; i < -DeltaX/2; i++)
                    {
                        CmBRotateRight_Click(sender, e);
                        ActX = true;
                    }
                }

                int DeltaY = LastMousePositionY - e.Y;
                if (DeltaY > 0)
                {
                    for (int i = 0; i < DeltaY/2; i++)
                    {
                        CmbRotateUp_Click(sender, e);
                        ActY = true;
                    }
                }
                else if (DeltaY < 0)
                {
                    for (int i = 0; i < -DeltaY/2; i++)
                    {
                        CmBRotateDown_Click(sender, e);
                        ActY = true;
                    }
                }

                if (ActX)
                {
                    LastMousePositionX = e.X;
                }
                if (ActY)
                {
                    LastMousePositionY = e.Y;
                }
            }
            else
            {
                LastMousePositionX = e.X;
                LastMousePositionY = e.Y;
            }
        }

        private void CmBaplicatCentrado_Click(object sender, EventArgs e)
        {
            try
            {
                Modelo.Solido.Tx = Convert.ToSingle(TxtTx.Text);
                Modelo.Solido.Ty = Convert.ToSingle(TxtTy.Text);
                Modelo.Solido.Tz = Convert.ToSingle(TxtTz.Text);
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
                sysEx.Data.Clear();
            }
            finally
            {
                TxtTx.Text = Modelo.Solido.Tx.ToString();
                TxtTy.Text = Modelo.Solido.Ty.ToString();
                TxtTz.Text = Modelo.Solido.Tz.ToString();
                UpdateViewPoint();
            }
        }

        private void CmBConex_Click(object sender, EventArgs e)
        {
            FrmConexion Conf = new FrmConexion(ref serialPort1);
            Conf.ShowDialog();

            Conf.Dispose();

            if (serialPort1.IsOpen && Modelo.Solido.PassTest)
            {
                CmBIniciar.Enabled = true;
            }
            else
            {
                CmBIniciar.Enabled = false;
            }
        }

        private void CmBParams_Click(object sender, EventArgs e)
        {
            FrmParametros Params = new FrmParametros();
            Params.ShowDialog(this);
        }

        #region Trazado

        Model Modelo;

        private void CmBIniciar_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            //Realiza la impresión del modelo
            Modelo.Trazar();

                System.Threading.Thread.Sleep(1000);

            this.Enabled = true;

            if(Modelo.Imprimiendo)
            {
                CmBTestSLT.Enabled = false;
                CmBIniciar.Enabled = false;

                CmBPausar.Enabled = true;
                CmBDetener.Enabled = true;

                CmBOrigen.Enabled = false;
                CmBGoTo.Enabled = false;

                System.Threading.Thread Th = new System.Threading.Thread(new System.Threading.ThreadStart(_RefrescoImpresion));
                Th.Start();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No se ha iniciado la impresión.", "Impresión:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //Fin del proceso
        }

        private delegate void RefrescoEscena();
        private void _RefrescoEscena()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.sceneControl.InvokeRequired)
            {
                RefrescoEscena Ref = _RefrescoEscena;
                this.Invoke(Ref);
            }
            else
            {
                UpdateViewPoint();
            }
        }

        private delegate void RefrescoBotones();
        private void _RefrescoBotones()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.CmBTestSLT.InvokeRequired)
            {
                RefrescoBotones Ref = _RefrescoBotones;
                this.Invoke(Ref);
            }
            else
            {
                CmBTestSLT.Enabled = true;
                CmBIniciar.Enabled = true;

                CmBPausar.Enabled = false;
                CmBDetener.Enabled = false;

                CmBOrigen.Enabled = true;
                CmBGoTo.Enabled = true;
            }
        }

        private void _RefrescoImpresion()
        {
            while (Modelo.Imprimiendo)
            {
                _RefrescoEscena();

                System.Threading.Thread.Sleep(500);
            }

            System.Windows.Forms.MessageBox.Show("Impresión terminada.");

            _RefrescoBotones();

            _RefrescoEscena();
        }
        #endregion

        private void CmBPausar_Click(object sender, EventArgs e)
        {
            if(Modelo.Pausado)
            {
                Modelo.ReanudarTrazado();
                CmBPausar.Text = "Pausar";
            }
            else
            {
                Modelo.PausarTrazado();
                CmBPausar.Text = "Reanudar";
            }
            
        }

        private void CmBDetener_Click(object sender, EventArgs e)
        {
            Modelo.DetenerTrazado();

            CmBTestSLT.Enabled = true;
            CmBIniciar.Enabled = true;

            CmBPausar.Enabled = false;
            CmBDetener.Enabled = false;

            CmBOrigen.Enabled = true;
            CmBGoTo.Enabled = true;
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void Main_Closing(object sender, FormClosingEventArgs e)
        {
            if (Modelo.Imprimiendo)
            {
                if (System.Windows.Forms.MessageBox.Show("La impresión se detendrá ¿Está seguro?", "Cerrar:", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    Modelo.DetenerTrazado();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void CmBOrigen_Click(object sender, EventArgs e)
        {
            Trazo Origen = new Trazo();
            Origen.Destino = new VertexSLT(0.0, 0.0, 0.0);
            Origen.Pendiente = true;
            Origen.Modo = Modos.ModoTraslacion;

            Modelo.EnviaTrazo(ref Origen);
        }

        private void CmBGoTo_Click(object sender, EventArgs e)
        {
            double X, Y, Z;
            if (double.TryParse(TxtGoToX.Text.Replace('.', ','), out X) && double.TryParse(TxtGoToY.Text.Replace('.', ','), out Y) && double.TryParse(TxtGoToZ.Text.Replace('.', ','), out Z))
            {
                Trazo Origen = new Trazo();
                Origen.Destino = new VertexSLT(X, Y, Z);
                Origen.Pendiente = true;
                Origen.Modo = Modos.ModoTraslacion;

                Modelo.EnviaTrazo(ref Origen);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Los valores de coordenadas no son válidos.", "ir a:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CmBIniciar_Click(sender, e);
        }
    }

    

    
}
