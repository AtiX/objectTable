using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgrammingTable.Code.General
{
    [Serializable()]
    public class ProgrammingTableSettings
    {
        /// <summary>
        /// The maximal distance an object may have to a beam from another object to be added to it's possible Destinations
        /// </summary>
        public double DestinationObjectMaxBeamDistance = 175;

        /// <summary>
        /// The delay between simulation Ticks in [ms]
        /// </summary>
        public int SimulationTickDelay = 200;
    }
}
