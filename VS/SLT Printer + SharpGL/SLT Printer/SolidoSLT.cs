using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;

namespace SLT_Printer
{
    public class FacetSLT
    {
        public VertexSLT _Normal;
        public IList<LoopSLT> _Loops;
        public UInt16 _Attribute;

        public FacetSLT()
        {
            _Normal = new VertexSLT();
            _Attribute = 0;
            _Loops = new List<LoopSLT>();
        }

        public void Trasladar(VertexSLT T)
        {
            for (int i = 0; i < _Loops.Count; i++)
            {
                _Loops[i].Trasladar(T);
            }
        }

        public bool EsValido()
        {
            if (_Normal.EsValido && _Loops.Count() > 0)
            {
                bool Res = true;

                foreach (LoopSLT L in _Loops)
                {
                    if (!L.EsValido())
                    {
                        Res = false;
                        break;
                    }
                }

                return Res;
            }
            else
            {
                return false;
            }
        }

        public bool CortaPlanoZ(double Z)
        {
            foreach(LoopSLT L in _Loops)
            {
                if(L.CortaPlanoZ(Z))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CortePlanoZ(double Z, out IList<RectaSLT> Corte)
        {
            return Intersecciones.IntFacetPlano(this, Z, out Corte);
        }
    }

    public class LoopSLT
    {
        public IList<VertexSLT> Vertices;

        private double _ZMax;
        private double _Zmin;

        public LoopSLT()
        {
            Vertices = new List<VertexSLT>();
            _ZMax = double.NaN;
            _Zmin = double.NaN;
        }

        public void ActualizaBoundingZ()
        {
            foreach (VertexSLT V in Vertices)
            {
                //compara mínimos
                if (double.IsNaN(_Zmin) || V.Z < _Zmin)
                {
                    _Zmin = V.Z;
                }

                //compara Máximos
                if (double.IsNaN(_ZMax) || V.Z > _ZMax)
                {
                    _ZMax = V.Z;
                }
            }
        }

        public void Trasladar(VertexSLT T)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Trasladar(T);
            }
        }

        public bool CortaPlanoZ(double Z)
        {
            if (double.IsNaN(_Zmin) || double.IsNaN(_ZMax))
            {
                this.ActualizaBoundingZ();
            }

            if (Z >= _Zmin - 0.000001 && Z <= _ZMax + 0.000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CortePlanoZ(double Z, out RectaSLT Corte)
        {
            return Intersecciones.IntLoopPlano(this, Z, out Corte);
        }

        public bool EsValido()
        {
            if (Vertices.Count() > 0)
            {
                bool Res = true;

                foreach (VertexSLT V in Vertices)
                {
                    if(!V.EsValido)
                    {
                        Res = false;
                        break;
                    }
                }

                return Res;
            }
            else
            {
                return false;
            }
        }
    }

    public class VertexSLT
    {
        double _X;
        double _Y;
        double _Z;

        public VertexSLT()
        {
            _X = double.NaN;
            _Y = double.NaN;
            _Z = double.NaN;
        }

        public VertexSLT(double X, double Y, double Z)
        {
            _X = X;
            _Y = Y;
            _Z = Z;
        }

        public bool EsValido
        {
            get
            {
                if (double.IsNaN(_X) || double.IsNaN(_Y) || double.IsNaN(_Z))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public double X
        {
            get
            {
                return _X;
            }
        }
        public double Y
        {
            get
            {
                return _Y;
            }
        }
        public double Z
        {
            get
            {
                return _Z;
            }
        }

        public float Xf
        {
            get
            {
                return (float)_X;
            }
        }
        public float Yf
        {
            get
            {
                return (float)_Y;
            }
        }
        public float Zf
        {
            get
            {
                return (float)_Z;
            }
        }

        public void Trasladar(VertexSLT T)
        {
            _X += T.X;
            _Y += T.Y;
            _Z += T.Z;
        }

        public bool EsIgual(VertexSLT Valor)
        {
            bool Res = false;
            const double Margen = 0.00001;
            if (Math.Abs(X - Valor.X) < Margen)
            {
                if (Math.Abs(Y - Valor.Y) < Margen)
                {
                    if (Math.Abs(Z - Valor.Z) < Margen)
                    {
                        Res = true;
                    }
                }
            }

            return Res;
        }
    }

    public class RectaSLT
    {
        VertexSLT _V1;
        VertexSLT _V2;

        public RectaSLT()
        {
            _V1 = new VertexSLT();
            _V2 = new VertexSLT();
        }

        public RectaSLT(VertexSLT V1, VertexSLT V2)
        {
            _V1 = V1;
            _V2 = V2;
        }

        public bool EsValido
        {
            get
            {
                if (_V1.EsValido && _V2.EsValido)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public VertexSLT V1
        {
            get
            {
                return _V1;
            }
        }
        public VertexSLT V2
        {
            get
            {
                return _V2;
            }
        }
        
        public void Trasladar(VertexSLT T)
        {
            _V1.Trasladar(T);
            _V2.Trasladar(T);
        }

        public bool EsIgual(RectaSLT Valor)
        {
            if (_V1.EsIgual(Valor.V1) && _V2.EsIgual(Valor.V2))
            {
                return true;
            }
            else if (_V2.EsIgual(Valor.V1) && _V1.EsIgual(Valor.V2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    class SolidoSLT
    {
        private string _Nombre;
        private IList<string> _Fallos;
        private IList<FacetSLT> _Facets;
        private VertexSLT _IzqFrontInf;
        private VertexSLT _DerPostSup;

        private VertexSLT _Centro;
        private double _Ancho;
        private double _Largo;
        private double _Alto;

        private bool _PassTest;

        private double _Tx;
        private double _Ty;
        private double _Tz;

        public double Tx
        {
            get
            {
                return _Tx;
            }
            set
            {
                _Centro = new VertexSLT();
                _Ancho = double.NaN;

                double DeltaTx = value - _Tx;
                if (DeltaTx != 0.0)
                {
                    VertexSLT T = new VertexSLT(DeltaTx, 0.0, 0.0);

                    _DerPostSup.Trasladar(T);
                    _IzqFrontInf.Trasladar(T);
                    _Centro.Trasladar(new VertexSLT(DeltaTx / 2, 0.0, 0.0));

                    for (int i = 0; i < _Facets.Count(); i++)
                    {
                        _Facets[i].Trasladar(T);
                    }
                }

                _Tx = value;
            }
        }

        public double Ty
        {
            get
            {
                return _Ty;
            }
            set
            {
                _Centro = new VertexSLT();
                _Ancho = double.NaN;

                double DeltaTy = value - _Ty;
                if (DeltaTy != 0.0)
                {
                    VertexSLT T = new VertexSLT(0.0, DeltaTy, 0.0);

                    _DerPostSup.Trasladar(T);
                    _IzqFrontInf.Trasladar(T);
                    _Centro.Trasladar(new VertexSLT(0.0, DeltaTy / 2, 0.0));

                    for (int i = 0; i < _Facets.Count(); i++)
                    {
                        _Facets[i].Trasladar(T);
                    }
                }

                _Ty = value;
            }
        }

        public double Tz
        {
            get
            {
                return _Tz;
            }
            set
            {
                _Centro = new VertexSLT();
                _Ancho = double.NaN;

                double DeltaTz = value - _Tz;
                if (DeltaTz != 0.0)
                {
                    VertexSLT T = new VertexSLT(0.0, 0.0, DeltaTz);

                    _DerPostSup.Trasladar(T);
                    _IzqFrontInf.Trasladar(T);
                    _Centro.Trasladar(new VertexSLT(0.0, 0.0, DeltaTz / 2));

                    for (int i = 0; i < _Facets.Count(); i++)
                    {
                        _Facets[i].Trasladar(T);
                    }
                }

                _Tz = value;
            }
        }

        public string Nombre
        {
            get
            {
                return _Nombre;
            }
        }

        public IList<string> Fallos
        {
            get
            {
                return _Fallos;
            }
        }

        public int NumFallos
        {
            get
            {
                return _Fallos.Count();
            }
        }

        public VertexSLT Centro
        {
            get
            {
                if(!_Centro.EsValido)
                {
                    _Centro = new VertexSLT(
                        (_IzqFrontInf.X + _DerPostSup.X) / 2,
                        (_IzqFrontInf.Y + _DerPostSup.Y) / 2,
                        (_IzqFrontInf.Z + _DerPostSup.Z) / 2);
                }
                return _Centro;
            }
        }

        public double Ancho
        {
            get
            {
                if (double.IsNaN(_Ancho))
                {
                    _Ancho = Math.Abs(_IzqFrontInf.X - _DerPostSup.X);
                }
                return _Ancho;
            }
        }

        public double Largo
        {
            get
            {
                if (double.IsNaN(_Largo))
                {
                    _Largo = Math.Abs(_IzqFrontInf.Y - _DerPostSup.Y);
                }
                return _Largo;
            }
        }

        public double Alto
        {
            get
            {
                if (double.IsNaN(_Alto))
                {
                    _Alto = Math.Abs(_IzqFrontInf.Z - _DerPostSup.Z);
                }
                return _Alto;
            }
        }

        public bool PassTest
        {
            get
            {
                return _PassTest;
            }
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event ChangedEventHandler Changed;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnReading(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        private void _Inicializa()
        {
            //Inicializa las variables
            _Nombre = "";
            _Fallos = new List<string>();
            _Facets = new List<FacetSLT>();
            _IzqFrontInf = new VertexSLT();
            _DerPostSup = new VertexSLT();
            _PassTest = false;

            _Centro = new VertexSLT();
            _Ancho = double.NaN;
            _Largo = double.NaN;
            _Alto = double.NaN;

            _Tx = 0.0;
            _Ty = 0.0;
            _Tz = 0.0;
        }

        public SolidoSLT()
        {
            _Inicializa();
        }

        public SolidoSLT(String FicheroSLT)
        {
            _Inicializa();
            LeeSLT(FicheroSLT);
        }

        public bool LeeSLT(String FicheroSLT)
        {
            bool Res = false;

            OnReading(EventArgs.Empty);

            _Inicializa();

            System.IO.StreamReader SR = new System.IO.StreamReader(FicheroSLT, Encoding.ASCII);
            //Evalua si es binario o ASCII
            string PrimeraLineaSLT = SR.ReadLine();
            SR.Close();
            SR.Dispose();

            if (PrimeraLineaSLT.StartsWith("solid "))
            {
                //ASCII
                Res = _LeeSLTASCII(FicheroSLT);
            }
            else
            {
                //Binario
                Res = _LeeSLTBinario(FicheroSLT);
            }

            //Obtiene las esquinas del BowndingBox
            if (_Facets.Count > 0)
            {
                foreach (FacetSLT F in _Facets)
                {
                    foreach (LoopSLT L in F._Loops)
                    {
                        foreach (VertexSLT V in L.Vertices)
                        {
                            //compara mínimos con _Izq
                            if (double.IsNaN(_IzqFrontInf.X) || V.X < _IzqFrontInf.X)
                            {
                                _IzqFrontInf = new VertexSLT(V.X, _IzqFrontInf.Y, _IzqFrontInf.Z);
                            }
                            if (double.IsNaN(_IzqFrontInf.Y) || V.Y < _IzqFrontInf.Y)
                            {
                                _IzqFrontInf = new VertexSLT(_IzqFrontInf.X, V.Y, _IzqFrontInf.Z);
                            }
                            if (double.IsNaN(_IzqFrontInf.Z) || V.Z < _IzqFrontInf.Z)
                            {
                                _IzqFrontInf = new VertexSLT(_IzqFrontInf.X, _IzqFrontInf.Y, V.Z);
                            }

                            //compara máximos con _Der
                            if (double.IsNaN(_DerPostSup.X) || V.X > _DerPostSup.X)
                            {
                                _DerPostSup = new VertexSLT(V.X, _DerPostSup.Y, _DerPostSup.Z);
                            }
                            if (double.IsNaN(_DerPostSup.Y) || V.Y > _DerPostSup.Y)
                            {
                                _DerPostSup = new VertexSLT(_DerPostSup.X, V.Y, _DerPostSup.Z);
                            }
                            if (double.IsNaN(_DerPostSup.Z) || V.Z > _DerPostSup.Z)
                            {
                                _DerPostSup = new VertexSLT(_DerPostSup.X, _DerPostSup.Y, V.Z);
                            }
                        }
                    }
                }
            }
            else
            {
                _IzqFrontInf = new VertexSLT();
                _DerPostSup = new VertexSLT();
            }

            return Res;
        }

        private bool _LeeSLTASCII(String FicheroSLT)
        {
            /*****ASCII STL

            An ASCII STL file begins with the line:

                solid name

            Where name is an optional string (though if name is omitted there must still be a space after solid). The file continues with any number of triangles, each represented as follows:

                facet normal ni nj nk
                    outer loop
                        vertex v1x v1y v1z
                        vertex v2x v2y v2z
                        vertex v3x v3y v3z
                    endloop
                endfacet

            Where each n or v is a doubleing-point number in sign-mantissa-"e"-sign-exponent format, e.g., "2.648000e-002" (noting that each v must be non-negative). The file concludes with:

                endsolid name

            The structure of the format suggests that other possibilities exist (e.g., facets with more than one "loop", or loops with more than three vertices). In practice, however, all facets are simple triangles.

            White space (spaces, tabs, newlines) may be used anywhere in the file except within numbers or words. The spaces between "facet" and "normal" and between "outer" and "loop" are required.*/

            bool Res = true;

            System.IO.StreamReader SR = new System.IO.StreamReader(FicheroSLT, Encoding.ASCII);
            //Evalua si es binario o ASCII
            string LineaSLT = SR.ReadLine();
            LineaSLT = LineaSLT.TrimStart(new char[2] { ' ', '\t' });

            if (LineaSLT.StartsWith("solid "))
            {
                _Nombre = LineaSLT.Substring(5);

                bool FinSolido = false;
                bool EnFacet = false;
                bool EnOuterLoop = false;

                VertexSLT TempVertex = new VertexSLT();
                LoopSLT TempLoop = new LoopSLT();
                FacetSLT TempFacet = new FacetSLT();

                do
                {
                    LineaSLT = SR.ReadLine();

                    LineaSLT = LineaSLT.TrimStart(new char[2] { ' ', '\t' });

                    if(LineaSLT.StartsWith("facet normal "))
                    {
                        //facet normal ni nj nk
                        if (EnOuterLoop)
                        {
                            _Fallos.Add("Lectura SLT ASCII: Nueva Faceta sin cierre correcto de Vértices");
                            Res = false;
                        }
                        else
                        {
                            //Obtengo el Vertex Normal y lo almaceno
                            LineaSLT = LineaSLT.Substring(12);
                            string[] TempStr = LineaSLT.Split(' ');
                            if (TempStr.Length == 3)
                            {
                                TempFacet._Normal = new VertexSLT(Convert.ToSingle(TempStr[0]), Convert.ToSingle(TempStr[1]), Convert.ToSingle(TempStr[2]));
                            }
                            else
                            {
                                _Fallos.Add("Lectura SLT ASCII: El Vector Normal de la Faceta no tiene 3 domensiones");
                                Res = false;
                            }
                        }

                        EnFacet = true;
                    }
                    else if (LineaSLT == "outer loop")
                    {
                        if (EnOuterLoop)
                        {
                            _Fallos.Add("Lectura SLT ASCII: Inicio de Loop sin cierre del anterior");
                            Res = false;
                        }
                        else
                        {
                            EnOuterLoop = true;
                        }
                    }
                    else if (LineaSLT.StartsWith("vertex "))
                    {
                        if (EnOuterLoop && EnFacet)
                        {
                            //vertex v1x v1y v1z
                            LineaSLT = LineaSLT.Substring(6);
                            string[] TempStr = LineaSLT.Split(' ');
                            if (TempStr.Length == 3)
                            {
                                TempLoop.Vertices.Add(new VertexSLT(Convert.ToSingle(TempStr[0]), Convert.ToSingle(TempStr[1]), Convert.ToSingle(TempStr[2])));
                            }
                            else
                            {
                                _Fallos.Add("Lectura SLT ASCII: Un Vértice de la Faceta no tiene 3 domensiones");
                                Res = false;
                            }
                        }
                        else
                        {
                            _Fallos.Add("Lectura SLT ASCII: Vértice fuera de un Loop o una Faceta");
                            Res = false;
                        }
                    }
                    else if (LineaSLT == "endloop" || LineaSLT == "end loop")
                    {
                        if(EnOuterLoop)
                        {
                            TempLoop.ActualizaBoundingZ();
                            TempFacet._Loops.Add(TempLoop);
                        }
                        else
                        {
                            _Fallos.Add("Lectura SLT ASCII: Cierre de Loop incorrecto");
                            Res = false;
                        }

                        TempLoop = new LoopSLT();
                        EnOuterLoop = false;
                    }
                    else if (LineaSLT == "endfacet" || LineaSLT == "end facet")
                    {
                        if (EnFacet)
                        {
                            if (TempFacet.EsValido())
                            {
                                _Facets.Add(TempFacet);
                            }
                            else
                            {
                                _Fallos.Add("Lectura SLT ASCII: Faceta no válida");
                                Res = false;
                            }
                        }
                        else
                        {
                            _Fallos.Add("Lectura SLT ASCII: Fin de Faceta no válida");
                            Res = false;
                        }

                        TempFacet = new FacetSLT();
                        EnFacet = false;
                    }
                    else if (LineaSLT=="endsolid " + _Nombre)
                    {
                        //Fin de sólido
                        if (!EnFacet && !EnOuterLoop)
                        {
                            FinSolido = true;
                        }
                        else
                        {
                            _Fallos.Add("Lectura SLT ASCII: Fin de solido sin cierre correcto de Facetas o Vértices");
                            Res = false;
                        }
                    }
                    else
                    {
                        //No contemplado
                    }
                }
                while (!SR.EndOfStream || !FinSolido);
            }
            else
            {
                Res = false;
                _Fallos.Add("Lectura SLT ASCII: El Fichero no comienza por 'solid '");
            }
            SR.Close();
            SR.Dispose();

            return Res;
        }

        private bool _LeeSLTBinario(String FicheroSLT)
        {
            /*****Binary STL

            Because ASCII STL files can become very large, a binary version of STL exists. A binary STL file has an 80-character header (which is generally ignored, but should never begin with 
            "solid" because that will lead most software to assume that this is an ASCII STL file). Following the header is a 4-byte unsigned integer indicating the number of triangular facets 
            in the file. Following that is data describing each triangle in turn. The file simply ends after the last triangle.

            Each triangle is described by twelve 32-bit doubleing-point numbers: three for the normal and then three for the X/Y/Z coordinate of each vertex – just as with the ASCII version of STL.
            After these follows a 2-byte ("short") unsigned integer that is the "attribute byte count" – in the standard format, this should be zero because most software does not understand anything else.

            doubleing-point numbers are represented as IEEE doubleing-point numbers and are assumed to be little-endian, although this is not stated in documentation.

                UINT8[80] – Header
                UINT32 – Number of triangles

                foreach triangle
                REAL32[3] – Normal vector
                REAL32[3] – Vertex 1
                REAL32[3] – Vertex 2
                REAL32[3] – Vertex 3
                UINT16 – Attribute byte count
                end
            */

            bool Res = true;

            System.IO.StreamReader SR = new System.IO.StreamReader(FicheroSLT, Encoding.ASCII);
            //Evalua si es binario o ASCII
            string LineaSLT = SR.ReadLine();
            LineaSLT = LineaSLT.TrimStart(new char[2] { ' ', '\t' });
            SR.Close();
            SR.Dispose();

            if (!LineaSLT.StartsWith("solid "))
            {
                System.IO.FileStream FS = new System.IO.FileStream(FicheroSLT, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.BinaryReader BR = new System.IO.BinaryReader(FS);

                //Cabecera UINT8[80] – Header
                for (int i = 0; i < 80; i++)
                {
                    char TempChar = BR.ReadChar();
                    if(TempChar != '\0')
                    {
                        _Nombre += TempChar;
                    }
                }

                //Número de triángulos UINT32 – Number of triangles
                UInt32 NumTriangulos = BR.ReadUInt32();

                if (NumTriangulos > 0)
                {
                    LoopSLT TempLoop = new LoopSLT();
                    FacetSLT TempFacet = new FacetSLT();

                    for (UInt32 Ui = 0; Ui < NumTriangulos; Ui++)
                    {
                        //Vector Normal REAL32[3] – Normal vector

                        TempFacet._Normal = new VertexSLT(Round(BR.ReadSingle()), Round(BR.ReadSingle()), Round(BR.ReadSingle()));

                        //REAL32[3] – Vertex 1
                        TempLoop.Vertices.Add(new VertexSLT(Round(BR.ReadSingle()), Round(BR.ReadSingle()), Round(BR.ReadSingle())));
                        //REAL32[3] – Vertex 2
                        TempLoop.Vertices.Add(new VertexSLT(Round(BR.ReadSingle()), Round(BR.ReadSingle()), Round(BR.ReadSingle())));
                        //REAL32[3] – Vertex 3
                        TempLoop.Vertices.Add(new VertexSLT(Round(BR.ReadSingle()), Round(BR.ReadSingle()), Round(BR.ReadSingle())));

                        TempLoop.ActualizaBoundingZ();
                        TempFacet._Loops.Add(TempLoop);

                        //Attribute byte count
                        TempFacet._Attribute = BR.ReadUInt16();

                        if (TempFacet.EsValido())
                        {
                            _Facets.Add(TempFacet);
                        }
                        else
                        {
                            Res = false;
                            _Fallos.Add("Lectura SLT Binario: Faceta no válida");
                        }

                        TempLoop = new LoopSLT();
                        TempFacet = new FacetSLT();
                    }
                }
                else
                {
                    Res = false;
                    _Fallos.Add("Lectura SLT Binario: Número de triángulos nulo o igual a 0");
                }

                BR.Close();
                FS.Close();
                BR.Dispose();
                FS.Dispose();
            }
            else
            {
                Res = false;
                _Fallos.Add("Lectura SLT Binario: El Fichero comienza por 'solid '");
            }
            
            return Res;
        }

        private double Round(double Numero)
        {
            double Res = double.NaN;
            try
            {
                string sRes = Numero.ToString("0.000000",System.Globalization.CultureInfo.InvariantCulture);
                sRes = sRes.Substring(0, sRes.Length - 1);

                Res = Convert.ToSingle(sRes, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (System.Exception) { }

            return Res;
        }

        public bool TestSLT()
        {
            bool Res = true;
            //http://www.ennex.com/~fabbers/StL.asp

            //Verifica la Orientación de cada faceta.
            foreach (FacetSLT  F in _Facets)
            {
                foreach (LoopSLT L in F._Loops)
                {
                    if (L.Vertices.Count() >= 3)
                    {
                        VertexSLT TempV = ProductoVectorialUnitario(L.Vertices[0], L.Vertices[1], L.Vertices[2], true);
                        if(F._Normal.EsIgual(TempV))
                        {
                            //OK
                        }
                        else
                        {
                            //Redefino el vector normal
                            F._Normal = TempV;
                        }
                    }
                    else
                    {
                        //Fallo, cara con menos de tres vértices
                        Res = false;
                        _Fallos.Add("Test SLT: Loop con menos de 3 vértices");
                    }
                }
            }

            //Verifica la Regla vértice a vértice.
            //Cada vértice tiene que existir por lo menos 3 veces
            IList<KeyValuePair<VertexSLT, byte>> ConteoVertices = new List<KeyValuePair<VertexSLT, byte>>();
            
            //Cargo el resumen de vértices verificando que no se dupliquen
            foreach (FacetSLT F in _Facets)
            {
                foreach (LoopSLT L in F._Loops)
                {
                    foreach (VertexSLT V in L.Vertices)
                    {
                        bool Ex = false;
                        VertexSLT TempV = new VertexSLT();
                        byte TempCount = 0;

                        for (int iV = 0; iV < ConteoVertices.Count(); iV++)
                        {
                            if (ConteoVertices[iV].Key.EsIgual(V))
                            {
                                Ex = true;

                                TempV = ConteoVertices[iV].Key;
                                TempCount = ConteoVertices[iV].Value;
                                
                                ConteoVertices.RemoveAt(iV);

                                TempCount++;
                                ConteoVertices.Add(new KeyValuePair<VertexSLT, byte>(TempV, TempCount));
                                break;
                            }
                        }

                        if (!Ex)
                        {
                            ConteoVertices.Add(new KeyValuePair<VertexSLT, byte>(V, 1));
                        }
                    }
                }
            }

            //Verifico el resumen de vértices
            for (int i = 0; i < ConteoVertices.Count();i++ )
            {
                if (ConteoVertices[i].Value < 3)
                {
                    Res = false;
                    _Fallos.Add("Test SLT: Fallo en la Regla vértice a vértice");
                }
            }

            _PassTest = Res;

            return Res;
        }

        private VertexSLT ProductoVectorialUnitario(VertexSLT V1, VertexSLT V2, VertexSLT V3, bool Unitario = true)
        {
            double V2x = V2.X - V1.X;
            double V2y = V2.Y - V1.Y;
            double V2z = V2.Z - V1.Z;

            double V3x = V3.X - V1.X;
            double V3y = V3.Y - V1.Y;
            double V3z = V3.Z - V1.Z;

            double X = (V2.Y * V3.Z) - (V2.Z * V3.Y);
            double Y = (V2.Z * V3.X) - (V2.X * V3.Z);
            double Z = (V2.X * V3.Y) - (V2.Y * V3.X);

            VertexSLT Res = new VertexSLT();

            if (Unitario)
            {
                double ModuloRes = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));

                Res = new VertexSLT(Convert.ToSingle(X / ModuloRes), Convert.ToSingle(Y / ModuloRes), Convert.ToSingle(Z / ModuloRes));
            }
            else
            {
                Res = new VertexSLT(Convert.ToSingle(X), Convert.ToSingle(Y), Convert.ToSingle(Z));
            }

            return Res;
        }

        public bool Renderiza(ref SharpGL.SceneControl scene_GLControl, double CotaZ = double.NaN, bool BBox = false)
        {
            bool Res = true;

            scene_GLControl.Scene.RenderBoundingVolumes = BBox;

            for(int i=0;i< scene_GLControl.Scene.SceneContainer.Children.Count();i++)
            {
                if (scene_GLControl.Scene.SceneContainer.Children[i].Name == "PlanoSLT")
                {
                    scene_GLControl.Scene.SceneContainer.RemoveChild(scene_GLControl.Scene.SceneContainer.Children[i]);
                    i--;
                }
                else if(scene_GLControl.Scene.SceneContainer.Children[i].Name == "ModeloSLT")
                {
                    scene_GLControl.Scene.SceneContainer.RemoveChild(scene_GLControl.Scene.SceneContainer.Children[i]);
                    i--;
                }
                /*else
                {
                    SharpGL.SceneGraph.Primitives.Folder Foldr = scene_GLControl.Scene.SceneContainer.Children[i] as SharpGL.SceneGraph.Primitives.Folder;

                    if (Foldr != null)
                    {
                        
                        foreach (SharpGL.SceneGraph.Core.SceneElement Var in Foldr.Children)
                        {
                            SharpGL.SceneGraph.Primitives.Grid Grd = Var as SharpGL.SceneGraph.Primitives.Grid;

                            if (Grd != null)
                            {
                                //Grd.Name;
                            }
                            else
                            {
                                SharpGL.SceneGraph.Primitives.Axies Ax = Var as SharpGL.SceneGraph.Primitives.Axies;

                                if (Ax != null)
                                {

                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }*/
            }

            if (!double.IsNaN(CotaZ))
            {
                //Dibuja el plano Z.
                SharpGL.SceneGraph.Primitives.Polygon Polig = new SharpGL.SceneGraph.Primitives.Polygon();
                Polig.Name = "PlanoSLT";
                Polig.DrawNormals = false;

                SharpGL.SceneGraph.Vertex[] Vertices = new SharpGL.SceneGraph.Vertex[4];

                float MargenX = (float)this.Ancho * 0.1f;
                float MargenY = (float)this.Alto * 0.1f;

                Vertices[0] = new SharpGL.SceneGraph.Vertex(_IzqFrontInf.Xf - MargenX, _IzqFrontInf.Yf - MargenY, (float)CotaZ);
                Vertices[1] = new SharpGL.SceneGraph.Vertex(_IzqFrontInf.Xf - MargenX, _DerPostSup.Yf + MargenY, (float)CotaZ);
                Vertices[2] = new SharpGL.SceneGraph.Vertex(_DerPostSup.Xf + MargenX, _DerPostSup.Yf + MargenY, (float)CotaZ);
                Vertices[3] = new SharpGL.SceneGraph.Vertex(_DerPostSup.Xf + MargenX, _IzqFrontInf.Yf - MargenY, (float)CotaZ);

                Polig.AddFaceFromVertexData(Vertices);

                Polig.Validate(true);

                scene_GLControl.Scene.SceneContainer.AddChild(Polig);
            }

            if (_Facets.Count > 0)
            {
                SharpGL.SceneGraph.Primitives.Polygon Polig = new SharpGL.SceneGraph.Primitives.Polygon();
                Polig.Name = "ModeloSLT";
                Polig.DrawNormals = false;

                foreach (FacetSLT F in _Facets)
                {
                    foreach (LoopSLT L in F._Loops)
                    {
                        bool Representar = true;

                        if(!double.IsNaN(CotaZ))
                        {
                            //Evalua si se tiene que mostrar o no por encontrarse bajo el plano
                            foreach (VertexSLT V in L.Vertices)
                            {
                                if(V.Z <= CotaZ)
                                {
                                    Representar = true;
                                    break;
                                }
                                else
                                {
                                    Representar = false;
                                }
                            }
                        }

                        if (Representar)
                        {
                            SharpGL.SceneGraph.Vertex[] Vertices = new SharpGL.SceneGraph.Vertex[L.Vertices.Count()];
                            for (int i = 0; i < L.Vertices.Count(); i++)
                            {
                                //Polig.Vertices.Add(new SharpGL.SceneGraph.Vertex(V._X, V._Y, V._Z));
                                Vertices[i] = new SharpGL.SceneGraph.Vertex(L.Vertices[i].Xf + (float)_Tx, L.Vertices[i].Yf + (float)_Ty, L.Vertices[i].Zf + (float)_Tz);
                            }

                            Polig.AddFaceFromVertexData(Vertices);
                        }
                    }
                }
                
                Polig.Validate(true);

                scene_GLControl.Scene.SceneContainer.AddChild(Polig);
            }

            return Res;
        }

        public bool CortaPlanoZ(double Z)
        {
            foreach (FacetSLT F in _Facets)
            {
                if (F.CortaPlanoZ(Z))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CortePlanoZ(double Z, out IList<RectaSLT> Corte)
        {
            Corte = new List<RectaSLT>();

            foreach (FacetSLT F in _Facets)
            {
                IList<RectaSLT> TempCorte = new List<RectaSLT>();
                if (Intersecciones.IntFacetPlano(F, Z, out TempCorte))
                {
                    foreach (RectaSLT TC in TempCorte)
                    {
                        Corte.Add(TC);
                    }
                }
            }

            return Corte.Count > 0;
        }
    }
}
