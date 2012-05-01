using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class AddObject:SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public AddObject()
        {
            this.Category = "basics";
            this.ShortSign = "+";
            this.Name = "Add";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Int;
            _value.value = 0;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "Add";
            GraphicsSettings.Text2 = "+";
        }

        public override void SimulationTick()
        {
            //Check for input
            CheckForInput();

            //Select Destination
            if (PossibleDestinations.Count > 0)
            {
                this.Destinations[0] = PossibleDestinations[0];
                GraphicsSettings.PrimScreenLineDest = Destinations[0];
            }
            else
            {
                this.Destinations[0] = null;
                GraphicsSettings.PrimScreenLineDest = null;
            }
        }

        public override void GfxUpdateTick()
        {

        }

        private void CheckForInput()
        {
            if (this.SourceObjects.Where(o => o.GetValue() != null).Any())
            {
                //Collect inputs - at this time, numbers only
                double internalValue = 0.0;
                foreach (SimulationObject so in SourceObjects.Where(o => o.GetValue() != null))
                {
                    SimulationValue sv = so.GetValue();
                    if ((sv.SimulationValueType == SimulationValue.ESimulationValueType.Int) ||
                        (sv.SimulationValueType == SimulationValue.ESimulationValueType.Double))
                    {
                        //add
                        internalValue += Convert.ToDouble(sv.value);
                    }
                }

                //Check whether this is a int or double (by comparing the rounded and the origial value)
                double rounded = System.Math.Round(internalValue);
                if ((internalValue > rounded) || (internalValue < rounded))
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Double;
                    _value.value = internalValue;
                }
                else
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Int;
                    _value.value = (int) internalValue;
                }

                GraphicsSettings.Text2 = internalValue.ToString();
                GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
            }
            else
            {
                //No Source
                GraphicsSettings.Text2 = "Add";
                GraphicsSettings.CircleColor = ObjectCircle.EColor.blue;
                _value.value = 0;
                _value.SimulationValueType = SimulationValue.ESimulationValueType.Int;
            }
        }

        public override SimulationValue GetValue()
        {
            return _value;
        }

        public override void Remove()
        {
        }
    }
}
