using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect.Structures;

namespace ObjectTable.Code.Recognition.DataStructures
{
    /// <summary>
    /// Contains data used for debugging, supplied by the RecognitionManager
    /// </summary>
    public class RecognitionDataPacket : ICloneable
    {
        public DepthImage rawDepthImage;
        public DepthImage correctedDepthImage;
        public List<TableObject> TableObjects;
        public HandObject HandObj;
        public bool[,] objectmap;
        public int[,,] neighbourmap;

        public Bitmap bmpRawDepth;
        public Bitmap bmpCorrectedDepth;
        public Bitmap bmpVideoFrame;

        /// <summary>
        /// the time [ms] how long the object recognition did take
        /// </summary>
        public int RecognitionDuration;

        public RecognitionDataPacket()
        {
            rawDepthImage = null;
            correctedDepthImage = null;
            TableObjects = new List<TableObject>();
            HandObj = null;
            objectmap = null;
            neighbourmap = null;

            bmpCorrectedDepth = null;
            bmpRawDepth = null;
            bmpVideoFrame = null;

            RecognitionDuration = 0;
        }

        private List<TableObject> CloneTO()
        {
            return TableObjects.Select(o => (TableObject) o.Clone()).ToList();
        }

        public object Clone()
        {
            RecognitionDataPacket p = new RecognitionDataPacket();
            if (correctedDepthImage != null)
                p.correctedDepthImage = correctedDepthImage.Clone();
            if (rawDepthImage != null)
                p.rawDepthImage = rawDepthImage.Clone();
            if (TableObjects != null)
                p.TableObjects = CloneTO();
            if (neighbourmap != null)
                p.neighbourmap = (int[,,])neighbourmap.Clone();
            if (objectmap != null)
                p.objectmap = (bool[,]) objectmap.Clone();
            if (bmpVideoFrame != null)
                p.bmpVideoFrame = (Bitmap) bmpVideoFrame.Clone();
            if (bmpRawDepth != null)
                p.bmpRawDepth = (Bitmap)bmpRawDepth.Clone();
            if (bmpCorrectedDepth != null)
                p.bmpCorrectedDepth = (Bitmap) bmpCorrectedDepth.Clone();
            if (HandObj != null)
                p.HandObj = (HandObject)HandObj.Clone();
            p.RecognitionDuration = this.RecognitionDuration;
            return p;
        }
    }
}
