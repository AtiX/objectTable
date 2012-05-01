using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Calibration
{
    /// <summary>
    /// Calibrates the Height of the table by generating the average of height values
    /// </summary>
    public class TableHeightCalibrator
    {
        public void GenerateCalibrationData(DepthImage img, List<TPoint> CalibrationPoints, out int TableDistance, out int TableHeightTolerance)
        {
            TableDistance = 0;
            TableHeightTolerance = 0;

            List<int> TableHeights = new List<int>();

            foreach (TPoint cp in CalibrationPoints)
            {
                TableHeights.Add(img.Data[cp.DepthX, cp.DepthY]);
            }

            //The average
            TableDistance = (int)Math.Round((double)TableHeights.Sum() / TableHeights.Count());

            //Difference between min and Max + 7 mm (thats how accurate the kinect is)
            TableHeightTolerance = (TableHeights.Max() - TableHeights.Min()) + 7;
        }

        public void SetCalibrationData(int TableDistance, int TableHeightTolerance)
        {
            SettingsManager.RecognitionSet.TableDistance = TableDistance;
            SettingsManager.RecognitionSet.TableDistanceRange = TableHeightTolerance;
        }
    }
}
