using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine
{
    /// <summary>
    /// Interaction logic for ScreenLineArrow.xaml
    /// </summary>
    public partial class ScreenLineArrow : UserControl
    {
        private TPoint _center;

        public int X
        {
            get { return _center.ScreenX; }
        }

        public int Y
        {
            get { return _center.ScreenY; }
        }

        public ScreenLineArrow()
        {
            InitializeComponent();
        }

        public ScreenLineArrow(Vector direction, TPoint Center, double scale)
        {
            InitializeComponent();

            _center = Center;

            this.Width = 96 * scale;
            this.Height = 40*scale;

            double angle = ScreenMathHelper.VectorToDegree(direction) - 90.0;
            RotateTransform rt = new RotateTransform(angle);
            this.RenderTransform = rt;
        }
    }
}
