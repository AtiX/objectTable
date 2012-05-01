using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ObjectTable.Code;
using ObjectTable.Code.Recognition.DataStructures;
using ProgrammingTable.Code.Simulation.Objects;

namespace ProgrammingTable.Code.Simulation.Menu
{
    class MenuGfxMgr
    {
        private List<MenuObject> _objectList;
        private int _index = 0;
        private Timer _menuScrollTimer;

        private TableManager _tableManager;
        private GUI.Table.Menu.Menu _gfxMenu;

        private GraphicalRegion _b1 = null, _b2 = null, _b3 = null, _b4= null;
        public bool Enabled
        {
            get { return _menuScrollTimer.Enabled; }
            set { _menuScrollTimer.Enabled = value; }
        }

        public MenuGfxMgr(TableManager tmgr)
        {
            _objectList = new List<MenuObject>();
            
            //Fill object List
            List<Type> typelist = GetSimObjectTypes();
            foreach(Type tp in typelist)
            {
                try
                {
                    object o = Activator.CreateInstance(tp);
                    SimulationObject so = o as SimulationObject;
                    MenuObject mo = new MenuObject();
                    mo.name = so.Name;
                    mo.category = so.Category;
                    mo.shortsign = so.ShortSign;
                    mo.type = tp;
                    _objectList.Add(mo);
                }
                catch (Exception)
                {
                    
                }
            }

            //AssignTableManager
            _tableManager = tmgr;
            _tableManager.OnNewObjectList += new TableManager.TableManagerObjectHandler(_tableManager_OnNewObjectList);
            //Create graphical menu
            _tableManager.DisplayManager.WorkThreadSafe((Action) (() =>
                                                                      {
                                                                          _gfxMenu = new GUI.Table.Menu.Menu();
                                                                          _tableManager.DisplayManager.AddElement(
                                                                              _gfxMenu,
                                                                              ObjectTable.Code.Display.DisplayManager.
                                                                                  DisplayLayer.bottom);
                                                                          Canvas.SetLeft(_gfxMenu, 0);
                                                                          Canvas.SetTop(_gfxMenu, 50);
                                                                          //Get Regions
                                                                          GetRegions(out _b4, out _b3, out _b2, out _b1);
                                                                      }), null,false);
            UpdateMenuGUI();

            //init Scrolltimer
            _menuScrollTimer = new Timer();
            _menuScrollTimer.Interval = 500;
            _menuScrollTimer.Elapsed += new ElapsedEventHandler(_menuScrollTimer_Elapsed);
            //timer is initialized when the first tableObjects are recognized
        }

        void _tableManager_OnNewObjectList()
        {
            //First table objects -> start timer
            _menuScrollTimer.Enabled = true;
        }

        void _menuScrollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _menuScrollTimer.Enabled = false;

            //Calculate the ScreenAreas
            _tableManager.DisplayManager.WorkThreadSafe((Action) (() => _gfxMenu.CalculateScreenAreas()), null,false);

            //Check whether there are any objects on the left or right arrows
            //Get The regions
            GraphicalRegion left = null;
            _tableManager.DisplayManager.WorkThreadSafe((Action)(() => { left = (GraphicalRegion)_gfxMenu.arrowLeft.Clone(); }), null, false);
            GraphicalRegion right = null;
            _tableManager.DisplayManager.WorkThreadSafe((Action)(() => { right = (GraphicalRegion)_gfxMenu.arrowRight.Clone(); }), null, false);

            //Now check every object
            bool scrollLeft = false;
            bool scrollRight = false;

            List<TableObject> objects = _tableManager.TableObjects;
            if (objects == null)
            {
                _menuScrollTimer.Enabled = true;
                return;
            }

            //If the program shuts down, these variables are null
            if (left == null || right == null)
            {
                _menuScrollTimer.Enabled = true;
                return;
            }

            foreach (TableObject tobj in objects.Where(o => o.CenterDefined))
            {
                if (left.IsInRegion(tobj.Center))
                    scrollLeft = true;
                if (right.IsInRegion(tobj.Center))
                    scrollRight = true;
            }

            //Don't scroll if objects on both arrows (both 'pressed')
            if (scrollLeft && scrollRight)
                return;

            //scroll left or right
            if (scrollLeft)
            {
                if (_index > 0)
                    _index--;
                UpdateMenuGUI();
            }
            else if (scrollRight)
            {
                int maxindex = _objectList.Count - 1;
                //Scroll only that far so that 4 objects still fit in the menue
                if (_index + 1 + 3 <= maxindex)
                    _index++;
                UpdateMenuGUI();
            }

            _menuScrollTimer.Enabled = true;
        }

        private void UpdateMenuGUI()
        {
            //Fill the menu according to the index
            MenuObject i1 = null, i2= null, i3=null, i4=null;

            //Special case: less than 5 items
            if (_objectList.Count < 4)
            {
                if (_objectList.Count == 0)
                {
                    i1 = new MenuObject();
                    i2 = new MenuObject();
                    i3 = new MenuObject();
                    i4 = new MenuObject();
                }
                else if (_objectList.Count < 2)
                {
                    i1 = _objectList[_index];
                    i2 = new MenuObject();
                    i3 = new MenuObject();
                    i4 = new MenuObject();
                }
                else if (_objectList.Count < 3)
                {
                    i1 = _objectList[_index];
                    i2 = _objectList[_index + 1];
                    i3 = new MenuObject();
                    i4 = new MenuObject();
                }
                else if (_objectList.Count < 4)
                {
                    i1 = _objectList[_index];
                    i2 = _objectList[_index + 1];
                    i3 = _objectList[_index + 2];
                    i4 = new MenuObject();
                }
            }
            else 
            {
                //More than 4 items
                i1 = _objectList[_index];
                i2 = _objectList[_index + 1];
                i3 = _objectList[_index + 2];
                i4 = _objectList[_index + 3];
            }

            //Now fill the items
            _tableManager.DisplayManager.WorkThreadSafe((Action) (() => _gfxMenu.SetElements(new MenuObject[]
                                                                                                 {i1, i2, i3, i4})), null);
        }
   
        public Type GetSimObjectType(TableObject obj)
        {
            if (!IsOnMenuField(obj))
                return null;

            //On which menu field
            int m = GetMenueField(obj);
            MenuObject mobj = _objectList[_index + m];
            //return type
            return mobj.type;
        }

        /// <summary>
        /// Checks wheter the object is on a Menu field. Returns false, too, when the object is on a empty menu field
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsOnMenuField(TableObject obj)
        {
            //Check for proper initialisation: if not, reinitialize
            if (_b1 == null)
            {
                GetRegions(out _b4, out _b3, out _b2, out _b1);
            }
            if (_b1.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 0)
                    return true;
            }
            else if (_b2.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 1)
                    return true;
            }
            else if (_b3.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 2)
                    return true;
            }
            else if (_b4.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 3)
                    return true;
            }
            return false;
        }

        private void GetRegions(out GraphicalRegion b4, out GraphicalRegion b3, out GraphicalRegion b2, out GraphicalRegion b1)
        {
            //recalculate screen areas
            _tableManager.DisplayManager.WorkThreadSafe((Action) (() => _gfxMenu.CalculateScreenAreas()), null);

            //Check wheter the object is in one of the regions of the menu objects
            b1 = null;
            GraphicalRegion b1_ = null;
            b2 = null;
            GraphicalRegion b2_ = null;
            b3 = null;
            GraphicalRegion b3_ = null;
            b4 = null;
            GraphicalRegion b4_ = null;

            _tableManager.BeamerScreen.Dispatcher.Invoke((Action) (() => { b1_ = (GraphicalRegion) _gfxMenu.MObject1.Clone(); }),
                                                        null);
            _tableManager.BeamerScreen.Dispatcher.Invoke((Action)(() => { b2_ = (GraphicalRegion)_gfxMenu.MObject2.Clone(); }),
                                                        null);
            _tableManager.BeamerScreen.Dispatcher.Invoke((Action)(() => { b3_ = (GraphicalRegion)_gfxMenu.MObject3.Clone(); }),
                                                        null);
            _tableManager.BeamerScreen.Dispatcher.Invoke((Action)(() => { b4_ = (GraphicalRegion)_gfxMenu.MObject4.Clone(); }),
                                                        null);

            b1 = b1_;
            b2 = b2_;
            b3 = b3_;
            b4 = b4_;
        }

        /// <summary>
        /// Returns the number of the menu field (0-3). -1 if the object isnt on a menue filed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetMenueField(TableObject obj)
        {
            if (_b1.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 0)
                    return 0;
            }
            else if (_b2.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 1)
                    return 1;
            }
            else if (_b3.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 2)
                    return 2;
            }
            else if (_b4.IsInRegion(obj.Center))
            {
                if (_objectList.Count > 3)
                    return 3;
            }
            return -1;
        }

        /// <summary>
        /// Gets all classes of simulationObjects 
        /// </summary>
        /// <returns></returns>
        private List<Type> GetSimObjectTypes()
        {
            Type[] tlist = Assembly.GetEntryAssembly().GetTypes();
            return tlist.Where(t => t.IsSubclassOf(typeof (SimulationObject))).ToList();
        }
    }
}
