using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Tracking.DataStructures
{
    class TrackObject : ICloneable
    {
        public TableObject ObjReference;
        public int BestCertainityWithID = 0;
        public double TrackCertainity = 0.0;

        public static List<TrackObject> ConvertToTrackObjects(List<TableObject> TableObjectList)
        {
            List<TrackObject> list = new List<TrackObject>();

            //Only take objects with a defined center
            foreach (TableObject tobj in TableObjectList.Where(obj => obj.CenterDefined))
            {
                TrackObject t = new TrackObject();
                t.ObjReference = tobj;
                list.Add(t);
            }

            return list;
        }

        public static List<TableObject> ConvertToTableObjects(List<TrackObject> TrackObjectList)
        {
            List<TableObject> ToList = new List<TableObject>();

            foreach (TrackObject o in TrackObjectList)
            {
                ToList.Add(o.ObjReference);
            }

            return ToList;
        }

        public object Clone()
        {
            TrackObject n = new TrackObject();
            n.BestCertainityWithID = BestCertainityWithID;
            n.TrackCertainity = TrackCertainity;
            n.ObjReference = (TableObject) ObjReference.Clone();
            return n;
        }
    }
}
