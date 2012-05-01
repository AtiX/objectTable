using System;
using System.Collections.Generic;
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

namespace ObjectTable
{
	/// <summary>
	/// Interaction logic for ScreenLineCircle.xaml
	/// </summary>
	public partial class ScreenLineCircle : UserControl
	{
	    private int _x = 0;
        public int X
        {
            get { return _x; }
        }

	    private int _y = 0;
	    public int Y
	    {
            get { return _y; }
	    }

	    public ScreenLineCircle()
		{
			this.InitializeComponent();
		}

        public ScreenLineCircle(TPoint center, double objectScale = 1.0)
        {
            this.InitializeComponent();

            //resize to objectScale
            this.Width = this.Width*objectScale;
            this.Height = this.Height*objectScale;

            //calculate x and y values of the upper left corner
            _x = (int)Math.Round(center.ScreenX - (this.Width  / 2));
            _y = (int)Math.Round(center.ScreenY - (this.Height / 2));
        }
	}
}