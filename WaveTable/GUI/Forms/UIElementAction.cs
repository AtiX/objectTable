using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaveTable.GUI.Forms
{
    class UIElementAction
    {
        public enum EActionType
        {
            SetAmp,
            SetMass,
            DelMass,
            SetWall,
            DelWall,
            SetSource,
            SetObject,
            NULL
        }

        public EActionType ActionType = EActionType.NULL;

        public Type ActionObjectType = null;

        public enum EShape
        {
            Square,
            Circle,
            NULL
        }

        public EShape Shape = EShape.NULL;
        public int Size = 0;

        public double Value = 0;
    }
}
