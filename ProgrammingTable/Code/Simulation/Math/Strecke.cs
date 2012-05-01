using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Math
{
    class Strecke
    {
        public Vector Stützvektor;
        public Vector Richtungsvektor;

        public Strecke(Vector stützV, Vector richtungsv)
        {
            Stützvektor = stützV;
            Richtungsvektor = richtungsv;
        }

        /// <summary>
        /// Berechnet den Abstand ab dem Startpunkt (=> Stützvektor) zu einem Punkt P 
        /// </summary>
        /// <param name="P"></param>
        /// <returns>negativ, wenn punkt vor streckenanfang liegt</returns>
        public double AbstandAbStartpunkt(Point P)
        {
            double r = 0;

            //Überprüfe ersten beiden positionen
            double d1 = PointAt(0.1).Distance(P);
            double d2 = PointAt(0.2).Distance(P);

            //wird der abstand größer, liefere den startpunktabstand negativ zurück
            if (d2 >= d1)
                return -1*d1;

            //der abstand wird kleiner
            //vergrößere so lange r, bis der abstand wieder größer wird
            d1 = 0;
            d2 = 0;

            while (PointAt(r).Distance(P) > PointAt(r+0.5).Distance(P))
            {
                r += 0.5;
            }

            //Nun berechne den abstand genauer - das minimum liegt zwischen r und r+0.5
            while (PointAt(r).Distance(P) > PointAt(r + 0.01).Distance(P))
            {
                r += 0.01;
            }

            //Gebe den abstand zurück
            return PointAt(r).Distance(P);
        }

        public Point PointAt(double t)
        {
            Point p = new Point();
            p.X = Stützvektor.X + t * Richtungsvektor.X;
            p.Y = Stützvektor.Y + t * Richtungsvektor.Y;
            return p;
        }
    }

    public static class GeradenMath
    {
        public static double Distance(this Point p, Point r)
        {
            return System.Math.Sqrt((p.X - r.X)*(p.X - r.X) + (p.Y - r.Y)*(p.Y - r.Y));
        }
    }
}
