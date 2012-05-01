using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    /// <summary>
    /// Contains method commonly used amongst SimulationObjects
    /// </summary>
    static class SimulationObjectHelper
    {
        /// <summary>
        /// Updates the object's screenLine
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="_screenLine">the screen line. null, if no screen line exists</param>
        /// <returns>returns a screen line if a screenline needs to be created</returns>
        public static ScreenLine UpdateBeam(this SimulationObject obj, ScreenLine _screenLine, SimulationObject destination, Vector Direction)
        {
            if (obj.LastTableObject.RotationDefined)
            {
                if (_screenLine == null)
                {
                    //recreate it
                    _screenLine = new ScreenLine(obj.LastTableObject.Center, Direction * 1000,
                                                 ScreenLine.EScreenLineElementType.Arrow);
                    return _screenLine;
                }
                else
                {
                    //just modify
                    //If destination is set, let the screen line end.if not:infinite
                    if (destination != null)
                    {
                        _screenLine.Source = obj.LastTableObject.Center;
                        _screenLine.Destination = destination.LastTableObject.Center;
                    }
                    else
                    {
                        _screenLine.Source = obj.LastTableObject.Center;
                        TPoint dest =
                            new TPoint(
                                Convert.ToInt32(obj.LastTableObject.Center.ScreenX +
                                                Direction.X * 1000),
                                Convert.ToInt32(obj.LastTableObject.Center.ScreenY +
                                                Direction.Y * 1000),
                                TPoint.PointCreationType.screen);
                        _screenLine.Destination = dest;
                    }
                }
            }
            return null;
        }
    }
}
