using System;

namespace SLT_Printer.SLT
{
    public class VertexSLT
    {
        double _Z;

        public VertexSLT()
        {
            X = double.NaN;
            Y = double.NaN;
            _Z = double.NaN;
        }

        public VertexSLT(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            _Z = Z;
        }

        public bool EsValido
        {
            get
            {
                if (double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(_Z))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public double X { get; private set; }
        public double Y { get; private set; }
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
                return (float)X;
            }
        }
        public float Yf
        {
            get
            {
                return (float)Y;
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
            X += T.X;
            Y += T.Y;
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
