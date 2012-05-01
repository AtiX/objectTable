using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.Recognition.DataStructures
{
    class TRectangle
    {
        public int X, Y;
        public int X2, Y2;

        public int Width
        {
            get
            {
                return X2 - X;
            }
        }

        public int Height
        {
            get
            {
                return Y2 - Y;
            }
        }

        public TRectangle(int X, int Y, int X2, int Y2)
        {
            this.X = X;
            this.Y = Y;
            this.X2 = X2;
            this.Y2 = Y2;
        }

        public TRectangle(int X, int Y, int Width, int Height, bool CutIntoBound, TRectangle bounds)
        {
            this.X = X;
            this.Y = Y;
            this.X2 = X + Width;
            this.Y2 = Y + Height;
            if (CutIntoBound)
                CutIntoBounds(bounds.X, bounds.Y, bounds.X2, bounds.Y2);
        }

        public TRectangle(int CenterX, int CenterY, int side, bool CutIntoBound, TRectangle bounds)
        {
            this.X = (int) Math.Round((double) (CenterX - side/2));
            this.Y = (int) Math.Round((double) (CenterY - side/2));

            this.X2  = (int) Math.Round((double)(CenterX + side/2));
            this.Y2 = (int) Math.Round((double)(CenterY + side/2));

            if (CutIntoBound)
                CutIntoBounds(bounds.X, bounds.Y, bounds.X2, bounds.Y2);
        }

        /// <summary>
        /// Cuts off the rectangle if it is exceeding the given bounds
        /// </summary>
        /// <param name="Xmin"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void CutIntoBounds(int Xmin, int Ymin, int Xmax, int Ymax)
        {
            if (X < Xmin)
                X = Xmin;
            if (X > Xmax)
                X = Xmax;
            if (Y < Ymin)
                Y = Ymin;
            if (Y > Ymax)    
                Y = Ymax;

            if (X2 < Xmin)
                X2 = Xmin;
            if (X2 > Xmax)
                X2 = Xmax;
            if (Y2 < Ymin)
                Y2 = Ymin;
            if (Y2 > Ymax)
                Y2 = Ymax;
        }
    }
}
