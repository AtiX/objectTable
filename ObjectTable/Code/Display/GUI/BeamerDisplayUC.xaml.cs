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

namespace ObjectTable.Code.Display.GUI
{
    /// <summary>
    /// Interaction logic for BeamerDisplayUC.xaml
    /// </summary>
    public partial class BeamerDisplayUC : UserControl
    {
        private Point _mousePosition;
        public Point MousePosition
        {
            get { return _mousePosition; }
        }

        public delegate void MouseEventHandler(Point position);
        public event MouseEventHandler OnMouseMoveDown;
        public event MouseEventHandler OnMouseMoveUp;
        public event MouseEventHandler OnMouseClick;

        public BeamerDisplayUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adjusts the size by adjusting the parents window size
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(Size size)
        {
            Window parent = Window.GetWindow(this);
            parent.Width = size.Width;
            parent.Height = size.Height;
        }

        public Size GetSize()
        {
            Window parent = Window.GetWindow(this);
            return new Size(parent.Width,parent.Height);
        }

        private void maingrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	Point mp = e.GetPosition(this);

            txt_coords.Visibility = System.Windows.Visibility.Visible;
            txt_coords.Margin = new Thickness(mp.X + 4, mp.Y, 0, 0);
            txt_coords.Text = "X:" + mp.X.ToString() + " Y:" + mp.Y.ToString();

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                txt_coords.Text += " LB";
                if (OnMouseMoveDown != null)
                    OnMouseMoveDown(mp);
            }
            else
            {
                txt_coords.Text += " -B";
                if (OnMouseMoveUp != null)
                    OnMouseMoveUp(mp);
            }
        }

        private void maingrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	txt_coords.Visibility = System.Windows.Visibility.Hidden;
        }

        private void maingrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(this);

            if (OnMouseMoveDown != null)
                OnMouseMoveDown(e.GetPosition(this));
        }

        private void maingrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(this);

            if (OnMouseMoveUp != null)
                OnMouseMoveUp(_mousePosition);
        }
    }
}
