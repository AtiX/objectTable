using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class NorObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public NorObject()
        {
            this.Category = "Gatter";
            this.ShortSign = "Nor";
            this.Name = "Nor Object";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = true;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "Nor";
            GraphicsSettings.Text2 = "true";
        }

        public override void SimulationTick()
        {
            //Check for input
            CheckForInput();

            //Check whether there is a possible destination object
            if (PossibleDestinations.Count > 0)
            {
                //Set the nearest as destination
                GraphicsSettings.PrimScreenLineDest = Destinations[0] = PossibleDestinations[0];
            }
            else
            {
                //Delete Destination object 
                GraphicsSettings.PrimScreenLineDest = Destinations[0] = null;
            }
        }

        public override void GfxUpdateTick()
        {
        }

        private void CheckForInput()
        {
            if (this.SourceObjects.Any(o => o.GetValue() != null))
            {
                //Collect inputs 
                bool internalValue = true;
                foreach (SimulationObject so in SourceObjects.Where(o => o.GetValue() != null))
                {
                    SimulationValue sv = so.GetValue();

                    switch (sv.SimulationValueType)
                    {
                        case SimulationValue.ESimulationValueType.Bool:
                            if ((bool)sv.value)
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Double:
                            if ((double)sv.value > 0.0)
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Int:
                            if ((int)sv.value > 0)
                            {
                                internalValue = false;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.String:
                            string s = (string)sv.value;
                            if ((s.Length > 0) && ((s != "false") && (s != "False")))
                            {
                                internalValue = false;
                            }
                            break;
                    }
                }

                if (internalValue)
                {
                    //all false -> result true
                    _value.value = true;
                    GraphicsSettings.Text2 = "true";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    //at least one true -> result false
                    _value.value = false;
                    GraphicsSettings.Text2 = "fals";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
                }
            }
            else
            {
                //No sources -> default
                _value.value = true;
                GraphicsSettings.Text2 = "true";
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
