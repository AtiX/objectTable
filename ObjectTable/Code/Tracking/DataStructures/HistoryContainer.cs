using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Tracking
{
    class HistoryContainer
    {
        public List<TableObject> ObjectList;

        /// <summary>
        /// The Age of the History Container. Each time a new history container is added (=new frame), this value is increased by 1
        /// </summary>
        public int FrameAge;

        public HistoryContainer()
        {
            ObjectList = new List<TableObject>();
            FrameAge = 0;
        }
    }
}
