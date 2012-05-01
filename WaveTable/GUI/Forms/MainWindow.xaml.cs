using System;
using System.Collections.Generic;
using System.Drawing;
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
using ObjectTable.Code;
using ObjectTable.Code.Display.GUI;
using ObjectTable.Code.SettingManagement;
using ObjectTableForms;
using ObjectTableForms.Forms.Debug;
using ObjectTableForms.Forms.Screen;
using WaveSimLib.Code.Wave;
using WaveTable.Code.Engine;
using WaveTable.GUI.Forms;
using WaveTable.Code.SimulationObjects;
using Microsoft.Win32;

namespace WaveTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TableManager _tableManager;
        private BeamerScreenWindow _beamerScreen;
        private WaveTableEngine _waveTableEngine;

        public MainWindow()
        {
            InitializeComponent();

            //Set version info
            l_version.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            
            //Because of the wave engine, the beamerscreen is in a seperate thread
            Thread d = new Thread(() =>
            {
                _beamerScreen = new BeamerScreenWindow();
                //Bind to the close event so that the thread is closed when the window closes
                _beamerScreen.Closed += (sender2, e2) => _beamerScreen.Dispatcher.InvokeShutdown();
                //make it WPF ready
                System.Windows.Threading.Dispatcher.Run();
            });

            d.SetApartmentState(ApartmentState.STA);
            d.Start();
            Thread.Sleep(500); //Wait for the screen to load

            BeamerDisplayUC beameruc = null;
            _beamerScreen.Dispatcher.Invoke((Action) (() => { beameruc = _beamerScreen.GetBeamerUC(); }));
            _tableManager = new TableManager(beameruc);

            _tableManager.ToggleObjectTracking = true;
            _tableManager.ToggleObjectRecognition = true;
            _tableManager.ToggleObjectRotationAnalysation = true;

            _waveTableEngine = new WaveTableEngine(_tableManager);
            UpdateSimResolution();
            _waveTableEngine.Initialize();

            //UI Data Init
            UpdateUiActions();
            UpdateSimParams(true);
        }

        #region menuevents
        private void mi_kinect_init_Click(object sender, System.Windows.RoutedEventArgs e)
        {
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
            if (_waveTableEngine != null)
            {
                if ((bool)micb_simPause.IsChecked)
                    _waveTableEngine.Stop();
                else
                    _waveTableEngine.Start();

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
            _waveTableEngine.Stop();
            _waveTableEngine = null;
            _tableManager.DisplayManager.ClearScreen();

            MessageBox.Show("Entfernen Sie alle Objekte von der Oberfläche!", "Achtung");

            //restart
            _waveTableEngine = new WaveTableEngine(_tableManager);
            UpdateSimResolution();
            UpdateSimParams(false);
            UpdateUiActions();

            MessageBox.Show("Simulation neu gestartet", "Hinweis");
            _waveTableEngine.Start();
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
            ScreenCalibrationWindow scw = new ScreenCalibrationWindow(_tableManager, false);
            scw.Show();
        }

        private void micb_objectrec_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            micb_objectrec_Checked(sender, e);
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



        #endregion menuevents

        /// <summary>
        /// Sets the UI Actions for Objects (WaveEngine) according to the user input
        /// </summary>
        private void UpdateUiActions()
        {
            if (!this.IsInitialized)
                return;

            //Mouse
            UIElementAction mouseAction = new UIElementAction();
            switch (cb_mouseAction.SelectedIndex)
            {
                case 0:// Poke
                    mouseAction.ActionType = UIElementAction.EActionType.SetAmp;
                    break;
                case 1://Set Wall
                    mouseAction.ActionType = UIElementAction.EActionType.SetWall;
                    break;
                case 2://Del Wall
                    mouseAction.ActionType = UIElementAction.EActionType.DelWall;
                    break;
                case 3://Set Mass
                    mouseAction.ActionType = UIElementAction.EActionType.SetMass;
                    break;
                case 4://Del Mass
                    mouseAction.ActionType = UIElementAction.EActionType.DelMass;
                    break;
                case 5://Create WaveSource
                    mouseAction.ActionType = UIElementAction.EActionType.SetSource;
                    break;
            }

            int size = Convert.ToInt32(((ComboBoxItem)cb_mouseSize.SelectedItem).Content);
            double value = sl_mouseValue.Value;
            if (mouseAction.ActionType == UIElementAction.EActionType.SetSource) //adjust values for source frequency
                value = sl_mouseValue.Value/13.0;

            mouseAction.Size = size;
            mouseAction.Value = value;

            //LTO
            UIElementAction LtoAction = new UIElementAction();
            switch (cb_ltoType.SelectedIndex)
            {
                case 0://WaveSource
                    LtoAction.ActionType = UIElementAction.EActionType.SetSource;
                    LtoAction.ActionObjectType = typeof(SimWaveSourceObject);
                    break;
                case 1://Mass
                    LtoAction.ActionType = UIElementAction.EActionType.SetMass;
                    break;
                case 2://Wall
                    LtoAction.ActionType = UIElementAction.EActionType.SetWall;
                    break;
                case 3://Plotter
                    LtoAction.ActionType = UIElementAction.EActionType.NULL;
                    LtoAction.ActionObjectType = typeof(SimPlotterObject);
                    break;
            }
            switch (cb_ltoShape.SelectedIndex)
            {
                case 0:
                    LtoAction.Shape = UIElementAction.EShape.Square;
                    break;
                case 1:
                    LtoAction.Shape = UIElementAction.EShape.Circle;
                    break;
            }
            LtoAction.Size = Convert.ToInt32(((ComboBoxItem)cb_ltoSize.SelectedItem).Content);
            LtoAction.Value = sl_ltoValue.Value;
            if (LtoAction.ActionObjectType == typeof(SimWaveSourceObject)) //Adjust wave source value 
                LtoAction.Value = sl_mouseValue.Value/13.0;

            //TOO
            UIElementAction tooAction = new UIElementAction();
            tooAction.Value = sl_tooValue.Value;
            switch (cb_tooAktion.SelectedIndex)
            {
                case 0:// Elongatioon ~ objectHeight
                    tooAction.ActionType = UIElementAction.EActionType.SetAmp;
                    tooAction.Value = -1;
                    break;
                case 1://Const Elongation
                    tooAction.ActionType = UIElementAction.EActionType.SetAmp;
                    break;
            }
            tooAction.Size = Convert.ToInt32(((ComboBoxItem)cb_tooSize.SelectedItem).Content);

            _waveTableEngine.UiLtoAction = LtoAction;
            _waveTableEngine.UiMouseAction = mouseAction;
            _waveTableEngine.UiTooAction = tooAction;
        }

        private void UpdateSimParams(bool read=false)
        {
            if (read)
            {
                WaveSettings sr = _waveTableEngine.GetSimulationData(false);
                txt_deltaT.Text = sr.DeltaT.ToString();
                txt_distanz.Text = sr.TeilchenDistanz.ToString();
                txt_dkopplung.Text = sr.FederkonstanteKopplung.ToString();
                txt_energie.Text = sr.Energieerhaltung.ToString();
                txt_dteilchen.Text = sr.FederkonstanteTeilchen.ToString();
                txt_fps.Text = sr.DesiredFPS.ToString();
                txt_stdmasse.Text = sr.Teilchenmasse.ToString();
            }
            else
            {
                WaveSettings set = new WaveSettings(new System.Drawing.Size(1, 1)); //Right size will be set by waveTableEngine
                try
                {
                    set.DeltaT = Convert.ToDouble(txt_deltaT.Text);
                    set.DesiredFPS = Convert.ToInt32(txt_fps.Text);
                    set.Energieerhaltung = Convert.ToDouble(txt_energie.Text);
                    set.FederkonstanteKopplung = Convert.ToDouble(txt_dkopplung.Text);
                    set.FederkonstanteTeilchen = Convert.ToDouble(txt_dteilchen.Text);
                    set.TeilchenDistanz = Convert.ToDouble(txt_distanz.Text);
                    set.Teilchenmasse = Convert.ToDouble(txt_stdmasse.Text);
                    _waveTableEngine.UpdateSimParams(set);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Fehler bei der Datenkonvertierung. Änderungen wurden nicht übernommen.", "Fehler");
                }
            }
        }

        private void mi_performance_Click(object sender, RoutedEventArgs e)
        {
            PerformanceViewer pv = new PerformanceViewer(_tableManager);
            pv.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Stop simulation
            _waveTableEngine.Stop();

            _tableManager.Stop();
            Thread.Sleep(250);
            _beamerScreen.Dispatcher.Invoke((Action) (() => _beamerScreen.Close()));

            //save settings
            SettingsManager.SaveSettings();
        }

        private void UpdateSimResolution()
        {
            if (!this.IsInitialized)
                return;

            System.Drawing.Size simSize = new System.Drawing.Size();

            switch (cb_ImageSize.SelectedIndex)
            {
                case 0:
                    simSize = new System.Drawing.Size(640, 480);
                    break;
                case 1:
                    simSize = new System.Drawing.Size(800, 600);
                    break;
                case 2:
                    simSize = new System.Drawing.Size(1024, 768);
                    break;
                case 3:
                    simSize = new System.Drawing.Size(1280, 800);
                    break;
                case 4:
                    simSize = new System.Drawing.Size(1920, 1080);
                    break;
                default:
                    simSize = new System.Drawing.Size(1280, 800);
                    break;
            }

            int SimDivisor = cb_elementDivisor.SelectedIndex + 1;

            bool wasRunning = _waveTableEngine.SimulationRunning;

            if (wasRunning)
                _waveTableEngine.Stop();

            _waveTableEngine.SimulationSize = simSize;
            _waveTableEngine.SimulationDivisor = SimDivisor;

            if (wasRunning)
                _waveTableEngine.Start();
        }
        #region GuiEvents
        private void cb_mouseAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void cb_mouseSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void sl_mouseValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateUiActions();
        }

        private void cb_ltoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void cb_ltoShape_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void cb_ltoSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void sl_ltoValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateUiActions();
        }

        private void cb_tooAktion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void cb_tooShape_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void cb_tooSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUiActions();
        }

        private void sl_tooValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateUiActions();
        }
        
        private void txt_paramset_Click(object sender, RoutedEventArgs e)
        {
            UpdateSimParams();
        }

        private void b_delElongation_Click(object sender, RoutedEventArgs e)
        {
            _waveTableEngine.ClearSimulation(true, false, false, false);
        }

        private void b_delWalls_Click(object sender, RoutedEventArgs e)
        {
            _waveTableEngine.ClearSimulation(false, true, false, false);
        }

        private void b_delWaveSources_Click(object sender, RoutedEventArgs e)
        {
            _waveTableEngine.ClearSimulation(false, false, false, true);
        }

        private void b_delMass_Click(object sender, RoutedEventArgs e)
        {
            _waveTableEngine.ClearSimulation(false, false, true, false);
        }
        
        private void cb_ImageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSimResolution();
        }

        private void cb_elementDivisor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSimResolution();
        }
        #endregion

        private void b_save_Click(object sender, RoutedEventArgs e)
        {
            _waveTableEngine.Stop();

            MessageBoxResult sres = MessageBox.Show("Objekte auf der Oberfläche auch einspeichern? \n Diese werden zu statischen Massen/Wand-Daten konvertiert.", "Speichern", MessageBoxButton.YesNoCancel);

            WaveSettings save = null;

            if (sres == MessageBoxResult.Cancel)
            {
                _waveTableEngine.Start();
                return;
            }
            else if (sres == MessageBoxResult.No)
            {
                save = _waveTableEngine.GetSimulationData(false);
            }
            else
            {
                save = _waveTableEngine.GetSimulationData(true);
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "WaveSim-Dateien|*.was|Alle Dateien|*.*";
            sfd.AddExtension = true;
            sfd.FileName = "NeueSimulation.was";

            bool sdr = (bool)sfd.ShowDialog();

            if (sdr)
            {
                bool res = save.SaveToFile(sfd.FileName);
                if (!res)
                    MessageBox.Show("Fehler beim Speichern.", "Speichern");
            }

            _waveTableEngine.Start();
        }

        private void b_load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "WaveSim-Dateien|*.was|Alle Dateien|*.*";

            if (ofd.ShowDialog() == true)
            {
                WaveSettings load = new WaveSettings();
                bool res = load.LoadFromFile(ofd.FileName);
                if (!res)
                {
                    MessageBox.Show("Fehler beim Laden der Datei.", "Laden");
                    return;
                }

                //Check sizes
                int simWidth = _waveTableEngine.SimulationSize.Width / _waveTableEngine.SimulationDivisor;
                int simHeight = _waveTableEngine.SimulationSize.Height / _waveTableEngine.SimulationDivisor;

                if ((load.Width != simWidth) || (load.Height != simHeight))
                {
                    MessageBox.Show("Fehler beim Laden der Datei.\n \nDie Simulationsgrößen stimmen nicht überein.");
                    return;
                }

                _waveTableEngine.LoadSimulation(load, (bool)cb_loadElongation.IsChecked, (bool)cb_LoadMass.IsChecked, (bool)cb_loadWalls.IsChecked, (bool)cb_loadWaveSources.IsChecked, (bool)cb_loadSimParams.IsChecked);
                
                if ((bool)cb_loadSimParams.IsChecked)
                    UpdateSimParams(true);
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //Update FPS
            l_fps.Content = _waveTableEngine.FPS.ToString();
        }


    }
}
