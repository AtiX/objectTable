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
using ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable
{
	/// <summary>
	/// Interaction logic for ScreenLine.xaml
	/// </summary>
	public partial class ScreenLine : UserControl
	{
	    private TPoint _source;
	    public TPoint Source
	    {
	        get { return _source; }
            set{ _source = value; }
	    }

	    private TPoint _destination;
	    public TPoint Destination
	    {
	        get { return _destination; }
            set { _destination = value; }
	    }

        public enum EScreenLineElementType{ Circle, Arrow}; 
        private EScreenLineElementType _lineElementType;
	    public EScreenLineElementType ScreenLineElementType
	    {
            get { return _lineElementType; }
            set {_lineElementType = value; }
	    }

	    public enum EScreenLineElementColor { Blue, Red, Yellow, Green, White};
	    private EScreenLineElementColor _lineElementColor;
	    public EScreenLineElementColor LineElementColor
	    {
            get { return _lineElementColor; }
            set { _lineElementColor = value;}
	    }

	    /// <summary>
	    /// The distance from one LineElement to another one
	    /// </summary>
	    public int LineElementDistance
	    {
            get { return _lineElementDistance; }
            set { _lineElementDistance = value;}
	    }

	    private int _lineElementDistance = 50;

		public ScreenLine()
		{
			this.InitializeComponent();
		    _source = new TPoint(0, 0, TPoint.PointCreationType.screen);
            _destination = new TPoint(0, 0, TPoint.PointCreationType.screen);
		}

        public void CalculateLine()
        {
            //set line distance
            if (_lineElementType == EScreenLineElementType.Arrow)
            {
                _lineElementDistance = 100;
            }
            else
                _lineElementDistance = 50;

            //Recalculate the screen positions if depth positions are given
            RecalculatePositions();

            //resize the element so that all Child Elements fit in
            //(calculate a rectangle with source and destination in the opposite corners)
            int height;
            int posy;
            int posx;
            int width;
            CalculateRect(out height, out posy, out posx, out width);

            Canvas.SetLeft(this, posx);
            Canvas.SetTop(this, posy);
            if (width < 100)
                width = 100;
            if (height < 100)
                height = 100;

            Width = width;
            Height = height;

            //now calculate the position of each Line element
            List<TPoint> elementCenters = CalculateElementCenters(posx, posy);

            //Return if this hasn't been fully initialized
            if (mainCanvas == null)
                return;

            //Delete all existing elements on the canvas
                this.mainCanvas.Children.Clear();

            //now create for each center a UIElement of the desired type and add it to the control's canvas
            foreach (TPoint p in elementCenters)
            {
                switch (_lineElementType)
                {
                    case EScreenLineElementType.Circle:
                        AddScreenCircle(p);
                        break;
                    case EScreenLineElementType.Arrow:
                        AddScreenArrow(p);
                        break;
                }
            }
        }
        private void AddScreenArrow(TPoint center)
        {
            Vector direction = new Vector(_destination.ScreenX - _source.ScreenX, _destination.ScreenY - _source.ScreenY);

            ScreenLineArrow sla = new ScreenLineArrow(direction, center,
                                                      SettingsManager.ScreenMappingSet.DisplayObjectScale);

            mainCanvas.Children.Add(sla);
            Canvas.SetLeft(sla, sla.X);
            Canvas.SetTop(sla, sla.Y);
        }

        private void AddScreenCircle(TPoint center)
        {
            ScreenLineCircle c = new ScreenLineCircle(center,SettingsManager.ScreenMappingSet.DisplayObjectScale);
            switch (_lineElementColor)
            {
                case EScreenLineElementColor.Blue:
                    c.el_fill.Fill = (GradientBrush)c.Resources["gradient_blue"];
                    break;
                case EScreenLineElementColor.Green:
                    c.el_fill.Fill = (GradientBrush)c.Resources["gradient_green"];
                    break;
                case EScreenLineElementColor.Red:
                    c.el_fill.Fill = (GradientBrush)c.Resources["gradient_red"];
                    break;
                case EScreenLineElementColor.White:
                    c.el_fill.Fill = (GradientBrush)c.Resources["gradient_white"];
                    break;
                case EScreenLineElementColor.Yellow:
                    c.el_fill.Fill = (GradientBrush)c.Resources["gradient_yellow"];
                    break;
            }
            mainCanvas.Children.Add(c);
            Canvas.SetLeft(c, c.X);
            Canvas.SetTop(c, c.Y);
        }

        /// <summary>
        /// Calculates the element centers of the lineElements
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
	    private List<TPoint> CalculateElementCenters(int canvasx, int canvasy)
	    {
	        //Create the direction vector
            Vector vDirection = new Vector(_destination.ScreenX - _source.ScreenX,
                                           _destination.ScreenY - _source.ScreenY);

            Vector vSource = new Vector(_source.ScreenX - canvasx, _source.ScreenY - canvasy);

            //Rescale the vector so that it seperates one element from another (according to _lineElementDistance)
	        Vector vElementSpacer = ScreenMathHelper.RescaleVector(vDirection, _lineElementDistance*SettingsManager.ScreenMappingSet.DisplayObjectScale);

	        //the position is calculated by adding n*vElementSpacer to the upperLeftCorner (0|0) until 
	        //n*vElementSpacer.Length >= vDirection.Length -> it reached the destination
	        List<TPoint> elementCenters = new List<TPoint>();
	        int n = 1;
	        while (vDirection.Length >= vElementSpacer.Length*n)
	        {
	            Vector vPosition = vSource + vElementSpacer*n;
	            TPoint p = new TPoint(Convert.ToInt32(vPosition.X), Convert.ToInt32(vPosition.Y),
	                                  TPoint.PointCreationType.screen);
	            elementCenters.Add(p);
	            n++;
	        }

	        return elementCenters;
	    }

	    /// <summary>
        /// calculate a rectangle with source and destination in the opposite corners
        /// </summary>
        /// <param name="height"></param>
        /// <param name="posy"></param>
        /// <param name="posx"></param>
        /// <param name="width"></param>
	    private void CalculateRect(out int height, out int posy, out int posx, out int width)
	    {
	        posx = 0;
	        width = 0;
	        width = 0;
	        if (_source.ScreenX > _destination.ScreenX)
	        {
	            posx = _destination.ScreenX;
	            width = _source.ScreenX - _destination.ScreenX;
	        }
	        else
	        {
	            posx = _source.ScreenX;
	            width = _destination.ScreenX - _source.ScreenX;
	        }

	        posy = 0;
	        height = 0;
	        height = 0;
	        if (_source.ScreenY > _destination.ScreenY)
	        {
	            posy = _destination.ScreenY;
	            height = _source.ScreenY - _destination.ScreenY;
	        }
	        else
	        {
	            posy = _source.ScreenY;
	            height = _destination.ScreenY - _source.ScreenY;
	        }
	    }

	    private void RecalculatePositions()
	    {
	        if ((_source.DepthX != 0) || (_source.DepthY != 0))
	        {
	            _source = PositionMapper.GetScreenCoordsfromDepth(_source);
	        }

	        if ((_destination.DepthX != 0) || (_destination.DepthY != 0))
	        {
	            _destination = PositionMapper.GetScreenCoordsfromDepth(_destination);
	        }
	    }

	    public ScreenLine(TPoint source, TPoint destination, EScreenLineElementType lineElementTypeType = EScreenLineElementType.Circle)
	    {
            this.InitializeComponent();

	        _source = source;
	        _destination = destination;
	        _lineElementType = lineElementTypeType;

            //Bind to screenSettingsUpdate event
            SettingsManager.ScreenMappingSet.OnScreenSettingsUpdate += new Code.PositionMapping.ScreenMappingSettings.ScreenSettingsUpdateHandler(ScreenMappingSet_OnScreenSettingsUpdate);

	        CalculateLine();
	    }

        public ScreenLine(TPoint source, Vector LineVector, EScreenLineElementType lineElementTypeType = EScreenLineElementType.Circle)
        {
            this.InitializeComponent();

            _source = source;
            _lineElementType = lineElementTypeType;

            _destination = new TPoint(
                Convert.ToInt32(_source.ScreenX + LineVector.X),
                 Convert.ToInt32(_source.ScreenY + LineVector.Y),
                TPoint.PointCreationType.screen
                );

            //Bind to screenSettingsUpdate event
            SettingsManager.ScreenMappingSet.OnScreenSettingsUpdate += new Code.PositionMapping.ScreenMappingSettings.ScreenSettingsUpdateHandler(ScreenMappingSet_OnScreenSettingsUpdate);

            CalculateLine();
        }

        void ScreenMappingSet_OnScreenSettingsUpdate()
        {
            //Recalculate the line
            Dispatcher.Invoke((Action) (CalculateLine));
        }
	}
}