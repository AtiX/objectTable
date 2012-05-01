using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ObjectTable;
using ObjectTable.Code.Display.GUI;

namespace ObjectTableForms.Forms.Screen
{
    /// <summary>
    /// Interaction logic for BeamerScreenWindow.xaml
    /// </summary>
    public partial class BeamerScreenWindow : Window
    {
        public BeamerScreenWindow()
        {
            InitializeComponent();
            this.Show();
            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public BeamerDisplayUC GetBeamerUC()
        {
            return beamerDisplay;
        }

        private void m_fullscreen_Click(object sender, RoutedEventArgs e)
        {
            //
            if (WindowState != System.Windows.WindowState.Maximized)
            {
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }
    }
}
