using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace SLT_Printer
{
    enum Modos { ModoTraslacion = 1, ModoRelleno = 2, ModoBorde = 4 }

    struct Trazo
    {
        public Modos Modo;
        //public VertexSLT Inicio;
        public VertexSLT Destino;
        public bool Pendiente;
        public double E;

        public string toPrinter()
        {
            string Res = "";
            if (double.IsNaN(E))
            {
                E = 0.0;
            }

            if (Destino.EsValido)
            {
                Res += "M =";
                Res += Convert.ToSByte(Modo).ToString();
                Res += "\n";

                Res += "X =";
                Res += Destino.X.ToString("0.000");
                Res += "\n";
                Res += "Y =";
                Res += Destino.Y.ToString("0.000");
                Res += "\n";
                Res += "Z =";
                Res += Destino.Z.ToString("0.000");
                Res += "\n";

                Res += "E =";
                Res += E.ToString("0.000");
                Res += "\n";

                Res += "RUN";
                Res += "\n";

                Res = Res.Replace(',', '.');
            }

            return Res;
        }
    }

    class Model
    {
        private double _ZTrazado = double.NaN;
        public double DeltaLayer = 0.25;
        private double _ZCalculo = 0.0;

        private Trazo[] _LayerActual;
        private IList<Trazo> _LayerCalculo;

        private bool _GenerandoLayer = false;
        private bool _EnPausa = false;//variable para PAUSAR la impresión
        private bool _Detenido = false;//variable para DETENER la impresión

        public bool Pausado
        {
            get
            {
                return _EnPausa;
            }
        }

        private bool Detenido
        {
            get
            {
                return _Detenido;
            }

            set
            {
                _Detenido = true;
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

        private Trazo _Trazo;

        public SolidoSLT Solido;

        private Thread _TImprimir;
        //private Thread _TGenerarLayer;

        private System.IO.Ports.SerialPort _Puerto;

        public Model(ref System.IO.Ports.SerialPort Puerto)
        {
            _Puerto = Puerto;
            _Puerto.DtrEnable = true;
            _Puerto.ReceivedBytesThreshold = 1;
            _Puerto.DataReceived += _Puerto_DataReceived;

            Solido = new SolidoSLT();

            Solido.Changed += Solido_Changed;
            _ZTrazado = double.NaN;
            DeltaLayer = 0.2;
            _ZCalculo = 0.0 ;

            _AnguloLayer = 0.0;

            _LayerActual = new Trazo[0];
            _LayerCalculo = new Trazo[0];

            _Imprimiendo = false;
            _EnPausa = false;
            _Detenido = false;

            //_TImprimir = new Thread(new ThreadStart(_Trazar));
            //_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
        }

        void Solido_Changed(object sender, EventArgs e)
        {
            if (_Imprimiendo)
            {
                Detenido = true;
            }
        }

        public double ZTrazado
        {
            get
            {
                return _ZTrazado;
            }
        }

        public double ZCalculo
        {
            get
            {
                return _ZCalculo;
            }
        }

        class ParejaVertices
        {
            public VertexSLT V1;
            public VertexSLT V2;
            public bool Procesado;

            public ParejaVertices()
            {
                V1 = new VertexSLT();
                V2 = new VertexSLT();
                Procesado = false;
            }
        }

        #region Comentado
        /*private IList<LoopSLT> _ShapesCorte(FacetSLT FacetCalculo)
        {
            IList<LoopSLT> Res = new List<LoopSLT>();

            if (FacetCalculo._Loops.Count > 0)
            {
                //Agrupa los Loop por continuidad para generar un LoopResultado con cada grupo
                IList<FacetSLT> GruposFacets = new List<FacetSLT>();//Cada uno de los polígonos cerrados
                FacetSLT TempFacet = new FacetSLT();//Faceta donde se van guardando cada uno de los Loop anexos
                //LoopSLT TestLoop = FacetCalculo._Loops[0];//Semilla para la búsqueda de anexos

                IList<VertexSLT> ListaVertices = new List<VertexSLT>();//Resumen de los vértices
                //foreach (VertexSLT V in FacetCalculo._Loops[0]._Vertex)
                {
                    //ListaVertices.Add(V);
                }

                IList<int> ListaCopiados = new List<int>();//Indices que ya hn sido tratados
                //ListaCopiados.Add(0);

                while (ListaCopiados.Count < FacetCalculo._Loops.Count)
                {

                    #region Bucle para encontrar los loops anexos
                    bool LoopSinEncontrar = true;
                    do
                    {
                        LoopSinEncontrar = true;

                        for (int i = 0; i < FacetCalculo._Loops.Count; i++)
                        {
                            if (ListaCopiados.Contains(i))
                            {
                                //ya fué analizado
                            }
                            else
                            {
                                //Si uno de los vértices coincide con uno guardado, añade el resto y el loop al TempFacet
                                bool Coincide = false;
                                foreach (VertexSLT V in FacetCalculo._Loops[i].Vertices)
                                {
                                    if (ListaVertices.Count == 0)
                                    {
                                        Coincide = true;//Ha encontrado un vértice de unión
                                        break;
                                    }
                                    else
                                    {
                                        foreach (VertexSLT VList in ListaVertices)
                                        {
                                            if (V.EsIgual(VList))
                                            {
                                                Coincide = true;//Ha encontrado un vértice de unión
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (Coincide)
                                {
                                    LoopSinEncontrar = false;

                                    ListaCopiados.Add(i);
                                    TempFacet._Loops.Add(FacetCalculo._Loops[i]);

                                    foreach (VertexSLT V1 in FacetCalculo._Loops[i].Vertices)
                                    {
                                        bool Existe = false;
                                        foreach (VertexSLT VList in ListaVertices)
                                        {
                                            if (V1.EsIgual(VList))
                                            {
                                                Existe = true;
                                                break;
                                            }
                                        }
                                        if (!Existe)
                                        {
                                            //Añado el vértice
                                            ListaVertices.Add(V1);
                                        }
                                    }
                                }

                            }
                        }
                    } while (!LoopSinEncontrar);
                    #endregion

                    GruposFacets.Add(TempFacet);
                    TempFacet = new FacetSLT();
                    ListaVertices = new List<VertexSLT>();
                }

                foreach(FacetSLT GFacet in GruposFacets)
                {
                    LoopSLT TempLoop = new LoopSLT();
                    IList<ParejaVertices> Parejas = new List<ParejaVertices>();

                    foreach (LoopSLT GLoop in GFacet._Loops)
                    {
                        IList<VertexSLT> TempResVertices = Interseccion(GLoop);//Obtengo el primer punto de intersección y el segundo
                        
                        //Almaceno la pareja para luego ordenarlos
                        if (TempResVertices.Count == 2)
                        {
                            ParejaVertices TempPareja = new ParejaVertices();
                            
                            TempPareja.V1 = TempResVertices[0];
                            TempPareja.V2 = TempResVertices[1];

                            //Elimino los duplicados
                            TempPareja.Procesado = TempPareja.V1.EsIgual(TempPareja.V2);

                            Parejas.Add(TempPareja);
                        }
                        else
                        {
                            //Ver cuantos puntos de intersección se han obtenido
                        }
                    }

                    //Ordeno las parejas de puntos
                    if(Parejas.Count>0)
                    {
                        if(Parejas.Count>1)
                        {
                            IList<VertexSLT> ListaAnt = new List<VertexSLT>();//se leerá en sentido contrario
                            IList<VertexSLT> ListaPost = new List<VertexSLT>();//se leerá en sentido directo

                            ListaAnt.Add(Parejas[0].V1);
                            ListaPost.Add(Parejas[0].V2);

                            bool Procesado;
                            do
                            {
                                Procesado = false;
                                for (int i = 1; i < Parejas.Count; i++)
                                {
                                    if (!Parejas[i].Procesado)
                                    {
                                        //ordena los vértices por igualdad de uno de los vértices
                                        if (Parejas[i].V1.EsIgual(ListaAnt[ListaAnt.Count - 1]))
                                        {
                                            ListaAnt.Add(Parejas[i].V2);
                                            Parejas[i].Procesado = true;
                                        }
                                        else if (Parejas[i].V2.EsIgual(ListaAnt[ListaAnt.Count - 1]))
                                        {
                                            ListaAnt.Add(Parejas[i].V1);
                                            Parejas[i].Procesado = true;
                                        }
                                        else if (Parejas[i].V1.EsIgual(ListaPost[ListaPost.Count - 1]))
                                        {
                                            ListaPost.Add(Parejas[i].V2);
                                            Parejas[i].Procesado = true;
                                        }
                                        else if (Parejas[i].V2.EsIgual(ListaPost[ListaPost.Count - 1]))
                                        {
                                            ListaPost.Add(Parejas[i].V1);
                                            Parejas[i].Procesado = true;
                                        }

                                        if(Parejas[i].Procesado)
                                        {
                                            Procesado = true;
                                        }
                                    }
                                }
                            } while (Procesado);

                            //Almaceno los puntos sin duplicados a excepción del primero y el último
                            for (int i = ListaAnt.Count - 1; i >= 0; i--)
                            {
                                TempLoop.Vertices.Add(ListaAnt[i]);
                            }
                            for (int i = 0; i < ListaPost.Count; i++)
                            {
                                TempLoop.Vertices.Add(ListaPost[i]);
                            }
                        }
                        else
                        {
                            TempLoop.Vertices.Add(Parejas[0].V1);
                            TempLoop.Vertices.Add(Parejas[0].V2);
                        }
                    }

                    if (TempLoop.Vertices.Count > 0)
                    {
                        Res.Add(TempLoop);
                    }
                }
            }
            return Res;
        }*/
#endregion

        private IList<VertexSLT> Interseccion(LoopSLT Loop)
        {
            IList<VertexSLT> Res = new List<VertexSLT>();

            VertexSLT TempVertex = new VertexSLT();
            bool IntInterna = false;
            for (int i = 1; i < Loop.Vertices.Count; i++)
            {
                TempVertex = new VertexSLT();
                IntInterna = false;
                if (Interseccion(Loop.Vertices[i - 1], Loop.Vertices[i], out TempVertex, out IntInterna))
                {
                    if(IntInterna)
                    {
                        Res.Add(TempVertex);
                    }
                }
            }

            TempVertex = new VertexSLT();
            IntInterna = false;
            if (Interseccion(Loop.Vertices[0], Loop.Vertices[Loop.Vertices.Count - 1], out TempVertex, out IntInterna))
            {
                if (IntInterna)
                {
                    Res.Add(TempVertex);
                }
            }

            return Res;
        }

        private bool Interseccion(VertexSLT V1, VertexSLT V2, out VertexSLT Interseccion, out bool Interna)
        {
            bool Res = false;
            
            try
            {
                Interseccion = new VertexSLT();
                Interna=false;

                VertexSLT Direccion = new VertexSLT(V2.X - V1.X, V2.Y - V1.Y, V2.Z - V1.Z);

                VertexSLT NormalPlanoZ = new VertexSLT(0.0, 0.0, 1.0);
                double D = -_ZCalculo;

                double t = ((NormalPlanoZ.X * V1.X) + (NormalPlanoZ.Y * V1.Y) + (NormalPlanoZ.Z * V1.Z) + D)
                / (-(NormalPlanoZ.X * Direccion.X) - (NormalPlanoZ.Y * Direccion.Y) - (NormalPlanoZ.Z * Direccion.Z));

                Interseccion = new VertexSLT(V1.X + t * Direccion.X, V1.Y + t * Direccion.Y, V1.Z + t * Direccion.Z);

                Res = true;

                if(0.0 <= t && t <= 1.0)
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

        #region Clase Recta comentada
        /*class Recta
        {
            public double m;
            public double b;

            public Recta()
            {
                m = 0.0;
                b = 0.0;
            }
            public Recta(double M, double B)
            {
                m = M;
                b = B;
            }
            public Recta(VertexSLT R1, VertexSLT R2)
            {
                try
                {
                    m = (R2.Y - R1.Y) / (R2.X - R1.X);
                    b = R1.Y - m * R1.X;
                }
                catch(System.Exception )
                {
                    m = double.PositiveInfinity;
                    b = R1.X;
                }
            }

            public Recta(double Radianes, VertexSLT P1)
            {
                try
                {
                    m = Convert.ToSingle(Math.Tan(Radianes));
                    b = P1.Y - m * P1.X;
                }
                catch(System.Exception)
                {
                    m = double.PositiveInfinity;
                    b = P1.X;
                }
            }

            public VertexSLT Intersecta2D(Recta R2, double Z = 0.0)
            {
                VertexSLT Res = new VertexSLT();

                try
                {
                    if (double.IsPositiveInfinity(m))
                    {
                        if (double.IsPositiveInfinity(R2.m))
                        {
                            Res = new VertexSLT();
                        }
                        else
                        {
                            Res = new VertexSLT(b, R2.m * b + R2.b, Z);
                        }
                    }
                    else if (double.IsPositiveInfinity(R2.m))
                    {
                        Res = new VertexSLT(R2.b, m * R2.b + b, Z);
                    }
                    else
                    {
                        if(R2.m == m)
                        {
                            Res = new VertexSLT();
                        }
                        else
                        {
                        double TempX = (b - R2.b) / (R2.m - m);
                        double TempY = m * TempX - b;
                        Res = new VertexSLT(TempX, TempY, Z);
                        }
                    }

                }
                catch (System.Exception)
                { 
                    Res = new VertexSLT(); 
                }

                return Res;
            }

            public void DesplazaRecta(double Radianes, double Traslacion)
            {
                if (Radianes == Convert.ToSingle(Math.PI / 2))
                {
                    b += Traslacion;
                }
                else
                {
                    b += Traslacion / Convert.ToSingle(Math.Sin(Convert.ToSingle(Math.PI / 2) - Radianes));
                }
            }
        }

        private int Interseccion(Recta R, LoopSLT Poligono, out IList<VertexSLT> Intersecciones)
        {
            int Res = 0;

            Intersecciones = new List<VertexSLT>();

            for (int i = 1; i < Poligono.Vertices.Count; i++)
            {
                VertexSLT V1 = Poligono.Vertices[i - 1];
                VertexSLT V2 = Poligono.Vertices[i];

                Recta R2 = new Recta(V1, V2);
                VertexSLT TempInterseccion = R.Intersecta2D(R2, V1.Z);

                if(TempInterseccion.EsValido)
                {
                    if (Math.Min(V1.X, V2.X) <= TempInterseccion.X && TempInterseccion.X <= Math.Max(V1.X, V2.X)
                        && Math.Min(V1.Y, V2.Y) <= TempInterseccion.Y && TempInterseccion.Y <= Math.Max(V1.Y, V2.Y))
                    {
                        Intersecciones.Add(TempInterseccion);
                        Res++;
                    }
                }
            }

            return Res;
        }*/
#endregion

        private bool _EnviaSigTrazo = false;
        private bool _Imprimiendo = false;

        public bool Imprimiendo
        {
            get
            {
                return _Imprimiendo;
            }
        }

        public void Trazar()
        {
            
            if(_TImprimir == null || !_TImprimir.IsAlive)
            {
                _TImprimir = new Thread(new ThreadStart(_Trazar));
                _TImprimir.Start();
            }
            else
            {
                //avisa de que ya está en ejecución
                System.Windows.Forms.MessageBox.Show("Proceso de impresión ejecutándose.");
            }
        }

        private void _Trazar()
        {
            _Imprimiendo = true;

            _AnguloLayer = 0.0;
            _ZCalculo = 0.0;

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();
            _GenerandoLayer = true;*/

            EnviaTemp("341");

            _GeneraLayer();

            /*while(_GenerandoLayer )
            {
                System.Threading.Thread.Sleep(500);
            }*/

            _CambioLayer();

            /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
            _TGenerarLayer.Start();*/

            _GeneraLayer();


            if (_LayerActual.Count() > 0)
            {
                _Trazo = new Trazo();
                _Trazo.Destino = _LayerActual[0].Destino;
                _Trazo.Modo = Modos.ModoTraslacion;
                _Trazo.Pendiente = true;

                //Envía el trazo
                EnviaTrazo(ref _Trazo);

                //Para calcular la distancia del trazado
                Punto AntDest = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);

                bool AnteriorTrazado = true;

                while (AnteriorTrazado)
                {
                    AnteriorTrazado = SiguienteTrazo();//lo almacena en _trazo

                    //Envía los sucesivos trazos
                    while(!_EnviaSigTrazo)
                    {
                        //hace tiempo hasta que termina de enviar el comando
                        Thread.Sleep(30);
                    }

                    if (EnviaTrazo(ref _Trazo, AntDest.Distancia(new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z))))
                    {
                        AntDest = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);
                    }

                    if (Detenido)
                    {
                        if (_GenerandoLayer)
                        {
                            //_TGenerarLayer.Abort();
                            _GenerandoLayer = false;
                        }
                        _ZTrazado = double.NaN;
                        break;
                    }

                    while (_EnPausa)
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                }
            }

            EnviaTemp();

            _ZTrazado = double.NaN;
            _EnPausa = false;
            _Detenido = false;

            //Fin del trazado
            _Imprimiendo = false;

            //presenta el trazado
            Trazo trzPresent = new Trazo();
            trzPresent.Destino = new VertexSLT(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z + 50.0/*mm*/);
            trzPresent.E = 0.0;
            trzPresent.Modo = Modos.ModoTraslacion;
            trzPresent.Pendiente = true;
            EnviaTrazo(ref trzPresent);

            trzPresent.Destino = new VertexSLT(0.0, 0.0, _Trazo.Destino.Z + 50.0);
            trzPresent.E = 0.0;
            trzPresent.Modo = Modos.ModoTraslacion;
            trzPresent.Pendiente = true;
            EnviaTrazo(ref trzPresent);
        }

        public void PausarTrazado()
        {
            if (_Imprimiendo)
            {
                _EnPausa = true;
            }
        }

        public void ReanudarTrazado()
        {
            if (_Imprimiendo)
            {
                _EnPausa = false;
            }
        }

        public void DetenerTrazado()
        {
            if (_Imprimiendo)
            {
                Detenido = true;
                _EnviaSigTrazo = false;
            }
        }

        private void _CambioLayer()
        {
            _LayerActual = new Trazo[_LayerCalculo.Count];

            for (int i = 0; i < _LayerCalculo.Count; i++)
            {
                _LayerActual[i] = _LayerCalculo[i];
                if (!_LayerActual[i].Pendiente)
                {
                    _LayerActual[i].Pendiente = true;
                }
            }

            _LayerCalculo = new List<Trazo>();
        }

        double _AnguloLayer = 0.0;
        /*double _AperturaBoquilla = 0.2;
        double _GrosorShell = 2.0;
        double _PorcentajeRelleno = 0.25;*/

        
        private void _GeneraLayer()
        {
            _GenerandoLayer = true;

            _LayerCalculo = new List<Trazo>();

            IList<RectaSLT> Corte = new List<RectaSLT>();
            //obtiene cada uno de los segmentos y puntos aislados de la sección
            Solido.CortePlanoZ(_ZCalculo, out Corte);
            
            //obtiene los polígonos e islas
            Poligonos TempPols = new Poligonos(Corte);

            
            IList<Trazo> ResEq = TempPols.TrazarPerimetro(Modos.ModoBorde);
            foreach (Trazo t in ResEq)
            {
                _LayerCalculo.Add(t);
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

            _GenerandoLayer = false;
        }

        private IList<Trazo> _GeneraShell(Poligonos Pols, double AnchoBoquilla, double Grosor)
        {
            IList<Trazo> Res = new List<Trazo>();

            //borde exterior i=0 no se calcula como shell
            //bordes para formar el shell i<= int.Parse((Grosor/AnchoBoquilla).ToString())
            for (int i = 1; i * AnchoBoquilla < Grosor; i++)
            {
                //genero la equidistancia del polígono
                Poligonos PolsEq = Pols.Equidista(i * AnchoBoquilla);

                //Obtengo la ruta del polígono
                IList<Trazo> ResEq = PolsEq.TrazarPerimetro(Modos.ModoRelleno);

                foreach (Trazo t in ResEq)
                {
                    Res.Add(t);
                }
            }

            return Res;
        }

        private IList<Trazo> _GeneraRelleno(Poligonos Pols, double AnchoBoquilla, double Porcentaje, double AnguloTrama)
        {
            IList<Trazo> Res = new List<Trazo>();

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
                            Trazo TT = new Trazo();

                            //trazo de traslación
                            TT.Modo = Modos.ModoTraslacion;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Final.X, TempSegmentos[i].Final.Y, TempSegmentos[i].Final.Z);

                            Res.Add(TT);

                            //trazo de relleno
                            TT.Modo = Modos.ModoRelleno;
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
                            Trazo TT = new Trazo();

                            //trazo de traslación
                            TT.Modo = Modos.ModoTraslacion;
                            TT.Pendiente = true;
                            TT.Destino = new VertexSLT(TempSegmentos[i].Inicio.X, TempSegmentos[i].Inicio.Y, TempSegmentos[i].Inicio.Z);

                            Res.Add(TT);

                            //trazo de relleno
                            TT.Modo = Modos.ModoRelleno;
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

        public Trazo TrazoActual
        {
            get
            {
                return _Trazo;
            }
        }


        public bool SiguienteTrazo()
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
                        Trazo TempTrazo = new Trazo();
                        TempTrazo.Pendiente = true;
                        TempTrazo.Modo = Modos.ModoTraslacion;
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
                            _CambioLayer();

                            TempTrazo.Destino = _LayerActual[0].Destino;

                            _GeneraLayer();
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

        public bool EnviaTrazo(ref Trazo Trazo, double mmMaterial = 0.0)
        {
            bool Res = false;

            Trazo.E = mmMaterial;

            try
            {
                _Puerto.WriteLine(Trazo.toPrinter());
                _EnviaSigTrazo = false;
                Trazo.Pendiente = false;
                Res = true;
            }
            catch (System.Exception sysEx)
            { 
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        public bool EnviaTemp(string Temperatura = "100,0")
        {
            bool Res = false;

            try
            {
                _Puerto.WriteLine("ST=" + Temperatura);
                Res = true;
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        string _UltimoComando = "";

        void _Puerto_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while(_Puerto.BytesToRead > 0)
            {
                char TempChar = Convert.ToChar(Convert.ToByte(_Puerto.ReadByte()));
                if (TempChar == '\n' || TempChar == '\r')
                {
                    //Interpreta el comando
                    InterpretaComando();
                    _UltimoComando = "";
                }
                else
                {
                    _UltimoComando += TempChar;
                }
                
            }
        }

        private void InterpretaComando()
        {
            if (_UltimoComando != null && _UltimoComando.Length > 3)
            {
                string Tipo = _UltimoComando.Substring(0, 3);
                string Comando = _UltimoComando.Substring(3);

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
                    case "DON":
                        _EnviaSigTrazo = true;
                        break;
                    default:
                        OnInformation(Tipo + Comando);
                        break;
                }
            }
        }
        #endregion

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChangeXYZ(double X, double Y, double Z)
        {
            ChangedXYZEventHandler _ChangeXYZ = ChangeXYZ;
            if (_ChangeXYZ != null)
                _ChangeXYZ(X, Y, Z);
        }

        protected virtual void OnInformation(string Info)
        {
            InformationEventHandler _Information = Information;
            if (_Information != null)
                _Information(Info);
        }

        protected virtual void OnWarning(string Warn)
        {
            WarningEventHandler _Warning = Warning;
            if (_Warning != null)
                _Warning(Warn);
        }
        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event ChangedXYZEventHandler ChangeXYZ;
        public event InformationEventHandler Information;
        public event WarningEventHandler Warning;

    }
    // A delegate type for hooking up change notifications.
    public delegate void ChangedXYZEventHandler(double X,double Y,double Z);
    // A delegate type for hooking up change notifications.
    public delegate void InformationEventHandler(string Info);
    // A delegate type for hooking up change notifications.
    public delegate void WarningEventHandler(string Warning);
}
