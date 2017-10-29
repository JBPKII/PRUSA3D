using System.Collections.Generic;

namespace SLT_Printer.SLT
{
    public class Intersecctions
    {
        public static bool IntLinePlane(LineSLT Recta, double ZPlano, out LineSLT Corte)
        {
            bool Res = false;
            Corte = new LineSLT();

            if (Recta.V1.Z != Recta.V2.Z)
            {
                if (Recta.V1.Z > Recta.V2.Z)
                {
                    if (ZPlano >= Recta.V2.Z && ZPlano <= Recta.V1.Z)
                    {
                        Res = true;
                    }
                }
                else if (Recta.V1.Z < Recta.V2.Z)
                {
                    if (ZPlano >= Recta.V1.Z && ZPlano <= Recta.V2.Z)
                    {
                        Res = true;
                    }
                }

                if(Res)
                {
                    double ResX = 0.0;
                    double ResY = 0.0;

                    double Tempz = ((ZPlano - Recta.V1.Z) / (Recta.V2.Z - Recta.V1.Z));

                    ResX = (Tempz * (Recta.V2.X - Recta.V1.X)) + Recta.V1.X;
                    ResY = (Tempz * (Recta.V2.Y - Recta.V1.Y)) + Recta.V1.Y; 

                    Corte = new LineSLT(new VertexSLT(ResX, ResY, ZPlano),
                                         new VertexSLT(ResX, ResY, ZPlano));
                }
            }
            else //evito la división por cero
            {
                if (Recta.V1.Z == ZPlano)
                {
                    Corte = Recta;
                    Res = true;
                }
            }

            return Res;
        }

        public static bool IntLoopPlane(LoopSLT L1, double ZPlano, out LineSLT Corte)
        {
            bool Res = false;
            Corte = new LineSLT();

            IList<LineSLT> TempLstCorte = new List<LineSLT>();

            //omito los loop contenidos en el plano Z
            bool Omite = true; ;
            for (int i = 1; i < L1.Vertices.Count; i++)
            {
                if(L1.Vertices[i].Z != ZPlano)
                {
                    Omite = false;
                    break;
                }
            }

            if (!Omite)
            {
                for (int i = 1; i < L1.Vertices.Count; i++)
                {
                    LineSLT TempCorte = new LineSLT();

                    if (IntLinePlane(new LineSLT(L1.Vertices[i - 1], L1.Vertices[i]), ZPlano, out TempCorte))
                    {
                        TempLstCorte.Add(TempCorte);
                    }
                }

                LineSLT TempCorteCierre = new LineSLT();

                if (IntLinePlane(new LineSLT(L1.Vertices[L1.Vertices.Count - 1], L1.Vertices[0]), ZPlano, out TempCorteCierre))
                {
                    TempLstCorte.Add(TempCorteCierre);
                }
            }

            if (TempLstCorte.Count == 0)
            {
                Res = false;
            }
            else
            {
                Res = true;

                //busco los puntos didtintos
                IList<LineSLT> TempList = new List<LineSLT>();

                foreach (LineSLT V in TempLstCorte)
                {
                    bool TR = false;
                    foreach(LineSLT R in TempList)
                    {
                        if(R.EsIgual(V))
                        {
                            TR = true;
                            break;
                        }
                    }

                    if(!TR)
                    {
                        TempList.Add(V);
                    }
                }

                switch (TempList.Count)
                {
                    case 1:
                        Corte = TempList[0];
                        break;
                    case 2:
                        Corte = new LineSLT(TempList[0].V1, TempList[1].V1);
                        break;
                    case 3:
                        if (!TempList[0].V1.EsIgual (TempList[0].V2))
                        {
                            Corte = new LineSLT(TempList[0].V1, TempList[0].V2);
                        }
                        else if (!TempList[1].V1.EsIgual(TempList[1].V2))
                        {
                            Corte = new LineSLT(TempList[1].V1, TempList[1].V2);
                        }
                        else if(!TempList[2].V1.EsIgual (TempList[2].V2))
                        {
                            Corte = new LineSLT(TempList[2].V1, TempList[2].V2);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("No se han encontrado vértices distintos en ninguna recta.");
                        }
                        break;
                    case 0:
                    default:
                        System.Windows.Forms.MessageBox.Show("Se han encontrado " + TempList.Count.ToString() + "cortes con el plano Z en un loop.");
                        break;
                }
            }

            return Res;
        }

        public static bool IntFacetPlane(FacetSLT F1, double ZPlano, out IList<LineSLT> Corte)
        {
            bool Res = false;
            Corte = new List<LineSLT>();

            foreach (LoopSLT L in F1._Loops )
            {
                LineSLT TempCorte = new LineSLT();

                if (IntLoopPlane(L, ZPlano, out TempCorte))
                {
                    Corte.Add(TempCorte);
                }
            }

            if (Corte.Count == 0)
            {
                Res = false;
            }
            else
            {
                Res = true;

                //busco los puntos didtintos
                IList<LineSLT> TempList = new List<LineSLT>();

                foreach (LineSLT R in Corte)
                {
                    bool TR = false;
                    foreach (LineSLT T in TempList)
                    {
                        if(T.EsIgual(R))
                        {
                            TR = true;
                            break;
                        }
                    }

                    if (!TR)
                    {
                        TempList.Add(R);
                    }
                }

                //Los segmentos y los puntos de unión pueden repetirse, elimina los puntos de unión.
                IList<LineSLT> ResList = new List<LineSLT>();

                IList<LineSLT> TempListPuntos = new List<LineSLT>();

                foreach(LineSLT r in TempList)
                {
                    if (r.V1.EsIgual(r.V2))
                    {
                        //evalúa si es un punto aislado
                        //verifica que el punto está en algún segmento y lo omite
                        TempListPuntos.Add(r);
                    }
                    else
                    {
                        //es un segmento
                        ResList.Add(r);
                    }
                }

                foreach (LineSLT r in TempListPuntos) 
                {
                    bool TempBol = false;
                    foreach (LineSLT t in ResList)
                    {
                        if(t.V1.EsIgual(t.V1) || t.V1.EsIgual(t.V2))
                        {
                            //ya se encuentra
                            TempBol = true;
                            break;
                        }
                    }    
        
                    if(!TempBol)
                    {
                        ResList.Add(r);
                    }
                }

                Corte = ResList;
            }

            return Res;
        }
    }
}
