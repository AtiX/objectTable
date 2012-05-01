using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgrammingTable.Code.Simulation.Menu
{
    public class MenuObject
    {
        public string name;
        public string category;
        public string shortsign;

        public Type type;

        public MenuObject()
        {
            name = "";
            category = "";
            shortsign = "";
            type = null;
        }
    }
}
