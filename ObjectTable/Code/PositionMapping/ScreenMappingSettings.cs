using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTable.Code.PositionMapping
{
    public class ScreenMappingSettings
    {
        /// <summary>
        /// Betrag, um den Das BeamerKoordinatensystem vom DepthSystem verschoben ist
        /// </summary>
        public int MoveX, MoveY;

        /// <summary>
        /// The scale factor of the beamerPicture
        /// </summary>
        public double ScaleX, ScaleY;

        /// <summary>
        /// This scale can be used by UIElements on the Beamer screen to scale themself
        /// </summary>
        public double DisplayObjectScale = 1.0;

        /// <summary>
        /// The amount how much the color points are moved after the calculation by the kinect runtime
        /// </summary>
        public int ColorMoveX = 0;

        /// <summary>
        /// /// The amount how much the color points are moved after the calculation by the kinect runtime
        /// </summary>
        public int ColorMoveY = 0;

        public delegate void ScreenSettingsUpdateHandler();
        public event ScreenSettingsUpdateHandler OnScreenSettingsUpdate;

        public void CauseUpdateEvent()
        {
            if (OnScreenSettingsUpdate != null)
                OnScreenSettingsUpdate();
        }
    }
}
