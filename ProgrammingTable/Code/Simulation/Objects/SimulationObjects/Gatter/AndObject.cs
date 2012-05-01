using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class AndObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public AndObject()
        {
            this.Category = "Gatter";
            this.ShortSign = "And";
            this.Name = "And Object";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = false;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "And";
            GraphicsSettings.Text2 = "false";
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
            if (this.SourceObjects.Where(o => o.GetValue() != null).Count() > 0)
            {
                //Collect inputs 
                bool internalValue = true;
                foreach (SimulationObject so in SourceObjects.Where(o => o.GetValue() != null))
                {
                    SimulationValue sv = so.GetValue();

                    switch (sv.SimulationValueType)
                    {
                        case SimulationValue.ESimulationValueType.Bool:
                            if (!(bool)sv.value)
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Double:
                            if (!((double)sv.value > 0.0))
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Int:
                            if (!((int)sv.value > 0))
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.String:
                            string s = (string)sv.value;
                            if ((s.Length == 0) || (s == "false") || (s == "False"))
                            {
                                internalValue = false;
                            }
                            break;
                    }

                    if (internalValue == false)
                        break;
                }

                if (internalValue)
                {
                    //all true -> result true
                    _value.value = true;
                    GraphicsSettings.Text2 = "true";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    //Not all true -> false
                    _value.value = false;
                    GraphicsSettings.Text2 = "false";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
                }
            }
            else
            {
                //No sources -> default
                _value.value = false;
                GraphicsSettings.Text2 = "false";
                GraphicsSettings.CircleColor = ObjectCircle.EColor.blue;
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
