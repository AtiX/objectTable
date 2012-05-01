using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class OutputObject:SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        public OutputObject()
        {
            this.Category = "Basics";
            this.Name = "Output";
            this.ShortSign = "Out";
        }

        public override void Init()
        {
            //Create circle
            GraphicsSettings.Text1 = "Out";
            GraphicsSettings.Text2 = "-";
        }

        public override void SimulationTick()
        {
            //Check for input
            if (SourceObjects.Where(o => o.GetValue() != null).Any())
            {
                SimulationValue sv = this.SourceObjects.Where(o => o.GetValue() != null).ToArray()[0].GetValue();

                switch (sv.SimulationValueType)
                {
                    case SimulationValue.ESimulationValueType.Other:
                        GraphicsSettings.Text2 = "???";
                        break;
                    default:
                        GraphicsSettings.Text2 = Convert.ToString(sv.value);
                        break;
                }
                GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
            }
            else
            {
                //No Source
                GraphicsSettings.Text2 = "-";
                GraphicsSettings.CircleColor = ObjectCircle.EColor.blue;
            }
        }

        public override void GfxUpdateTick()
        {
        }

        public override SimulationValue GetValue()
        {
            return null;
        }

        public override void Remove()
        {
        }
    }
}
