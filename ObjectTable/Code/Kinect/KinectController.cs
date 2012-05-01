using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Kinect
{
    public class KinectController
    {
        private Runtime _kinectruntime;

        public delegate void DepthFrameHandler(int[] deptharray, PlanarImage img);
        public event DepthFrameHandler OnDepthFrame;

        public delegate void VideoFrameHandler(Bitmap iframe);
        public event VideoFrameHandler OnVideoFrame;

        private DateTime _depthtime = DateTime.Now;
        /// <summary>
        /// The Delay between two Kinect-DepthFrames [ms]
        /// </summary>
        public int DelayBetweenDepthFrames;

        public enum EKinectMode
        {
            Depth,
            DepthAndSceleton
        };

        private object lock_depth = "";

        private int[] _lastDepthFrame;
        public int[] LastDepthFrame
        {
            get
            {
                lock (lock_depth)
                {
                    if (_lastDepthFrame == null)
                        return null;
                    return (int[])_lastDepthFrame.Clone();
                }
            }
        }

        private object lock_video = "";
        private Bitmap _lastVideoFrame;
        public Bitmap LastVideoFrame
        {
            get
            {
                lock (lock_video)
                {
                    if (_lastVideoFrame == null)
                        return null;
                    return (Bitmap)_lastVideoFrame.Clone();
                }
            }
        }

        private object lock_viewarea = "";
        private ImageViewArea _viewarea;
        public ImageViewArea VideoViewArea
        {
            get
            {
                lock (lock_viewarea)
                {
                    return _viewarea;
                }
            }
        }
        public bool Init()
        {
            _kinectruntime = Runtime.Kinects[0];

            _viewarea = new ImageViewArea();
            _viewarea.CenterX = 0;
            _viewarea.CenterY = 0;

            _kinectruntime.DepthFrameReady +=
                new EventHandler<ImageFrameReadyEventArgs>(KinectruntimeDepthFrameReady);
            _kinectruntime.VideoFrameReady +=
                new EventHandler<ImageFrameReadyEventArgs>(KinectruntimeVideoFrameReady);

            try
            {
                if (SettingsManager.KinectSet.KinectMode == EKinectMode.Depth)
                {
                    _kinectruntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
                }
                else
                {
                    _kinectruntime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseColor);
                }

                if (SettingsManager.KinectSet.KinectMode == EKinectMode.Depth)
                {
                    _kinectruntime.VideoStream.Open(ImageStreamType.Video, 2, SettingsManager.KinectSet.VideoResolution, ImageType.Color);
                    _kinectruntime.DepthStream.Open(ImageStreamType.Depth, 2, SettingsManager.KinectSet.DepthResolution, ImageType.Depth);
                }
                else
                {
                    _kinectruntime.DepthStream.Open(ImageStreamType.Depth, 2, SettingsManager.KinectSet.DepthResolution, ImageType.DepthAndPlayerIndex);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        void KinectruntimeVideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            var imageData = e.ImageFrame.Image;

            //imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr32, null, imageData.Bits, imageData.Width * imageData.BytesPerPixel
            var bsrc = BitmapSource.Create(imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr32, null, imageData.Bits,
                                imageData.Width * imageData.BytesPerPixel);

            //Convert Source to bitmap
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bsrc));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }

            //Flip horizontally
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            lock (lock_video)
            { _lastVideoFrame = (Bitmap)bitmap.Clone(); }

            //lock (lock_viewarea)
            //{_viewarea = e.ImageFrame.ViewArea;}

            if (OnVideoFrame != null)
                OnVideoFrame(bitmap);
        }

        void KinectruntimeDepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Calculate time between frames
            TimeSpan ts = DateTime.Now - _depthtime;
            _depthtime = DateTime.Now;
            DelayBetweenDepthFrames = (int)Math.Round(ts.TotalMilliseconds);

            PlanarImage img = e.ImageFrame.Image;
            int[] data = ConvertPlanarImageToDepthArray(img);

            lock (lock_depth)
            { _lastDepthFrame = (int[])data.Clone(); }
            
            if (OnDepthFrame != null)
                OnDepthFrame(data, img);
        }

        public void GetColorPixelCoordinatesFromDepthPixel(int depth_x, int depth_y, short depthValue, out int color_x, out int color_y)
        {
            _kinectruntime.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(SettingsManager.KinectSet.VideoResolution, this.VideoViewArea, depth_x, depth_y, depthValue, out color_x, out color_y);
        }

        /// <summary>
        /// Sets the camera-tilt (Servo)
        /// </summary>
        /// <param name="tilt">tilt from -27 to +27</param>
        public void SetCameraTilt(int tilt)
        {
            if (tilt > 27)
                tilt = 27;
            if (tilt < -27)
                tilt = -27;

            _kinectruntime.NuiCamera.ElevationAngle = tilt;
        }

        private int[] ConvertPlanarImageToDepthArray(PlanarImage img)
        {
            byte[] DepthData = img.Bits;
            int[] ResultData = new int[img.Width * img.Height];

            var Width = img.Width;
            var Height = img.Height;

            var imgIndex = 0;
            var depthIndex = 0;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var distance = GetDistance(DepthData[depthIndex], DepthData[depthIndex + 1]);
                    ResultData[imgIndex] = distance;
                    depthIndex += 2;
                    imgIndex++;
                }
            }

            return ResultData;
        }
        private int GetDistance(byte firstFrame, byte secondFrame)
        {
            if (SettingsManager.KinectSet.KinectMode == EKinectMode.DepthAndSceleton)
            {
                return (int)(firstFrame >> 3 | secondFrame << 5);
            }
            else
            {
                return (int)(firstFrame | secondFrame << 8);
            }
        }

        public void Stop()
        {
            if (_kinectruntime != null)
            {
                _kinectruntime.Uninitialize();
                _kinectruntime = null;
            }
        }
    }
}
