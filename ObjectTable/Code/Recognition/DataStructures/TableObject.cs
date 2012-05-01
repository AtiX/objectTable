using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;

namespace ObjectTable.Code.Recognition.DataStructures
{
    /// <summary>
    /// The Superclass of any Object that is recognized on or over the table
    /// </summary>
    public class TableObject : ICloneable
    {
        /// <summary>
        /// If tracked, each object gets assignet an unique Object ID
        /// </summary>
        public int ObjectID;

        /// <summary>
        /// NotTracked: The object isn't tracked
        /// ShortlyTracked: The object is recognized and tracked, but appeared recently
        /// LongTermTracked: The object is tracked for a certain amount of frames
        /// LTGuessed: The object was long therm tracked, but isn't visible at the moment and is supplied for a certain amount of time
        /// </summary>
        public enum ETrackingStatus
        {
            NotTracked,
            ShortlyTracked,
            LongTermTracked,
            LongTermGuessed
        };

        /// <summary>
        /// The object's tracking status (refer to ETrackingStatus)
        /// </summary>
        public ETrackingStatus TrackingStatus = ETrackingStatus.NotTracked;

        /// <summary>
        /// for how many frames does the object exist and is tracked? 
        /// </summary>
        public int TrackingFrameExistence = 0;

        /// <summary>
        /// The Center of the Object, used as the object's Position
        /// </summary>
        public TPoint Center;

        /// <summary>
        /// Sets whether the object has a defined Center/Position
        /// </summary>
        public bool CenterDefined = false;

        /// <summary>
        /// The size of the Object
        /// </summary>
        public int Radius;

        /// <summary>
        /// Defines how many Milimeters the object is above the table surface
        /// </summary>
        public int Height;

        /// <summary>
        /// The Rotation of the object, represented by a vector
        /// </summary>
        public Vector DirectionVector;

        /// <summary>
        /// Defines whether the object's rotation is recognized and defined
        /// </summary>
        public bool RotationDefined = false;

        /// <summary>
        /// An extract of the Color-Image, showing the object
        /// </summary>
        public Bitmap ExtractedBitmap;

        public object Clone()
        {
            TableObject obj = new TableObject();
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
            return obj;
        }
    }
}
