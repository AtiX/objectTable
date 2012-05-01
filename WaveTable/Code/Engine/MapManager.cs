using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using WaveSimLib.Code.Wave;
using WaveTable.Code.SimulationObjects;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace WaveTable.Code.Engine
{
    /// <summary>
    /// Merges the different maps together for the SimulationEngine
    /// </summary>
    class MapManager
    {
        private WaveSettings _loadedSettings;
        public WaveSettings LoadedSettings
        {
            get { return _loadedSettings; }
        }

        private WaveSettings _additionalSettings;

        private System.Drawing.Size _simulationSize;

        private WaveEngine _waveEngineRef;

        public MapManager(WaveEngine waveEngine)
        {
            _waveEngineRef = waveEngine;
        }

        public void Start(Size simulationSize)
        {
            _simulationSize = simulationSize;

                _loadedSettings = new WaveSettings(simulationSize);
                _additionalSettings = new WaveSettings(simulationSize);

            _waveEngineRef.Settings = _loadedSettings;
        }

        public bool LoadSettings(WaveSettings loaded, bool elong, bool sources, bool wall, bool mass, bool parameters)
        {
            _waveEngineRef.Stop();
           
            _loadedSettings = _waveEngineRef.Settings; //some times, we dont want to load all values - reuse current values that arent loaded


            if ((loaded.Width != _loadedSettings.Width)||(loaded.Height != _loadedSettings.Height))
            {
                _waveEngineRef.Start();
                return false;
            }

            if (elong)
                _loadedSettings.ElongationMap = loaded.ElongationMap;
            if (wall)
                _loadedSettings.WallMap = loaded.WallMap;
            if (mass)
                _loadedSettings.MassMap = loaded.MassMap;
            if (sources)
            {
                _loadedSettings.WaveSources.Clear();
                    _loadedSettings.WaveSources = loaded.WaveSources;
            }
            if (parameters)
            {
                _loadedSettings.DeltaT = loaded.DeltaT;
                _loadedSettings.DesiredFPS = loaded.DesiredFPS;
                _loadedSettings.Energieerhaltung = loaded.Energieerhaltung;
                _loadedSettings.FederkonstanteKopplung = loaded.FederkonstanteKopplung;
                _loadedSettings.FederkonstanteTeilchen = loaded.FederkonstanteTeilchen;
                _loadedSettings.TeilchenDistanz = loaded.TeilchenDistanz;
                _loadedSettings.Teilchenmasse = loaded.Teilchenmasse;
            }

            _waveEngineRef.Settings = _loadedSettings;

            _waveEngineRef.Start();
            return true;
        }

        public void Update(List<SimulationObject> SimObjList)
        {
            System.Diagnostics.Debug.WriteLine("MapManager: Update");
            if (!SimObjList.Any())
                return;

            //Stop the engine for synchronisation reasons
            _waveEngineRef.Stop();

            //Debug.WriteLine("MapManager: Update");  
            //divide the objects in three categorys: changed wave sources, changed elong-object, changed mass, changed wall 
            List<SimulationObject> sources =
                SimObjList.Where(o => o.GetType() == typeof (SimWaveSourceObject)).ToList();
            List<SimulationObject> elongation =
                SimObjList.Where(
                    o =>
                    o.ObjectType == SimulationObject.EObjectType.ConstElongation ||
                    o.ObjectType == SimulationObject.EObjectType.RelElongation).Where(o => o.GetType() != typeof(SimWaveSourceObject)).ToList();
            List<SimulationObject> mass =
                SimObjList.Where(o => o.ObjectType == SimulationObject.EObjectType.Mass).ToList();
            List<SimulationObject> wall =
                SimObjList.Where(o => o.ObjectType == SimulationObject.EObjectType.Wall).ToList();

            //Clear all maps where objects did change, and recreate data
            if (elongation.Where(o => o.PositionOrRotationChange == true).Any())
            {
                _additionalSettings.ElongationMap = new double[_simulationSize.Width,_simulationSize.Height];
                GenerateSimObjData(elongation.Where(o => o.PositionOrRotationChange == true).ToList());
            }

            if (mass.Where(o => o.PositionOrRotationChange == true).Any())
            {
                _additionalSettings.MassMap = new double[_simulationSize.Width,_simulationSize.Height];
                GenerateSimObjData(mass.Where(o => o.PositionOrRotationChange == true).ToList());
            }

            if (wall.Where(o => o.PositionOrRotationChange == true).Any())
            {
                _additionalSettings.WallMap = new bool[_simulationSize.Width,_simulationSize.Height];
                GenerateSimObjData(wall.Where(o => o.PositionOrRotationChange == true).ToList());
            }

            //Merge _additionalSettings with _loadedSettings
            //(SimObjects with mouse/stored values)
            WaveSettings exportSettings = GetMergedMaps();
            //Apply changes
            _waveEngineRef.Settings = exportSettings;

            if (sources.Where(o => o.PositionOrRotationChange == true).Any())
            {
                _additionalSettings.WaveSources.Clear();
                _waveEngineRef.ResetSources();
                GenerateSimObjData(sources);
                //Add Wave Sources
                //loadedSettings.sources refers directly to _waveEngine.Sources
                foreach (WaveSource src in _loadedSettings.WaveSources)
                {
                    _waveEngineRef.AddWaveSoucre(src);
                }
                foreach (WaveSource src in _additionalSettings.WaveSources)
                {
                    _waveEngineRef.AddWaveSoucre(src);
                }
            }

            //all changes done - restart the engine
            _waveEngineRef.Start();
        }

        /// <summary>
        /// Merges Velocity,Elogation, mass and walls of additional and loaded settings
        /// </summary>
        /// <returns>The Merged settings</returns>
        public WaveSettings GetMergedMaps()
        {
            WaveSettings exportSettings = new WaveSettings(_simulationSize);
            WaveSettings currentSetings = _waveEngineRef.Settings;

            exportSettings.VelocityMap = currentSetings.VelocityMap;
            exportSettings.ElongationMap = currentSetings.ElongationMap;

            for (int x = 0; x < _simulationSize.Width; x++)
            {
                for (int y = 0; y < _simulationSize.Height; y++)
                {
                    //Elongation
                    if (Math.Abs(_additionalSettings.ElongationMap[x, y] - 0.0) > 0.00000001)
                        exportSettings.ElongationMap[x, y] = _additionalSettings.ElongationMap[x, y];

                    //Mass
                    if (Math.Abs(_additionalSettings.MassMap[x, y] - 0.0) > 0.00000001)
                        exportSettings.MassMap[x, y] = _additionalSettings.MassMap[x, y];
                    else
                        exportSettings.MassMap[x, y] = _loadedSettings.MassMap[x, y];

                    //Walls
                    if (_additionalSettings.WallMap[x, y])
                        exportSettings.WallMap[x, y] = _additionalSettings.WallMap[x, y];
                    else
                        exportSettings.WallMap[x, y] = _loadedSettings.WallMap[x, y];
                }
            }
            return exportSettings;
        }

        /// <summary>
        /// Generates wall/elongation/mass data in the _additionalSettings Maps for all simulation Objects
        /// </summary>
        /// <param name="SimObjList"></param>
        private void GenerateSimObjData(List<SimulationObject> SimObjList)
        {
            foreach (SimulationObject simObj in SimObjList)
            {
                if (simObj.GetType() == typeof (SimPlotterObject))
                {
                    SimPlotterObject spo = (SimPlotterObject) simObj;

                    spo.Elongation = _waveEngineRef.GetPosition((int) simObj.SimPosition.X, (int) simObj.SimPosition.Y);
                    spo.Velocity = _waveEngineRef.GetVelocity((int) simObj.SimPosition.X, (int) simObj.SimPosition.Y);
                }
                else if (simObj.GetType() == typeof (SimWaveSourceObject))
                {
                    //Adjust Frequency by rotating the object
                    SimWaveSourceObject sws = (SimWaveSourceObject) simObj;

                    if (sws.oldDirection != null)
                    {
                        //Calculate the change (in degree)
                        //TODO: Berechnung des Drehwinkels, anpassung frequenz
                    }
                    else
                    {
                    }
                    //Adjust Source position
                    sws.WaveSourceRef.X = sws.SimPosition.X;
                    sws.WaveSourceRef.Y = sws.SimPosition.Y;

                    //Add Source to list
                    _additionalSettings.WaveSources.Add(sws.WaveSourceRef);
                }
                else
                {
                    //Simple object
                    switch (simObj.ObjectType)
                    {
                        case SimulationObject.EObjectType.Mass:
                            SetData(simObj.SimPosition, 1, 1, simObj.ObjectSize, 2, simObj.ObjectValue);
                            break;
                        case SimulationObject.EObjectType.ConstElongation:
                            SetData(simObj.SimPosition, 2, 1, simObj.ObjectSize, 2, simObj.ObjectValue);
                            break;
                        case SimulationObject.EObjectType.RelElongation:
                            SetData(simObj.SimPosition, 2, 1, simObj.ObjectSize, 2, simObj.ObjectValue);
                            break;
                        case SimulationObject.EObjectType.Wall:
                            SetData(simObj.SimPosition, 3, 1, simObj.ObjectSize, 2, simObj.ObjectValue/100.0);
                            break;
                    }
                }
            }
        }

        public void ClearMass(Point SimPosition, int Size)
        {
            SetData(SimPosition, 1, 1, Size, 1, 1.0);
        }

        public void ClearElongation(Point SimPosition, int Size)
        {
            SetData(SimPosition, 2, 1, Size, 1, 0.0);
        }

        public void ClearWall(Point SimPosition, int Size)
        {
            SetData(SimPosition, 4, 1, Size, 1, 0.0);
        }

        public void SetMass(Point SimPosition, double Value, int Size)
        {
            SetData(SimPosition, 1, 1, Size, 1, Value);
        }

        public void ClearSources()
        {
            _waveEngineRef.ResetSources();
            _loadedSettings.WaveSources.Clear();
        }

        public void SetWall(Point SimPosition, int Size)
        {
            SetData(SimPosition, 3, 1, Size,1,0);
        }

        public void SetSource(Point SimPosition, double Value)
        {
            SinusWaveSource sws = new SinusWaveSource();
            sws.Frequency = Value;
            sws.X = SimPosition.X;
            sws.Y = SimPosition.Y;
            _loadedSettings.WaveSources.Add(sws);
            _waveEngineRef.AddWaveSoucre(sws);
        }

        public void RemoveSource(WaveSource src)
        {
            _loadedSettings.WaveSources.Remove(src);
            _additionalSettings.WaveSources.Remove(src);
        }

        public void SetElongation(Point SimPosition, double Value, int Size)
        {
            SetData(SimPosition, 2, 1, Size, 1, Value);
        }


        /// <summary>
        /// Sets simulation Data
        /// </summary>
        /// <param name="SimPosition">The Center</param>
        /// <param name="type">1: Mass, 2: Elongation, 3: Set Wall, 4:destroy wall</param>
        /// <param name="shape">1: Rect, 2: Circle</param>
        /// <param name="size">The size of the shape</param>
        /// <param name="layer">1: Normal Paint Layer, 2: layer for Table Objects</param>
        private void SetData(Point SimPosition, int type, int shape, int size, int layer,double value)
        {
            //Check bounds
            if (SimPosition.X < 0 || SimPosition.X >= _loadedSettings.Width || SimPosition.Y < 0 || SimPosition.Y >= _loadedSettings.Height)
                return;

            if (shape == 1) //Rect
            {
                //Calculate rect bounds
                int xMin = SimPosition.X - (int)Math.Floor(size / 2.0);
                int xMax = (int)Math.Round(SimPosition.X + Math.Ceiling(size/2.0));
                int yMin = SimPosition.Y - (int) Math.Floor(size/2.0);
                int yMax = (int) Math.Round(SimPosition.Y + Math.Ceiling(size/2.0));

                if (xMin < 0)
                    xMin = 0;
                if (yMin < 0)
                    yMin = 0;
                if (yMax > _loadedSettings.Height)
                    yMax = _loadedSettings.Height;
                if (xMax > _loadedSettings.Width)
                    yMax = _loadedSettings.Width;

                //Fill Rectangle
                for (int x = xMin; x < xMax; x++)
                {
                    for (int y = yMin; y < yMax; y++)
                    {
                        //normal layer or tableObject layer
                        if (layer == 1)
                        {
                            //set according to type
                            switch (type)
                            {
                                case 1: //mass
                                    _waveEngineRef.SetMass(x, y, value);
                                    break;
                                case 2: //elongation
                                    _waveEngineRef.Poke(x, y, value, 0);
                                    break;
                                case 3: //wall
                                    _waveEngineRef.SetWall(x, y, true);
                                    break;
                                case 4: //remove wall
                                    _waveEngineRef.SetWall(x, y, false);
                                    break;
                            }
                        }
                        else
                        { //Table object layer
                            //set according to type
                            switch (type)
                            {
                                case 1: //mass
                                    _additionalSettings.MassMap[x, y] = value;
                                    break;
                                case 2: //elongation
                                    _additionalSettings.ElongationMap[x, y] = value;
                                    break;
                                case 3: //wall
                                    _additionalSettings.WallMap[x, y] = true;
                                    break;
                                case 4: //remove wall
                                    _additionalSettings.WallMap[x, y] = false;
                                    break;
                            }
                        }
                    }
                }
            }
            else if (shape == 2)
            {
                //Circle
                //TODO
            }
        }
    }
}
