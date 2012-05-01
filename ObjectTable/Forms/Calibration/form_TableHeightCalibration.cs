using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObjectTable.Code.Calibration;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Forms.Calibration
{
    public partial class form_TableHeightCalibration : Form
    {
        private int[] _deptharray;
        private int _width, _height;

        private TableHeightCalibrator _theightc;
        private List<TPoint> _calibrationPoints;

        public form_TableHeightCalibration(int[] DepthArray, int Width, int Height)
        {
            InitializeComponent();

            _width = Width;
            _height = Height;

            _deptharray = DepthArray;
            _theightc = new TableHeightCalibrator();
            _calibrationPoints = new List<TPoint>();
        }

        private void form_TableHeightCalibration_Load(object sender, EventArgs e)
        {
            //Generate a greyscale Image from the DepthData
            DepthFrameVisualizer dfv = new DepthFrameVisualizer();
            dfv.RecalculateMinMax(_deptharray);
            Bitmap bmp = dfv.ConvertToGrayscale(_deptharray, _width, _height);
            p_img.BackgroundImage = bmp;

            //if existent, load values
            if (SettingsManager.PreprocessingSet.DefaultCorrectionMap != null)
            {
                txt_bottom.Text = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffBOttom.ToString();
                txt_left.Text = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffLeft.ToString();
                txt_r.Text = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffRight.ToString();
                txt_top.Text = SettingsManager.PreprocessingSet.DefaultCorrectionMap.CutOffTop.ToString();
            }
        }

        private void p_img_MouseClick(object sender, MouseEventArgs e)
        {
            //Add to Calibrationpoints
            Point mpoint = new Point(e.X, e.Y);
            TPoint tp = new TPoint(mpoint.X, mpoint.Y, TPoint.PointCreationType.depth);
            _calibrationPoints.Add(tp);

            Update();
        }

        private void Update()
        {
            int th, tht;
            if (_calibrationPoints.Count > 0)
            {
                //Generate Calibration Data
                _theightc.GenerateCalibrationData(new DepthImage(_deptharray, _width, _height), _calibrationPoints,
                                                  out th, out tht);
            }
            else
            {
                th = 0;
                tht = 0;
            }

            l_calibpoints.Text = _calibrationPoints.Count().ToString();
            l_distancetolerance.Text = tht.ToString() + " mm";
            l_tabledistance.Text = th.ToString() + " mm";
        }

        private void b_reset_Click(object sender, EventArgs e)
        {
            _calibrationPoints.Clear();
            Update();
        }

        private void b_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void b_calibrate_Click(object sender, EventArgs e)
        {
            Update();

            int th, tht;
            //Generate Calibration Data
            _theightc.GenerateCalibrationData(new DepthImage(_deptharray, _width, _height), _calibrationPoints,
                                              out th, out tht);
            //Set Calibration Data
            _theightc.SetCalibrationData(th, tht);

            MessageBox.Show("Calibration Data Saved!", "Success");
        }

        private void form_TableHeightCalibration_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void b_depth_Click(object sender, EventArgs e)
        {
            int th, tht;

            //Generate Calibration Data
            _theightc.GenerateCalibrationData(new DepthImage(_deptharray, _width, _height), _calibrationPoints,
                                              out th, out tht);

            DepthMapPreprocessor dmp = new DepthMapPreprocessor();
            DepthCorrectionMap dcm = dmp.CreateDepthCorrectionMap(new DepthImage(_deptharray, _width, _height), th);

            //Set tableHeightTolerance to 25, as there was a depth CorrectionMap created
            tht = 25;

            //If there are border-cutoffs, use them
            try
            {
                if (txt_bottom.Text != "")
                    dcm.CutOffBOttom = Convert.ToInt32(txt_bottom.Text);
                if (txt_left.Text != "")
                    dcm.CutOffLeft = Convert.ToInt32(txt_left.Text);
                if (txt_r.Text != "")
                    dcm.CutOffRight = Convert.ToInt32(txt_r.Text);
                if (txt_top.Text != "")
                    dcm.CutOffTop = Convert.ToInt32(txt_bottom.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Ungültige werte beim Border CutOff!", "Fehler");
                return;
            }

            SettingsManager.PreprocessingSet.DefaultCorrectionMap = dcm;
            _theightc.SetCalibrationData(th, tht);

            //Save the CorrectionMap
            if (!SettingsManager.SaveSettings())
            {
                MessageBox.Show("Depth map created and applied, but settings couldn't be saved!", "Warning!");
            }
            else
            {
                MessageBox.Show("DepthMap Data Created and saved. Calibration is now finished.", "Success!");
            }
        }

        private void p_img_MouseMove(object sender, MouseEventArgs e)
        {
            tssl.Text = "X: " + e.X.ToString() + "  Y: " + e.Y.ToString();
        }

    }
}
