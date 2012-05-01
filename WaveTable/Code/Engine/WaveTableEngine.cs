using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Media.Imaging;
using ObjectTable.Code;
using ObjectTable.Code.Display;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using WaveSimLib.Code.Visualisation;
using WaveSimLib.Code.Wave;
using WaveTable.Code.SimulationObjects;
using WaveTable.GUI.Forms;

namespace WaveTable.Code.Engine
{
    class WaveTableEngine
    {
        public bool SimulationRunning
        {
            get { if (_waveEngine != null) return _waveEngine.SimulationRunning; else return false; }
        }

        private WaveEngine _waveEngine;
        private DynamicColorVisualizer _dynColVis;
        private MapManager _mapManager;
        private TableManager _tableManager;

        public UIElementAction UiLtoAction;
        public UIElementAction UiMouseAction;
        public UIElementAction UiTooAction;

        public int FPS;

        /// <summary>
        /// Every object that is above this Height [mm] will be declared as an object above the surface (TOO), everything beneath this border as a potentioal LTO on the surface
        /// </summary>
        public int TooMinHeight = 100;

        private Size _realSimulationSize = new Size(320, 200);
        private Size _simulationScreenSize = new Size(1280,800);
        public Size SimulationSize
        {
            get { return _simulationScreenSize; }
            set { _simulationScreenSize = value; ChangeSimValues(); }
        }

        private int _simulationDivisor = 4;
        public int SimulationDivisor
        {
            get { return _simulationDivisor; }
            set { _simulationDivisor = value; ChangeSimValues(); }
        }

        private List<SimulationObject> _simulationObjects;
        private object _lockSimuObjects = "";
        private bool _busyFrame = false;

        private Timer _updateTimer;
        //Methods ------------------------------------------------------------------------------------------------------------------------------------
        public WaveTableEngine(TableManager tmgr)
        {
            _simulationObjects = new List<SimulationObject>();

            _tableManager = tmgr;
            _tableManager.OnNewLongTermObject += new TableManager.TableManagerObjectChange(_tableManager_OnNewLongTermObject);
            _tableManager.OnNewObject += new TableManager.TableManagerObjectChange(_tableManager_OnNewObject);
            _tableManager.OnObjectMove += new TableManager.TableManagerObjectChange(_tableManager_OnObjectMove);
            _tableManager.OnObjectRemove += new TableManager.TableManagerObjectChange(_tableManager_OnObjectRemove);
            _tableManager.OnObjectRotate += new TableManager.TableManagerObjectChange(_tableManager_OnObjectRotate);

            _tableManager.BeamerScreen.OnMouseClick += new ObjectTable.Code.Display.GUI.BeamerDisplayUC.MouseEventHandler(BeamerScreen_OnMouseClick);
            _tableManager.BeamerScreen.OnMouseMoveDown += new ObjectTable.Code.Display.GUI.BeamerDisplayUC.MouseEventHandler(BeamerScreen_OnMouseMoveDown);

            _waveEngine = new WaveEngine();
            _waveEngine.OnNewSimulationFrame += new WaveEngine.NewSimulationFrameHandler(_waveEngine_OnNewSimulationFrame);

            _dynColVis = new DynamicColorVisualizer();

            _mapManager = new MapManager(_waveEngine);

            _updateTimer = new Timer(1000);
            _updateTimer.Enabled = false;
            _updateTimer.Elapsed += new ElapsedEventHandler(_updateTimer_Elapsed);
        }

        void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Update all the sim objects
            lock (_lockSimuObjects)
            {
                _mapManager.Update(_simulationObjects);
            }
            //Disable timer - will be enabled by Event handlers like OnObjectMove
            _updateTimer.Enabled = false;
        }

        void BeamerScreen_OnMouseMoveDown(System.Windows.Point position)
        {
            DoMouseAction(position);
        }

        public bool Initialize()
        {
            ChangeSimValues();

            _simulationObjects.Clear();

            return true;
        }

        public void Start()
        {
            if (_waveEngine != null)
                _waveEngine.Start();
        }

        public void Stop()
        {
            if (_waveEngine != null)
                _waveEngine.Stop();
        }

        private void ChangeSimValues()
        {
            _realSimulationSize.Width = _simulationScreenSize.Width / _simulationDivisor;
            _realSimulationSize.Height = _simulationScreenSize.Height / _simulationDivisor;

            _waveEngine.Init(_realSimulationSize.Width, _realSimulationSize.Height);
            _mapManager.Start(_realSimulationSize);    

            //Change beamer screen size
            _tableManager.BeamerScreen.Dispatcher.Invoke((Action) (() => { 
                _tableManager.BeamerScreen.SetSize(new System.Windows.Size(_simulationScreenSize.Width,_simulationScreenSize.Height));
            }));
        }

        public void UpdateSimParams(WaveSettings paramsOnly)
        {
            //adjust to right size, otherwise the map manager wont load it
            paramsOnly.Width = _realSimulationSize.Width;
            paramsOnly.Height = _realSimulationSize.Height;

            _mapManager.LoadSettings(paramsOnly, false, false, false, false, true);
        }

        public void ClearSimulation(bool Elongation, bool Walls, bool Mass, bool Sources)
        {
            if (Elongation)
                _waveEngine.ResetElongation();
            if (Walls)
                _waveEngine.ResetWalls();
            if (Mass)
                _waveEngine.ResetMass();
            if (Sources)
                _waveEngine.ResetSources();
        }

        public WaveSettings GetSimulationData(bool mergedWithObjects)
        {
            if (!mergedWithObjects)
            {
                return _mapManager.LoadedSettings;
            }
            else
            {
                //Get Sim params and loaded sources
                WaveSettings settings = _mapManager.LoadedSettings;
                //Merge Loaded and additional settings
                WaveSettings mergedMaps = _mapManager.GetMergedMaps();
                settings.ElongationMap = mergedMaps.ElongationMap;
                settings.VelocityMap = mergedMaps.VelocityMap;
                settings.WallMap = mergedMaps.WallMap;
                settings.MassMap = mergedMaps.MassMap;
                //Add additional wave sources (Objects)
                lock(_lockSimuObjects)
                {
                    foreach (SimWaveSourceObject sws in _simulationObjects)
                    {
                        settings.WaveSources.Add(sws.WaveSourceRef);
                    }
                }

                return settings;
            }
        }

        public bool LoadSimulation(WaveSettings load, bool Elongation, bool Mass, bool Walls, bool Sources, bool Parameters)
        {
            return _mapManager.LoadSettings(load, Elongation, Sources, Walls, Mass, Parameters);
        }
        //Events ---------------------------------------------------------------------------------------------------------------------------------------
        void _waveEngine_OnNewSimulationFrame(double[,] positionMap, bool[,] wallMap, bool[,] addonWallMap, double[,] massMap, double[,] addonMassMap, int fps)
        {
            if (!_busyFrame)
            {
                _busyFrame = true;
                FPS = (FPS + fps)/2;

                Bitmap res = _dynColVis.VisualizePositionMapSafe(positionMap, wallMap, addonWallMap, massMap, addonMassMap,_waveEngine.Width,
                                                              _waveEngine.Height);

                _tableManager.DisplayManager.SetBackgroundImage(res);
                res.Dispose();
                _busyFrame = false;
            }
        }
        
        void _tableManager_OnObjectRotate(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: ObjectRotate");
            foreach (int i in ObjectIDList)
            {
                UpdateObject(i);
            }
        }

        void _tableManager_OnObjectRemove(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: ObjectRemove");
            foreach (int id in ObjectIDList)
            {
                lock (_simulationObjects)
                {
                    if (_simulationObjects.Where(o => o.ObjectID == id).Count() == 0)
                        continue;
                    SimulationObject rem = _simulationObjects.Where(o => o.ObjectID == id).ToArray()[0];
                    //Do Cleanup tasks and finally remove
                    if (rem.GetType() == typeof(SimWaveSourceObject))
                    {
                        SimWaveSourceObject swo = (SimWaveSourceObject) rem;
                        _mapManager.RemoveSource(swo.WaveSourceRef);
                    }
                    if (rem.GetType()  == typeof(SimPlotterObject))
                    {
                        SimPlotterObject spo = (SimPlotterObject) rem;
                        spo.Remove();
                    }
                    _simulationObjects.Remove(_simulationObjects.Where(o => o.ObjectID == id).ToArray()[0]);
                }
            }

            _updateTimer.Enabled = true;
        }

        void _tableManager_OnObjectMove(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: ObjectMove");
            foreach (int i in ObjectIDList)
            {
                UpdateObject(i);
            }

            _updateTimer.Enabled = true;
        }

        private void UpdateObject(int i)
        {
            lock (_lockSimuObjects)
            {
                if (_simulationObjects.Where(o => o.ObjectID == i).Count() == 0)
                {
                    //(Re)add it if it doesnt exist
                    TableObject too = _tableManager.GetTableObject(i);
                    if (too == null)
                        return;

                    if (too.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked)
                        _tableManager_OnNewLongTermObject(new List<int> {i});
                    else
                        _tableManager_OnNewObject(new List<int> {i});
                    return;
                }

                SimulationObject so = _simulationObjects.Where(o => o.ObjectID == i).ToArray()[0];
                TableObject to = _tableManager.GetTableObject(i);
                if (to == null)
                    return;

                so.Position = PositionMapper.GetScreenCoordsfromDepth(to.Center);
                Point screen = new Point(so.Position.ScreenX, so.Position.ScreenY);
                so.SimPosition = CalculateSimPosition(screen);

                //If the object is out of bounds (outside the simulation), then remove it
                if (so.SimPosition.X >= _waveEngine.Width || so.SimPosition.Y >= _waveEngine.Height || so.SimPosition.Y < 1 || so.SimPosition.X < 1)
                {
                    _simulationObjects.Remove(so);
                    return;
                }

                if (to.RotationDefined)
                {
                    so.ObjectRotation = to.DirectionVector;
                }

                so.PositionOrRotationChange = true;

                if (so.ObjectType == SimulationObject.EObjectType.RelElongation)
                {
                    so.ObjectValue = to.Height;
                }
            }
        }

        void _tableManager_OnNewObject(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: NewObject");
            //Add Objects according to height. objects on table have to be smaller than 20cm (Height)
            foreach (int i in ObjectIDList)
            {
                TableObject to = _tableManager.GetTableObject(i);
                if (to == null)
                    continue;

                if (to.Height > TooMinHeight)
                {
                    //Add TOO Object
                    SimulationObject so = AddSimObject(i, UiTooAction, SimulationObject.ETableObjectType.TOO);
                    if (so == null)
                        return;

                    so.Position = to.Center;
                    if (to.RotationDefined)
                        so.ObjectRotation = to.DirectionVector;
                }
                else
                {
                    //On surface - LTO
                    //Wait for object to add till it is an lto (-> on long term object method)
                }
            }

            _updateTimer.Enabled = true;
        }

        void _tableManager_OnNewLongTermObject(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: New LTO");
            //Add LTO accordingly to rules
            foreach (int i in ObjectIDList)
            {
                //Check whether the object isn't too high (if it is, it already exists as a TOO)
                TableObject to = _tableManager.GetTableObject(i);
                if (to == null)
                    continue;

                if (to.Height > TooMinHeight)
                    continue;
                
                //If it is a TOO that is just below the border -> check whether the object already exists
                lock (_lockSimuObjects)
                {
                    if (_simulationObjects.Count(o => o.ObjectID == i) > 0)
                        continue;
                }

                //Its not an TOO -> add an LTO
                SimulationObject so = AddSimObject(i,UiLtoAction,SimulationObject.ETableObjectType.LTO);
            }

            _updateTimer.Enabled = true;
        }

        private SimulationObject AddSimObject(int ObjectID, UIElementAction elementAction, SimulationObject.ETableObjectType TableObjectType)
        {
            SimulationObject simObj = new SimulationObject();

            if (elementAction.ActionObjectType == typeof(SimPlotterObject))
            {
                simObj = new SimPlotterObject();
            }
            else if (elementAction.ActionObjectType == typeof(SimWaveSourceObject))
            {
                SimWaveSourceObject simwave = new SimWaveSourceObject();
                simwave.WaveSourceRef = new SinusWaveSource();
                simwave.WaveSourceRef.Frequency = elementAction.Value;
                simObj = simwave;
            }

            simObj.ObjectID = ObjectID;
            simObj.ObjectValue = UiLtoAction.Value;
            simObj.TableObjectType = TableObjectType;
            simObj.PositionOrRotationChange = true;
            simObj.ObjectSize = elementAction.Size;

            switch (elementAction.ActionType)
            {
                case UIElementAction.EActionType.SetMass:
                    simObj.ObjectType = SimulationObject.EObjectType.Mass;
                    break;
                case UIElementAction.EActionType.SetWall:
                    simObj.ObjectType = SimulationObject.EObjectType.Wall;
                    break;
                case UIElementAction.EActionType.SetSource:
                    //nothing to do, everything done before
                    break;
                case UIElementAction.EActionType.SetAmp:
                    if (simObj.ObjectValue == -1)
                    {
                        simObj.ObjectType = SimulationObject.EObjectType.RelElongation;
                    }
                    else
                    {
                        simObj.ObjectType = SimulationObject.EObjectType.ConstElongation;
                    }
                    break;
            }

            switch (elementAction.Shape)
            {
                case UIElementAction.EShape.Circle:
                    simObj.ObjectShape = SimulationObject.EObjectShape.Circle;
                    break;
                case UIElementAction.EShape.Square:
                    simObj.ObjectShape = SimulationObject.EObjectShape.Square;
                    break;
                case UIElementAction.EShape.NULL:
                    simObj.ObjectShape = SimulationObject.EObjectShape.NULL;
                    break;
            }

            TableObject to = _tableManager.TableObjects.Where(o => o.ObjectID == ObjectID).ToArray()[0];

            simObj.Position = to.Center;
            simObj.Position = PositionMapper.GetScreenCoordsfromDepth(simObj.Position);
            Point screen = new Point(simObj.Position.ScreenX, simObj.Position.ScreenY);

            simObj.SimPosition = CalculateSimPosition(screen);

            //If the object is out of bounds (outside the simulation), then dont add it
            if (simObj.SimPosition.X >= _waveEngine.Width || simObj.SimPosition.Y >= _waveEngine.Height || simObj.SimPosition.Y < 1 || simObj.SimPosition.X < 1)
                return null;

            if (to.RotationDefined)
                simObj.ObjectRotation = to.DirectionVector;

            lock (_lockSimuObjects)
            {
                _simulationObjects.Add(simObj);
            }

            return simObj;
        }

        private void DoMouseAction(System.Windows.Point position)
        {
            var SimuPoint = CalculateSimPosition(position);

            switch (UiMouseAction.ActionType)
            {
                case UIElementAction.EActionType.DelMass:
                    _mapManager.ClearMass(SimuPoint, UiMouseAction.Size);
                    break;
                case UIElementAction.EActionType.DelWall:
                    _mapManager.ClearWall(SimuPoint, UiMouseAction.Size);
                    break;
                case UIElementAction.EActionType.SetAmp:
                    _mapManager.SetElongation(SimuPoint, UiMouseAction.Value, UiMouseAction.Size);
                    break;
                case UIElementAction.EActionType.SetMass:
                    _mapManager.SetMass(SimuPoint, UiMouseAction.Value, UiMouseAction.Size);
                    break;
                case UIElementAction.EActionType.SetSource:
                    _mapManager.SetSource(SimuPoint, UiMouseAction.Value);
                    break;
                case UIElementAction.EActionType.SetWall:
                    _mapManager.SetWall(SimuPoint, UiMouseAction.Size);
                    break;
            }
        }

        private Point CalculateSimPosition(System.Windows.Point position)
        {
            Point SimuPoint = new Point();

            if (!_tableManager.BeamerScreen.Dispatcher.CheckAccess())
            {
                //requires invoke
                Point p = new Point();
                _tableManager.BeamerScreen.Dispatcher.Invoke((Action)(() => { p = CalculateSimPosition(position); }));
                return p;
            }

            System.Windows.Size screenWidht = _tableManager.BeamerScreen.GetSize();

            double xFactor = screenWidht.Width / _realSimulationSize.Width;
            double yFactor = screenWidht.Height / _realSimulationSize.Height;

            SimuPoint.X = (int) Math.Round(position.X/xFactor);
            SimuPoint.Y = (int) Math.Round(position.Y/yFactor);

            //Check for illegal values: sometimes, the size of the beamerscreen isn't known yet, which results into very high negative numbers
            //use default pixel/simu relation
            if (SimuPoint.X < 0 || Double.IsInfinity(SimuPoint.X) || Double.IsNaN(SimuPoint.X))
            {
                SimuPoint.X = (int) Math.Round(position.X / _simulationDivisor);
                SimuPoint.Y = (int)Math.Round(position.Y / _simulationDivisor);
            }

            return SimuPoint;
        }

        private Point CalculateSimPosition(System.Drawing.Point position)
        {
            if (!_tableManager.BeamerScreen.Dispatcher.CheckAccess())
            {
                //requires invoke
                Point p = new Point();
                _tableManager.BeamerScreen.Dispatcher.Invoke((Action) (() => { p = CalculateSimPosition(position); }));
                return p;
            }

            Point SimuPoint = new Point();

            System.Windows.Size screenWidht = _tableManager.BeamerScreen.GetSize();

            double xFactor = screenWidht.Width / _realSimulationSize.Width;
            double yFactor = screenWidht.Height / _realSimulationSize.Height;

            SimuPoint.X = (int)Math.Round(position.X / xFactor);
            SimuPoint.Y = (int)Math.Round(position.Y / yFactor);

            //Check for illegal values: sometimes, the size of the beamerscreen isn't known yet, which results into very high negative numbers
            //use default pixel/simu relation
            if (SimuPoint.X < 0 || Double.IsInfinity(SimuPoint.X) || Double.IsNaN(SimuPoint.X))
            {
                SimuPoint.X = (int)Math.Round((double)position.X / _simulationDivisor);
                SimuPoint.Y = (int)Math.Round((double)position.Y / _simulationDivisor);
            }

            return SimuPoint;
        }

        void BeamerScreen_OnMouseClick(System.Windows.Point position)
        {
            DoMouseAction(position);
        }
        //END Events -----------------------------------------------------------------------------------------------------------------------------------
    }
}
