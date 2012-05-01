using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgrammingTable.Code.Simulation.Objects
{
    /// <summary>
    /// A SimulationValue is any kind of data that can be transported from object to object
    /// </summary>
    public class SimulationValue
    {
        public enum ESimulationValueType
        {
            Bool,
            Int,
            Double,
            String,
            Other
        }

        public ESimulationValueType SimulationValueType;

        public object value = null;
    }
}
