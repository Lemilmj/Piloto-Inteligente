using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piloto_Inteligente
{
    public static class Unidades
    {
        public const double g = 9.80665;
        public static double NewtontoPound(double NewtonValue)
        {
            double OneNewton = 0.224809;
            return (NewtonValue * OneNewton);
        }
        public static double PoundtoNewton(double PoundValue)
        {
            double OnePound = 4.4482216;
            return (PoundValue * OnePound);
        }
       public static double FuerzaNeta(double aacMaxAlc, double angIncl, double MV, double MC)
        {
            double MasaTot = MV + MC;
            double sin = Math.Abs(Math.Sin(angIncl));
            return (aacMaxAlc * sin * MasaTot);
        }
        public static double[] CombReq(double RevReq, double ConsumoPR, double NivelComb, double DurDesp)
        {
            double ConsPorSeg = RevReq / 60;
            ConsPorSeg = ConsPorSeg * ConsumoPR;
            double ConsporTiempo = ConsPorSeg * DurDesp;
            ConsporTiempo = ConsporTiempo / 1000;
            return new double[] { ConsporTiempo, ConsPorSeg };
        }
        public static double RevReq(double Maxrpm, double MaxPot, double PotReq)
        {
            double torque = MaxPot * 9550;
            torque = torque / Maxrpm;
            double Revreq = PotReq * 9550;
            Revreq = Revreq / torque;
            return Revreq;
        }
        public static double CalcPot(double Fuerza, double Distancia, double Tiempo)
        {
            double Potencia = Fuerza * Distancia;
            Potencia = Potencia / Tiempo;
            Potencia = Potencia * 0.0013406254545454;
            return Potencia;
        }
    }
}
