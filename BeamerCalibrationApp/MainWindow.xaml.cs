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
using ObjectTable.Code;
using ObjectTableForms;
using ObjectTableForms.Forms.Debug;
using ObjectTableForms.Forms.Screen;
using ObjectTableForms.Forms.Settings;

namespace BeamerCalibrationApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TableManager _tableManager;
        private BeamerScreenWindow _beamerScreenWindow;

        public MainWindow()
        {
            InitializeComponent();
            _beamerScreenWindow = new BeamerScreenWindow();
            _beamerScreenWindow.Show();

            _tableManager = new TableManager(_beamerScreenWindow.GetBeamerUC());
        }

        private void b_kinect_Click(object sender, RoutedEventArgs e)
        {
            ModuleTogglerForm mtf = new ModuleTogglerForm(ref _tableManager);
            mtf.Show();
        }

        private void b_depthcalibration_Click(object sender, RoutedEventArgs e)
        {
            DepthCalibrationForm dcf = new DepthCalibrationForm(_tableManager);
            dcf.Show();
        }

        private void b_beamercalib_Click(object sender, RoutedEventArgs e)
        {
            ScreenCalibrationWindow scw = new ScreenCalibrationWindow(_tableManager,true);
            scw.Show();
        }

        private void b_depthmap_Click(object sender, RoutedEventArgs e)
        {
            DepthMapViewer dmv = new DepthMapViewer(ref _tableManager);
            dmv.Show();
        }
    }
}
