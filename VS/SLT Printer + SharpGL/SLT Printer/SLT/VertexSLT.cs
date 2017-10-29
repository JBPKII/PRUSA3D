using System;

namespace SLT_Printer.SLT
{
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
}
