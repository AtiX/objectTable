using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// Preprocesses an depth-map for further work
    /// </summary>
    public class DepthMapPreprocessor
    { 
        /// <summary>
        /// Applies a correctionMap to a raw DepthImage
        /// </summary>
        /// <param name="rawImage"></param>
        /// <param name="correctionMap"></param>
        /// <returns></returns>
        public DepthImage ApplyDepthCorrection(DepthImage rawImage, DepthCorrectionMap correctionMap)
        {
            if (rawImage.Width != correctionMap.Width ||rawImage.Height != correctionMap.Height)
            {
                throw new Exception("Image size does not match");
            }

            //calculate the values for the section that is not involved in border cutting
            int BorderCutXmin = 0 + correctionMap.CutOffLeft;
            int BorderCutXmax = rawImage.Width - 1 - correctionMap.CutOffRight;
            int BorderCutYmin = 0 + correctionMap.CutOffTop;
            int BorderCutYmax = rawImage.Height - 1 - correctionMap.CutOffBOttom; 

            //Apply the Depth Correction
            for (int x=0; x < rawImage.Width; x++)
            {
                for (int y = 0; y < rawImage.Height; y++)
                {
                    if (rawImage.Data[x,y] > 0) //Only if there wasn't a reading error
                        rawImage.Data[x, y] = rawImage.Data[x, y] + correctionMap.CorrectionData[x, y];
                    
                    //Coordinates outside the CutOffBorder?
                    if (!((BorderCutXmin <= x) && (x <= BorderCutXmax) && (BorderCutYmin <= y) && (y <= BorderCutYmax)))
                        rawImage.Data[x, y] = 0;
                }
            }

            return rawImage;
        }

        /// <summary>
        /// Creates a DepthCorrectionMap from an uncorrected raw image and the desired TableDistance
        /// </summary>
        /// <param name="uncorrectedImage"></param>
        /// <param name="averageTableDistance"></param>
        /// <returns></returns>
        public DepthCorrectionMap CreateDepthCorrectionMap(DepthImage uncorrectedImage, int averageTableDistance)
        {
            DepthCorrectionMap correctionMap = new DepthCorrectionMap(uncorrectedImage.Width, uncorrectedImage.Height);
            int lastValue = 0;

            for (int x = 0; x < uncorrectedImage.Width; x++)
            {
                for (int y = 0; y < uncorrectedImage.Height; y++)
                {
                    if (uncorrectedImage.Data[x, y] > 0)
                    {
                        //Correctly recognized point: calculate correction value
                        correctionMap.CorrectionData[x, y] = averageTableDistance - uncorrectedImage.Data[x, y];
                        lastValue = averageTableDistance - uncorrectedImage.Data[x, y];
                    }
                    else
                    {
                        //Not recognized point (height = 0) use recent calibration value as approximation
                        correctionMap.CorrectionData[x, y] = lastValue;
                    }
                }
            }

            return correctionMap;
        }


        public DepthImage NormalizeHeights(DepthImage source)
        {
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int height = source.Data[x, y];

                    //Normalize Height (Calculate the object height in respect to the table surface)
                    height = SettingsManager.RecognitionSet.TableDistance - height;

                    //is the height within the Range of the table surface? then set to 0
                    //also set to 0 if height is negative -> "under" the table
                    if (height < 0 || SettingsManager.RecognitionSet.TableDistanceRange > height)
                        height = 0;

                    //is the height too high, set it to 0
                    if (height > SettingsManager.RecognitionSet.HandMaximalHeight)
                        height = 0;

                    source.Data[x, y] = height;
                }
            }
            return source;
        }
 

    }
}
