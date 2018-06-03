using System;

namespace SLT_Printer.SLT
{
    internal class StrokeSLT
    {
        public Modes Mode;
        //public VertexSLT Inicio;
        public VertexSLT Destino;
        public bool Pendiente;
        public double E;

        public StrokeSLT()
        {
            Mode = Modes.ModeTraslation;
            Destino = new VertexSLT(0.0, 0.0, 0.0);
            Pendiente = true;
            E = 0.0;
        }

        public StrokeSLT(Modes mode, VertexSLT destino, bool pendiente, double e)
        {
            Mode = mode;
            Destino = destino;
            Pendiente = pendiente;
            E = e;
        }

        public string ToPrinter()
        {
            string Res = "";
            if (double.IsNaN(E))
            {
                E = 0.0;
            }

            if (Destino.EsValido)
            {
                Res += "M =";
                Res += Convert.ToSByte(Mode).ToString();
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

                //Res = Res.Replace(',', '.');
            }

            return Res;
        }
    }
}
