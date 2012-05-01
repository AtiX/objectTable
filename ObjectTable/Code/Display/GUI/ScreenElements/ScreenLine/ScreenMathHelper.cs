using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine
{
    /// <summary>
    /// Contains Math functions to help calculate UI-Element positions etc
    /// </summary>
    public static class ScreenMathHelper
    {
        /// <summary>
        /// Rescales a 2D-Vector to the desired length
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desiredLength"></param>
        /// <returns></returns>
        public static Vector RescaleVector(Vector input, double desiredLength)
        {
            double currentLength = input.Length;
            double scaleFactor = desiredLength/currentLength;

            Vector result = new Vector(input.X, input.Y);

            result.X = result.X*scaleFactor;
            result.Y = result.Y*scaleFactor;

            return result;
        }

        /// <summary>
        /// Returns the angle of the vector in Degree
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double VectorToDegree(Vector v)
        {
            v = RescaleVector(v, 1.0);
            //-1 due to the different directions of the y-axis in computer/mathematical coord-systems
            double winkel = Math.Atan2(v.X, v.Y*-1);
            //radians -> degree
            return (360/(2*Math.PI))*winkel;
        }

        public static Vector Average(List<Vector> vectorList)
        {
            Vector v = new Vector();
            foreach (Vector a in vectorList)
            {
                //Equal length
                Vector tmp = ScreenMathHelper.RescaleVector(a, 1.0);
                v.X += tmp.X;
                v.Y += tmp.Y;
            }
            v.X = v.X / vectorList.Count();
            v.Y = v.Y / vectorList.Count();
            return v;
        }
    }
}
