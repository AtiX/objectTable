using System;
using System.Collections.Generic;
using System.Drawing;
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
using ObjectTable.Code.Calibration;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Display;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;
using Point = System.Windows.Point;

namespace ObjectTableForms
{
	/// <summary>
	/// Interaction logic for DepthCalibrationForm.xaml
	/// </summary>
	public partial class DepthCalibrationForm : Window
	{
	    private DepthImage _dimage;
	    private TableManager _tmgr;
	    private List<TPoint> points;
	    private TableHeightCalibrator _theightc;

		public DepthCalibrationForm(TableManager tmgr)
		{
			this.InitializeComponent();
		    points = new List<TPoint>();
            _tmgr = tmgr;
		    _theightc = new TableHeightCalibrator();
			// Insert code required on object creation below this point.
		    b_aktualisieren_Click(null, null);

		}

        private void b_aktualisieren_Click(object sender, RoutedEventArgs e)
        {
            if (_tmgr.LastKinectDepthFrame == null)
            {
                MessageBox.Show("Kinect muss zuerst initialisiert werden!", "Fehler!");
                return;
            }

            int width, height;
            SettingsManager.KinectSet.GetDepthResolution(out width, out height);
            _dimage = new DepthImage(_tmgr.LastKinectDepthFrame, width, height);

            Bitmap bmp = MapVisualizer.VisualizeDepthImage(_dimage,false,false);
            image.Source = bmp.ToWpfBitmap();

            s_cutoff_down.Value = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffBOttom;
            s_cutoff_left.Value = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffLeft;
            s_cutoff_right.Value = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffRight;
            s_cutoff_top.Value = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffTop;

            l_höhe.Text = SettingsManager.RecognitionSet.TableDistance.ToString();
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(image);
            points.Add(new TPoint((int)p.X,(int)p.Y,TPoint.PointCreationType.depth));
            int distance;
            int tolerance;

            _theightc.GenerateCalibrationData(_dimage, points, out distance, out tolerance);

            l_höhe.Text = distance.ToString();
        }

        private void b_deleteHeightData_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            l_höhe.Text = "0";
        }

        private void b_calibrate_Click(object sender, RoutedEventArgs e)
        {
            //margins
            int top = (int)s_cutoff_top.Value;
            int left = (int) s_cutoff_left.Value;
            int right = (int) s_cutoff_right.Value;
            int bottom = (int) s_cutoff_down.Value;

            //calibrate
            if (points.Count == 0)
            {
                MessageBox.Show("Es muss mindestens ein kalibrationspunkt vorhanden sein!", "Fehler!");
                return;
            }

            int distance;
            int tolerance;

            _theightc.GenerateCalibrationData(_dimage, points, out distance, out tolerance);
            _theightc.SetCalibrationData(distance, 25);

            DepthMapPreprocessor dmp = new DepthMapPreprocessor();

            SettingsManager.PreprocessingSet.DefaultCorrectionMap = dmp.CreateDepthCorrectionMap(_dimage, distance);
            SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffBOttom = bottom;
            SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffLeft = left;
            SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffRight = right;
            SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffTop = top;

            if (SettingsManager.SaveSettings())
            {
                MessageBox.Show("Kalibration erfolgreich ausgeführt, Einstellungen gespeichert", "Erfolg");
            }
            else
            {
                MessageBox.Show("Kalibration ausgeführt, Einstellungen konnten aber niht gespeichert werden.", "Achtung");
            }

            this.Close();
        }
	}
}