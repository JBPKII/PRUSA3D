using System;
using System.Collections.Generic;

namespace SLT_Printer
{
    public class Punto
    {
        double _X;
        double _Y;
        double _Z;

        public Punto()
        {
            _X = double.NaN;
            _Y = double.NaN;
            _Z = double.NaN;
        }

        public Punto(double X, double Y, double Z)
        {
            _X = X;
            _Y = Y;
            _Z = Z;
        }

        public bool EsValido
        {
            get
            {
                if(!double.IsNaN(_X) && !double.IsNaN(_Y) && !double.IsNaN(_Z))
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
                if (Math.Abs(this._X - P._X) < Margen && Math.Abs(this._Y - P._Y) < Margen && Math.Abs(this._Z - P._Z) < Margen)
                {
                    Res = true;
                }
            }

            return Res;
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

        public double Distancia(Punto Pto)
        {
            double Res = Math.Sqrt(Math.Pow(this.X - Pto.X, 2) + Math.Pow(this.Y - Pto.Y, 2) + Math.Pow(this.Z - Pto.Z, 2));

            return double.Parse(Res.ToString());
        }
    }

    public class BBox
    {
        Punto _InfIzq; //minimos
        Punto _SupDer; //Máximos

        private void _Inicializa()
        {
            _InfIzq = new Punto();
            _SupDer = new Punto();
        }

        public BBox()
        {
            _Inicializa();
        }

        public BBox(Punto InferiorIzquierda, Punto SuperiorDerecha)
        {
            _Inicializa();

            _InfIzq = InferiorIzquierda;
            _SupDer = SuperiorDerecha;
        }

        public Punto Minimos
        {
            get
            {
                return _InfIzq;
            }
        }

        public Punto Maximos
        {
            get
            {
                return _SupDer;
            }
        }

        public Punto Centro
        {
            get
            {
                return new Punto((_SupDer.X - _InfIzq.X) / 2,
                                (_SupDer.Y - _InfIzq.Y) / 2,
                                (_SupDer.Z - _InfIzq.Z) / 2);
            }
        }

        public double Diagonal
        {
            get
            {
                if (_SupDer.EsValido && _InfIzq.EsValido)
                {
                    return _InfIzq.Distancia(_SupDer);
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
            if (!_InfIzq.EsValido || !_SupDer.EsValido)
            {
                if (!_InfIzq.EsValido)
                {
                    _InfIzq = Punto;
                }

                if (!_SupDer.EsValido)
                {
                    _SupDer = Punto;
                }
            }
            else
            {
                //compara con _InfIzq
                if(Punto.X<_InfIzq.X )
                {
                    _InfIzq = new Punto(Punto.X, _InfIzq.Y, _InfIzq.Z);
                }
                if (Punto.Y < _InfIzq.Y)
                {
                    _InfIzq = new Punto(_InfIzq.X, Punto.Y, _InfIzq.Z);
                }
                if (Punto.Z < _InfIzq.Z)
                {
                    _InfIzq = new Punto(_InfIzq.X, _InfIzq.Y, Punto.Z);
                }

                //compara con _SupDer
                if (Punto.X > _SupDer.X)
                {
                    _SupDer = new Punto(Punto.X, _SupDer.Y, _SupDer.Z);
                }
                if (Punto.Y > _SupDer.Y)
                {
                    _SupDer = new Punto(_SupDer.X, Punto.Y, _SupDer.Z);
                }
                if (Punto.Z > _SupDer.Z)
                {
                    _SupDer = new Punto(_SupDer.X, _SupDer.Y, Punto.Z);
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
