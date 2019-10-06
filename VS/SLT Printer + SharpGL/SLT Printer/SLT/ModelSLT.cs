using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SLT_Printer.SLT
{
    enum Modes { ModeTraslation = 1, ModoFill = 2, ModoRim = 4 }

    class ModelSLT
    {
        public SolidSLT Solido;

        private Thread _TImprimir;
        private bool _IsTest = false;
        // private Thread _TGenerarLayer;

        private static System.IO.Ports.SerialPort _serialPort;

        public double ZCalculo { get; private set; } = 0.0;
        public double ZTrazado { get; private set; } = double.NaN;

        // private bool _SendNextStroke = true;

        public bool Printing { get; private set; } = false;

        public double DeltaLayer = 0.25;

        private int CurrentStrokeIndex = 0;
        private StrokeSLT[] _LayerActual;
        private IList<StrokeSLT> _LayerCalculo;

        private bool _GeneratingLayer = false;
        private bool _Stoped = false;//variable para DETENER la impresión

        public bool Paused { get; private set; } = false;

        private bool Stoped
        {
            get
            {
                return _Stoped;
            }

            set
            {
                _Stoped = true;
                FrmCierre FrmC = new FrmCierre();

                FrmC.Show();
                FrmC.PGB.Visible = true;

                FrmC.Update();

                FrmC.PGB.Value = 20;
                Thread.Sleep(300);
                FrmC.PGB.Value = 40;
                Thread.Sleep(300);
                FrmC.PGB.Value = 60;
                Thread.Sleep(300);
                FrmC.PGB.Value = 80;
                Thread.Sleep(300);
                FrmC.PGB.Value = 100;
                Thread.Sleep(800);

                FrmC.Update();

                FrmC.Close();
            }
        }


        public ModelSLT(ref System.IO.Ports.SerialPort serialPort)
        {
            _serialPort = serialPort;
            _serialPort.DtrEnable = true;
            _serialPort.ReceivedBytesThreshold = 1;
            _serialPort.DataReceived += _Port_DataReceived;

            Solido = new SolidSLT();

            Solido.Changed += Solido_Changed;
            ZTrazado = double.NaN;
            DeltaLayer = 0.2;
            ZCalculo = 0.0 ;

            _AnguloLayer = 0.0;

            _LayerActual = new StrokeSLT[0];
            _LayerCalculo = new StrokeSLT[0];

            Printing = false;
            Paused = false;
            _Stoped = false;

            //_TImprimir = new Thread(new ThreadStart(_Trazar));
            //_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
        }

        void Solido_Changed(object sender, EventArgs e)
        {
            if (Printing)
            {
                Stoped = true;
            }
        }


        private IList<VertexSLT> Intersection(LoopSLT Loop)
        {
            IList<VertexSLT> Res = new List<VertexSLT>();

            VertexSLT TempVertex = new VertexSLT();
            bool IntInterna = false;
            for (int i = 1; i < Loop.Vertices.Count; i++)
            {
                TempVertex = new VertexSLT();
                IntInterna = false;
                if (Intersection(Loop.Vertices[i - 1], Loop.Vertices[i], out TempVertex, out IntInterna))
                {
                    if (IntInterna)
                    {
                        Res.Add(TempVertex);
                    }
                }
            }

            TempVertex = new VertexSLT();
            IntInterna = false;
            if (Intersection(Loop.Vertices[0], Loop.Vertices[Loop.Vertices.Count - 1], out TempVertex, out IntInterna))
            {
                if (IntInterna)
                {
                    Res.Add(TempVertex);
                }
            }

            return Res;
        }

        private bool Intersection(VertexSLT V1, VertexSLT V2, out VertexSLT Interseccion, out bool Interna)
        {
            bool Res = false;

            try
            {
                Interseccion = new VertexSLT();
                Interna = false;

                VertexSLT Direccion = new VertexSLT(V2.X - V1.X, V2.Y - V1.Y, V2.Z - V1.Z);

                VertexSLT NormalPlanoZ = new VertexSLT(0.0, 0.0, 1.0);
                double D = -ZCalculo;

                double t = ((NormalPlanoZ.X * V1.X) + (NormalPlanoZ.Y * V1.Y) + (NormalPlanoZ.Z * V1.Z) + D)
                / (-(NormalPlanoZ.X * Direccion.X) - (NormalPlanoZ.Y * Direccion.Y) - (NormalPlanoZ.Z * Direccion.Z));

                Interseccion = new VertexSLT(V1.X + t * Direccion.X, V1.Y + t * Direccion.Y, V1.Z + t * Direccion.Z);

                Res = true;

                if (0.0 <= t && t <= 1.0)
                {
                    Interna = true;
                }
            }
            catch (System.Exception sysEx)
            {
                sysEx.Data.Clear();
                Res = false;
                Interseccion = new VertexSLT();
                Interna = false;
            }

            return Res;
        }

        public void Draw(bool isTest)
        {
            _IsTest = isTest;

            if (_TImprimir == null || !_TImprimir.IsAlive)
            {
                _TImprimir = new Thread(new ThreadStart(_Draw));
                _TImprimir.Start();
            }
            else
            {
                //avisa de que ya está en ejecución
                System.Windows.Forms.MessageBox.Show("Proceso de impresión ejecutándose.");
            }
        }

        bool IsTrazando = false;

        private void _Draw()
        {
            StrokeSLT _Trazo = null;

            Printing = true;

            _AnguloLayer = 0.0;
            ZCalculo = 0.0;

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();
            _GenerandoLayer = true;*/

            SendTemperature(160.0);

            _GenerateLayer();

            /*while(_GenerandoLayer )
            {
                System.Threading.Thread.Sleep(500);
            }*/

            _ChangeLayer();

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();*/

            _GenerateLayer();


            if (ZCalculo <= Solido.Top)
            {
                IsTrazando = true;

                _Trazo = new StrokeSLT();

                if (_LayerActual.Count() > 0)
                {
                    _Trazo.Destino = _LayerActual[0].Destino;
                }
                else
                {
                    _Trazo.Destino = new VertexSLT(0.0, 0.0, ZTrazado);
                }

                _Trazo.Mode = Modes.ModeTraslation;
                _Trazo.Pendiente = true;

                //Para calcular la distancia del trazado
                lastPoint = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);

                //Envía el trazo
                SendStroke(_Trazo);

       
                

                while (IsTrazando)
                {
                    /*_Trazo = NextStroke();//lo almacena en _trazo

                    AnteriorTrazado = (_Trazo != null);

                    //Envía los sucesivos trazos
                    while(!_SendNextStroke)
                    {
                        //hace tiempo hasta que termina de enviar el comando
                        Thread.Sleep(30);
                    }

                    if (SendStroke(ref _Trazo, AntDest.Distancia(new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, AntDest.Z))))
                    {
                        AntDest = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);
                    }

                    //_SendNBextStroke = false;*/

                    if (Stoped)
                    {
                        if (_GeneratingLayer)
                        {
                            //_TGenerarLayer.Abort();
                            _GeneratingLayer = false;
                        }
                        ZTrazado = double.NaN;
                        break;
                    }

                    while (Paused)
                    {
                        System.Threading.Thread.Sleep(300);
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }

            SendTemperature();

            ZTrazado = double.NaN;
            Paused = false;
            _Stoped = false;

            // Fin del trazado
            Printing = false;

            // Presenta el trazado
            StrokeSLT trzPresent = new StrokeSLT();
            trzPresent.Destino = new VertexSLT(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z + 50.0/*mm*/);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            SendStroke(trzPresent);

            trzPresent.Destino = new VertexSLT(0.0, 0.0, _Trazo.Destino.Z + 50.0);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            SendStroke(trzPresent);
        }

        public void PauseDraw()
        {
            if (Printing)
            {
                Paused = true;
            }
        }

        public void ResumeDraw()
        {
            if (Printing)
            {
                Paused = false;
            }
        }

        public void StopDraw()
        {
            if (Printing)
            {
                Stoped = true;
                // _SendNextStroke = false;
            }
        }

        private void _ChangeLayer()
        {
            _LayerActual = new StrokeSLT[_LayerCalculo.Count];

            for (int i = 0; i < _LayerCalculo.Count; i++)
            {
                _LayerActual[i] = _LayerCalculo[i];
            }

            _LayerCalculo = new List<StrokeSLT>();
        }

        double _AnguloLayer = 0.0;
        double _AperturaBoquilla = 0.2;
        double _GrosorShell = 2.0;
        double _PorcentajeRelleno = 0.25;

        
        private void _GenerateLayer()
        {
            //_GeneratingLayer = true;
            if (ZCalculo <= Solido.Top)
            {
                _LayerCalculo = new List<StrokeSLT>();

                IList<LineSLT> Corte = new List<LineSLT>();
                //obtiene cada uno de los segmentos y puntos aislados de la sección
                Solido.CortePlanoZ(ZCalculo, out Corte);

                //obtiene los polígonos e islas
                Poligonos TempPols = new Poligonos(Corte);


                IList<StrokeSLT> ResEq = TempPols.TrazarPerimetro(Modes.ModoRim);

                /*if (ResEq.Count == 0)
                {
                    Stroke stroke = new Stroke
                    {
                        Destino = new VertexSLT(0.0, 0.0, _ZCalculo),
                        E = 0.0,
                        Mode = Modes.ModeTraslation,
                        Pendiente = true
                    };

                    _LayerCalculo.Add(stroke);

                    stroke.Destino = new VertexSLT(10.0, 10.0, _ZCalculo);

                    _LayerCalculo.Add(stroke);
                }
                else
                {*/
                foreach (StrokeSLT t in ResEq)
                {
                    _LayerCalculo.Add(t);
                }
                //}



                //Shell en modo relleno
                /*IList<Trazo> TempShell = _GeneraShell(TempPols, _AperturaBoquilla, _GrosorShell);
                foreach (Trazo t in TempShell)
                {
                    _LayerCalculo.Add(t);
                }*/



                //Relleno
                /*IList<Trazo> TempRelleno = _GeneraRelleno(TempPols, _AperturaBoquilla, _PorcentajeRelleno, _AnguloLayer);
                foreach (Trazo t in TempRelleno)
                {
                    _LayerCalculo.Add(t);
                }*/


                //soportes
                //TODO

                //Layer terminada

                _AnguloLayer += Math.PI / 6.0;

                ZTrazado = ZCalculo;
                ZCalculo += DeltaLayer;

                //_GeneratingLayer = false;
            }
            else
            {
                _LayerCalculo = null;
            }
        }
        // TODO: analizar el uso de las capas de trabajo y cálculo y reprogramar el proceso, tener en cuenta que se pueden producir capas sin trazos, teneer en cuenta el bbox.z del poligono 
        private IList<StrokeSLT> _GenerateShell(Poligonos Pols, double AnchoBoquilla, double Grosor)
        {
            IList<StrokeSLT> Res = new List<StrokeSLT>();

            //borde exterior i=0 no se calcula como shell
            //bordes para formar el shell i<= int.Parse((Grosor/AnchoBoquilla).ToString())
            for (int i = 1; i * AnchoBoquilla < Grosor; i++)
            {
                //genero la equidistancia del polígono
                Poligonos PolsEq = Pols.Equidista(i * AnchoBoquilla);

                //Obtengo la ruta del polígono
                IList<StrokeSLT> ResEq = PolsEq.TrazarPerimetro(Modes.ModoFill);

                foreach (StrokeSLT t in ResEq)
                {
                    Res.Add(t);
                }
            }

            return Res;
        }

        private IList<StrokeSLT> _GenerateFill(Poligonos Pols, double AnchoBoquilla, double Porcentaje, double AnguloTrama)
        {
            IList<StrokeSLT> Res = new List<StrokeSLT>();

            if(Porcentaje > 1.0)
            {
                Porcentaje = 1.0;
            }

            if(Porcentaje < 0.0)
            {
                Porcentaje = 0.0;
            }

            double PasoTrama = Porcentaje * AnchoBoquilla;// /1 

            if (PasoTrama > 0.0)//omite PasoTrama inferior o igual a 0
            {
                //con el AnguloTrama y el PasoTrama intersectar con los poligonos
                //recorre el bounding box de los polígonos
                Punto Centro = Pols.Bownding.Centro;
                double SemiDiagonal = Pols.Bownding.Diagonal * 1.1 / 2.0;//con un 10% de margen

                double DeltaX = Convert.ToSingle(Math.Sin(Convert.ToDouble(AnguloTrama))) * SemiDiagonal;
                double DeltaY = Convert.ToSingle(Math.Sin((Math.PI / 2) - Convert.ToDouble(AnguloTrama))) * SemiDiagonal;

                //oriento el rayo
                Segmento Rayo = new Segmento(new Punto(Centro.X - DeltaX, Centro.Y - DeltaY, Centro.Z),
                                             new Punto(Centro.X + DeltaX, Centro.Y + DeltaY, Centro.Z));
                //desplazo al inicio
                Rayo = Rayo.Equidista(-SemiDiagonal);

                IList<Segmento> ResSegmentos = new List<Segmento>();

                bool InvierteTrazo = false;

                int Pasos = Convert.ToInt32(SemiDiagonal * 2.0 / PasoTrama);
                while (Pasos > 0)
                {
                    IList<Segmento> TempSegmentos = Pols.SegmentosInteriores(Rayo);

                    if (InvierteTrazo)
                    {
                        for (int i = TempSegmentos.Count - 1; i >= 0; i--)
                        {
                            StrokeSLT TT = new StrokeSLT();

                            //trazo de traslación
                            TT.Mode = Modes.ModeTraslation;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Final.X, TempSegmentos[i].Final.Y, TempSegmentos[i].Final.Z);

                            Res.Add(TT);

                            //trazo de relleno
                            TT.Mode = Modes.ModoFill;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Inicio.X, TempSegmentos[i].Inicio.Y, TempSegmentos[i].Inicio.Z);
                            //trazo de traslación
                            Res.Add(TT);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < TempSegmentos.Count; i++)
                        {
                            StrokeSLT TT = new StrokeSLT();

                            //trazo de traslación
                            TT.Mode = Modes.ModeTraslation;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Inicio.X, TempSegmentos[i].Inicio.Y, TempSegmentos[i].Inicio.Z);

                            Res.Add(TT);

                            //trazo de relleno
                            TT.Mode = Modes.ModoFill;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Final.X, TempSegmentos[i].Final.Y, TempSegmentos[i].Final.Z);
                            //trazo de traslación
                            Res.Add(TT);
                        }
                    }

                    //a la próxima la dirección es la contraria
                    InvierteTrazo = !InvierteTrazo;

                    //desplaza PasoTrama el rayo y lo recalcula
                    Rayo = Rayo.Equidista(PasoTrama);

                    //evalua si se ha salido del bounding box
                    Pasos--;
                }
            }
            return Res;
        }


        public StrokeSLT NextStroke()
        {
            StrokeSLT ResTrazo = null;

            // Only for cuit to next layer
            if(_LayerActual.Length == 0)
            {
                _ChangeLayer();

                _GenerateLayer();

                ResTrazo = new StrokeSLT();
                ResTrazo.Pendiente = true;
                ResTrazo.Mode = Modes.ModeTraslation;
                ResTrazo.Destino = new VertexSLT(0.0, 0.0, ZTrazado);

            }

            for (int i = 0; i < _LayerActual.Length; i++)
            {
                if(_LayerActual[i].Pendiente)
                {
                    _LayerActual[i].Pendiente = false;

                    if (i == (_LayerActual.Length - 1))
                    {
                        //Último trazo
                        StrokeSLT TempTrazo = new StrokeSLT();
                        TempTrazo.Pendiente = true;
                        TempTrazo.Mode = Modes.ModeTraslation;
                        TempTrazo.Destino = _LayerActual[i].Destino;

                        /*while (_GenerandoLayer)
                        {
                            System.Threading.Thread.Sleep(300);
                        }*/

                        if (_LayerCalculo == null)
                        {
                            //Ha terminado
                            ResTrazo = null;
                        }
                        else
                        {
                            //Carga y continúa con la siguiente capa
                            _ChangeLayer();

                            TempTrazo.Destino = _LayerActual[0].Destino;

                            _GenerateLayer();
                            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
                            _TGenerarLayer.Start();*/
                        }
                        ResTrazo = TempTrazo;
                    }
                    else
                    {
                        //Siguiente trazo
                        ResTrazo = _LayerActual[i + 1];
                    }
                    break;
                }
            }
            return ResTrazo;
        }

        #region interprete de comandos

        public bool SendStroke(StrokeSLT Trazo, double mmMaterial = 0.0)
        {
            bool Res = false;

            if (_IsTest)
            {
                mmMaterial = 0.0;
            }

            Trazo.E = mmMaterial;

            try
            {
                _serialPort.WriteLine(Trazo.ToPrinter());

                OnLog("SEND: " + Trazo.ToPrinter());

                Res = true;
            }
            catch (System.Exception sysEx)
            { 
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        public bool SendTemperature(double temperature = 100.0)
        {
            if (_IsTest)
            {
                return true;
            }

            return _SendTemperature(temperature.ToString("F1").Replace('.', ','));
        }

        private bool _SendTemperature(string temperature = "100,0")
        {
            bool Res = false;

            try
            {
                _serialPort.WriteLine("ST=" + temperature);

                OnLog("SEND: " + "ST=" + temperature);

                Res = true;
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        private static string _LastCommand = "";//not nullable, requires lock

        void _Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while(_serialPort.BytesToRead > 0)
            {
                char TempChar = Convert.ToChar(Convert.ToByte(_serialPort.ReadByte()));
                
                if (TempChar == '\n' || TempChar == '\r')
                {
                    //Interpreta el comando
                    if (_LastCommand != "" && ReadCommand())
                    {
                        _LastCommand = "";
                    }
                }
                else if (TempChar != 'Â')
                {
                    _LastCommand += TempChar;
                }
            }
        }

        Punto lastPoint = new Punto(0.0, 0.0, 0.0);
        private bool ReadCommand()
        {
            bool Executed = false;

            lock (_LastCommand)
            {
                if (_LastCommand != null && _LastCommand.Length > 3)
                {
                    string Tipo = _LastCommand.Substring(0, 3);
                    string Comando = _LastCommand.Substring(3);

                    OnLog("READ: " + Tipo + Comando);

                    Executed = true;

                    switch (Tipo)
                    {
                        case "XYZ":
                            //Muestra la posición
                            string[] temp = Comando.Split(';');
                            try
                            {
                                OnChangeXYZ(Convert.ToSingle(temp[0]), Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]));
                            }
                            catch (System.Exception)
                            {
                                OnChangeXYZ(double.NaN, double.NaN, double.NaN);
                            }
                            break;
                        case "INF":
                            //Información del funcionamiento
                            OnInformation(Comando);
                            break;
                        case "WRN":
                            //Advertencia
                            OnWarning(Comando);
                            break;
                        /*case "DON"://E
                            if (Comando == "E")
                            {
                                _SendNBextStroke = true;
                            }
                            OnLog("Comando: " + Tipo);
                            break;*/
                        /*case "WAI"://TING
                            if (Comando == "TING")
                            {
                                _SendNextStroke = true;
                            }
                            OnLog("Comando: " + Tipo);
                            break;*/
                        case "BFR":
                            if (Comando == "FULL")
                            {
                                // _SendNextStroke = false;
                            }
                            else if (Comando == "EMPTY")
                            {
                                // _SendNextStroke = true;

                                StrokeSLT strokeSLT = NextStroke();

                                if(IsTrazando && strokeSLT != null)
                                {
                                    double distance = lastPoint.Distancia(new Punto(strokeSLT.Destino.X, strokeSLT.Destino.Y, ZTrazado));
                                    lastPoint = new Punto(strokeSLT.Destino.X, strokeSLT.Destino.Y, ZTrazado);

                                    SendStroke(strokeSLT, distance);
                                }
                                else
                                {
                                    IsTrazando = false;
                                }
                            }
                            OnLog("Comando: " + Tipo+ Comando);
                            break;
                        default:
                            OnInformation("El tipo de comando '" + Tipo + "' con valor '" + Comando + "' no se reconoce.");
                            OnLog("Comando no reconocido: " + _LastCommand);
                            break;
                    }
                }
                else
                {
                    Executed = false;
                    OnLog("Comando re-encolado: " + _LastCommand);
                }
            }

            return Executed;
        }
        #endregion

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChangeXYZ(double X, double Y, double Z)
        {
            ChangeXYZ?.Invoke(X, Y, Z);
        }

        protected virtual void OnInformation(string Info)
        {
            Information?.Invoke(Info);
        }

        protected virtual void OnWarning(string Warn)
        {
            Warning?.Invoke(Warn);
        }

        protected virtual void OnLog(string Logging)
        {
            Log?.Invoke(Logging);
        }
        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event ChangedXYZEventHandler ChangeXYZ;
        public event InformationEventHandler Information;
        public event WarningEventHandler Warning;
        public event LogEventHandler Log;

    }
    // A delegate type for hooking up change notifications.
    public delegate void ChangedXYZEventHandler(double X,double Y,double Z);
    // A delegate type for hooking up change notifications.
    public delegate void InformationEventHandler(string Info);
    // A delegate type for hooking up change notifications.
    public delegate void WarningEventHandler(string Warning);
    // A delegate type for hooking up change notifications.
    public delegate void LogEventHandler(string Log);
}
