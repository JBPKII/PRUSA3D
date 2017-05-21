using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SLT_Printer
{
    class FacetSLT
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

        public bool EsValido()
        {
            if (_Normal.EsValido() && _Loops.Count() > 0)
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
    }

    class LoopSLT
    {
        public IList<VertexSLT> _Vertex;

        public LoopSLT()
        {
            _Vertex = new List<VertexSLT>();
        }
        public bool EsValido()
        {
            if(_Vertex.Count()>0)
            {
                bool Res = true;

                foreach (VertexSLT V in _Vertex)
                {
                    if(!V.EsValido())
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

    class VertexSLT
    {
        public float _X;
        public float _Y;
        public float _Z;

        public VertexSLT()
        {
            _X = float.NaN;
            _Y = float.NaN;
            _Z = float.NaN;
        }

        public VertexSLT(float X, float Y, float Z)
        {
            _X = X;
            _Y = Y;
            _Z = Z;
        }

        public bool EsValido()
        {
            if (!float.IsNaN(_X) && !float.IsNaN(_Y) && !float.IsNaN(_Z))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool EsIgual(VertexSLT Valor)
        {
            bool Res = false;

            if (_X == Valor._X)
            {
                if (_Y == Valor._Y)
                {
                    if (_Z == Valor._Z)
                    {
                        Res = true;
                    }
                }
            }

            return Res;
        }
    }

    class SolidoSLT
    {
        private string _Nombre;
        private IList<string> _Fallos;
        private IList<FacetSLT> _Facets;
        private VertexSLT _IzqFrontInf;
        private VertexSLT _DerPostSup;

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
                return new VertexSLT(
                    (_IzqFrontInf._X + _DerPostSup._X) / 2, 
                    (_IzqFrontInf._Y + _DerPostSup._Y) / 2, 
                    (_IzqFrontInf._Z + _DerPostSup._Z) / 2);
            }
        }

        public float Ancho
        {
            get
            {
                return Math.Abs(_IzqFrontInf._X - _DerPostSup._X);
            }
        }

        public float Largo
        {
            get
            {
                return Math.Abs(_IzqFrontInf._Y - _DerPostSup._Y);
            }
        }

        public float Alto
        {
            get
            {
                return Math.Abs(_IzqFrontInf._Z - _DerPostSup._Z);
            }
        }


        private void _Inicializa()
        {
            //Inicializa las variables
            _Nombre = "";
            _Fallos = new List<string>();
            _Facets = new List<FacetSLT>();
            _IzqFrontInf = new VertexSLT();
            _DerPostSup = new VertexSLT();
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
                        foreach (VertexSLT V in L._Vertex)
                        {
                            //compara mínimos con _Izq
                            if (double.IsNaN(_IzqFrontInf._X) || V._X < _IzqFrontInf._X)
                            {
                                _IzqFrontInf._X = V._X;
                            }
                            if (double.IsNaN(_IzqFrontInf._Y) || V._Y < _IzqFrontInf._Y)
                            {
                                _IzqFrontInf._Y = V._Y;
                            }
                            if (double.IsNaN(_IzqFrontInf._Z) || V._Z < _IzqFrontInf._Z)
                            {
                                _IzqFrontInf._Z = V._Z;
                            }

                            //compara máximos con _Der
                            if (double.IsNaN(_DerPostSup._X) || V._X > _DerPostSup._X)
                            {
                                _DerPostSup._X = V._X;
                            }
                            if (double.IsNaN(_DerPostSup._Y) || V._Y > _DerPostSup._Y)
                            {
                                _DerPostSup._Y = V._Y;
                            }
                            if (double.IsNaN(_DerPostSup._Z) || V._Z > _DerPostSup._Z)
                            {
                                _DerPostSup._Z = V._Z;
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

            Where each n or v is a floating-point number in sign-mantissa-"e"-sign-exponent format, e.g., "2.648000e-002" (noting that each v must be non-negative). The file concludes with:

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
                                TempLoop._Vertex.Add(new VertexSLT(Convert.ToSingle(TempStr[0]), Convert.ToSingle(TempStr[1]), Convert.ToSingle(TempStr[2])));
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

            Each triangle is described by twelve 32-bit floating-point numbers: three for the normal and then three for the X/Y/Z coordinate of each vertex – just as with the ASCII version of STL.
            After these follows a 2-byte ("short") unsigned integer that is the "attribute byte count" – in the standard format, this should be zero because most software does not understand anything else.

            Floating-point numbers are represented as IEEE floating-point numbers and are assumed to be little-endian, although this is not stated in documentation.

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
                        TempFacet._Normal = new VertexSLT(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());

                        //REAL32[3] – Vertex 1
                        TempLoop._Vertex.Add(new VertexSLT(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle()));
                        //REAL32[3] – Vertex 2
                        TempLoop._Vertex.Add(new VertexSLT(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle()));
                        //REAL32[3] – Vertex 3
                        TempLoop._Vertex.Add(new VertexSLT(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle()));
                        
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

        public bool TestSLT()
        {
            bool Res = true;
            //http://www.ennex.com/~fabbers/StL.asp

            //Verifica la Orientación de cada faceta.
            foreach (FacetSLT  F in _Facets)
            {
                foreach (LoopSLT L in F._Loops)
                {
                    if(L._Vertex.Count()>=3)
                    {
                        VertexSLT TempV = ProductoVectorialUnitario(L._Vertex[0], L._Vertex[1], L._Vertex[2],true);
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
                    foreach (VertexSLT V in L._Vertex)                    {
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

            return Res;
        }

        private VertexSLT ProductoVectorialUnitario(VertexSLT V1, VertexSLT V2, VertexSLT V3, bool Unitario = true)
        {
            double V2x = V2._X - V1._X;
            double V2y = V2._Y - V1._Y;
            double V2z = V2._Z - V1._Z;

            double V3x = V3._X - V1._X;
            double V3y = V3._Y - V1._Y;
            double V3z = V3._Z - V1._Z;

            double X = (V2._Y * V3._Z) - (V2._Z * V3._Y);
            double Y = (V2._Z * V3._X) - (V2._X * V3._Z);
            double Z = (V2._X * V3._Y) - (V2._Y * V3._X);

            VertexSLT Res = new VertexSLT();

            if (Unitario)
            {
                double ModuloRes = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));

                Res._X = Convert.ToSingle(X / ModuloRes);
                Res._Y = Convert.ToSingle(Y / ModuloRes);
                Res._Z = Convert.ToSingle(Z / ModuloRes);
            }
            else
            {
                Res._X = Convert.ToSingle(X);
                Res._Y = Convert.ToSingle(Y);
                Res._Z = Convert.ToSingle(Z);
            }

            return Res;
        }

        public bool Renderiza(Model XNAModel)
        {
            bool Res = true;

            //http://msdn.microsoft.com/en-us/library/bb197293%28v=xnagamestudio.31%29.aspx

            if (_Facets.Count > 0)
            {
                foreach (FacetSLT F in _Facets)
                {
                    foreach (LoopSLT L in F._Loops)
                    {
                        bool Representar = true;
                        switch (L._Vertex.Count)
                        {
                            case 0:
                            case 1:
                            case 2:
                                Representar = false;
                                break;
                            case 3:
                                //gl.Begin(OpenGL.GL_TRIANGLES);
                                break;
                            default:
                                //gl.Begin(OpenGL.GL_POLYGON);
                                break;
                        }

                        if (Representar)
                        {
                            foreach (VertexSLT V in L._Vertex)
                            {
                                //gl.Color((byte)200, (byte)200, (byte)220);
                                //gl.Vertex(V._X, V._Y, V._Z);
                            }
                        }
                    }
                }
            }
            
            //gl.End();

            return Res;
        }
    }
}
