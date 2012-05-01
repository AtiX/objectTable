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

namespace ObjectTableForms.Forms.Debug
{
    /// <summary>
    /// Interaction logic for PerformanceViewer.xaml
    /// </summary>
    public partial class PerformanceViewer : Window
    {
        private TableManager _tmgr;
        private List<double> frameRateAvg;

        public PerformanceViewer(TableManager tmgr)
        {
            InitializeComponent();
            _tmgr = tmgr;
            _tmgr.OnNewObjectList += new TableManager.TableManagerObjectHandler(_tmgr_OnNewObjectList);
            frameRateAvg = new List<double>();
        }

        void _tmgr_OnNewObjectList()
        {
            this.Dispatcher.Invoke((Action) (() => b_update_Click(null, null)), null);
        }

        private void b_update_Click(object sender, RoutedEventArgs e)
        {
            l_depthframes.Content = _tmgr.DelayBetweenKinectDepthFrames.ToString();
            l_recognition.Content = _tmgr.RecognitionDuration.ToString();
            l_rotation.Content = _tmgr.RotationDetectionDuration.ToString();
            l_tracking.Content = _tmgr.TrackingDuration.ToString();


            l_frameDuration.Content = (_tmgr.DelayBetweenKinectDepthFrames + _tmgr.RotationDetectionDuration +
                                      _tmgr.RecognitionDuration + _tmgr.TrackingDuration).ToString();

            l_framerate.Content =
                (1000.0/(_tmgr.DelayBetweenKinectDepthFrames + _tmgr.RotationDetectionDuration +
                         _tmgr.RecognitionDuration + _tmgr.TrackingDuration)).ToString("0.00");

            frameRateAvg.Add((1000.0/(_tmgr.DelayBetweenKinectDepthFrames + _tmgr.RotationDetectionDuration +
                                      _tmgr.RecognitionDuration + _tmgr.TrackingDuration)));
            l_averageFps.Content = frameRateAvg.Average().ToString("0.000");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _tmgr.OnNewObjectList -= _tmgr_OnNewObjectList;
        }
    }
}
