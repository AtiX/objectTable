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
using System.Windows.Shapes;

namespace WaveTable.GUI
{
    /// <summary>
    /// Interaction logic for ColorPickWindow.xaml
    /// </summary>
    public partial class ColorPickWindow : Window
    {
        public Color pickedColor;
        public bool DidPickColor = false;

        public ColorPickWindow()
        {
            InitializeComponent();
        }

        private void sl_R_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GenColor();
        }

        private void GenColor()
        {
            Color c = new Color();

            c.R = (byte) Math.Round((sl_R.Value));
            c.G = (byte)Math.Round((sl_G.Value));
            c.B = (byte)Math.Round((sl_B.Value));
            c.A = 255;

            r_preview.Fill = new SolidColorBrush(c);
            r_preview.Stroke = new SolidColorBrush(c);
            pickedColor = c;
        }

        private void b_ok_Click(object sender, RoutedEventArgs e)
        {
            DidPickColor = true;
            this.Close();
        }

        private void b_cancel_Click(object sender, RoutedEventArgs e)
        {
            DidPickColor = false;
            Close();
        }

        private void sl_G_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GenColor();
        }

        private void sl_B_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GenColor();
        }
    }
}
