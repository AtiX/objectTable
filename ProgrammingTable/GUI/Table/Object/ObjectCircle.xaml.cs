using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ProgrammingTable
{
	/// <summary>
	/// Interaction logic for Circle.xaml
	/// </summary>
	public partial class ObjectCircle : UserControl
	{
	    /// <summary>
	    /// The rotation Speed of the circle, in [degree/second]
	    /// </summary>
	    public double RotationSpeed { get; set; }
	    private double _currentRotation;

	    private DispatcherTimer _rotationTimer;

	    public TPoint Center
	    {
            get { return _center; }
            set { _center = value;
                ChangeSize();
            }
	    }
	    private TPoint _center;

	    private int _size = 150;
	    public int Size
	    {
            get { return _size; }
            set
            {
                _size = value;
                ChangeSize();
            }
	    }

        private void ChangeSize()
        {
            //Automatically calculate the screen position if the depthPosition is given; set correct size of the circle
            _center.CalculateScreenfromDepthCoords();

            int x = (int) Math.Round((double)_center.ScreenX - (_size/2));
            int y = (int)Math.Round((double)_center.ScreenY - (_size / 2));

            //this.Margin = new Thickness(x, y, 0, 0);
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            this.Width = _size* SettingsManager.ScreenMappingSet.DisplayObjectScale;
            Height = _size * SettingsManager.ScreenMappingSet.DisplayObjectScale;
        }

        /// <summary>
        /// Sets one of the two texts
        /// </summary>
        /// <param name="number">1 or 2</param>
        /// <param name="Text"></param>
        public void SetText(int number, string Text)
        {
            switch (number)
            {
                case 1:
                    txt_1.Text = Text;
                    break;
                case 2:
                    txt_2.Text = Text;
                    break;
            }
        }
        public ObjectCircle()
        {
            this.InitializeComponent();

            _center = new TPoint(0, 0, TPoint.PointCreationType.screen);

            _rotationTimer = new DispatcherTimer();
            _rotationTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _rotationTimer.Tick += new EventHandler(OnRotationTimerTick);
            _rotationTimer.IsEnabled = true;
            RotationSpeed = 20;

            SettingsManager.ScreenMappingSet.OnScreenSettingsUpdate += new ScreenMappingSettings.ScreenSettingsUpdateHandler(ScreenMappingSettings_OnScreenSettingsUpdate);
        }

        public ObjectCircle(TPoint Center)
		{
			this.InitializeComponent();

		    _center = Center;

            //Automatically calculate the screen position if the depthPosition is given; set correct size of the circle
            _center.CalculateScreenfromDepthCoords();

            //update
            ChangeSize();

		    _rotationTimer = new DispatcherTimer();
		    _rotationTimer.Interval = new TimeSpan(0,0,0,0,50);
		    _rotationTimer.Tick += new EventHandler(OnRotationTimerTick);
            _rotationTimer.IsEnabled = true;
            RotationSpeed = 20;

            SettingsManager.ScreenMappingSet.OnScreenSettingsUpdate += new ScreenMappingSettings.ScreenSettingsUpdateHandler(ScreenMappingSettings_OnScreenSettingsUpdate);
		}

        void ScreenMappingSettings_OnScreenSettingsUpdate()
        {
            //use the dispatcher because the event might be from another thread
            this.Dispatcher.Invoke((Action) (() => { 
            //Recalculate the screen position if depthPosition is given
            _center.CalculateScreenfromDepthCoords();
            //update
            ChangeSize();
            })); // (Dispatcher)
        }

	    /// <summary>
        /// Increases the rotation of the object
        /// </summary>
        /// <param name="obj"></param>
        private void OnRotationTimerTick(object obj, EventArgs arg)
        {
            if (RotationSpeed == 0.0)
                return;

            _currentRotation += RotationSpeed/20;
            if (_currentRotation > 360.0)
                _currentRotation = 0.0;

            RotateTransform rt = new RotateTransform(_currentRotation);
            TransformGroup g = new TransformGroup();
            g.Children.Add(rt);
            this.RenderTransform = rt;
        }

        public void SetColor(EColor color)
        {
            switch (color)
            {
                case EColor.blue:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_blue"];
                    break;
                case EColor.cyan:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_cyan"];
                    break;
                case EColor.green:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_green"];
                    break;
                case EColor.pink:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_pink"];
                    break;
                case EColor.red:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_red"];
                    break;
                case EColor.white:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_white"];
                    break;
                case EColor.yellow:
                    el_color.Fill = (RadialGradientBrush) Resources["gradient_yellow"];
                    break;
            }
        }

	    public enum EColor
	    {
	        green,
	        blue,
	        pink,
	        cyan,
	        red,
	        white,
	        yellow
	    };

	}
}