using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;
using ObjectTable.Forms;

namespace ObjectTable.Code
{
    /// <summary>
    /// The RecognitionManager encapsulates all the other classes that do the work
    /// </summary>
    public class RecognitionManager
    {
        private KinectController _kinectController;
        public Bitmap LastKinectColorFrame
        {
            get { return _kinectController.LastVideoFrame; }
        }
        public int[] LastKinectDepthFrame
        {
            get
            {
                return _kinectController.LastDepthFrame;
            }
        }
        private object lock_reconpacket = "";
        private RecognitionDataPacket _lastReconPacket;
        /// <summary>
        /// The recognition Packet stores all relevant (Debug)Data to use in the application
        /// </summary>
        public RecognitionDataPacket LastReconPacket
        {
            get
            {
                lock (lock_reconpacket)
                {
                    if (_lastReconPacket == null)
                        return null;
                    return (RecognitionDataPacket) _lastReconPacket.Clone();
                }
            }
        }

        public delegate void RecognitionEventHandler();
        public event RecognitionEventHandler OnNewRecognitionPacket;

        /// <summary>
        /// The delay between 2 depth Frames in [ms]
        /// </summary>
        public int DelayBetweenDepthFrames
        {
            get { return _kinectController.DelayBetweenDepthFrames; }
        }

        /// <summary>
        /// Switches the object Recognition on or off
        /// </summary>
        public bool ToggleObjectRecognition = false;

        /// <summary>
        /// Toggles whether multiple recognitionThreads are allowed
        /// </summary>
        public bool AllowMultipleRecognitionThreads = false;
        private bool _recognitionThreadrunning = false;

        private RecognitionThread _rthread;
        private RecognitionDataPacket _lastDataPacket;
        
        /// <summary>
        /// The FormSupllier generates Forms, e.g. the DepthCalibration Form
        /// </summary>
        public FormSupplier Forms;

        public bool Init()
        {
            //Stop everything
            Stop();

            //init the kinect
            _kinectController = new KinectController();

            try
            {
                if (!_kinectController.Init())
                    return false;
               
                _kinectController.OnDepthFrame += new KinectController.DepthFrameHandler(_kinectController_OnDepthFrame);
                _kinectController.OnVideoFrame += new KinectController.VideoFrameHandler(_kinectController_OnVideoFrame);
            }
            catch (Exception)
            {
                return false;
            }

            //init settings
            int width = 0;
            int height = 0;
            SettingsManager.KinectSet.GetDepthResolution(out width, out height);
            

            //init the other classes
            PositionMapper.AssignKinectController(_kinectController);
            _rthread = new RecognitionThread();
            _rthread.OnRecognitionFinished += new RecognitionThread.RecognitionFinished(_rthread_OnRecognitionFinished);
            Forms = new FormSupplier(this);
            _lastReconPacket = new RecognitionDataPacket();

            //Everything worked, return true
            return true;
        }

        void _rthread_OnRecognitionFinished(RecognitionDataPacket result)
        {
            //Save recognition data
            lock (lock_reconpacket)
            {
                _lastReconPacket = result;
            }

            //set the running property to false
            _recognitionThreadrunning = false;
            
            //raise event
            OnNewRecognitionPacket();
        }

        void _kinectController_OnVideoFrame(Bitmap iframe)
        {
        }

        void _kinectController_OnDepthFrame(int[] deptharray, Microsoft.Research.Kinect.Nui.PlanarImage img)
        {
            if (!ToggleObjectRecognition)
                return;

            //Are multiple recognition Threads allowed? if not, check wheter one thread is already running
            if (!AllowMultipleRecognitionThreads)
            {
                if (_recognitionThreadrunning)
                    return;
            }

            //Start recognition Thread. If there is a video-Frame, use this as well
            _recognitionThreadrunning = true;

            if (_kinectController.LastVideoFrame != null)
                _rthread.StartRecognition(img, deptharray,_kinectController.LastVideoFrame);
            else
                _rthread.StartRecognition(img, deptharray); //Recognition without video Bitmap extraction
        }

        public void Stop()
        {
            if (Forms != null)
                Forms.CloseAllForms();

            if (_kinectController != null)
            {
                _kinectController.Stop();
                _kinectController = null;
            }
        }
    }
}
