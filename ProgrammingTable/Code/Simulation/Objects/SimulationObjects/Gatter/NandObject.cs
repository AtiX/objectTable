using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class NandObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public NandObject()
        {
            this.Category = "Gatter";
            this.ShortSign = "nand";
            this.Name = "Nand Object";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = true;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "Nand";
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
                Destinations[0] = PossibleDestinations[0];
                GraphicsSettings.PrimScreenLineDest = Destinations[0];
            }
            else
            {
                //Delete Destination object 
                Destinations[0] = null;
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
                bool internalValue = false;
                foreach (SimulationObject so in SourceObjects.Where(o => o.GetValue() != null))
                {
                    SimulationValue sv = so.GetValue();

                    switch (sv.SimulationValueType)
                    {
                        case SimulationValue.ESimulationValueType.Bool:
                            if (!(bool)sv.value)
                            {
                                internalValue = true;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Double:
                            if (!((double)sv.value > 0.0))
                            {
                                internalValue = true;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Int:
                            if (!((int)sv.value > 0))
                            {
                                internalValue = true;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.String:
                            string s = (string)sv.value;
                            if ((s.Length == 0) || (s == "false") || (s == "False"))
                            {
                                internalValue = true;
                            }
                            break;
                    }

                    if (internalValue == true)
                        break;
                }

                if (internalValue)
                {
                    //at least one is false -> true
                    _value.value = true;
                    GraphicsSettings.Text2 = "true";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    //all are true -> false
                    _value.value = false;
                    GraphicsSettings.Text2 = "false";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
                }
            }
            else
            {
                //No sources -> default (true)
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
