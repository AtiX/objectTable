using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable.Code;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.SettingManagement;
using ObjectTable.Forms.Calibration;

namespace ObjectTable.Forms
{
    /// <summary>
    /// Allows the Application to show the user different Calibration and Debug Forms
    /// </summary>
    public class FormSupplier
    {
        private RecognitionManager _tmanager;

        private form_TableHeightCalibration _tablecalibrationform;

        public FormSupplier(RecognitionManager recognitionMgr)
        {
            _tmanager = recognitionMgr;
        }

        public void ShowTableCalibrationForm()
        {
                int width, height;
                SettingsManager.KinectSet.GetDepthResolution(out width, out height);
                
                form_TableHeightCalibration thc = new form_TableHeightCalibration(_tmanager.LastKinectDepthFrame, width, height);
                thc.Show();
        }

        public void CloseAllForms()
        {
            if (_tablecalibrationform != null)
            {
                _tablecalibrationform.Close();
                _tablecalibrationform = null;
            }
        }
    }
}
