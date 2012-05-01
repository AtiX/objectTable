using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.Tracking
{
    [Serializable()]
    public class TrackingSettings
    {
        /// <summary>
        /// The maximum amount (frames) the old points are stored
        /// </summary>
        public int MaximumHistoryCount = 20;

        //--------------------------------------- Certainity settings

        /// <summary>
        /// The required certainity for non-moving long term objects (first track for each round)
        /// </summary>
        public double LtNonMovingCertainity = 0.9;

        /// <summary>
        /// The required certainity for moving long term objects (second check)
        /// </summary>
        public double LtMovingCertainity = 0.8;

        /// <summary>
        /// The required certainity for shortly added objects (third check)
        /// </summary>
        public double ShortlyAddedCertainity = 0.85;

        /// <summary>
        /// The minimal age (frames) an object has to has before it can become a long term tracked object
        /// </summary>
        public int MinimalLongTermAge = 13;

        /// <summary>
        /// If true, the Tracker will add long term objects that were visible, but arent visible anymore for the specified maximum amount of frames
        /// </summary>
        public bool GuessLongTermObjects = true;

        /// <summary>
        /// The maximum amount of frames an invisible LongTerm object will be guessed before it will be removed
        /// </summary>
        public int GuessObjectMaxAge = 8;
        
        /// <summary>
        /// The minimal distance an object has to been moved (in relation to the last frame) [depth pixel] to get in the MovedObject-List
        /// </summary>
        public int ObjectMoveListMinDistance = 4;

        /// <summary>
        /// If true, the Object position and rotation will be exchanged with an average value of the last 3 frames
        /// </summary>
        public bool SmoothObjects = false;
    }
}
