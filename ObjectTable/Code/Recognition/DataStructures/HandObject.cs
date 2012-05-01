using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;

namespace ObjectTable.Code.Recognition.DataStructures
{
    /// <summary>
    /// If a hand/arm of a Person is recognized, it is saved as a HandObject
    /// </summary>
    public class HandObject : TableObject
    {
        public HandObject()
        {
            //The hand doesn't have a center
            CenterDefined = false;
        }

        /// <summary>
        /// Defines the point where the Hand/Finger points at
        /// </summary>
        public TPoint PointsAt;

        public new object Clone()
        {
            HandObject obj = new HandObject();
            if (Center != null)
                obj.Center = Center.Clone();
            obj.CenterDefined = CenterDefined;
            if (ExtractedBitmap != null)
                obj.ExtractedBitmap = (Bitmap)ExtractedBitmap.Clone();
            obj.Height = Height;
            obj.ObjectID = ObjectID;
            obj.Radius = Radius;
            obj.DirectionVector = DirectionVector;
            obj.RotationDefined = RotationDefined;
            obj.TrackingStatus = TrackingStatus;
            obj.TrackingFrameExistence = TrackingFrameExistence;
            if (this.PointsAt != null)
            {
                obj.PointsAt = PointsAt.Clone();
            }
            return obj;
        }
    }
}
