using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects.basics
{
    /// <summary>
    /// Splits up a screen line of an object
    /// </summary>
    class PathDivisor : SimulationObject
    {
        public override event ScreenElementHandler OnNewMiddleScreenElement;
        public override event ScreenElementHandler OnNewBottomScreenElement;
        public override event ScreenElementHandler OnScreenElementRemove;
        public override event ScreenElementModify OnScreenElementModify;

        private SimulationValue _value;

        public PathDivisor()
        {
            this.Category = "basics";
            this.Name = "PathDivisor";
            this.ShortSign = "<pd>";

            AdditionalDirections = new List<System.Windows.Vector>();
            PossibleAdditionalDestinations = new List<List<SimulationObject>>();
            Destinations = new List<SimulationObject>();
        }

        public override void Init()
        {
            GraphicsSettings.Text1 = GraphicsSettings.Text2 = "<pd>";
            GraphicsSettings.CircleColor = ObjectCircle.EColor.white;
        }

        public override void SimulationTick()
        {
            if (this.SourceObjects.Count > 1)
            {
                GraphicsSettings.CircleColor = ObjectCircle.EColor.red;
                Destinations.Clear();
                AdditionalDirections.Clear();
                _value = null;
            }
            else if (SourceObjects.Count == 1)
            {
                GraphicsSettings.CircleColor = ObjectCircle.EColor.white;
                _value = SourceObjects[0].GetValue();

                //Set directions
                //original objects destination
                AdditionalDirections.Clear();
                AdditionalDirections.Add(SourceObjects[0].LastTableObject.DirectionVector);
                //own direction is set automatically in the tableObjectvector

                //set destinations (two destinations: the first is the own, the second the original. null, if no possible destination)
                Destinations.Clear();
                //own beam
                if (PossibleDestinations.Count > 0)
                {
                    Destinations.Add(PossibleDestinations[0]);
                }
                else
                {
                    Destinations.Add(null);
                }
                //original beam
                if (PossibleAdditionalDestinations.Count > 0)
                {
                    if (PossibleAdditionalDestinations[0].Count > 0)
                    {
                        Destinations.Add(null); //own beam doesn't have a destination
                        Destinations.Add(PossibleAdditionalDestinations[0][0]);
                    }
                    else
                    {
                        Destinations.Add(null);
                    }
                }
                else
                {
                    Destinations.Add(null);
                }
            }
            else
            {
                GraphicsSettings.CircleColor = ObjectCircle.EColor.white;
                Destinations.Clear();
                Destinations.Add(null);
                Destinations.Add(null);
                AdditionalDirections.Clear();
                _value = null;
            }
        }

        public override void GfxUpdateTick()
        {
            if (Destinations.Count == 0)
                return;

            //own line
            GraphicsSettings.PrimScreenLineDest = Destinations[0];

            if (AdditionalDirections.Count == 0)
                return;

            //original beam
            GraphicsSettings.DrawSecScreenLine = true;
            GraphicsSettings.SecScreenLineDest = Destinations[1];
            GraphicsSettings.SecScreenLineDirection = AdditionalDirections[0];
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
