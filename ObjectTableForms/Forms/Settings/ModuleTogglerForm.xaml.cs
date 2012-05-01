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
using ObjectTable.Code;

namespace ObjectTableForms.Forms.Settings
{
    /// <summary>
    /// Interaction logic for ModuleTogglerForm.xaml
    /// </summary>
    public partial class ModuleTogglerForm : Window
    {
        private TableManager _tableManager;

        public ModuleTogglerForm(ref TableManager tableManager)
        {
            InitializeComponent();
            _tableManager = tableManager;
            UpdateUI();
        }

        public void UpdateUI()
        {
            //Kinect button
            if (_tableManager.KinectRunning)
            {
                b_kinect.Content = "Kinect läuft";
                cb_kinect.IsEnabled = true;
                cb_rotation.IsEnabled = true;
                cb_tracking.IsEnabled = true;
            }
            else
            {
                b_kinect.Content = "Kinect läuft nicht";
                cb_kinect.IsEnabled = false;
                cb_rotation.IsEnabled = false;
                cb_tracking.IsEnabled = false;
            }

            //CBs
            cb_kinect.IsChecked = _tableManager.ToggleObjectRecognition;
            cb_rotation.IsChecked = _tableManager.ToggleObjectRotationAnalysation;
            cb_tracking.IsChecked = _tableManager.ToggleObjectTracking;
        }

        private void cb_kinect_Checked(object sender, RoutedEventArgs e)
        {
            _tableManager.ToggleObjectRecognition = (bool)cb_kinect.IsChecked;
            UpdateUI();
        }

        private void cb_tracking_Checked(object sender, RoutedEventArgs e)
        {
            _tableManager.ToggleObjectTracking = (bool) cb_tracking.IsChecked;
            UpdateUI();
        }

        private void cb_rotation_Checked(object sender, RoutedEventArgs e)
        {
            _tableManager.ToggleObjectRotationAnalysation = (bool) cb_rotation.IsChecked;
            UpdateUI();
        }

        private void b_kinect_Click_1(object sender, RoutedEventArgs e)
        {
            if (_tableManager.KinectRunning)
            {
                _tableManager.Stop();
            }
            else
            {
                bool res = _tableManager.Start();
                if (!res)
                {
                    MessageBox.Show("Fehler bei der Initialisierung des KINECT-Sensors.", "Fehler");
                }
            }
            UpdateUI();
        }

    }
}
