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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ObjectTable;
using ObjectTable.Code;
using ObjectTableForms;
using ObjectTableForms.Forms.Debug;
using ObjectTable.Code.Recognition;
using ObjectTableForms.Forms.Screen;
using ObjectTableForms.Forms.Settings;

namespace DebugApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TableManager _tablemanager;
        private BeamerScreenWindow _bscrw;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Create Beamerscreen
            _bscrw = new BeamerScreenWindow();
            _bscrw.Show();

            _bscrw.Closed += new EventHandler(_bscrw_Closed);
            _tablemanager = new TableManager(_bscrw.GetBeamerUC());

            //Add debug object
            ScreenCircle sc1 =
                new ScreenCircle(new ObjectTable.Code.Recognition.DataStructures.TPoint(500, 500,
                                                                                        ObjectTable.Code.Recognition.
                                                                                            DataStructures.TPoint.
                                                                                            PointCreationType.screen));
            sc1.SetColor(ScreenCircle.EColor.green);
            sc1.RotationSpeed = 10.0;

            ScreenCrosshair sch1 = new ScreenCrosshair(new ObjectTable.Code.Recognition.DataStructures.TPoint(200, 300,
                                                                                                              ObjectTable
                                                                                                                  .Code.
                                                                                                                  Recognition
                                                                                                                  .
                                                                                                                  DataStructures
                                                                                                                  .
                                                                                                                  TPoint
                                                                                                                  .
                                                                                                                  PointCreationType
                                                                                                                  .
                                                                                                                  screen));

            _tablemanager.DisplayManager.AddElement(sc1, ObjectTable.Code.Display.DisplayManager.DisplayLayer.top);
            _tablemanager.DisplayManager.AddElement(sch1, ObjectTable.Code.Display.DisplayManager.DisplayLayer.top);
            //Canvas.SetLeft(sch1, 450);
            //Canvas.SetTop(sch1, 150);
        }

        void _bscrw_Closed(object sender, EventArgs e)
        {
            //recreate the window
            _bscrw = new BeamerScreenWindow();
            _bscrw.Closed += new EventHandler(_bscrw_Closed);
            _bscrw.Show();
            _tablemanager.BeamerScreen = _bscrw.GetBeamerUC();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ModuleTogglerForm mtf = new ModuleTogglerForm(ref _tablemanager);
            mtf.Show();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DepthCalibrationForm dcf = new DepthCalibrationForm(_tablemanager);
            dcf.Show();
        } 

        private void b_viewdepth_Click(object sender, RoutedEventArgs e)
        {
            DepthMapViewer dmv = new DepthMapViewer(ref _tablemanager);
            dmv.Show();
        }
    }
}
