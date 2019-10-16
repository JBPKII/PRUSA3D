using System;
using System.Windows.Forms;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SLT_Printer.SLT;

namespace SLT_Printer
{
    public partial class Main : Form
    {
        private const string constExtruder = "Extruder";

        private static System.IO.Ports.SerialPort SerialPortToArduino;
        //private static ModelSLT Modelo;
        PrintController Controller;

        //private SolidoSLT SolSLt = new SolidoSLT();
        double Aspecto;

        #region Configuraciones
        public bool MostrarBBox = false;
        public bool MostrarFPS = true;

        public string SerialPortDefaultPortName = "COM4";
        public int SerialPortDefaultBaudRate = 115200;

        private void InitializeSerialPort()
        {
            if(SerialPortToArduino.IsOpen)
            {
                SerialPortToArduino.Close();
            }

            SerialPortToArduino.PortName = SerialPortDefaultPortName;
            SerialPortToArduino.BaudRate = SerialPortDefaultBaudRate;
        }

        #endregion

        public Main()
        {
            InitializeComponent();

            sceneControl.Cursor = Cursors.Cross;
            sceneControl.Scene.RenderBoundingVolumes = MostrarBBox;
            sceneControl.DrawFPS = MostrarFPS;
            sceneControl.FrameRate = 60;

            Aspecto = sceneControl.Scene.CurrentCamera.AspectRatio;

            SerialPortToArduino = new System.IO.Ports.SerialPort(SerialPortDefaultPortName, SerialPortDefaultBaudRate);
            
            //Modelo = new ModelSLT(ref SerialPortToArduino);
            Controller = new PrintController(ref SerialPortToArduino);

            Controller.Interpreter.Information += OnInformation;
            Controller.Interpreter.ChangeXYZ += OnChangeXYZ;
            Controller.Interpreter.Warning += OnWarning;
        }

        delegate void SetInformationCallback(string Warn);
        private void OnInformation(string Info)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TxtInfo.InvokeRequired)
            {
                try
                {
                    SetInformationCallback d = new SetInformationCallback(OnInformation);
                    this.Invoke(d, new object[] { Info });
                }
                catch (Exception) { }
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
                    this.TxtInfo.Text += (System.Environment.NewLine + "INFO:" + Info);
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
                try
                {
                    SetXYZCallback d = new SetXYZCallback(OnChangeXYZ);
                    this.Invoke(d, new object[] { X, Y, Z });
                }
                catch (Exception) { }
            }
            else
            {
                this.TxtX.Text = X.ToString("0.000");
                this.TxtY.Text = Y.ToString("0.000");
                this.TxtZ.Text = Z.ToString("0.000");

                foreach (SharpGL.SceneGraph.Core.SceneElement item in sceneControl.Scene.SceneContainer.Children)
                {
                    if (item.Name == constExtruder && item is SharpGL.SceneGraph.Primitives.Folder)
                    {
                        if (item.Effects.Count != 0)
                        {
                            for (int i = 0; i < item.Effects.Count; i++)
                            {
                                if(item.Effects[i] is SharpGL.SceneGraph.Effects.LinearTransformationEffect)
                                {
                                    item.RemoveEffect(item.Effects[i]);
                                }
                            }
                        }

                        SharpGL.SceneGraph.Effects.LinearTransformationEffect linearTransformationEffect = new SharpGL.SceneGraph.Effects.LinearTransformationEffect();
                        linearTransformationEffect.LinearTransformation.TranslateX = Convert.ToSingle(X);
                        linearTransformationEffect.LinearTransformation.TranslateY = Convert.ToSingle(Y);
                        linearTransformationEffect.LinearTransformation.TranslateZ = Convert.ToSingle(Z);

                        item.AddEffect(linearTransformationEffect);
                    }
                }
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
                try
                {
                    SetWarningCallback d = new SetWarningCallback(OnWarning);
                    this.Invoke(d, new object[] { Warn });
                }
                catch (Exception) { }
            }
            else
            {
                this.TxtWarning.Text += (System.Environment.NewLine + "WARN:" + Warn);
            }
        }
        /*void sceneControl_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            UpdateViewPoint(false);
        }*/

        void sceneControl_Resized(object sender, System.EventArgs e)
        {
            Aspecto = sceneControl.Scene.CurrentCamera.AspectRatio;
        }

        private void CmbSalir_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            try
            {
                SerialPortToArduino.DiscardOutBuffer();
                SerialPortToArduino.DiscardInBuffer();
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
                Controller.Model.Solido.LeeSLT(TxtFileSLT.Text);

                if (Controller.Model.Solido.NumFallos > 0)
                {
                    TxtWarning.Text = "\nWRN - Carga del fichero SLT KO:";
                    foreach (string str in Controller.Model.Solido.Fallos)
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

                VistaOjo[0] = float.Parse((Controller.Model.Solido.Centro.X - Controller.Model.Solido.Ancho * 2).ToString());
                VistaOjo[1] = float.Parse((Controller.Model.Solido.Centro.Y - Controller.Model.Solido.Largo * 2).ToString());
                VistaOjo[2] = float.Parse((Controller.Model.Solido.Centro.Z + Controller.Model.Solido.Alto * 2).ToString());

                VistaCentro[0] = float.Parse(Controller.Model.Solido.Centro.X.ToString());
                VistaCentro[1] = float.Parse(Controller.Model.Solido.Centro.Y.ToString());
                VistaCentro[2] = float.Parse(Controller.Model.Solido.Centro.Z.ToString());

                UpdateViewPoint(true);

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

            if (Controller.Model.Solido.NumFallos == 0)
            {
                Res = Controller.Model.Solido.TestSLT();

                TSPGB.Value = 100;
                this.Refresh();
            }

            //Resultado OK o KO
            
            if (Res)
            {
                TxtWarning.Text += (Environment.NewLine + "INFO:Test del fichero SLT OK.");

                Res = SerialPortToArduino.IsOpen;
                
                UpdateViewPoint(true);
            }
            else
            {
                TxtWarning.Text += "\nWRN - Carga y Test del fichero SLT KO:";
                foreach (string str in Controller.Model.Solido.Fallos)
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

        private void _InitViewPoint()
        {
            VistaOjo = new float[3] { -10.0f, -10.0f, 10.0f };
            VistaCentro = new float[3] { 0.0f, 0.0f, 0.0f };
        }

        private void UpdateViewPoint(bool LoadedModel)
        {
            Controller.Model.Solido.Renderiza(ref sceneControl, LoadedModel, LoadedModel, Controller.Model.ZTrazado, MostrarBBox);

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

        private void sceneControl_Load(object sender, EventArgs e)
        {
            SharpGL.SceneGraph.Quadrics.Cylinder cylinder = new SharpGL.SceneGraph.Quadrics.Cylinder
            {
                BaseRadius = 0.0,
                Height = 5.0,
                TopRadius = 2.0,
            };

            SharpGL.SceneGraph.Quadrics.Cylinder cylinderTop = new SharpGL.SceneGraph.Quadrics.Cylinder
            {
                BaseRadius = 2.0,
                Height = 20.0,
                TopRadius = 2.0
            };
            
            SharpGL.SceneGraph.Transformations.LinearTransformation linearTransformation = new SharpGL.SceneGraph.Transformations.LinearTransformation
            {
                TranslateZ = 5
            };
            SharpGL.SceneGraph.Effects.LinearTransformationEffect effect = new SharpGL.SceneGraph.Effects.LinearTransformationEffect
            {
                LinearTransformation = linearTransformation
            };
            cylinderTop.Effects.Add(effect);

            SharpGL.SceneGraph.Primitives.Folder extruder = new SharpGL.SceneGraph.Primitives.Folder
            {
                Name = constExtruder
            };
            
            sceneControl.Scene.SceneContainer.AddChild(extruder);

            extruder.AddChild(cylinder);
            extruder.AddChild(cylinderTop);
        }

        private void CmBCenterView_Click(object sender, EventArgs e)
        {
            _InitViewPoint();
            UpdateViewPoint(false);
        }

        private void CmBZoomIn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                VistaOjo[i] = VistaOjo[i] * 0.8f;
            }

            UpdateViewPoint(false);
        }

        private void CmBZoomOut_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                VistaOjo[i] = VistaOjo[i] * 1.2f;
            }

            UpdateViewPoint(false);
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

            UpdateViewPoint(false);
        }

        private void CmBRotateRight_Click(object sender, EventArgs e)
        {
            double alfa = 3.14f / 360.0 * 5.0;//Giro de la vista

            VistaOjo[0] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Cos(-alfa) - (VistaOjo[1] - VistaCentro[1]) * (float)Math.Sin(-alfa) + VistaCentro[0];
            VistaOjo[1] = (VistaOjo[0] - VistaCentro[0]) * (float)Math.Sin(-alfa) + (VistaOjo[1] - VistaCentro[1]) * (float)Math.Cos(-alfa) + VistaCentro[1];

            UpdateViewPoint(false);
        }

        private void CmbRotateUp_Click(object sender, EventArgs e)
        {
            VistaOjo[2] = VistaOjo[2] * 1.2f;

            UpdateViewPoint(false);
        }

        private void CmBRotateDown_Click(object sender, EventArgs e)
        {

            VistaOjo[2] = VistaOjo[2] * 0.8f;

            UpdateViewPoint(false);
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

        private void CmBaplicaCentrado_Click(object sender, EventArgs e)
        {
            try
            {
                Controller.Model.Solido.Tx = Convert.ToSingle(TxtTx.Text);
                Controller.Model.Solido.Ty = Convert.ToSingle(TxtTy.Text);
                Controller.Model.Solido.Tz = Convert.ToSingle(TxtTz.Text);
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
                sysEx.Data.Clear();
            }
            finally
            {
                TxtTx.Text = Controller.Model.Solido.Tx.ToString();
                TxtTy.Text = Controller.Model.Solido.Ty.ToString();
                TxtTz.Text = Controller.Model.Solido.Tz.ToString();
                UpdateViewPoint(false);
            }
        }
        #endregion

        private void CmBConex_Click(object sender, EventArgs e)
        {
            InitializeSerialPort();

            FrmConexion Conf = new FrmConexion(ref SerialPortToArduino);
            Conf.ShowDialog();

            Conf.Dispose();

            if (SerialPortToArduino.IsOpen && Controller.Model.Solido.PassTest)
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

        private void CmBIniciarTest_Click(object sender, EventArgs e)
        {
            Controller.Interpreter.Log += this.OnLog;

            Iniciar(true);
        }

        private void CmBIniciar_Click(object sender, EventArgs e)
        {
            Iniciar(false);
        }
        
        private void Iniciar(bool isTest)
        {
            this.Enabled = false;
            //Realiza la impresión del modelo
            Controller.Print(isTest);

            this.Enabled = true;

            if(Controller.Status == PrinterStatus.Printing)
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
                try
                {
                    RefrescoEscena Ref = _RefrescoEscena;
                    this.Invoke(Ref);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    UpdateViewPoint(false);
                }
                catch (Exception) { }
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
            while (Controller.Status == PrinterStatus.Printing)
            {
                _RefrescoEscena();

                System.Threading.Thread.Sleep(100);
            }

            System.Windows.Forms.MessageBox.Show("Impresión terminada.");

            _RefrescoBotones();

            _RefrescoEscena();
        }
        #endregion

        private void CmBPausar_Click(object sender, EventArgs e)
        {
            if(Controller.Status == PrinterStatus.Pause)
            {
                Controller.Resume();
                CmBPausar.Text = "Pausar";
            }
            else if(Controller.Status == PrinterStatus.Printing)
            {
                Controller.Pause();
                CmBPausar.Text = "Reanudar";
            }
        }

        private void CmBDetener_Click(object sender, EventArgs e)
        {
            Controller.Stop();

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
            if (Controller.Status== PrinterStatus.Printing)
            {
                if (System.Windows.Forms.MessageBox.Show("La impresión se detendrá ¿Está seguro?", "Cerrar:", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    Controller.Stop();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void CmBOrigen_Click(object sender, EventArgs e)
        {
            StrokeSLT Origen = new StrokeSLT();
            Origen.Destino = new VertexSLT(0.0, 0.0, 0.0);
            Origen.Pendiente = true;
            //Origen.Mode = Modes.ModeTraslation;

            switch (CBModo.SelectedItem)
            {
                case "Relleno":
                    Origen.Mode = Modes.ModoFill;
                    break;
                case "Bordes":
                    Origen.Mode = Modes.ModoRim;
                    break;
                case "Traslación":
                default:
                    Origen.Mode = Modes.ModeTraslation;
                    break;
            }

            Controller.Interpreter.SendStroke(Origen);
        }

        private void CmBGoTo_Click(object sender, EventArgs e)
        {
            double X, Y, Z;
            if (double.TryParse(TxtGoToX.Text.Replace('.', ','), out X) && double.TryParse(TxtGoToY.Text.Replace('.', ','), out Y) && double.TryParse(TxtGoToZ.Text.Replace('.', ','), out Z))
            {
                StrokeSLT Origen = new StrokeSLT();
                Origen.Destino = new VertexSLT(X, Y, Z);
                Origen.Pendiente = true;

                switch (CBModo.SelectedItem)
                {
                    case "Relleno":
                        Origen.Mode = Modes.ModoFill;
                        break;
                    case "Bordes":
                        Origen.Mode = Modes.ModoRim;
                        break;
                    case "Traslación":
                    default:
                        Origen.Mode = Modes.ModeTraslation;
                        break;
                }

                Controller.Interpreter.SendStroke(Origen);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Los valores de coordenadas no son válidos.", "ir a:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void TxtWarning_DoubleClick(object sender, EventArgs e)
        {
            FrmShowLog WarningLog = new FrmShowLog("Warning:", ((TextBox)sender).Text);
            WarningLog.Show(this);
            Controller.Interpreter.Warning += WarningLog.OnLog;
        }

        private void TxtInfo_DoubleClick(object sender, EventArgs e)
        {
            FrmShowLog InformationLog = new FrmShowLog("Information:", ((TextBox)sender).Text);
            InformationLog.Show(this);
            Controller.Interpreter.Information += InformationLog.OnLog;
        }

        private System.IO.StreamWriter sw = null;
        delegate void SetLogCallback(string Warn);
        public void OnLog(string Log)
        {
            try
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.InvokeRequired)
                {
                    SetLogCallback d = new SetLogCallback(OnLog);
                    this.Invoke(d, new object[] { Log });
                }
                else
                {
                    if (sw == null)
                    {
                        sw = new System.IO.StreamWriter(string.Format(  "{0}\\{1}.log", 
                                                                        System.Environment.CurrentDirectory, 
                                                                        System.DateTime.Now.ToString("yyyyMMdd HHmmss")),true);
                    }

                    sw.WriteLine(Log);
                }
            }
            catch(Exception) { }
        }

        int InfoTemperature = 40;//ºC
        int WarnTemperature = 50;//ºC

        private void Txt_TextChanged(TextBox sender)
        {
            int Temp = 0;
            if (sender.Text.Contains(".") && int.TryParse(sender.Text.Split('.')[0], out Temp))
            {
                if(Temp >= WarnTemperature)
                {
                    sender.BackColor = System.Drawing.Color.IndianRed;
                }
                else if (Temp >= InfoTemperature)
                {
                    sender.BackColor = System.Drawing.Color.Orange;
                }
                else if (Temp <= 0)
                {
                    sender.BackColor = System.Drawing.Color.LightBlue;
                }
                else
                {
                    sender.BackColor = System.Drawing.Color.LightGreen;
                }
            }
            else
            {
                sender.BackColor = System.Drawing.Color.White;
            }

        }

        private void TxtTempX_TextChanged(object sender, EventArgs e)
        {
            Txt_TextChanged(TxtTempX);
        }

        private void TxtTempY_TextChanged(object sender, EventArgs e)
        {
            Txt_TextChanged(TxtTempY);
        }

        private void TxtTempZ_TextChanged(object sender, EventArgs e)
        {
            Txt_TextChanged(TxtTempZ);
        }

        private void TxtTempE1_TextChanged(object sender, EventArgs e)
        {
            Txt_TextChanged(TxtTempE1);
        }

        private void TxtTempE2_TextChanged(object sender, EventArgs e)
        {
            Txt_TextChanged(TxtTempE2);
        }

        private void TxtInfo_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
