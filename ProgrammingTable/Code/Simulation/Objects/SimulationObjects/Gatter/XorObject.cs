using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class XorObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public XorObject()
        {
            this.Category = "Gatter";
            this.ShortSign = "Xor";
            this.Name = "Xor Object";
            
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = false;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "Xor";
            GraphicsSettings.Text2 = "false";
        }

        public override void SimulationTick()
        {
            //Check for input
            CheckForInput();

            //Select Destination
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
                int trueInputs = 0;

                foreach (SimulationObject so in SourceObjects.Where(o => o.GetValue() != null))
                {
                    SimulationValue sv = so.GetValue();

                    switch (sv.SimulationValueType)
                    {
                        case SimulationValue.ESimulationValueType.Bool:
                            if ((bool)sv.value)
                            {
                                trueInputs++;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Double:
                            if ((double)sv.value > 0.0)
                            {
                                trueInputs++;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.Int:
                            if ((int)sv.value > 0)
                            {
                                trueInputs++;
                            }
                            break;
                        case SimulationValue.ESimulationValueType.String:
                            string s = (string)sv.value;
                            if ((s.Length > 0) && ((s != "false") && (s != "False")))
                            {
                                trueInputs++; 
                            }
                            break;
                    }
                }

                bool internalValue = false;

                //Xor is true, when an odd amount of inputs is true and the rest is false
                if(trueInputs % 2 == 1)
                {
                    //odd
                    internalValue = true;
                }

                if (internalValue)
                {
                    //true
                    _value.value = true;
                    GraphicsSettings.Text2 = "true";
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    //false
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
