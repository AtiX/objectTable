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
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable
{
	/// <summary>
	/// Interaction logic for ScreenCrosshair.xaml
	/// </summary>
	public partial class ScreenCrosshair : UserControl
	{
	    private TPoint _center;

		public ScreenCrosshair(TPoint Center)
		{
			this.InitializeComponent();
            _center = Center;
            SettingsManager.ScreenMappingSet.OnScreenSettingsUpdate += new Code.PositionMapping.ScreenMappingSettings.ScreenSettingsUpdateHandler(ScreenMappingSet_OnScreenSettingsUpdate);
		    ChangePosition();
		}

        void ScreenMappingSet_OnScreenSettingsUpdate()
        {
            //Adjust size to new screenMapping settings, using the dispatcher
            this.Dispatcher.Invoke((Action) (ChangePosition), null);
        }

	    private void ChangePosition()
	    {
	        //Recalculate the screen position if depthPosition is given
            if ((_center.DepthX != 0) || (_center.DepthY != 0))
            {
                _center = PositionMapper.GetScreenCoordsfromDepth(_center);
            }
            
            int x = (int)Math.Round((double)_center.ScreenX - (this.Width / 2));
            int y = (int)Math.Round((double)_center.ScreenY - (this.Height / 2));

            //this.Margin = new Thickness(x, y, 0, 0);
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
	    }
	}
}