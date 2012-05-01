using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;
using ObjectTable.Forms;
using System.Threading;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// This class encapsules an Thread that does all the recognition work
    /// </summary>
    class RecognitionThread
    {
        public delegate void RecognitionFinished(RecognitionDataPacket result);
        public event RecognitionFinished OnRecognitionFinished;

        private Thread _recognitionThread;

        public RecognitionThread()
        {
        }

        /// <summary>
        /// Starts the object recognition. When the recognition is finished, OnRecognitionFinished will be raised
        /// </summary>
        /// <param name="img">the PlanarImage from the Depth-Sensor</param>
        /// <param name="depthmap">the Depthmap supplied by the kinect-Controller</param>
        public void StartRecognition(Microsoft.Research.Kinect.Nui.PlanarImage img, int[] depthmap)   
        {
            _recognitionThread = new Thread(new ParameterizedThreadStart(DoRecognitionWork));
            _recognitionThread.Name = "ObjectTable RecognitionThread";
            _recognitionThread.Start(new object[] {img, depthmap, null});
        }

        /// <summary>
        /// Starts the object recognition and extracts the relevant position from the bitmap. When the recognition is finished, OnRecognitionFinished will be raised. 
        /// </summary>
        /// <param name="img">the PlanarImage from the Depth-Sensor</param>
        /// <param name="depthmap">the Depthmap supplied by the kinect-Controller</param>
        /// <param name="ColorVideoFrame">The Frame from the VideoCamera of the Kinects</param>
        public void StartRecognition(Microsoft.Research.Kinect.Nui.PlanarImage img, int[] depthmap, Bitmap ColorVideoFrame)   
        {
            _recognitionThread = new Thread(new ParameterizedThreadStart(DoRecognitionWork));
            _recognitionThread.Name = "ObjectTable RecognitionThread";
            _recognitionThread.Start(new object[] {img, depthmap, ColorVideoFrame});
        }

        private void DoRecognitionWork(object data)
        {
            object[] dataArray = (object[]) data;
            PlanarImage pimg = (PlanarImage) dataArray[0];
            int[] deptharray = (int[]) dataArray[1];
            Bitmap colorFrame = (Bitmap) dataArray[2];

            RecognitionDataPacket rpacket = new DataStructures.RecognitionDataPacket();
            DateTime dtBegin = DateTime.Now;

            //Create DepthImage
            DepthImage dimg = new DepthImage(deptharray,pimg.Width,pimg.Height);
            rpacket.rawDepthImage = dimg.Clone();

            //Correct the image
            DepthMapPreprocessor dmp = new DepthMapPreprocessor();
            dimg = dmp.ApplyDepthCorrection(dimg, SettingsManager.PreprocessingSet.DefaultCorrectionMap);
            dimg = dmp.NormalizeHeights(dimg);

            ObjectSeperator objectSeperator = new ObjectSeperator();

            //Seperate objects
            bool[,] boolmap_object;
            int[,,] neighbourmap;
            List<TableObject> objects = objectSeperator.SeperateObjects(ref dimg,out boolmap_object,out neighbourmap);

            //if supplied, extract the relevant bitmap parts from the ColorFrame
            if (colorFrame != null)
            {
                ObjectVideoBitmapAssigner ovba = new ObjectVideoBitmapAssigner();
                ovba.AssignVideoBitmap(objects, colorFrame);
            }

            //Extract hand object from table objects
            if (objects.Where( o => o.GetType() == typeof(HandObject)).Count() > 0)
            {
                rpacket.HandObj = (HandObject)objects.Where(o => o.GetType() == typeof (HandObject)).ToArray()[0];
            }

            //Fill DataPacket with Data
            rpacket.correctedDepthImage = dimg;
            rpacket.TableObjects = objects;
            rpacket.objectmap = boolmap_object;
            rpacket.neighbourmap = neighbourmap;
            rpacket.bmpVideoFrame = colorFrame;

            TimeSpan ts = DateTime.Now - dtBegin;
            rpacket.RecognitionDuration = (int)Math.Round(ts.TotalMilliseconds);

            if (SettingsManager.RecognitionSet.SaveDebugMaps)
            {
                Bitmap bmp = MapVisualizer.VisualizeDepthImage(rpacket.rawDepthImage);
                bmp.Save("rawDepthImage.bmp");
            }

            //Event
            OnRecognitionFinished(rpacket);
        }
    }
}
