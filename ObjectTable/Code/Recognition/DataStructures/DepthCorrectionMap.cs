using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition.DataStructures
{
    /// <summary>
    /// Stores per-pixel values to correct the Depthmap
    /// </summary>
    [Serializable()]
    public class DepthCorrectionMap
    {
        public int Width, Height;
        
        /// <summary>
        /// The Amount of how many pixel are set to zero on each side (to prevent objects at the borders disturb the image)
        /// </summary>
        public int CutOffLeft, CutOffTop, CutOffRight, CutOffBOttom;

        public DepthCorrectionMap()
        {
            //For the sake of serialisation, a constructor with no arguments
            SettingsManager.KinectSet.GetDepthResolution(out Width, out Height);
            CorrectionData = new int[Width, Height];
        }

        public DepthCorrectionMap(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;

            CorrectionData = new int[Width,Height];
        }

        /// <summary>
        /// These values have to be ADDED to the initial DepthImage to create a homogeneous depth-image
        /// </summary>
        public int[,] CorrectionData;
    }
}
