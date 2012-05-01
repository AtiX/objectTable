using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class EqualsFilterObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public EqualsFilterObject()
        {
            this.Category = "Filter";
            this.ShortSign = "==";
            this.Name = "Equals";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = false;
        }

        public override void Init()
        {
            GraphicsSettings.Text2 = "true";
            GraphicsSettings.Text1 = "==";
        }

        public override void SimulationTick()
        {
            //Check for input
            CheckForInput();

            //Check whether there is a possible destination object
            if (PossibleDestinations.Count > 0)
            {
                //Set the nearest as destination
                Destinations[0] = PossibleDestinations[0];
                GraphicsSettings.PrimScreenLineDest = Destinations[0];
            }
            else
            {
                //Delete Destination object 
                Destinations[0] = null;
                GraphicsSettings.PrimScreenLineDest = null;
            }
        }

        public override void GfxUpdateTick()
        { 
        }

        private void CheckForInput()
        {
            List<SimulationObject> src = this.SourceObjects.Where(o => o.GetValue() != null).ToList();
            
            if (src.Count() > 0)
            {
                SimulationValue lastvalue = src[0].GetValue();
                bool equal = true;

                foreach (SimulationObject so in src)
                {
                    if (!lastvalue.value.Equals(so.GetValue().value))
                    {
                        equal = false;
                        break;
                    }
                    else
                    {
                        lastvalue = so.GetValue();
                    }
                }

                //Compare
                if (equal)
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
                    _value.value = true;
                    GraphicsSettings.Text2 = "==";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
                    _value.value = false;
                    GraphicsSettings.Text2 = "!=";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
                }
            }
            else
            {
                //no src -> default
                _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
                _value.value = true;
                GraphicsSettings.Text2 = "==";
                GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
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
