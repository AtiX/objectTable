using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Menu
{
    public class GraphicalRegion : ICloneable
    {
        public TPoint TopLeft;
        public TPoint BottomRight;

        public GraphicalRegion (TPoint TopLeft, TPoint BottomRight)
        {
            this.TopLeft = TopLeft;
            this.BottomRight = BottomRight;
        }
        public bool IsInRegion(TPoint p)
        {
            p.CalculateScreenfromDepthCoords();

            if ((p.ScreenX >= TopLeft.ScreenX) && (p.ScreenX <= BottomRight.ScreenX) && (p.ScreenY >= TopLeft.ScreenY) && (p.ScreenY <= BottomRight.ScreenY))
                return true;

            return false;
        }

        public object Clone()
        {
            return new GraphicalRegion(TopLeft.Clone(), BottomRight.Clone());
        }
    }
}
