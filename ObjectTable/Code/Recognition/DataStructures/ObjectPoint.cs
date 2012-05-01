using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.Recognition.DataStructures
{
    class ObjectPoint
    {
        public int X, Y;
        public int Neigbours;
        public int RectSize;

        public ObjectPoint(int X, int Y, int Neigbours, int RectSize)
        {
            this.X = X;
            this.Y = Y;
            this.Neigbours = Neigbours;
            this.RectSize = RectSize;
        }
    }
}
