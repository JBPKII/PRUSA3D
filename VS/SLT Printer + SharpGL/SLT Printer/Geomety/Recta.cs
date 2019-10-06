namespace SLT_Printer
{
    public class Segmento
    {
        double _Longitud;

        public Segmento()
        {
            Inicio = new Punto();
            Final = new Punto();
            _Longitud = double.NaN;
        }

        public Segmento(Punto Inicio, Punto Final)
        {
            this.Inicio = Inicio;
            this.Final = Final;
            _Longitud = double.NaN;
        }

        public Punto Inicio { get; }

        public Punto Final { get; }

        double Longitud
        {
            get
            {
                if (double.IsNaN(_Longitud))
                {
                    _Longitud = Inicio.Distancia(Final);
                }

                return _Longitud;
            }
        }

        //a que lado se encuentra el punto del segmento
        //retorna -1 Derecha, 0, 1 Izquierda
        public static int Lado(Punto Pivote, Punto Segm, Punto Punt)
        {
            int Res = 0;

            double Calc = ((Segm.X - Pivote.X) * (Punt.Y - Pivote.Y)) - ((Segm.Y - Pivote.Y) * (Punt.X - Pivote.X));

            if (Calc > 0)
            {
                Res = 1;
            }
            else if (Calc < 0)
            {
                Res = -1;
            }
            /*else
            {
                Res = 0;
            }*/

            return Res;
        }

        public bool Corta(Segmento Segm)
        {
            bool Res = false;
            //corte entre this y Segm
            int Lado1, Lado2;

            Lado1 = Lado(this.Inicio, this.Final, Segm.Inicio);
            Lado2 = Lado(this.Final, this.Inicio, Segm.Final);

            if (Lado1 == 0 && Lado2 == 0)
            {
                //Segmento superpuesto
                //Res = false;
            }
            else if (Lado1 == 0)
            {
                Res = true;
            }
            else if (Lado2 == 0)
            {
                Res = true;
            }
            else
            {
                if ((Lado1 > 0 && Lado2 > 0) || (Lado1 < 0 && Lado2 < 0))
                {
                    Res = true;
                }
                /*else
                {
                    Res = false;
                }*/
            }

            return Res;
        }

        //intersección real
        public bool Corta(Segmento Segm, out Punto Int)
        {
            if (Corta(Segm))
            {
                //calculo el punto de corte
                return Interseccion2D(Segm, out Int);
            }
            else
            {
                Int = new Punto();
                return false;
            }
        }

        //intersección ficticia o real
        public bool Interseccion2D(Segmento Segm, out Punto Int)
        {
            Int = new Punto();

            double resX, resY, resZ;
            double ma, mb;

            //calculo la pendiente de this, ma
            if (this.Inicio.X == this.Final.X)
            {
                ma = double.PositiveInfinity;
            }
            else
            {
                ma = (this.Final.Y - this.Inicio.Y) / (this.Final.X - this.Inicio.X);
            }

            //calculo la pendiente de Segm, mb
            if (Segm.Inicio.X == Segm.Final.X)
            {
                mb = double.PositiveInfinity;
            }
            else
            {
                mb = (Segm.Final.Y - Segm.Inicio.Y) / (Segm.Final.X - Segm.Inicio.X);
            }

            if (ma == mb)
            {
                //paralelas o coincidentes
                return false;
            }
            else
            {
                if (double.IsPositiveInfinity(ma))
                {
                    //this es vertical
                    resX = this.Inicio.X;
                    resY = mb * (resX - Segm.Inicio.X) + Segm.Inicio.Y;
                }
                else if (double.IsPositiveInfinity(mb))
                {
                    //Segm es vertical
                    resX = Segm.Inicio.X;
                    resY = ma * (resX - this.Inicio.X) + this.Inicio.Y;
                }
                else
                {
                    //se cortan
                    resX = (ma * this.Inicio.X - mb * Segm.Inicio.X + Segm.Inicio.Y - this.Inicio.Y) / (ma - mb);
                    resY = ma * (resX - this.Inicio.X) + this.Inicio.Y;
                }

                //calculo resZ
                resZ = this.Inicio.Z;

                Int = new Punto(resX, resY, resZ);
                return true;
            }
        }

        public Segmento Equidista(double Equidistancia)
        {
            //Equidistancia > 0 => equidisa hacia la izquierda

            Punto V1 = new Punto(this.Inicio.X,
                                this.Inicio.Y,
                                0.0);//2D
            Punto V2 = new Punto(this.Final.X,
                                this.Final.Y,
                                0.0);//2D

            Punto V = new Punto(V2.X - V1.X,
                                V2.Y - V1.Y,
                                0.0);//2D
            double Vd = V1.Distancia(V2);

            if (Vd == 0.0)
            {
                //Punto inicial y final son el mismo
                return this;
            }
            else
            {
                //vector unitario
                Punto v = new Punto(V.X / Vd,
                                    V.Y / Vd,
                                    0.0);

                //vector equidistancia perpendicular
                Punto u = new Punto(-v.Y * Equidistancia,
                                    v.X * Equidistancia,
                                    0.0);

                Segmento Res = new Segmento(
                    new Punto(this.Inicio.X + u.X, this.Inicio.Y + u.Y, this.Inicio.Z + u.Z),
                    new Punto(this.Final.X + u.X, this.Final.Y + u.Y, this.Final.Z + u.Z));

                return Res;
            }
        }
    }

    class Rayo
    {
        Punto _Inicio;
        Punto _Final;

        private void _Inicializa()
        {
            _Inicio = new Punto();
            _Final = new Punto();
        }

        public Rayo()
        {
            _Inicializa();
        }

        public Rayo(Punto Inicio, Punto Final)
        {
            _Inicializa();
            _Inicio = Inicio;
            _Final = Final;
        }
    }
}
