using System;

namespace SLT_Printer.SLT
{
    struct Stroke
    {
        public Modes Mode;
        //public VertexSLT Inicio;
        public VertexSLT Destino;
        public bool Pendiente;
        public double E;

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
