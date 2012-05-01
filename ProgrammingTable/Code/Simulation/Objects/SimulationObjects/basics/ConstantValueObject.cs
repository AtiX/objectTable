using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    class ConstantValueObject : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public ConstantValueObject()
        {
            //Set info for menu
            this.Category = "Basics";
            this.Name = "Constant Value";
            this.ShortSign = "#";
        }

        public override void Init()
        {
            //Create circle
            GraphicsSettings.Text1 = "#";
            GraphicsSettings.Text2 = "";

            //Show value select screen (needs to be in the STA thread)
            OnScreenElementModify((Action)(() => {
            ConstantValueObjectWindow cow = new ConstantValueObjectWindow();
            cow.ShowDialog();
            this._value = new SimulationValue();
            _value.value = cow.Value;
            _value.SimulationValueType = cow.ValueType;
            GraphicsSettings.Text2 = _value.ToString();
            }),null);
        }

        public override void SimulationTick()
        {
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

        public override SimulationValue GetValue()
        {
            return _value;
        }

        public override void Remove()
        {
        }
    }
}
