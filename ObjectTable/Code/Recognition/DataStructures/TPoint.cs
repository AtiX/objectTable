using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.PositionMapping;

namespace ObjectTable.Code.Recognition.DataStructures
{
    /// <summary>
    /// Because The Pixel-Coordinates (X,Y) is different on each type of image, this structure stores all the values for one point
    /// </summary>
    public class TPoint
    {
        /// <summary>
        /// The position on the depth-image
        /// </summary>
        public int DepthX, DepthY;

        /// <summary>
        /// The position on the Kinect Color-image
        /// </summary>
        public int ColorX, ColorY;

        /// <summary>
        /// The position on the Beamer-screen
        /// </summary>
        public int ScreenX, ScreenY;

        public enum PointCreationType
        {
            depth,
            color,
            screen
        };

        /// <summary>
        /// The Imagetype the point was initially created for
        /// </summary>
        public PointCreationType InitialCreationType;

        /// <summary>
        /// Creates a Point
        /// </summary>
        /// <param name="X">The X Coordinate</param>
        /// <param name="Y">The Y Coordinate</param>
        /// <param name="type">Defines to which Imagetype the coordinates should be initially assigned</param>
        public TPoint(int X, int Y, PointCreationType type)
        {
            switch(type)
            {
                case PointCreationType.color:
                    ColorX = X;
                    ColorY = Y;
                    break;
                case PointCreationType.depth:
                    DepthX = X;
                    DepthY = Y;
                    break;
                case PointCreationType.screen:
                    ScreenX = X;
                    ScreenY = Y;
                    break;
            }

            InitialCreationType = type;
        }

        /// <summary>
        /// Calculates the distance to another Point
        /// </summary>
        /// <param name="SecondPoint"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public double DistanceTo(TPoint SecondPoint, PointCreationType layer)
        {
            int X1=0, X2=0, Y1=0, Y2=0;
            //Assign coords
            switch (layer)
            {
                case PointCreationType.color:
                    X1 = this.ColorX;
                    Y1 = this.ColorY;
                    X2 = SecondPoint.ColorX;
                    Y2 = SecondPoint.ColorY;
                    break;
                case PointCreationType.depth:
                    X1 = this.DepthX;
                    Y1 = this.DepthY;
                    X2 = SecondPoint.DepthX;
                    Y2 = SecondPoint.DepthY;
                    break;
                case PointCreationType.screen:
                    X1 = this.ScreenX;
                    Y1 = this.ScreenY;
                    X2 = SecondPoint.ScreenX;
                    Y2 = SecondPoint.ScreenY;
                    break;
            }

            //Calculate distance
            return Math.Sqrt((X1 - X2)*(X1 - X2) + (Y1 - Y2)*(Y1 - Y2));
        }

        public bool CalculateScreenfromDepthCoords()
        {
            if ((DepthX != 0)&&(DepthY != 0))
            {
                TPoint p = PositionMapper.GetScreenCoordsfromDepth(this);
                this.ScreenX = p.ScreenX;
                this.ScreenY = p.ScreenY;

                return true;
            }
            else
            {
                return false;
            }
        }

        public TPoint Clone()
        {
            TPoint p = new TPoint(this.DepthX, this.DepthY, PointCreationType.depth);
            p.ColorX = ColorX;
            p.ColorY = ColorY;
            p.ScreenX = ScreenX;
            p.ScreenY = ScreenY;
            p.InitialCreationType = InitialCreationType;
            return p;
        }
    }
}
