using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Rotation
{
    /// <summary>
    /// Motherclass. All RotationDetection Methods must be derived from this class
    /// </summary>
    public abstract class RotationDetector
    {
        public abstract List<TableObject> DetectRotation(List<TableObject> ObjectList); 
        public int RotationDetectionDuration;
    }


}
