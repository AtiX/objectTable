using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable;
using ProgrammingTable.Code.Simulation.Objects;

namespace ProgrammingTable.Code.Graphics
{
    class SimObjGraphicsSettings
    {
        public bool DrawCircle = true;
        public string Text1 = "";
        public string Text2 = "";
        public ObjectCircle.EColor CircleColor = ObjectCircle.EColor.blue;

        public bool DrawPrimScreenLine = true;
        public ScreenLine.EScreenLineElementType PrimSlElementType = ScreenLine.EScreenLineElementType.Arrow;
        public ScreenLine.EScreenLineElementColor PrimSlElementColor = ScreenLine.EScreenLineElementColor.Blue;
        public SimulationObject PrimScreenLineDest;
        public Vector PrimScreenLineDirection;

        public bool DrawSecScreenLine = false;
        public ScreenLine.EScreenLineElementType SecSlElementType = ScreenLine.EScreenLineElementType.Arrow;
        public ScreenLine.EScreenLineElementColor SecSlElementColor = ScreenLine.EScreenLineElementColor.Blue;
        public SimulationObject SecScreenLineDest;
        public Vector SecScreenLineDirection;

        public ObjectCircle CircleRef;
        public ScreenLine PrimScreenLineRef;
        public ScreenLine SecScreenLineRef;
    }
}
