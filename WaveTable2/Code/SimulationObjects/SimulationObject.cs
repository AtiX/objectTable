using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable.Code.Recognition.DataStructures;
using Point = System.Drawing.Point;

namespace WaveTable.Code.SimulationObjects
{
    class SimulationObject
    {
        public int ObjectID;

        public System.Drawing.Point SimPosition;
        public TPoint Position;
        public Vector ObjectRotation;
        public bool PositionOrRotationChange = false;

        public enum ETableObjectType
        {
            LTO,
            TOO,
            NotOnTable
        }
        public ETableObjectType TableObjectType;

        public enum EObjectType
        {
            ConstElongation,
            RelElongation,
            Mass,
            Wall,
            Special
        }
        public EObjectType ObjectType;

        public enum EObjectShape
        {
            Square,
            Circle,
            NULL
        }
        public EObjectShape ObjectShape;

        public int ObjectSize;

        public double ObjectValue;
    }
}
