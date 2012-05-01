using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Recognition
{
    [Serializable()]
    public class RecognitionSettings
    {
        //General Settings ---------------------------------------------------------------------------

        /// <summary>
        /// The distance of the table surface from the Kinect [in mm]
        /// </summary>
        public int TableDistance;

        /// <summary>
        /// The range [in mm] the Measurements may vary +- from the stored TableDistance
        /// </summary>
        public int TableDistanceRange = 20;

        //Object Seperation ---------------------------------------------------------------------------



        /// <summary>
        /// The minimal Radius [depth-pixel] an Object must have to be counted as an object
        /// </summary>
        public int ObjectMinimalRadius = 3;

        /// <summary>
        /// The maximal Radius [depth-pixel] an Object can have
        /// </summary>
        public int ObjectMaximalRadius = 25;

        /// <summary>
        /// The Maximal Height [mm] an object can have
        /// </summary>
        public int ObjectMaximalHeight = 280;

        /// <summary>
        /// The maximal height [mm] a ponting hand may have. minimal height is the Object maximal height
        /// </summary>
        public int HandMaximalHeight = 1000;

        //Hand Recognition ---------------------------------------------------------------------------

        /// <summary>
        /// The amount of how much the angle is increased each time to check for a handpoint. lower values mean higher accuracy
        /// </summary>
        public double HandRecognitionDeltaAngle = 0.01;

        //Object Recognition ---------------------------------------------------------------------------

        /// <summary>
        /// If set to true, debug bitmaps (object recognition) will be saved)
        /// </summary>
        public bool SaveDebugMaps = false;

        /// <summary>
        /// If set to true, bitmaps of the objects will be saved
        /// </summary>
        public bool SaveDebugRotationBitmaps = false;

        /// <summary>
        /// The space [depth-px] between the points for the initial check of the boolmap
        /// </summary>
        public int ObjectRecognitionGridSpacing = 5;

        /// <summary>
        /// The amount of pixel the neighbour-count-rectangle is increased each time.
        /// </summary>
        public int ObjectRecognitionRectIncrease = 2;

        /// <summary>
        /// The percentage at what percentage of non-object pixels in a given set of pixels the neigbour-calculation for one pixel is stopped/finished.
        /// </summary>
        public double ObjectRecognitionNeighbourcountThreshold = 0.3;

        /// <summary>
        /// The amount of px the object's bitmap will be enlarged on each side
        /// </summary>
        public int ObjectVideoBitmapEnlargement = 0;
    }
}
