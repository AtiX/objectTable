using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.Rotation
{
    [Serializable()]
    public class RotationRecognitionSettings
    {
        /// <summary>
        /// Average - given percentage counts as black
        /// </summary>
        public double blackThresholdPercentage = 0.3;

        /// <summary>
        /// average + given percentage counts as white
        /// </summary>
        public double whiteThresholdPercentage = 0.3;

        /// <summary>
        /// If true, Pixels in between and white pixels will all be counted as white. improves recognition on white surfaces
        /// </summary>
        public bool whiteBackground = false;

        /// <summary>
        /// If more than the specific amount of objects exist, a new thread will be started
        /// </summary>
        public int MaxObjectsPerThread = 15;
    }
}
