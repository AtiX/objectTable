using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WaveSimLib.Code.Wave;

namespace WaveTable.Code.SimulationObjects
{
    class SimWaveSourceObject : SimulationObject
    {
        public SinusWaveSource WaveSourceRef;
        public Vector oldDirection;
    }
}
