using System;
using System.Collections.Generic;

namespace SLT_Printer
{
    public class Punto
    {
        public Punto()
        {
            X = double.NaN;
            Y = double.NaN;
            Z = double.NaN;
        }

        public Punto(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public bool EsValido
        {
            get
            {
                if(!double.IsNaN(X) && !double.IsNaN(Y) && !double.IsNaN(Z))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool EsIgual(Punto P)
        {
            bool Res = false;
            const double Margen = 0.00001;
            if (this.EsValido && P.EsValido)
            {
                if (Math.Abs(this.X - P.X) < Margen && Math.Abs(this.Y - P.Y) < Margen && Math.Abs(this.Z - P.Z) < Margen)
                {
                    Res = true;
                }
            }

            return Res;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double Distancia(Punto Pto)
        {
            double Res = Math.Sqrt(Math.Pow(this.X - Pto.X, 2) + Math.Pow(this.Y - Pto.Y, 2) + Math.Pow(this.Z - Pto.Z, 2));

            return double.Parse(Res.ToString());
        }
    }

    public class BBox
    {
        private void _Inicializa()
        {
            Minimos = new Punto();
            Maximos = new Punto();
        }

        public BBox()
        {
            _Inicializa();
        }

        public BBox(Punto InferiorIzquierda, Punto SuperiorDerecha)
        {
            _Inicializa();

            Minimos = InferiorIzquierda;
            Maximos = SuperiorDerecha;
        }

        public Punto Minimos { get; private set; }

        public Punto Maximos { get; private set; }

        public Punto Centro
        {
            get
            {
                return new Punto((Maximos.X - Minimos.X) / 2,
                                (Maximos.Y - Minimos.Y) / 2,
                                (Maximos.Z - Minimos.Z) / 2);
            }
        }

        public double Diagonal
        {
            get
            {
                if (Maximos.EsValido && Minimos.EsValido)
                {
                    return Minimos.Distancia(Maximos);
                }
                else
                {
                    return 0.0;
                }
            }
        }

        public void Actualizar(Punto Punto)
        {
            //si cualquiera no es válido lo reemplaza
            if (!Minimos.EsValido || !Maximos.EsValido)
            {
                if (!Minimos.EsValido)
                {
                    Minimos = Punto;
                }

                if (!Maximos.EsValido)
                {
                    Maximos = Punto;
                }
            }
            else
            {
                //compara con _InfIzq
                if(Punto.X<Minimos.X )
                {
                    Minimos = new Punto(Punto.X, Minimos.Y, Minimos.Z);
                }
                if (Punto.Y < Minimos.Y)
                {
                    Minimos = new Punto(Minimos.X, Punto.Y, Minimos.Z);
                }
                if (Punto.Z < Minimos.Z)
                {
                    Minimos = new Punto(Minimos.X, Minimos.Y, Punto.Z);
                }

                //compara con _SupDer
                if (Punto.X > Maximos.X)
                {
                    Maximos = new Punto(Punto.X, Maximos.Y, Maximos.Z);
                }
                if (Punto.Y > Maximos.Y)
                {
                    Maximos = new Punto(Maximos.X, Punto.Y, Maximos.Z);
                }
                if (Punto.Z > Maximos.Z)
                {
                    Maximos = new Punto(Maximos.X, Maximos.Y, Punto.Z);
                }
            }
        }

        public void Actualizar(IList<Punto> Puntos)
        {
            foreach(Punto p in Puntos )
            {
                Actualizar(p);
            }
        }
    }
}
