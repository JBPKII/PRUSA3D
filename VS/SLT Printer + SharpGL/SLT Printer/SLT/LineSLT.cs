namespace SLT_Printer.SLT
{
    public class LineSLT
    {
        VertexSLT _V1;
        VertexSLT _V2;

        public LineSLT()
        {
            _V1 = new VertexSLT();
            _V2 = new VertexSLT();
        }

        public LineSLT(VertexSLT V1, VertexSLT V2)
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

        public bool EsIgual(LineSLT Valor)
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
}
