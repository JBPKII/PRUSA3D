using SLT_Printer.SLT;
using System.Collections.Generic;
using System.Linq;

namespace SLT_Printer
{
    class Poligonos
    {
        IList<Poligono> _Poligonos;
        BBox _Bownding;

        private void _Inicializa()
        {
            _Poligonos = new List<Poligono>();
            _Bownding = new BBox();
        }

        public Poligonos()
        {
            _Inicializa();
        }

        public Poligonos(IList<LineSLT> Bordes)
        {
            _Inicializa();

            //obtengo cada uno de los distintos polígonos cerrados
            IList<Shape> Pols = Shape.GenerarPoligonos(Bordes);

            IList<bool> Usados = new List<bool>(Pols.Count);
            for (int t = 0; t < Pols.Count; t++)
            {
                Usados.Add(false);
            }

            //veo que polígonos no están contenidos por otros
            for (int S = 0; S < Pols.Count; S++)
            {
                bool Contenido = false;
                for (int s = 0; s < Pols.Count; s++)
                {
                    if (S != s && Pols[s].ContieneA(Pols[S]))
                    {
                        Contenido = true;
                        break;
                    }
                }

                if (!Contenido)
                {
                    //he encontrado un borde exterior
                    Poligono TempPol = new Poligono(Pols[S]);
                    Usados[S] = true;

                    //busco sus islas
                    for (int i = 0; i < Pols.Count; i++)
                    {
                        if (S != i && Pols[S].ContieneA(Pols[i]))
                        {
                            TempPol.AddIsla(Pols[i]);
                            Usados[i] = true;
                        }
                    }

                    this._Poligonos.Add(TempPol);
                    _Bownding.Actualizar(TempPol.Bownding.Maximos);
                    _Bownding.Actualizar(TempPol.Bownding.Minimos);
                }
            }

            for (int r = 0; r < Pols.Count; r++)
            {
                if (!Usados[r])
                {
                    Poligono restPol = new Poligono(Pols[r]);
                    this._Poligonos.Add(restPol);
                    _Bownding.Actualizar(restPol.Bownding.Maximos);
                    _Bownding.Actualizar(restPol.Bownding.Minimos);
                }
            }
        }

        public BBox Bownding
        {
            get
            {
                return _Bownding;
            }
        }

        public void Add(Poligono Poligono)
        {
            _Poligonos.Add(Poligono);
            _Bownding.Actualizar(Poligono.Bownding.Maximos);
            _Bownding.Actualizar(Poligono.Bownding.Minimos);
        }

        public void Add(IList<Poligono> Poligonos)
        {
            foreach (Poligono p in Poligonos)
            {
                _Poligonos.Add(p);
                _Bownding.Actualizar(p.Bownding.Maximos);
                _Bownding.Actualizar(p.Bownding.Minimos);
            }
        }

        public IList<Segmento> SegmentosInteriores(Segmento Rayo)
        {
            IList<Segmento> Res = new List<Segmento>();

            foreach (Poligono P in _Poligonos)
            {
                IList<Segmento> TempRes = P.SegmentosInteriores(Rayo);
                foreach (Segmento s in TempRes)
                {
                    Res.Add(s);
                }
            }

            return Res;
        }

        public IList<Stroke> TrazarPerimetro(Modes M)
        {
            IList<Stroke> Res = new List<Stroke>();

            foreach (Poligono P in _Poligonos)
            {
                IList<Stroke> ResP = P.TrazarPerimetro(M);
                foreach (Stroke tP in ResP)
                {
                    Res.Add(tP);
                }
            }

            return Res;
        }

        public Poligonos Equidista(double Distancia)
        {
            //retorna la equidistancia
            Poligonos Res = new Poligonos();
            //eqidistancia con signo, así las islas equidistan en el sentido correcto

            foreach (Poligono P in _Poligonos)
            {
                Res.Add(P.Equidista(Distancia));
            }

            return Res;
        }
    }

    class Poligono
    {
        Shape _Exterior;
        IList<Shape> _Islas;
        double _area;
        BBox _Bownding;

        private void _Inicializa()
        {
            _Exterior = new Shape();
            _Islas = new List<Shape>();
            _area = double.NaN;
            _Bownding = new BBox();
        }

        public Poligono()
        {
            _Inicializa();
        }

        public Poligono(Shape S)
        {
            _Inicializa();
            _Exterior = S;
            _Bownding = new BBox(S.Bownding.Minimos, S.Bownding.Maximos);
        }

        public Poligono(IList<LineSLT> Bordes)
        {
            _Inicializa();
            //obtengo cada uno de los distintos polígonos cerrados
            IList<Shape> Pols = Shape.GenerarPoligonos(Bordes);

            //Ordeno por superficie y me quedo con el mayor para ser el exterior
            Pols = Pols.OrderByDescending(p => p.Superficie).ToList<Shape>();

            if (Pols.Count > 0)//por lo menos tiene el exterior
            {
                _Exterior = Pols[0];

                if (Pols.Count > 1)//Contiene islas
                {
                    for (int i = 1; i < Pols.Count; i++)
                    {
                        _Islas.Add(Pols[i]);
                    }
                }
            }

            //Normalizo los polígonos
            if (_Exterior.Superficie < 0)
            {
                _Exterior = _Exterior.Inverso;
            }
            _Bownding.Actualizar(_Exterior.Bownding.Maximos);
            _Bownding.Actualizar(_Exterior.Bownding.Minimos);

            for (int i = 0; i < _Islas.Count; i++)
            {
                if (_Islas[i].Superficie > 0)
                {
                    _Islas[i] = _Islas[i].Inverso;
                }
                _Bownding.Actualizar(_Islas[i].Bownding.Maximos);
                _Bownding.Actualizar(_Islas[i].Bownding.Minimos);
            }
        }

        public BBox Bownding
        {
            get
            {
                return _Bownding;
            }
        }

        public void AddIsla(Shape Isla)
        {
            if (Isla.Superficie > 0)
            {
                _Islas.Add(Isla.Inverso);
            }
            else
            {
                _Islas.Add(Isla);
            }

            _Bownding.Actualizar(Isla.Bownding.Maximos);
            _Bownding.Actualizar(Isla.Bownding.Minimos);
        }

        public double Superficie
        {
            get
            {
                if (double.IsNaN(_area))//solo lo calcula si no se ha calculado antes
                {
                    _area = _Exterior.Superficie;
                    foreach(Shape Is in _Islas)
                    {
                        _area += Is.Superficie;
                    }
                }

                return _area;
            }
        }

        public IList<Segmento> SegmentosInteriores(Segmento Rayo)
        {
            IList<Segmento> Res = new List<Segmento>();

            Res = _Exterior.SegmentosInteriores(Rayo, false);

            if (Res.Count > 0)//si no corta el exterior tampoco a las islas
            {
                foreach (Shape Isla in _Islas)//por cada isla compara con todos los segmentos
                {
                    for (int i = 0; i < Res.Count; i++)
                    {
                        IList<Segmento> ResIsla = new List<Segmento>();

                        ResIsla = Isla.SegmentosInteriores(Res[i], true);

                        //reemplaza el segmento interior por el nuevo si han producido intersecciones
                        if (ResIsla.Count > 0)
                        {
                            Res.RemoveAt(i);

                            for (int j = 0; j < ResIsla.Count; j++)
                            {
                                Res.Insert(i + j, ResIsla[i]);
                            }
                        }
                    }
                }
            }

            return Res;
        }

        public IList<Stroke> TrazarPerimetro(Modes M)
        {
            IList<Stroke> Res = new List<Stroke>();

            IList<Stroke> ResExt = _Exterior.TrazarPerimetro(M);
            foreach (Stroke tE in ResExt)
            {
                Res.Add(tE);
            }

            foreach (Shape S in _Islas)
            {
                IList<Stroke> ResIsl = S.TrazarPerimetro(M);
                foreach (Stroke tI in ResExt)
                {
                    Res.Add(tI);
                }
            }

            return Res;
        }

        public Poligono Equidista(double Distancia)
        {
            //retorna la equidistancia
            Poligono Res = new Poligono();
            //eqidistancia con signo, así las islas equidistan en el sentido correcto

            Res._Exterior = _Exterior.Equidista(Distancia);

            foreach(Shape S in _Islas)
            {
                Res.AddIsla(S.Equidista(Distancia));
            }

            return Res;
        }
    }

    class Shape
    {
        IList<Punto> _Puntos;
        double _area;
        BBox _Bownding;

        private void _Inicializa()
        {
            _Puntos = new List<Punto>();
            _area = double.NaN;
            _Bownding = new BBox();
        }

        public Shape()
        {
            _Inicializa();
        }

        public Shape(IList<Punto> Puntos)
        {
            _Inicializa();
            foreach (Punto p in Puntos)
            {
                _Puntos.Add(p);
                _Bownding.Actualizar(p);
            }
        }

        public Shape(IList<VertexSLT> Vertexs)
        {
            _Inicializa();
            foreach (VertexSLT v in Vertexs)
            {
                Punto p = new Punto(v.X, v.Y, v.Z);
                _Puntos.Add(p);
                _Bownding.Actualizar(p);
            }
        }

        public BBox Bownding
        {
            get
            {
                return _Bownding;
            }
        }

        public void AddPunto(Punto Punto)
        {
            _area = double.NaN; //ya no es válido cualquier valor anterior del área.
            _Puntos.Add(Punto);
            _Bownding.Actualizar(Punto);
        }

        public void AddPunto(int Indice, Punto Punto)
        {
            _area = double.NaN; //ya no es válido cualquier valor anterior del área.
            _Puntos.Insert(Indice, Punto);
            _Bownding.Actualizar(Punto);
        }

        public void AddPunto(VertexSLT Vertex)
        {
            AddPunto(new Punto(Vertex.X, Vertex.Y, Vertex.Z));
        }

        public void AddPunto(int Indice, VertexSLT Vertex)
        {
            AddPunto(Indice, new Punto(Vertex.X, Vertex.Y, Vertex.Z));
        }

        public double Superficie
        {
            get
            {
                if (double.IsNaN(_area) || _area == 0.0)//solo lo calcula si no se ha calculado antes
                {
                    _area = 0.0;
                    if (_Puntos.Count > 2)//si no el área es 0
                    {
                        //área por Determinante de Gauss
                        //sumatorio de los determinantes
                        for (int p1 = 0; p1 < _Puntos.Count; p1++)
                        {
                            int p2;
                            if (p1 == _Puntos.Count - 1)
                            {
                                p2 = 0;
                            }
                            else
                            {
                                p2 = p1 + 1;
                            }

                            //determinante entre p y p+1
                            _area += (_Puntos[p1].X * _Puntos[p2].Y) - (_Puntos[p2].X * _Puntos[p1].Y);
                        }

                        _area = _area / 2;
                    }
                }

                return _area;
            }
        }

        public Shape Inverso
        {
            get
            {
                IList<Punto> resInv = new List<Punto>();

                for (int p = _Puntos.Count - 1; p >= 0; p--)
                {
                    resInv.Add(_Puntos[p]);
                }

                return new Shape(resInv);
            }
        }

        public static IList<Shape> GenerarPoligonos(IList<LineSLT> Bordes)
        {
            IList<Shape> resIslas = new List<Shape>();
            IList<KeyValuePair<LineSLT, bool>> _Bordes = new List<KeyValuePair<LineSLT, bool>>();

            if (Bordes.Count > 0)
            {
                //almaceno los bordes para usar
                foreach (LineSLT r in Bordes)
                {
                    if (!r.V1.EsIgual(r.V2))
                    {
                        _Bordes.Add(new KeyValuePair<LineSLT, bool>(r, false));
                    }
                }

                bool QuedanSinProcesar = true;
                //mientras que existan rectas sin procesar
                while (QuedanSinProcesar)
                {
                    //Comienzo a crear el shape con el primer punto sin procesar
                    IList<LineSLT> BordesConcat = new List<LineSLT>();
                    for (int i = 0; i < _Bordes.Count; i++)
                    {
                        if (!_Bordes[i].Value)
                        {
                            BordesConcat.Add(_Bordes[i].Key);
                            _Bordes[i] = new KeyValuePair<LineSLT, bool>(_Bordes[i].Key, true);
                            break;
                        }
                    }

                    if (_Bordes.Count > 0)
                    {
                        //mientras que se añada algún vértice
                        bool Añadido = true;
                        while (Añadido)
                        {
                            Añadido = false;

                            //Busco si se puede añadir alguno
                            for (int i = 0; i < _Bordes.Count; i++)
                            {
                                if (!_Bordes[i].Value)
                                {
                                    KeyValuePair<LineSLT, bool> kv = _Bordes[i];
                                    bool Usado = false;

                                    if (kv.Key.V1.EsIgual(BordesConcat.First().V1))
                                    {
                                        //rotar el borde y almacenar al principio
                                        BordesConcat.Insert(0, new LineSLT(kv.Key.V2, kv.Key.V1));
                                        Usado = true;
                                    }
                                    else if (kv.Key.V2.EsIgual(BordesConcat.First().V1))
                                    {
                                        //almacenar al principio
                                        BordesConcat.Insert(0, kv.Key);
                                        Usado = true;
                                    }
                                    else if (kv.Key.V1.EsIgual(BordesConcat.Last().V2))
                                    {
                                        //almacenar al final
                                        BordesConcat.Add(kv.Key);
                                        Usado = true;
                                    }
                                    else if (kv.Key.V2.EsIgual(BordesConcat.Last().V2))
                                    {
                                        //rotar el borde y almacenar al final
                                        BordesConcat.Add(new LineSLT(kv.Key.V2, kv.Key.V1));
                                        Usado = true;
                                    }

                                    //ha sido marcado como utilizado y se almacena como tal
                                    if (Usado)
                                    {
                                        _Bordes[i] = new KeyValuePair<LineSLT, bool>(kv.Key, Usado);

                                        //Para continuar buscando más polígonos
                                        Añadido = true;
                                    }
                                }
                            }
                        }

                        //Genero el shape con el conjunto de bordes
                        Shape TempShape = new Shape();
                        foreach (LineSLT r in BordesConcat)
                        {
                            TempShape.AddPunto(r.V1);
                        }

                        /*//si no coinciden, añade el último punto
                        if (!BordesConcat.Last<RectaSLT>().V2.EsIgual(BordesConcat.First<RectaSLT>().V1))
                        {
                            TempShape.AddPunto(BordesConcat.Last<RectaSLT>().V2);
                        }*/

                        //invierto o no el shape
                        if (TempShape.Superficie < 0.0)
                        {
                            resIslas.Add(TempShape.Inverso);
                        }
                        else
                        {
                            resIslas.Add(TempShape);
                        }

                        //evaluo si quedan para continuar o terminar el proceso
                        foreach (KeyValuePair<LineSLT, bool> kv in _Bordes)
                        {
                            if (!kv.Value)
                            {
                                //quedan pendientes
                                QuedanSinProcesar = true;
                                break;
                            }

                            //terminado
                            QuedanSinProcesar = false;
                        }
                    }
                }//Termina de procesar

                //cruzo los polígonos buscando lados comunes
                resIslas = AgruparPoligonos(resIslas);


            }//Bordes.Count > 0

            return resIslas;
        }

        public static IList<Shape> AgruparPoligonos(IList<Shape> Islas)
        {
            IList<Shape> resIslas = new List<Shape>();
            IList<KeyValuePair<Shape, bool>> _Islas = new List<KeyValuePair<Shape, bool>>();

            if (Islas.Count > 0)
            {
                //almaceno las islas a usar
                foreach (Shape S in Islas)
                {
                    if (S.Superficie != 0.0)
                    {
                        _Islas.Add(new KeyValuePair<Shape, bool>(S, false));
                    }
                }

                Shape Join;
                bool Pendientes = true;

                while (Pendientes)
                {
                    Pendientes = false;
                    for (int S = 0; S < _Islas.Count; S++)
                    {
                        if (!_Islas[S].Value)
                        {
                            bool Saltar = true;
                            Pendientes = true;
                            Join = _Islas[0].Key;
                            for (int s = 0; s < _Islas.Count; s++)
                            {
                                if (S != s && !_Islas[s].Value)
                                {
                                    Shape TempJoin;
                                    if (_Islas[S].Key.CompartenLado(_Islas[s].Key, out TempJoin))
                                    {
                                        //se marcan s como usada
                                        _Islas[s] = new KeyValuePair<Shape, bool>(_Islas[s].Key, true);

                                        //se reemplaza el valor de S por el del Join
                                        _Islas[S] = new KeyValuePair<Shape, bool>(TempJoin, false);
                                        Saltar = false;
                                    }
                                }
                            }

                            //si no se ha usado S la salta para la próxima
                            if(Saltar)
                            {
                                _Islas[S] = new KeyValuePair<Shape, bool>(_Islas[S].Key, true); 
                                //añade al resultado
                                resIslas.Add(_Islas[S].Key);
                            }
                            else
                            {
                                //vuelve a comparar con todos los polígonos
                                S--;
                            }
                        }
                    }
                }

            }//Islas.Count > 0

            return resIslas;
        }



        public bool EsInterior(Punto Pto)
        {
            //variación del windingnumber solo teniendo en cuenta el cambio de cuadrante
            bool Res = false;

            if (this._Puntos.Count > 2)
            {
                int CuadranteAnterior = 0, CuadranteActual = 0, ConteoCuadrantes = 0;

                IList<Punto> _Borde_ = new List<Punto>();
                
                foreach(Punto p in this._Puntos)
                {
                    _Borde_.Add(p);
                }

                _Borde_.Add(this._Puntos[0]);//repite contra el primero

                Punto pAnt = new Punto();
                foreach (Punto p in _Borde_)
                {
                    CuadranteActual = _Cuadrante(Pto, p);
                    if (CuadranteAnterior == 0)
                    {
                        //evita que el primer cálculo cuente como salto de cuadrante;
                        CuadranteAnterior = CuadranteActual;
                    }

                    if (CuadranteActual != 0 && CuadranteActual != CuadranteAnterior)
                    {
                        //evalúa si se suman pasos por cuadrantes o se restan
                        int SaltoCuadrantes = CuadranteActual - CuadranteAnterior;
                        if (SaltoCuadrantes == 1 || SaltoCuadrantes == -1)
                        {
                            //paso a un cuadrante adyacente
                            if (SaltoCuadrantes > 0)
                            {
                                ConteoCuadrantes += SaltoCuadrantes;
                            }
                        }
                        else if (SaltoCuadrantes == 2 || SaltoCuadrantes == -2)
                        {
                            //Evalúa si el salto de dos cuadrantes se realiza en sentido horario (-) o no (+);
                            int SignoSalto = Segmento.Lado(pAnt, p, Pto);
                            if (SignoSalto == 0)
                            {
                                //el punto se encuentra en la frontera del polígono
                                Res = true;
                                break;
                            }
                            else
                            {
                                ConteoCuadrantes += SignoSalto * 2;
                            }
                        }
                        else if (SaltoCuadrantes == 3 || SaltoCuadrantes == -3)
                        {
                            //salto entre el cuadrante 4 y el 1
                            //se invierte el SaltoCuadrante
                            ConteoCuadrantes -= SaltoCuadrantes / 3;
                        }
                    }
                    else
                    {
                        //puntos coincidentes o en el mismo cuadrante se omiten
                        //pertenece al polígono
                        Res = true;
                        break;
                    }
                    pAnt = p;
                }

                if (!Res)
                {
                    //evalua si se ha producido una revolución
                    if (ConteoCuadrantes >= 4 || ConteoCuadrantes <= -4)
                    {
                        Res = true;
                    }
                    /*else
                    {
                        Res = false;
                    }*/
                }
                /*else
                {
                    //salida forzada porcoincidencia etre punto y vértice
                    Res = true;
                }*/
            }

            return Res;
        }

        public bool CompartenLado(Shape s, out Shape Join)
        {
            bool Res = false;

            IList<Segmento> Segs = new List<Segmento>();
            IList<Segmento> segs = new List<Segmento>();

            //alamaceno en las listas los vértices
            for (int P = 0; P < this._Puntos.Count - 1; P++)
            {
                Segs.Add(new Segmento(this._Puntos[P], this._Puntos[P + 1]));
            }
            Segs.Add(new Segmento(this._Puntos[this._Puntos.Count - 1], this._Puntos[0]));

            for (int p = 0; p < s._Puntos.Count - 1; p++)
            {
                Segs.Add(new Segmento(s._Puntos[p], s._Puntos[p + 1]));
            }
            Segs.Add(new Segmento(s._Puntos[s._Puntos.Count - 1], s._Puntos[0]));

            //cruza la listas evaluando si existen segmentos compartidos
            int IndxSLado = -1;
            int IndxsLado = -1;

            for (int I = 0; I < Segs.Count; I++)
            {
                for (int i = 0; i < segs.Count; i++)
                {
                    if (Segs[I].Inicio.EsIgual(segs[i].Final) && Segs[I].Final.EsIgual(segs[i].Inicio))
                    {
                        //ha encontrado un lado
                        IndxSLado = I;
                        IndxsLado = i;
                        break;
                    }
                }

                if (IndxSLado != -1)
                {
                    break;
                }
            }

            if (IndxsLado != -1)
            {
                //comparten lado
                Res = true;

                //genera el Join
                Join = new Shape();
                for (int I = 0; I < Segs.Count; I++)
                {
                    if (I == IndxSLado)
                    {
                        //omite el lado y añade los otros en sentido normal
                        for (int i = IndxsLado + 1; i < segs.Count; i++)
                        {
                            Join.AddPunto(segs[i].Inicio);
                        }
                        for (int i = 0; i < IndxsLado; i++)
                        {
                            Join.AddPunto(segs[i].Inicio);
                        }
                    }
                    else
                    {
                        Join.AddPunto(Segs[I].Inicio);
                    }
                }
            }
            else
            {
                //No comparten lado
                Res = false;
                Join = new Shape();
            }

            return Res;
        }

        public bool ContieneA(Shape S)
        {
            //variación del windingnumber solo teniendo en cuenta el cambio de cuadrante
            bool Res = true;

            foreach(Punto p in S._Puntos)
            {
                if(!this.EsInterior(p))
                {
                    Res = false;
                    break;
                }
            }

            return Res;
        }

        //retorna el cuadrante en el que se encuentra el punto, 1, 2, 3 o 4
        private int _Cuadrante(Punto Ref, Punto Dest)
        {
            if (Dest.X > Ref.X && Dest.Y >= Ref.Y)
            {
                return 1;
            }
            else if (Dest.X <= Ref.X && Dest.Y > Ref.Y)
            {
                return 2;
            }
            else if (Dest.X < Ref.X && Dest.Y <= Ref.Y)
            {
                return 3;
            }
            else if (Dest.X >= Ref.X && Dest.Y < Ref.Y)
            {
                return 4;
            }
            else
            {
                //fallo al evaluar la pertenencia al cuadrante
                //es coincidente con el punto de referencia
                return 0;
            }
        }

        public IList<Segmento> SegmentosInteriores(Segmento Rayo, bool Invertido = false)
        {
            IList<Segmento> Res = new List<Segmento>();

            IList<Punto> TempCortes = new List<Punto>();
            Punto pAnt = this._Puntos.Last<Punto>();

            //añado el punto inicial y final si se encuentran dentro del polígono
            if (this.EsInterior(Rayo.Inicio))
            {
                TempCortes.Add(Rayo.Inicio);
            }
            if (this.EsInterior(Rayo.Final))
            {
                TempCortes.Add(Rayo.Final);
            }

            foreach (Punto p in this._Puntos)
            {
                Segmento s = new Segmento(pAnt, p);
                Punto TempPto = new Punto();
                //calcula el corte real con el borde
                if (s.Corta(Rayo, out TempPto))
                {
                    if (!TempCortes.Contains<Punto>(TempPto))
                    {
                        TempCortes.Add(TempPto);
                    }
                }

                pAnt = p;
            }

            //TODO: //ordeno en función de la distancia a Rayo.Inicio
            TempCortes = TempCortes.OrderBy<Punto, double>(p => p.Distancia(Rayo.Inicio)).ToList<Punto>();

            //con TempCortes voy generando segmentos por pares y evaluando si el punto medio se encuentra dentro o no del polígono
            for (int p = 1; p < TempCortes.Count; p++)
            {
                bool Añadir = false;
                if (this.EsInterior(new Punto((TempCortes[p].X + TempCortes[p - 1].X) / 2.0,
                                              (TempCortes[p].Y + TempCortes[p - 1].Y) / 2.0,
                                              (TempCortes[p].Z + TempCortes[p - 1].Z) / 2.0)))
                {
                    if (!Invertido)//lo añado
                    {
                        Añadir = true;
                    }
                }
                else
                {
                    if (Invertido)//lo añado
                    {
                        Añadir = true;
                    }
                }

                if (Añadir)
                {
                    Res.Add(new Segmento(TempCortes[p - 1], TempCortes[p]));
                }
            }

            return Res;
        }

        public IList<Stroke> TrazarPerimetro(Modes M)
        {
            IList<Stroke> Res = new List<Stroke>();

            if (_Puntos.Count > 0)
            {
                foreach (Punto p in _Puntos)
                {
                    Stroke TempTrazo = new Stroke();
                    TempTrazo.Pendiente = true;
                    if (Res.Count == 0)
                    {
                        TempTrazo.Mode = Modes.ModeTraslation;
                    }
                    else
                    {
                        TempTrazo.Mode = M;
                    }
                    TempTrazo.Destino = new VertexSLT(p.X, p.Y, p.Z);

                    Res.Add(TempTrazo);
                }

                //cierro
                Stroke TempT = new Stroke();
                TempT.Pendiente = true;
                TempT.Mode = Modes.ModoRim;
                TempT.Destino = new VertexSLT(_Puntos[0].X, _Puntos[0].Y, _Puntos[0].Z);

                Res.Add(TempT);
            }

            return Res;
        }

        public Shape Equidista(double Distancia)
        {
            //retorna la equidistancia
            Shape Res = new Shape();
            //eqidistancia con signo, así las islas equidistan en el sentido correcto

            if(this._Puntos.Count>2)//salta por no ser viable la equidistancia
            {
                Segmento Sn = new Segmento(this._Puntos.Last<Punto>(), this._Puntos.First<Punto>());
                Segmento Sn1 = new Segmento();

                for (int i = 0; i < this._Puntos.Count; i++)
                {
                    int j = i + 1;
                    if (j >= this._Puntos.Count)
                    {
                        j = 0;
                    }

                    Sn1 = new Segmento(this._Puntos[i], this._Puntos[j]);

                    Punto TempRes = new Punto();
                    if(Sn.Interseccion2D(Sn1, out TempRes))
                    {
                        Res.AddPunto(TempRes);
                    }

                    Sn = Sn1;
                }

            }

            return Res;
        }
    }
}
