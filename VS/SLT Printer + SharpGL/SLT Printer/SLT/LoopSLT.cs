using System.Collections.Generic;
using System.Linq;

namespace SLT_Printer.SLT
{
    public class LoopSLT
    {
        public IList<VertexSLT> Vertices;

        private double _ZMax;
        private double _Zmin;

        public LoopSLT()
        {
            Vertices = new List<VertexSLT>();
            _ZMax = double.NaN;
            _Zmin = double.NaN;
        }

        public void ActualizaBoundingZ()
        {
            foreach (VertexSLT V in Vertices)
            {
                //compara mínimos
                if (double.IsNaN(_Zmin) || V.Z < _Zmin)
                {
                    _Zmin = V.Z;
                }

                //compara Máximos
                if (double.IsNaN(_ZMax) || V.Z > _ZMax)
                {
                    _ZMax = V.Z;
                }
            }
        }

        public void Trasladar(VertexSLT T)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Trasladar(T);
            }
        }

        public bool CortaPlanoZ(double Z)
        {
            if (double.IsNaN(_Zmin) || double.IsNaN(_ZMax))
            {
                this.ActualizaBoundingZ();
            }

            if (Z >= _Zmin - 0.000001 && Z <= _ZMax + 0.000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CortePlanoZ(double Z, out LineSLT Corte)
        {
            return Intersecctions.IntLoopPlane(this, Z, out Corte);
        }

        public bool EsValido()
        {
            if (Vertices.Count() > 0)
            {
                bool Res = true;

                foreach (VertexSLT V in Vertices)
                {
                    if(!V.EsValido)
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
    
}
