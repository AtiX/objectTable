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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ObjectTable.Code;
using ObjectTable.Code.SettingManagement;
using ObjectTableForms;
using ObjectTableForms.Forms.Debug;
using ObjectTableForms.Forms.Screen;
using ProgrammingTable.Code.Simulation;
using ProgrammingTable.Code.General;

namespace ProgrammingTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TableManager _tableManager;
        private BeamerScreenWindow _beamerScreen;
        private SimulationEngine _simulationEngine;

        public MainWindow()
        {
            InitializeComponent();

            //Set version info
            l_version.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Load Settings
            bool sload = LocalSettingsManager.LoadSettings();
            if (!sload)
            {
                MessageBox.Show("Einstellungen konnten nicht geladen werden. Standardeinstellungen werden verwendet",
                                "SimulationsEngine");
                LocalSettingsManager.SaveSettings();
            }

            //Init
            _beamerScreen = new BeamerScreenWindow();
            _beamerScreen.Show();
            _tableManager = new TableManager(_beamerScreen.GetBeamerUC(),"..\\..\\settings\\ProgrammingTable\\");
            _tableManager.ToggleObjectTracking = true;
            _tableManager.ToggleObjectRecognition = true;
            _tableManager.ToggleObjectRotationAnalysation = true;

            _simulationEngine = new SimulationEngine(_tableManager);
        }

#region menuevents
        private void mi_kinect_init_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_simulationEngine.SimulationRunning)
            {
                MessageBoxResult mbres = MessageBox.Show(
                    "Wenn die Simulatin neu gestartet wird gehen sämtliche Daten sowie der Aufbau verloren!", "Warnung");
                if (mbres == MessageBoxResult.Cancel)
                    return;

                _simulationEngine.SimulationRunning = false;
                MessageBox.Show("Entfernen sie alle Objekte von der Oberfläche!", "Achtung");
            }
            bool res = _tableManager.Start();
            if (!res)
            {
                MessageBox.Show("Kinect konnte nicht initialisiert werden!", "Fehler!");
            }

            if (_tableManager.KinectRunning)
            {
                mi_kinect_init.IsChecked = true;
                l_kinectstatus.Text = "in Betrieb";
                l_kinectstatus.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                mi_kinect_init.IsChecked = false;
                l_kinectstatus.Text = "nicht initialisiert";
                l_kinectstatus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void mi_kinect_calib_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_tableManager.KinectRunning)
            {
                MessageBox.Show("Kinect muss zuerst initialisiert werden!", "Achtung!");
            }
            else
            {
                DepthCalibrationForm dcf = new DepthCalibrationForm(_tableManager);
                dcf.Show();
            }
        }

        private void micb_objectrec_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_tableManager != null)
            _tableManager.ToggleObjectRecognition = Convert.ToBoolean(micb_objectrec.IsChecked);
        }

        private void micb_tracking_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_tableManager != null)
            _tableManager.ToggleObjectTracking = Convert.ToBoolean(micb_tracking.IsChecked);
        }

        private void micb_rotation_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_tableManager != null)
            _tableManager.ToggleObjectRotationAnalysation = Convert.ToBoolean(micb_rotation.IsChecked);
        }

        private void micb_simPause_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_simulationEngine != null)
            {
                _simulationEngine.SimulationRunning = !Convert.ToBoolean(micb_simPause.IsChecked);
                if (!Convert.ToBoolean(micb_simPause.IsChecked))
                {
                    l_simulationstatus.Text = "läuft";
                    l_simulationstatus.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    l_simulationstatus.Text = "pausiert";
                    l_simulationstatus.Foreground = new SolidColorBrush(Colors.Yellow);
                }
            }
        }

        private void mi_restart_sim_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Wenn die Simulatin neu gestartet wird gehen sämtliche Daten sowie der Aufbau verloren!", "Warnung", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.Cancel)
                return;

            //delete all objects on table
            //_simulationEngine.SimulationRunning = false;
            //_simulationEngine = null;
            _tableManager.DisplayManager.ClearScreen();

            MessageBox.Show("Entfernen Sie alle Objekte von der Oberfläche!", "Achtung");

            //restart
            //_simulationEngine = new SimulationEngine(_tableManager);
            //_simulationEngine.SimulationRunning = Convert.ToBoolean(micb_simPause.IsChecked);

            MessageBox.Show("Simulation neu gestartet", "Hinweis");
        }

        private void mi_showdepthmaps_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_tableManager.KinectRunning)
            {
                MessageBox.Show("Kinect muss zuerst initialisiert werden!", "Achtung!");
            }
            else
            {
                //open this window in own thread, because these calculations are expensive
                Thread d = new Thread(() =>
                                          {
                                            DepthMapViewer dmv = new DepthMapViewer(ref _tableManager);
                                            //Bind to the close event so that the thread is closed when the window closes
                                            dmv.Closed += (sender2, e2) => dmv.Dispatcher.InvokeShutdown();
                                            //make it WPF ready
                                            System.Windows.Threading.Dispatcher.Run();
                                          });
                d.SetApartmentState(ApartmentState.STA);
                d.Start();
            }
        }

        private void mi_beamercalibration_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ScreenCalibrationWindow scw = new ScreenCalibrationWindow(_tableManager,true);
            scw.Show();
        }

        private void micb_objectrec_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            micb_objectrec_Checked(sender,e);
        }

        private void micb_tracking_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            micb_tracking_Checked(sender, e);
        }

        private void micb_rotation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            micb_rotation_Checked(sender, e);
        }

        private void micb_simPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            micb_simPause_Checked(sender, e);
        }

        private void mi_end_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        

        #endregion menuevents

        private void mi_performance_Click(object sender, RoutedEventArgs e)
        {
            PerformanceViewer pv = new PerformanceViewer(_tableManager);
            pv.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Stop simulation
            _simulationEngine.SimulationRunning = false;
            Thread.Sleep(200);
            _tableManager.Stop();
            Thread.Sleep(250);
            _beamerScreen.Close();

            //save settings
            LocalSettingsManager.SaveSettings();
            SettingsManager.SaveSettings();
        }

        private void sl_Simuspeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_simulationEngine == null)
                return;

            //update label
            l_simdelay.Text = sl_Simuspeed.Value.ToString() + " ms";
            //update engine
            _simulationEngine.SimulationDelay = new TimeSpan(0, 0, 0, 0, (int)sl_Simuspeed.Value);
        }


    }
}
