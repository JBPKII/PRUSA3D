using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SLT_Printer.SLT
{
    enum Modes { ModeTraslation = 1, ModoFill = 2, ModoRim = 4 }

    class ModelSLT
    {
        private double _ZTrazado = double.NaN;
        public double DeltaLayer = 0.25;
        private double _ZCalculo = 0.0;

        private Stroke[] _LayerActual;
        private IList<Stroke> _LayerCalculo;

        private bool _GeneratingLayer = false;
        private bool _Paused = false;//variable para PAUSAR la impresión
        private bool _Stoped = false;//variable para DETENER la impresión

        public bool Paused
        {
            get
            {
                return _Paused;
            }
        }

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

        private Stroke _Trazo;

        public SolidSLT Solido;

        private Thread _TImprimir;
        //private Thread _TGenerarLayer;

        private System.IO.Ports.SerialPort _serialPort;

        public ModelSLT(ref System.IO.Ports.SerialPort serialPort)
        {
            _serialPort = serialPort;
            _serialPort.DtrEnable = true;
            _serialPort.ReceivedBytesThreshold = 1;
            _serialPort.DataReceived += _Port_DataReceived;

            Solido = new SolidSLT();

            Solido.Changed += Solido_Changed;
            _ZTrazado = double.NaN;
            DeltaLayer = 0.2;
            _ZCalculo = 0.0 ;

            _AnguloLayer = 0.0;

            _LayerActual = new Stroke[0];
            _LayerCalculo = new Stroke[0];

            _Printing = false;
            _Paused = false;
            _Stoped = false;

            //_TImprimir = new Thread(new ThreadStart(_Trazar));
            //_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
        }

        void Solido_Changed(object sender, EventArgs e)
        {
            if (_Printing)
            {
                Stoped = true;
            }
        }

        public double ZTrazado
        {
            get
            {
                return _ZTrazado;
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
                double D = -_ZCalculo;

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

        public double ZCalculo
        {
            get
            {
                return _ZCalculo;
            }
        }

        private bool _SendNBextStroke = true;
        private bool _Printing = false;

        public bool Printing
        {
            get
            {
                return _Printing;
            }
        }

        public void Draw()
        {
            if(_TImprimir == null || !_TImprimir.IsAlive)
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

        private void _Draw()
        {
            _Printing = true;

            _AnguloLayer = 0.0;
            _ZCalculo = 0.0;

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();
            _GenerandoLayer = true;*/

            SendTemperature("160");

            _GenerateLayer();

            /*while(_GenerandoLayer )
            {
                System.Threading.Thread.Sleep(500);
            }*/

            _ChangeLayer();

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();*/

            _GenerateLayer();


            if (_ZCalculo <= Solido.Top)
            {
                _Trazo = new Stroke();

                if (_LayerActual.Count() > 0)
                {
                    _Trazo.Destino = _LayerActual[0].Destino;
                }
                else
                {
                    _Trazo.Destino = new VertexSLT(0.0, 0.0, _ZCalculo);
                }

                _Trazo.Mode = Modes.ModeTraslation;
                _Trazo.Pendiente = true;


                //Envía el trazo
                SendStroke(ref _Trazo);

                //Para calcular la distancia del trazado
                Punto AntDest = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);

                bool AnteriorTrazado = true;

                while (AnteriorTrazado)
                {
                    AnteriorTrazado = NextStroke();//lo almacena en _trazo

                    //Envía los sucesivos trazos
                    while(!_SendNBextStroke)
                    {
                        //hace tiempo hasta que termina de enviar el comando
                        Thread.Sleep(30);
                    }

                    if (SendStroke(ref _Trazo, AntDest.Distancia(new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, AntDest.Z))))
                    {
                        AntDest = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);
                    }

                    //_SendNBextStroke = false;

                    if (Stoped)
                    {
                        if (_GeneratingLayer)
                        {
                            //_TGenerarLayer.Abort();
                            _GeneratingLayer = false;
                        }
                        _ZTrazado = double.NaN;
                        break;
                    }

                    while (_Paused)
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                }
            }

            SendTemperature();

            _ZTrazado = double.NaN;
            _Paused = false;
            _Stoped = false;

            //Fin del trazado
            _Printing = false;

            //presenta el trazado
            Stroke trzPresent = new Stroke();
            trzPresent.Destino = new VertexSLT(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z + 50.0/*mm*/);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            SendStroke(ref trzPresent);

            trzPresent.Destino = new VertexSLT(0.0, 0.0, _Trazo.Destino.Z + 50.0);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            SendStroke(ref trzPresent);
        }

        public void PauseDraw()
        {
            if (_Printing)
            {
                _Paused = true;
            }
        }

        public void ResumeDraw()
        {
            if (_Printing)
            {
                _Paused = false;
            }
        }

        public void StopDraw()
        {
            if (_Printing)
            {
                Stoped = true;
                _SendNBextStroke = false;
            }
        }

        private void _ChangeLayer()
        {
            _LayerActual = new Stroke[_LayerCalculo.Count];

            for (int i = 0; i < _LayerCalculo.Count; i++)
            {
                _LayerActual[i] = _LayerCalculo[i];
                if (!_LayerActual[i].Pendiente)
                {
                    _LayerActual[i].Pendiente = true;
                }
            }

            _LayerCalculo = new List<Stroke>();
        }

        double _AnguloLayer = 0.0;
        double _AperturaBoquilla = 0.2;
        double _GrosorShell = 2.0;
        double _PorcentajeRelleno = 0.25;

        
        private void _GenerateLayer()
        {
            _GeneratingLayer = true;

            _LayerCalculo = new List<Stroke>();

            IList<LineSLT> Corte = new List<LineSLT>();
            //obtiene cada uno de los segmentos y puntos aislados de la sección
            Solido.CortePlanoZ(_ZCalculo, out Corte);
            
            //obtiene los polígonos e islas
            Poligonos TempPols = new Poligonos(Corte);


            IList<Stroke> ResEq = TempPols.TrazarPerimetro(Modes.ModoRim);

            if (ResEq.Count == 0)
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
            {
                foreach (Stroke t in ResEq)
                {
                    _LayerCalculo.Add(t);
                }
            }



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

            _ZTrazado = _ZCalculo;
            _ZCalculo += DeltaLayer;

            _GeneratingLayer = false;
        }

        private IList<Stroke> _GenerateShell(Poligonos Pols, double AnchoBoquilla, double Grosor)
        {
            IList<Stroke> Res = new List<Stroke>();

            //borde exterior i=0 no se calcula como shell
            //bordes para formar el shell i<= int.Parse((Grosor/AnchoBoquilla).ToString())
            for (int i = 1; i * AnchoBoquilla < Grosor; i++)
            {
                //genero la equidistancia del polígono
                Poligonos PolsEq = Pols.Equidista(i * AnchoBoquilla);

                //Obtengo la ruta del polígono
                IList<Stroke> ResEq = PolsEq.TrazarPerimetro(Modes.ModoFill);

                foreach (Stroke t in ResEq)
                {
                    Res.Add(t);
                }
            }

            return Res;
        }

        private IList<Stroke> _GenerateFill(Poligonos Pols, double AnchoBoquilla, double Porcentaje, double AnguloTrama)
        {
            IList<Stroke> Res = new List<Stroke>();

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
                            Stroke TT = new Stroke();

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
                            Stroke TT = new Stroke();

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

        public Stroke TrazoActual
        {
            get
            {
                return _Trazo;
            }
        }


        public bool NextStroke()
        {
            bool Res = true;
            for (int i = 0; i < _LayerActual.Length; i++)
            {
                if(_LayerActual[i].Pendiente)
                {
                    _LayerActual[i].Pendiente = false;
                    if(i==(_LayerActual.Length-1))
                    {
                        //Último trazo
                        Stroke TempTrazo = new Stroke();
                        TempTrazo.Pendiente = true;
                        TempTrazo.Mode = Modes.ModeTraslation;
                        TempTrazo.Destino = _LayerActual[i].Destino;

                        /*while (_GenerandoLayer)
                        {
                            System.Threading.Thread.Sleep(300);
                        }*/

                        if (_LayerCalculo.Count == 0)
                        {
                            //Ha terminado
                            Res = false;
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
                        _Trazo = TempTrazo;
                    }
                    else
                    {
                        //Siguiente trazo
                        _Trazo = _LayerActual[i + 1];
                    }
                    break;
                }
            }
            return Res;
        }

        #region interprete de comandos

        public bool SendStroke(ref Stroke Trazo, double mmMaterial = 0.0)
        {
            bool Res = false;

            Trazo.E = mmMaterial;

            try
            {
                _serialPort.WriteLine(Trazo.ToPrinter());

                OnLog("SEND: " + Trazo.ToPrinter());

                Trazo.Pendiente = false;
                Res = true;
            }
            catch (System.Exception sysEx)
            { 
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        public bool SendTemperature(string temperature = "100,0")
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

        string _LastCommand = "";

        void _Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while(_serialPort.BytesToRead > 0)
            {
                char TempChar = Convert.ToChar(Convert.ToByte(_serialPort.ReadByte()));
                if (TempChar == '\n' || TempChar == '\r')
                {
                    //Interpreta el comando
                    if (_LastCommand != "")
                    {
                        ReadCommand();
                    }
                    _LastCommand = "";
                }
                else
                {
                    _LastCommand += TempChar;
                }
                
            }
        }

        private void ReadCommand()
        {
            if (_LastCommand != null && _LastCommand.Length > 3)
            {
                string Tipo = _LastCommand.Substring(0, 3);
                string Comando = _LastCommand.Substring(3);

                switch (Tipo)
                {
                    case "XYZ":
                        //Muestra la posición
                        string[] temp = Comando.Split(';');
                        try
                        {
                            OnChangeXYZ(Convert.ToSingle(temp[0].Replace('.',',')), Convert.ToSingle(temp[1].Replace('.', ',')), Convert.ToSingle(temp[2].Replace('.',',')));
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
                    case "DON"://E
                        if (Comando == "E")
                        {
                            _SendNBextStroke = true;
                        }
                        OnLog("Comando: " + Tipo);
                        break;
                    case "WAI"://TING
                        if (Comando == "TING")
                        {
                            _SendNBextStroke = true;
                        }
                        OnLog("Comando: " + Tipo);
                        break;
                    case "BFR":
                        _SendNBextStroke = false;
                        OnLog("Comando: " + Tipo);
                        break;
                    default:
                        OnInformation("El tipo de comando '" + Tipo + "' con valor '" + Comando + "' no se reconoce.");
                        OnLog("Comando no reconocido: " + _LastCommand);
                        break;
                }
            }
            else
            {
                OnLog("Comando descartado: " + _LastCommand);
            }
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
