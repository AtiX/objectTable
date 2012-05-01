using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code;
using ObjectTable.Code.Display;
using ObjectTable.Code.Recognition.DataStructures;
using ProgrammingTable.Code.Simulation.Objects;

namespace ProgrammingTable.Code.Graphics
{
    /// <summary>
    /// Handles the drawing and Visualisation of SimulationObjects
    /// </summary>
    class SimObjDrawer
    {
        private TableManager _tmgr;

        public SimObjDrawer(TableManager tmgr)
        {
            _tmgr = tmgr;
        }

        public void UpdateObjectGfx(List<SimulationObject> Objects)
        {
            if (!_tmgr.BeamerScreen.Dispatcher.CheckAccess())
            {
                _tmgr.BeamerScreen.Dispatcher.Invoke((Action)(() => RunUpdate(Objects)));
            }
            else
            {
                RunUpdate(Objects);
            }
        }

        private void RunUpdate(List<SimulationObject> Objects)
        {
            foreach (SimulationObject SimO in Objects)
            {
                SimObjGraphicsSettings gfxSet = SimO.GraphicsSettings;

                //Check ObjectCircle
                UpdateObjecCircle(SimO, gfxSet);

                //Check Primary&Secondary ScreenLine
                UpdatePrimScreenLine(SimO, gfxSet);
                UpdateSecScreenLine(SimO,gfxSet);
            }
        }

        private void UpdatePrimScreenLine(SimulationObject SimO, SimObjGraphicsSettings gfxSet)
        {
            if (gfxSet.DrawPrimScreenLine && SimO.LastTableObject.RotationDefined)
            {
                bool recalculate = false;

                if (gfxSet.PrimScreenLineDest != null)
                {
                    //Case 1: a target object is set
                    if (gfxSet.PrimScreenLineRef == null)
                    {
                        gfxSet.PrimScreenLineRef = new ScreenLine(SimO.LastTableObject.Center,
                                                                  gfxSet.PrimScreenLineDest.LastTableObject.Center,
                                                                  gfxSet.PrimSlElementType);

                        gfxSet.PrimScreenLineRef.LineElementColor = gfxSet.PrimSlElementColor;
                        _tmgr.DisplayManager.AddElement(gfxSet.PrimScreenLineRef, DisplayManager.DisplayLayer.middle);
                        recalculate = true;
                    }
                    else
                    {
                        gfxSet.PrimScreenLineRef.LineElementColor = gfxSet.PrimSlElementColor;

                        if (!gfxSet.PrimScreenLineRef.Source.Equals(SimO.LastTableObject.Center))
                        {
                            gfxSet.PrimScreenLineRef.Source = SimO.LastTableObject.Center;
                            recalculate = true;
                        }
                        if (!gfxSet.PrimScreenLineRef.Destination.Equals(gfxSet.PrimScreenLineDest.LastTableObject.Center))
                        {
                            gfxSet.PrimScreenLineRef.Destination = gfxSet.PrimScreenLineDest.LastTableObject.Center;
                            recalculate = true;
                        }
                    }
                }
                else
                {
                    //Case 2: Use direction Vector
                    gfxSet.PrimScreenLineDirection = SimO.LastTableObject.DirectionVector;

                    if (gfxSet.PrimScreenLineRef == null)
                    {
                        gfxSet.PrimScreenLineRef = new ScreenLine(SimO.LastTableObject.Center, gfxSet.PrimScreenLineDirection,
                                                                  gfxSet.PrimSlElementType);
                        gfxSet.PrimScreenLineRef.LineElementColor = gfxSet.PrimSlElementColor;
                        _tmgr.DisplayManager.AddElement(gfxSet.PrimScreenLineRef, DisplayManager.DisplayLayer.middle);
                        recalculate = true;
                    }
                    else
                    {
                        gfxSet.PrimScreenLineRef.LineElementColor = gfxSet.PrimSlElementColor;

                        if (!gfxSet.PrimScreenLineRef.Source.Equals(SimO.LastTableObject.Center))
                        {
                            gfxSet.PrimScreenLineRef.Source = SimO.LastTableObject.Center;
                            recalculate = true;
                        }

                        TPoint dest =
                            new TPoint(
                                Convert.ToInt32(SimO.LastTableObject.Center.ScreenX +
                                                gfxSet.PrimScreenLineDirection.X*1000),
                                Convert.ToInt32(SimO.LastTableObject.Center.ScreenY +
                                                gfxSet.PrimScreenLineDirection.Y*1000),
                                TPoint.PointCreationType.screen);

                        if (!gfxSet.PrimScreenLineRef.Destination.Equals(dest))
                        {
                            gfxSet.PrimScreenLineRef.Destination = dest;
                            recalculate = true;
                        }
                    }
                }

                if (recalculate)
                    gfxSet.PrimScreenLineRef.CalculateLine();
            }
            else
            {
                //Do not draw ScreenLine
                if (gfxSet.PrimScreenLineRef != null)
                {
                    _tmgr.DisplayManager.DeleteElement(gfxSet.PrimScreenLineRef);
                    gfxSet.PrimScreenLineRef = null;
                }
            }
        }

        private void UpdateSecScreenLine(SimulationObject SimO, SimObjGraphicsSettings gfxSet)
        {
            if (gfxSet.DrawSecScreenLine && SimO.LastTableObject.RotationDefined)
            {
                bool recalculate = false;

                if (gfxSet.SecScreenLineDest != null)
                {
                    //Case 1: a target object is set
                    if (gfxSet.SecScreenLineRef == null)
                    {
                        gfxSet.SecScreenLineRef = new ScreenLine(SimO.LastTableObject.Center,
                                                                  gfxSet.SecScreenLineDest.LastTableObject.Center,
                                                                  gfxSet.SecSlElementType);

                        gfxSet.SecScreenLineRef.LineElementColor = gfxSet.SecSlElementColor;
                        _tmgr.DisplayManager.AddElement(gfxSet.SecScreenLineRef, DisplayManager.DisplayLayer.middle);
                        recalculate = true;
                    }
                    else
                    {
                        gfxSet.SecScreenLineRef.LineElementColor = gfxSet.SecSlElementColor;

                        if (!gfxSet.SecScreenLineRef.Source.Equals(SimO.LastTableObject.Center))
                        {
                            gfxSet.SecScreenLineRef.Source = SimO.LastTableObject.Center;
                            recalculate = true;
                        }
                        if (!gfxSet.SecScreenLineRef.Destination.Equals(gfxSet.SecScreenLineDest.LastTableObject.Center))
                        {
                            gfxSet.SecScreenLineRef.Destination = gfxSet.SecScreenLineDest.LastTableObject.Center;
                            recalculate = true;
                        }
                    }
                }
                else
                {
                    //Case 2: Use direction Vector
                    if (gfxSet.SecScreenLineRef == null)
                    {
                        gfxSet.SecScreenLineRef = new ScreenLine(SimO.LastTableObject.Center, gfxSet.SecScreenLineDirection,
                                                                  gfxSet.SecSlElementType);
                        gfxSet.SecScreenLineRef.LineElementColor = gfxSet.SecSlElementColor;
                        _tmgr.DisplayManager.AddElement(gfxSet.SecScreenLineRef, DisplayManager.DisplayLayer.middle);
                        recalculate = true;
                    }
                    else
                    {
                        gfxSet.SecScreenLineRef.LineElementColor = gfxSet.SecSlElementColor;

                        if (!gfxSet.SecScreenLineRef.Source.Equals(SimO.LastTableObject.Center))
                        {
                            gfxSet.SecScreenLineRef.Source = SimO.LastTableObject.Center;
                            recalculate = true;
                        }

                        TPoint dest =
                            new TPoint(
                                Convert.ToInt32(SimO.LastTableObject.Center.ScreenX +
                                                gfxSet.SecScreenLineDirection.X * 1000),
                                Convert.ToInt32(SimO.LastTableObject.Center.ScreenY +
                                                gfxSet.SecScreenLineDirection.Y * 1000),
                                TPoint.PointCreationType.screen);

                        if (!gfxSet.SecScreenLineRef.Destination.Equals(dest))
                        {
                            gfxSet.SecScreenLineRef.Destination = dest;
                            recalculate = true;
                        }
                    }
                }

                if (recalculate)
                    gfxSet.SecScreenLineRef.CalculateLine();
            }
            else
            {
                //Do not draw ScreenLine
                if (gfxSet.SecScreenLineRef != null)
                {
                    _tmgr.DisplayManager.DeleteElement(gfxSet.SecScreenLineRef);
                    gfxSet.SecScreenLineRef = null;
                }
            }
        }

        private void UpdateObjecCircle(SimulationObject SimO, SimObjGraphicsSettings gfxSet)
        {
            if (gfxSet.DrawCircle)
            {
                if (gfxSet.CircleRef == null)
                {
                    gfxSet.CircleRef = new ObjectCircle(SimO.LastTableObject.Center);
                    _tmgr.DisplayManager.AddElement(gfxSet.CircleRef, DisplayManager.DisplayLayer.top);
                }
                else
                {
                    gfxSet.CircleRef.Center = SimO.LastTableObject.Center;
                }

                gfxSet.CircleRef.SetColor(gfxSet.CircleColor);
                gfxSet.CircleRef.SetText(1, gfxSet.Text1);
                gfxSet.CircleRef.SetText(2, gfxSet.Text2);
            }
            else
            {
                //Do not draw circle
                if (gfxSet.CircleRef != null)
                {
                    _tmgr.DisplayManager.DeleteElement(gfxSet.CircleRef);
                    gfxSet.CircleRef = null;
                }
            }
        }

        public void RemoveObjectGfx(SimulationObject SimO)
        {
            if (_tmgr.BeamerScreen.Dispatcher.CheckAccess())
            {
                RemoveAllGfx(SimO);
            }
            else
            {
                _tmgr.BeamerScreen.Dispatcher.BeginInvoke((Action) (() => RemoveAllGfx(SimO)));
            }
        }

        private void RemoveAllGfx(SimulationObject SimO)
        {
            if (SimO.GraphicsSettings.CircleRef != null)
                _tmgr.DisplayManager.DeleteElement(SimO.GraphicsSettings.CircleRef);
            if (SimO.GraphicsSettings.PrimScreenLineRef != null)
                _tmgr.DisplayManager.DeleteElement(SimO.GraphicsSettings.PrimScreenLineRef);
            if (SimO.GraphicsSettings.SecScreenLineRef != null)
                _tmgr.DisplayManager.DeleteElement(SimO.GraphicsSettings.SecScreenLineRef);
        }
    }
}
