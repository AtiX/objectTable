using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class SmallerThanFilterObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public SmallerThanFilterObject()
        {
            this.Category = "Filter";
            this.ShortSign = "<";
            this.Name = "SmallerThan";
            _value = new SimulationValue();
            _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
            _value.value = false;
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = "<";
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
            List<SimulationObject> src = this.SourceObjects.Where(o => o.GetValue() != null).ToList();
            
            if (src.Count() == 2)
            {
                //Needs two inputs: two values
                //The object that is nearer to this obj determines the threshold, the object that is further away supplies the value to test
                SimulationValue threshold = src[0].GetValue();
                SimulationValue test = src[1].GetValue();

                //Convert threshold
                double thresholdValue = 0.0;
                switch (threshold.SimulationValueType)
                {
                    case SimulationValue.ESimulationValueType.Bool:
                        if (Convert.ToBoolean(threshold.value))
                            thresholdValue = 1.0;
                        break;
                    case SimulationValue.ESimulationValueType.Double:
                        thresholdValue = (double) threshold.value;
                        break;
                    case SimulationValue.ESimulationValueType.Int:
                        thresholdValue = Convert.ToDouble((int) threshold.value);
                        break;
                }

                //Convert test value
                double testValue = 0.0;
                switch (test.SimulationValueType)
                {
                    case SimulationValue.ESimulationValueType.Bool:
                        if (Convert.ToBoolean(threshold.value))
                            testValue = 1.0;
                        break;
                    case SimulationValue.ESimulationValueType.Double:
                        testValue = (double)test.value;
                        break;
                    case SimulationValue.ESimulationValueType.Int:
                        testValue = Convert.ToDouble((int)test.value);
                        break;
                    case SimulationValue.ESimulationValueType.String:
                        testValue = ((string) test.value).Length;
                        break;
                }

                //Compare
                if (testValue < thresholdValue)
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Double;
                    _value.value = testValue;
                    GraphicsSettings.Text2 = testValue.ToString("0.0") + "<" + thresholdValue.ToString("0.0");
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.green;
                }
                else
                {
                    _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
                    _value.value = false;
                    GraphicsSettings.Text2 = testValue.ToString("0.0") + "<" + thresholdValue.ToString("0.0");
                    GraphicsSettings.CircleColor = ObjectCircle.EColor.cyan;
                }
            }
            else if (src.Count > 2)
            {
                //Too many items!
                _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
                _value.value = false;
                GraphicsSettings.Text2 = "++";
                GraphicsSettings.CircleColor = ObjectCircle.EColor.red;
            }
            else if (src.Count < 2)
            {
                //Not enough items
                _value.SimulationValueType = SimulationValue.ESimulationValueType.Bool;
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
