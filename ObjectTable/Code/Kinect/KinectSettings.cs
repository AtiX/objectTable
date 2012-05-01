using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace ObjectTable.Code.Kinect
{
    /// <summary>
    /// This Class Stores the settings for the Kinect-Sensor
    /// </summary>
    [Serializable()]
    public class KinectSettings
    {
        /// <summary>
        /// The Mode that is used to run the kinect
        /// </summary>
        public KinectController.EKinectMode KinectMode = KinectController.EKinectMode.Depth;

        /// <summary>
        /// The Resolution the Depth-Sensor Images are produced. Valid are 640x480 (no sceleton tracking!) and 320x240
        /// </summary>
        public ImageResolution DepthResolution = ImageResolution.Resolution320x240;
        public void GetDepthResolution(out int Width, out int Height)
        {
            Height = 0;
            Width = 0;

            switch(DepthResolution)
            {
                case ImageResolution.Resolution80x60:
                    Width = 80;
                    Height = 60;
                    break;
                case ImageResolution.Resolution320x240:
                    Width = 320;
                    Height = 240;
                    break;
                case ImageResolution.Resolution640x480:
                    Width = 640;
                    Height = 480;
                    break;
                case ImageResolution.Resolution1280x1024:
                    Width = 1280;
                    Height = 1024;
                    break;
            }
        }

        /// <summary>
        /// The Resolution the VideoImages are produced
        /// </summary>
        public ImageResolution VideoResolution = ImageResolution.Resolution640x480;
        public void GetVideoResolution(out int Width, out int Height)
        {
            Height = 0;
            Width = 0;

            switch (VideoResolution)
            {
                case ImageResolution.Resolution80x60:
                    Width = 80;
                    Height = 60;
                    break;
                case ImageResolution.Resolution320x240:
                    Width = 320;
                    Height = 240;
                    break;
                case ImageResolution.Resolution640x480:
                    Width = 640;
                    Height = 480;
                    break;
                case ImageResolution.Resolution1280x1024:
                    Width = 1280;
                    Height = 1024;
                    break;
            }
        }
    }
}
