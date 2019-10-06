namespace SLT_Printer.SLT
{
    public class LineSLT
    {
        public VertexSLT V1 { get; }
        public VertexSLT V2 { get; }

        public LineSLT()
        {
            V1 = new VertexSLT();
            V2 = new VertexSLT();
        }

        public LineSLT(VertexSLT V1, VertexSLT V2)
        {
            this.V1 = V1;
            this.V2 = V2;
        }

        public bool EsValido
        {
            get
            {
                if (V1.EsValido && V2.EsValido)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Trasladar(VertexSLT T)
        {
            V1.Trasladar(T);
            V2.Trasladar(T);
        }

        public bool EsIgual(LineSLT Valor)
        {
            if (V1.EsIgual(Valor.V1) && V2.EsIgual(Valor.V2))
            {
                return true;
            }
            else if (V2.EsIgual(Valor.V1) && V1.EsIgual(Valor.V2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
