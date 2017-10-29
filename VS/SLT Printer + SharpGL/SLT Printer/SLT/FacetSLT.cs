
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLT_Printer.SLT
{
    public class FacetSLT
    {
        public VertexSLT _Normal;
        public IList<LoopSLT> _Loops;
        public UInt16 _Attribute;

        public FacetSLT()
        {
            _Normal = new VertexSLT();
            _Attribute = 0;
            _Loops = new List<LoopSLT>();
        }

        public void Trasladar(VertexSLT T)
        {
            for (int i = 0; i < _Loops.Count; i++)
            {
                _Loops[i].Trasladar(T);
            }
        }

        public bool EsValido()
        {
            if (_Normal.EsValido && _Loops.Count() > 0)
            {
                bool Res = true;

                foreach (LoopSLT L in _Loops)
                {
                    if (!L.EsValido())
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

        public bool CortaPlanoZ(double Z)
        {
            foreach(LoopSLT L in _Loops)
            {
                if(L.CortaPlanoZ(Z))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CortePlanoZ(double Z, out IList<LineSLT> Corte)
        {
            return Intersecctions.IntFacetPlane(this, Z, out Corte);
        }
    }
}
