using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;
using ObjectTable.Code.Display;
using ObjectTable.Code.Display.GUI;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.Rotation;
using ObjectTable.Code.SettingManagement;
using ObjectTable.Code.Tracking;

namespace ObjectTable.Code
{
    public class TableManager
    {
        #region properties

        //Switches
        public bool ToggleObjectRecognition
        {
            get { return _recognitionManager.ToggleObjectRecognition;}
            set { _recognitionManager.ToggleObjectRecognition = value;}
        }
        public bool ToggleObjectTracking = true;
        public bool ToggleObjectRotationAnalysation = false;

        //Kinect Data
        public int DelayBetweenKinectDepthFrames
        {
            get { return _recognitionManager.DelayBetweenDepthFrames; }
        }

        public int[] LastKinectDepthFrame
        {
            get { return _recognitionManager.LastKinectDepthFrame; }
        }

        public Bitmap LastKinectColorFrame
        {
            get { return _recognitionManager.LastKinectColorFrame; }
        }
        //RecognitionDetails
        public RecognitionDataPacket LastRecognitionDataPacket
        {
            get { return _recognitionManager.LastReconPacket; }
        }
        /// <summary>
        /// The Duration of the object Recognition Process in [ms]
        /// </summary>
        public int RecognitionDuration
        {
            get { return _recognitionManager.LastReconPacket.RecognitionDuration; }
        }

        //Rotation
        /// <summary>
        /// The rotationDetector has to be set manually by assigning a class derived by the motherclass RotationDetector
        /// the BlackWhiteRotationDetector is assigned by default
        /// </summary>
        public RotationDetector RotationDetector { get; set; }
        public int RotationDetectionDuration
        {
            get { return RotationDetector.RotationDetectionDuration; }
        }

        //Tracking
        /// <summary>
        /// The time [ms] how long tracking takes
        /// </summary>
        public int TrackingDuration
        {
            get { return _objectTracker.TrackingDuration; }
        }

        //General
        private bool _kinectRunning;
        public bool KinectRunning
        {
            get { return _kinectRunning; }
        }

        private object lock_tableObjects = "";
        private List<TableObject> _tableObjects;
        public List<TableObject> TableObjects
        {
            get
            {
                lock (lock_tableObjects)
                {
                    return _tableObjects;
                }
            }
        }

        private RecognitionManager _recognitionManager;
        private ObjectTracker _objectTracker;
        private DisplayManager _displayManager;
        public DisplayManager DisplayManager
        {
            get { return _displayManager; }
            set {}
        }
        public BeamerDisplayUC BeamerScreen
        {
            get { return _displayManager.GetBeamerScreen(); }
            set { _displayManager.UpdateBeamerScreen(value); }
        }
        
        public delegate void TableManagerObjectHandler();
        public event TableManagerObjectHandler OnNewObjectList;

        public delegate void TableManagerObjectChange(List<int> ObjectIDList);

        public event TableManagerObjectChange OnNewObject;
        public event TableManagerObjectChange OnNewLongTermObject;
        public event TableManagerObjectChange OnObjectMove;
        public event TableManagerObjectChange OnObjectRotate;
        public event TableManagerObjectChange OnObjectRemove;

        #endregion properties

        public TableManager(BeamerDisplayUC BeamerScreen, string SettingsPath = "")
        {
            //instances
            _recognitionManager = new RecognitionManager();
            _recognitionManager.OnNewRecognitionPacket += new RecognitionManager.RecognitionEventHandler(_recognitionManager_OnNewRecognitionPacket);
            _objectTracker = new ObjectTracker();
            _displayManager = new DisplayManager(BeamerScreen);
            
            //Assign the BlackWhiteDetector by default;
            //RotationDetector = new BlackWhiteRotationDetector();
            RotationDetector = new ImprovedBWRotDetector();

            //try to load settings. if the settings don't exist, try to save the default settings to the given path
            //Set to default if not specified
            if (SettingsPath == "")
                SettingsPath = SettingsManager.Path;

            bool loadsuccess = false;
            if (Directory.Exists(SettingsPath))
            {
                //Load
                if (!SettingsManager.LoadSettings(SettingsPath))
                {
                    MessageBox.Show(
                        "Einstellungen konnten nicht geladen werden. Standardeinstellungen werden gespeichert.",
                        "TableManager Init");
                }
                else
                {
                    loadsuccess = true;
                }
            }
            if (!Directory.Exists(SettingsPath) || !loadsuccess)
            {
                //try to save default
                if (!SettingsManager.SaveSettings(SettingsPath))
                {
                    MessageBox.Show("Standardeinstellungen konnten nicht gespeichert werden!", "TableManager Fehler!");
                }
            }
        }

        public bool Start()
        {
            if (!_kinectRunning)
            {
                bool res = _recognitionManager.Init();
                _kinectRunning = res;
            }
            return _kinectRunning;
        }

        public void Stop()
        {
            if (_kinectRunning)
            {
                if (_recognitionManager != null)
                    _recognitionManager.Stop();
                _kinectRunning = false;
            }
        }

        void _recognitionManager_OnNewRecognitionPacket()
        {
            //Get tableObjects
            List<TableObject> tlist = _recognitionManager.LastReconPacket.TableObjects;

            if (ToggleObjectTracking)
            {
                tlist = _objectTracker.TrackObjects(_recognitionManager.LastReconPacket.TableObjects);
            }

            if (ToggleObjectRotationAnalysation)
            {
                //Only if rotationDetector is assigned
                if (RotationDetector != null)
                {
                    tlist = RotationDetector.DetectRotation(tlist);
                }

                //debug:save bitmaps
                if (SettingsManager.RecognitionSet.SaveDebugRotationBitmaps)
                {
                    foreach (
                        TableObject obj in
                            tlist.Where(o => o.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked))
                    {
                        if (obj.ExtractedBitmap != null)
                            obj.ExtractedBitmap.Save("ObjBitmap_" + obj.ObjectID.ToString() + ".bmp");
                    }
                }
            }

            lock (lock_tableObjects)
            {
                _tableObjects = tlist;
            }

            List<int> deletedObjects = _objectTracker.GetDeletedObjects();
            List<int> newObjects = _objectTracker.GetNewObjects();
            List<int> movedObjects = _objectTracker.GetMovedObjects();
            List<int> longTermObjects = _objectTracker.GetNewLongTermObjects();
            List<int> rotatedObjects = _objectTracker.GetRotationChangedObjects();

            //Raise the events
            if (OnNewObjectList != null)
                OnNewObjectList();
            if (longTermObjects.Count > 0)
                if (OnNewLongTermObject != null)
                    OnNewLongTermObject(longTermObjects);

            if (deletedObjects.Count > 0)
                if (OnObjectRemove != null)
                    OnObjectRemove(deletedObjects);

            if (newObjects.Count > 0)
                if (OnNewObject != null)
                    OnNewObject(newObjects);

            if (movedObjects.Count > 0)
                if (OnObjectMove != null)
                    OnObjectMove(movedObjects);

            if (rotatedObjects.Count > 0)
                if (OnObjectRotate != null)
                    OnObjectRotate(rotatedObjects);
        }

        /// <summary>
        /// Returns the TableObject (a threadsafe copy) with the matching ID. Returns null if no object with the given ID exists.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public TableObject GetTableObject(int ID)
        {
            TableObject res = null;

            lock (lock_tableObjects)
            {
                if (_tableObjects.Count(o => o.ObjectID == ID) > 0)
                {
                    res = (TableObject)_tableObjects.Where(o => o.ObjectID == ID).ToArray()[0].Clone();
                }
            }

            return res;
        }
    }
}
