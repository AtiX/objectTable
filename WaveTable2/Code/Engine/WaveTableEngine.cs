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
using ObjectTable.Code.Kinect.Structures;
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
        public DynamicColorVisualizer ColorVisualizer
        {
            get { return _dynColVis; }
            set { _dynColVis = value; }
        }

        private MapManager _mapManager;
        private TableManager _tableManager;

        public UIElementAction UiMouseAction;

        public int FPS;

        public int MassMaxHeight = 30;
        public double MassValue = 1.5;
        public int WallMaxHeight = 180;

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

        private bool _wallCreationBusy;

        //Methods ------------------------------------------------------------------------------------------------------------------------------------
        public WaveTableEngine(TableManager tmgr)
        {
            _simulationObjects = new List<SimulationObject>();

            _tableManager = tmgr;
            _tableManager.OnObjectMove += new TableManager.TableManagerObjectChange(_tableManager_OnObjectMove);
            _tableManager.OnNewObjectList += new TableManager.TableManagerObjectHandler(_tableManager_OnNewObjectList);
            _tableManager.BeamerScreen.OnMouseClick += new ObjectTable.Code.Display.GUI.BeamerDisplayUC.MouseEventHandler(BeamerScreen_OnMouseClick);
            _tableManager.BeamerScreen.OnMouseMoveDown += new ObjectTable.Code.Display.GUI.BeamerDisplayUC.MouseEventHandler(BeamerScreen_OnMouseMoveDown);

            _waveEngine = new WaveEngine();
            _waveEngine.OnNewSimulationFrame += new WaveEngine.NewSimulationFrameHandler(_waveEngine_OnNewSimulationFrame);

            _mapManager = new MapManager(_waveEngine);
            
            _dynColVis = new DynamicColorVisualizer();
        }

        void _tableManager_OnNewObjectList()
        {
            if (_wallCreationBusy)
                return;
            _wallCreationBusy = true;

            //Create wall map out of each object that is higher than 1cm and smaller tahn 15 cm (--> using Corrected Depth Map)
            RecognitionDataPacket dp = _tableManager.LastRecognitionDataPacket;
            DepthImage corr = dp.correctedDepthImage;
           
            //Where in the depthmap is the simulation?
            TPoint SimUpperLeft =
                PositionMapper.GetDepthCoordsfromScreen(new TPoint(0, 0, TPoint.PointCreationType.screen));
            TPoint SimLowerRight = PositionMapper.GetDepthCoordsfromScreen(new TPoint(_simulationScreenSize.Width,_simulationScreenSize.Height, TPoint.PointCreationType.screen));

            int SimDepthWidth = SimLowerRight.DepthX - SimUpperLeft.DepthX;
            int SimDepthHeight = SimLowerRight.DepthY - SimUpperLeft.DepthY;

            double xScaleFactor = (double)_waveEngine.Width / SimDepthWidth;
            double yScaleFactor = (double)_waveEngine.Height / SimDepthHeight;

            bool[,] WallMap = new bool[_waveEngine.Width,_waveEngine.Height];
            double[,] MassMap = new double[_waveEngine.Width, _waveEngine.Height];

            for (int x = SimUpperLeft.DepthX; x < SimLowerRight.DepthX; x++)
            {
                for (int y = SimUpperLeft.DepthY; y < SimLowerRight.DepthY; y++)
                {
                    int px = (int) Math.Round((x-SimUpperLeft.DepthX)*xScaleFactor);
                    int py = (int) Math.Round((y-SimUpperLeft.DepthY)*yScaleFactor);


                    //only objects smaller than the threshold (and bigger than the massThreshold)
                    if (!(x < 0 || y < 0))
                    {
                        if (corr.Data[x, y] <= MassMaxHeight && corr.Data[x, y] >= 10)
                        {
                            MassMap[px, py] = MassValue;
                        }
                        else if (corr.Data[x, y] < WallMaxHeight && corr.Data[x, y] > MassMaxHeight)
                        {
                            WallMap[px, py] = true;
                        }
                    }
                }
            }

            //apply wall map
            _waveEngine.ImportAddonWallMap(WallMap);
            _waveEngine.ImportAddonMassMap(MassMap);

            _wallCreationBusy = false;
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

            _waveEngine.Settings.FederkonstanteKopplung = paramsOnly.FederkonstanteKopplung;
            _waveEngine.Settings.TeilchenDistanz = paramsOnly.TeilchenDistanz;
            _waveEngine.Settings.Teilchenmasse = paramsOnly.Teilchenmasse;
            _waveEngine.Settings.DeltaT = paramsOnly.DeltaT;
            _waveEngine.Settings.DesiredFPS = paramsOnly.DesiredFPS;
            _waveEngine.Settings.Energieerhaltung = paramsOnly.Energieerhaltung;
            _waveEngine.Settings.FederkonstanteTeilchen = paramsOnly.FederkonstanteTeilchen;
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

        //Events ---------------------------------------------------------------------------------------------------------------------------------------
        void _waveEngine_OnNewSimulationFrame(double[,] positionMap, bool[,] wallMap, bool[,] addonWallMap, double[,] massMap, double[,] addonMassMap, int fps)
        {
            if (!_busyFrame)
            {
                _busyFrame = true;
                FPS = (FPS + fps)/2;

                Bitmap res = _dynColVis.VisualizePositionMapSafe(positionMap, wallMap, addonWallMap, massMap, addonMassMap, _waveEngine.Width,
                                                              _waveEngine.Height);

                _tableManager.DisplayManager.SetBackgroundImage(res);
                res.Dispose();
                _busyFrame = false;
            }
        }

        void _tableManager_OnObjectMove(List<int> ObjectIDList)
        {
            //Debug.WriteLine("WaveTableEngine: ObjectMove");
            foreach (int i in ObjectIDList)
            {
                TableObject o = _tableManager.GetTableObject(i);
                if (o == null)
                    continue;

                //Only poke if at least 18 com above surface
                if (o.Height > 180)
                {
                    o.Center = PositionMapper.GetScreenCoordsfromDepth(o.Center);
                    Point p = new Point(o.Center.ScreenX,o.Center.ScreenY);
                    Point s = CalculateSimPosition(p);
                    _waveEngine.Poke(s.X,s.Y,10,0);
                }
            }
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
                lock (_lockSimuObjects)
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
