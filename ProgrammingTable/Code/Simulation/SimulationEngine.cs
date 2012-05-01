using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using ObjectTable.Code;
using ObjectTable.Code.Recognition.DataStructures;
using ProgrammingTable.Code.Graphics;
using ProgrammingTable.Code.Simulation.Math;
using ProgrammingTable.Code.Simulation.Menu;
using ProgrammingTable.Code.Simulation.Objects;
using ProgrammingTable.Code.General;

namespace ProgrammingTable.Code.Simulation
{
    class SimulationEngine
    {
        private TableManager _tablemanager;
        private MenuGfxMgr _menumgr;
        private SimObjDrawer _objDrawer;

        private object _lockObjectList = "";
        private List<SimulationObject> _objectList;

        //private Timer _simulationTimer;
        private DispatcherTimer _dispatcherSimulationTimer;
        private DispatcherTimer _dispatcherGfxUpdateTimer;

        private object _lockModifiedObjects = "";
        private List<int> _modifiedObjects;

        public TimeSpan SimulationDelay
        {
            get { return _dispatcherSimulationTimer.Interval; }
            set { _dispatcherSimulationTimer.Interval = value; }
        }

        public bool SimulationRunning
        {
            get { return _dispatcherSimulationTimer.IsEnabled; }
            set { _dispatcherSimulationTimer.IsEnabled = value;
            _menumgr.Enabled = value;
            }
        }

        public SimulationEngine(TableManager tmgr)
        {
            _tablemanager = tmgr;
            
            _menumgr = new MenuGfxMgr(_tablemanager);
            _objectList = new List<SimulationObject>();

            _modifiedObjects = new List<int>();

            //_simulationTimer = new Timer(SimulationTick, null, 500, (int)LocalSettingsManager.PrgTblSet.SimulationTickDelay);
            _dispatcherSimulationTimer = new DispatcherTimer(DispatcherPriority.Normal,
                                                             _tablemanager.BeamerScreen.Dispatcher);

            _dispatcherSimulationTimer.Interval = new TimeSpan(0,0,0,0,LocalSettingsManager.PrgTblSet.SimulationTickDelay);
            _dispatcherSimulationTimer.Tick +=new EventHandler(SimulationTick);
            _dispatcherSimulationTimer.IsEnabled = false;

            _dispatcherGfxUpdateTimer = new DispatcherTimer(DispatcherPriority.Normal,
                                                            _tablemanager.BeamerScreen.Dispatcher);
            _dispatcherGfxUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherGfxUpdateTimer.Tick += new EventHandler(_dispatcherGfxUpdateTimer_Tick);
            _dispatcherGfxUpdateTimer.IsEnabled = true;

            _objDrawer = new SimObjDrawer(_tablemanager);

            _tablemanager.OnNewLongTermObject += new TableManager.TableManagerObjectChange(_tablemanager_OnNewLongTermObject);
            _tablemanager.OnObjectMove += new TableManager.TableManagerObjectChange(_tablemanager_OnObjectMove);
            _tablemanager.OnObjectRotate += new TableManager.TableManagerObjectChange(_tablemanager_OnObjectRotate);
            _tablemanager.OnObjectRemove += new TableManager.TableManagerObjectChange(_tablemanager_OnObjectRemove);
        }

        void _dispatcherGfxUpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (_lockObjectList)
            {
                _objDrawer.UpdateObjectGfx(_objectList);
            }
        }
      
        void SimulationTick(object too, EventArgs e)
        {
            //First, update changed objects
                lock (_lockModifiedObjects)
                {
                    UpdateTableObjects(_modifiedObjects);
                    _modifiedObjects.Clear();
                }

            lock (_lockObjectList)
            {
                SimulationObject crashedObject = null;

                foreach (SimulationObject obj in _objectList)
                {
                    //First, calculate the possible destinations for the object
                    //but only, if the object set the 'ShootBeam' to true
                    if (obj.ShootBeam)
                    {
                        List<SimulationObject> PossibleDestinations = CalculateDestinations(obj,obj.LastTableObject.DirectionVector);
                        obj.PossibleDestinations = PossibleDestinations;

                        //Any additional directions
                        if (obj.AdditionalDirections != null)
                        {
                            if (obj.PossibleAdditionalDestinations == null)
                                obj.PossibleAdditionalDestinations = new List<List<SimulationObject>>();
                            else
                                obj.PossibleAdditionalDestinations.Clear();

                            foreach (Vector direction in obj.AdditionalDirections)
                            {
                                List<SimulationObject> dest = CalculateDestinations(obj, direction);
                                obj.PossibleAdditionalDestinations.Add(dest);
                            }
                        }
                    }

                    //Now calculate the object's sources
                    CalculateObjSources(obj);

                    //Let the object do calculations
                    try
                    {
                        obj.SimulationTick();
                    }
                    catch (Exception ex)
                    {
                        //Inform the user about the exception and remove the object from the simulation
                        MessageBox.Show("A Simulation object has crashed.\n \n Name: " + obj.Name +
                                        " \n \n Exception:\n" + ex.ToString()+ "\n \n The object will be removed from Simulation","Warning!");
                        //remove the object
                        try
                        {
                            obj.Remove();
                            crashedObject = obj;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Removal of the object was unsuccessful", "Error");
                        }
                    }
                }

                //If an object crashed, remove it
                if (crashedObject!= null)
                {
                    _objectList.Remove(crashedObject);
                }
            }
        }

        private void CalculateObjSources(SimulationObject obj)
        {
            obj.SourceObjects = _objectList.Where(o => o.Destinations.Contains(obj)).ToList();

            List<double> distances = new List<double>();
            foreach (SimulationObject sob in obj.SourceObjects)
            {
                distances.Add(obj.LastTableObject.Center.DistanceTo(sob.LastTableObject.Center, TPoint.PointCreationType.screen));
            }

            //sort - nearest objects first
            SortByDistance(distances, obj.SourceObjects);
        }

        private List<SimulationObject> CalculateDestinations(SimulationObject obj, Vector direction)
        {
            if (obj.LastTableObject == null)
                return new List<SimulationObject>();

            if (obj.LastTableObject.RotationDefined == false)
                return new List<SimulationObject>();
            
            obj.LastTableObject.Center.CalculateScreenfromDepthCoords();
            
            Vector stütztV = new Vector(obj.LastTableObject.Center.ScreenX, obj.LastTableObject.Center.ScreenY);
            Strecke beam = new Strecke(stütztV, direction);

            List<SimulationObject> targets = new List<SimulationObject>();
            List<double> distances = new List<double>();

            //Do this for all objects with the same layer (except for the object itself)
            foreach(SimulationObject simObj in _objectList.Where(o => o.Layer == obj.Layer).Where(o => o.LastTableObject != null).Where(o => o.LastTableObject.ObjectID != obj.LastTableObject.ObjectID))
            {
                Point pcenter = new Point(simObj.LastTableObject.Center.ScreenX, simObj.LastTableObject.Center.ScreenY);
                double distance = beam.AbstandAbStartpunkt(pcenter);

                if ((distance > 0)&&(distance <= LocalSettingsManager.PrgTblSet.DestinationObjectMaxBeamDistance))
                {
                    targets.Add(simObj);
                    //distance between two objects
                    distances.Add(obj.LastTableObject.Center.DistanceTo(simObj.LastTableObject.Center,TPoint.PointCreationType.screen));
                }
            }

            //sort - nearest objects first
            SortByDistance(distances, targets);
            return targets;
        }

        private static void SortByDistance(List<double> distances, List<SimulationObject> targets)
        {
            for (int i = 0; i < targets.Count - 1; i++)
            {
                for (int k = i; k < targets.Count; k++)
                {
                    if (distances[k] < distances[i])
                    {
                        //switch
                        double tmp = distances[k];
                        distances[k] = distances[i];
                        distances[i] = tmp;

                        SimulationObject o = targets[k];
                        targets[k] = targets[i];
                        targets[i] = o;
                    }
                }
            }
        }

        void _tablemanager_OnObjectRemove(List<int> ObjectIDList)
        {
            foreach (int ID in ObjectIDList)
            {
                //Remove the objects from the simulation if they are removed from the table
                lock (_lockObjectList)
                {
                    List<SimulationObject> delobjl = _objectList.Where(o => o.LastTableObject.ObjectID == ID).ToList();
                    if (delobjl.Count > 0)
                    {
                        //Let the object do cleanup routines
                        delobjl[0].Remove();
                        //Remove Gfx
                        _objDrawer.RemoveObjectGfx(delobjl[0]);
                        //remove it from the list
                        _objectList.Remove(delobjl[0]);
                    }
                }
            }
        }

        void _tablemanager_OnObjectRotate(List<int> ObjectIDList)
        {
            lock (_lockModifiedObjects)
            {
                foreach(int i in ObjectIDList)
                {
                    if (!_modifiedObjects.Contains(i))
                        _modifiedObjects.Add(i);
                }
            }
        }

        void _tablemanager_OnObjectMove(List<int> ObjectIDList)
        {
            lock (_lockModifiedObjects)
            {
                foreach (int i in ObjectIDList)
                {
                    if (!_modifiedObjects.Contains(i))
                        _modifiedObjects.Add(i);
                }
            }
        }

        /// <summary>
        /// Updates the tableObjects assigned to the simObjects with the specific IDs
        /// </summary>
        /// <param name="ObjectIDList"></param>
        public void UpdateTableObjects(List<int> ObjectIDList)
        {
            lock (_lockObjectList)
            {
                //Get TableObjectList
                List<TableObject> tobjList = _tablemanager.TableObjects;

                //Get the SimObjects
                List<SimulationObject> objectList = new List<SimulationObject>();
                foreach (int id in ObjectIDList)
                {
                    SimulationObject[] soa = _objectList.Where(s => s.LastTableObject.ObjectID == id).ToArray();
                    if (soa.Count() > 0)
                    {
                        objectList.Add(soa[0]);
                    }
                }

                //Assign updated TableObjects
                foreach (SimulationObject simobj in objectList)
                {
                    if (tobjList.Where(o => o.ObjectID == simobj.LastTableObject.ObjectID).Count() == 0)
                        return;

                    TableObject newTO = tobjList.Where(o => o.ObjectID == simobj.LastTableObject.ObjectID).ToArray()[0];
                    
                    //If rotation isn't recognized, use the old value
                    if ((newTO.RotationDefined == false)&&(simobj.LastTableObject != null))
                    {
                        newTO.DirectionVector = simobj.LastTableObject.DirectionVector;
                        newTO.RotationDefined = simobj.LastTableObject.RotationDefined;
                    }
                    //Also use the old value, if the new values are NaN
                    if (((Double.IsNaN(newTO.DirectionVector.X))||(Double.IsNaN(newTO.DirectionVector.Y)))&&(simobj.LastTableObject != null))
                    {
                        newTO.DirectionVector = simobj.LastTableObject.DirectionVector;
                        newTO.RotationDefined = simobj.LastTableObject.RotationDefined;
                    }

                    simobj.LastTableObject = newTO;
                    simobj.TableObjectDidChange = true;
                }
            }
        }

        void _tablemanager_OnNewLongTermObject(List<int> ObjectIDList)
        {
            foreach ( int ID in ObjectIDList)
            {
                TableObject tobj = _tablemanager.TableObjects.Where(o => o.ObjectID == ID).ToArray()[0];
                tobj.Center.CalculateScreenfromDepthCoords();

                //Check whether object is on a menue field
                if (_menumgr.IsOnMenuField(tobj))
                {
                    //Get the type of the simulationobject from the menumgr
                    Type p = _menumgr.GetSimObjectType(tobj);

                    //Create instance and add to list
                    SimulationObject simo = (SimulationObject)Activator.CreateInstance(p);
                    
                    simo.Destinations = new List<SimulationObject>();
                    simo.Destinations.Add(null);

                    simo.LastTableObject = tobj;
                    
                    //bind to events
                    simo.OnNewMiddleScreenElement += new SimulationObject.ScreenElementHandler(simo_OnNewMiddleScreenElement);
                    simo.OnScreenElementModify += new SimulationObject.ScreenElementModify(simo_OnScreenElementModify);
                    simo.OnScreenElementRemove += new SimulationObject.ScreenElementHandler(simo_OnScreenElementRemove);
                    simo.OnNewBottomScreenElement += new SimulationObject.ScreenElementHandler(simo_OnNewBottomScreenElement);

                    //let the object initialize
                    simo.Init();

                    lock (_lockObjectList)
                    {
                        _objectList.Add(simo);
                    }
                }
            }
        }

        void simo_OnNewBottomScreenElement(UIElement e)
        {
            _tablemanager.DisplayManager.AddElement(e, ObjectTable.Code.Display.DisplayManager.DisplayLayer.bottom);
        }

        void simo_OnScreenElementRemove(UIElement e)
        {
            _tablemanager.DisplayManager.DeleteElement(e);
            //_tablemanager.DisplayManager.DeleteElementNotThreadSafe(e);
        }

        void simo_OnScreenElementModify(Delegate action, object[] args)
        {
            _tablemanager.DisplayManager.WorkThreadSafe(action, args);
        }

        void simo_OnNewMiddleScreenElement(UIElement e)
        {
            _tablemanager.DisplayManager.AddElement(e, ObjectTable.Code.Display.DisplayManager.DisplayLayer.middle);
        }
    }
}
