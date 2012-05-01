using System;
using System.Windows.Controls;
using ObjectTable.Code.Recognition.DataStructures;
using ProgrammingTable.Code.Simulation.Menu;

namespace ProgrammingTable.GUI.Table.Menu
{
	/// <summary>
	/// Interaction logic for Menu.xaml
	/// </summary>
	public partial class Menu : UserControl
	{
        //The graphical regions
	    public GraphicalRegion arrowLeft;
        public GraphicalRegion arrowRight;
        public GraphicalRegion MObject1;
        public GraphicalRegion MObject2;
        public GraphicalRegion MObject3;
        public GraphicalRegion MObject4;

		public Menu()
		{
			this.InitializeComponent();
		    CalculateScreenAreas();
		}

        public void SetElements(MenuObject[] obj)
        {
            mi1.SetInfo(obj[0].name, obj[0].category, obj[0].shortsign);
            mi2.SetInfo(obj[1].name, obj[1].category, obj[1].shortsign);
            mi3.SetInfo(obj[2].name, obj[2].category, obj[2].shortsign);
            mi4.SetInfo(obj[3].name, obj[3].category, obj[3].shortsign);
        }

        /// <summary>
        /// Calculates the screenAreas for the buttons and fields
        /// </summary>
        public void CalculateScreenAreas()
        {
            //Arrow Left/up
            double Tx = r_up.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            double Ty = r_up.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            double Bx = r_up.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + r_up.RenderedGeometry.Bounds.Width;
            double By = r_up.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + r_up.Height;
            TPoint left = new TPoint((int) Math.Round(Tx), (int) Math.Round(Ty), TPoint.PointCreationType.screen);
            TPoint right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            arrowLeft = new GraphicalRegion(left, right);

            //Arrow right/down
            Tx = r_down.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            Ty = r_down.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            Bx = r_down.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + r_down.Width;
            By = r_down.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + r_down.Height;
            left = new TPoint((int)Math.Round(Tx), (int)Math.Round(Ty), TPoint.PointCreationType.screen);
            right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            arrowRight = new GraphicalRegion(left, right);

            //MenuObject 1
            Tx = c1.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            Ty = c1.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            Bx = c1.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + c1.Width;
            By = c1.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + c1.Height;
            left = new TPoint((int)Math.Round(Tx), (int)Math.Round(Ty), TPoint.PointCreationType.screen);
            right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            MObject1 = new GraphicalRegion(left, right);

            //MenuObject 2
            Tx = c2.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            Ty = c2.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            Bx = c2.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + c2.Width;
            By = c2.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + c2.Height;
            left = new TPoint((int)Math.Round(Tx), (int)Math.Round(Ty), TPoint.PointCreationType.screen);
            right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            MObject2 = new GraphicalRegion(left, right);

            //MenuObject 3
            Tx = c3.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            Ty = c3.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            Bx = c3.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + c3.Width;
            By = c3.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + c3.Height;
            left = new TPoint((int)Math.Round(Tx), (int)Math.Round(Ty), TPoint.PointCreationType.screen);
            right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            MObject3 = new GraphicalRegion(left, right);

            //MenuObject 4
            Tx = c4.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this);
            Ty = c4.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this);
            Bx = c4.Margin.Left + menugrid.Margin.Left + Canvas.GetLeft(this) + c4.Width;
            By = c4.Margin.Top + menugrid.Margin.Top + Canvas.GetTop(this) + c4.Height;
            left = new TPoint((int)Math.Round(Tx), (int)Math.Round(Ty), TPoint.PointCreationType.screen);
            right = new TPoint((int)Math.Round(Bx), (int)Math.Round(By), TPoint.PointCreationType.screen);
            MObject4 = new GraphicalRegion(left, right);
        }
	}
}