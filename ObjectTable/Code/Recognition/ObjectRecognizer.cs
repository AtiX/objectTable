using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// Recognizes the Objects via a depth image and a prepared boolmap
    /// </summary>
    class ObjectRecognizer
    {
        /// <summary>
        /// Recognizes the Objects via a depth image and a prepared boolmap
        /// </summary>
        /// <param name="prepared_boolmap">a prepared boolmap, with only objects marked as true</param>
        /// <param name="image">the depth image, used to calculate the height of the recognized objects</param>
        /// <returns></returns>
        public List<TableObject> RecognizeObjects(bool[,] prepared_boolmap, DepthImage image, out int[,,] neighbourmap)
        {
            //Check in a raster whether there are any true pixels
            List<TPoint> TrueRasterPoints = GetTrueRasterPoints(prepared_boolmap, image);

            //Create a neighbourmap - an int[x,y,n] array, where the int stores the count of the whitepixel-neighbours of the object (n=0) and the size of the circle/rect used 
            //to count these neighbours (n=1). The circle/rect is scaled up until a defined percentage of the pixels is false, meaning no part of the object
            neighbourmap = CreateNeighbourMap(prepared_boolmap, image, TrueRasterPoints);

            //DEBUG
            if (SettingsManager.RecognitionSet.SaveDebugMaps)
            {
                Bitmap bmp = MapVisualizer.VisualizeNeighbourMap(neighbourmap, image.Width, image.Height);
                bmp.Save("neigbourmap.bmp");
            }

            //Now select the ObjectCenters (maximum neigbour-values on the neigbourmap)
            List<ObjectPoint> pointlist = SelectObjectCenters(neighbourmap, image.Width, image.Height);

            //Create TableObjects from the ObjectCenters List
            return GenerateTableObjects(pointlist, image);
        }

        private List<TableObject> GenerateTableObjects(List<ObjectPoint> pointlist, DepthImage image)
        {
            List<TableObject> tableobjects = new List<TableObject>();

            foreach (ObjectPoint op in pointlist)
            {
                TableObject tobj = new TableObject();

                tobj.Center = new TPoint(op.X, op.Y, TPoint.PointCreationType.depth);
                tobj.CenterDefined = true;
                tobj.Radius = op.RectSize;
                tobj.Height = image.Data[op.X, op.Y];

                tableobjects.Add(tobj);
            }

            return tableobjects;
        }

        private  List<ObjectPoint> SelectObjectCenters(int [,,] neigbourmap, int width, int height)
        {
            //Set the threshold to the maximal neigbours a point can possibly have
            int maxThreshold = SettingsManager.RecognitionSet.ObjectMaximalRadius*SettingsManager.RecognitionSet.ObjectMaximalRadius;
            int minThreshold = SettingsManager.RecognitionSet.ObjectMinimalRadius*SettingsManager.RecognitionSet.ObjectMinimalRadius;

            int threshold = maxThreshold;

            List<ObjectPoint> pointlist = new List<ObjectPoint>();

            while (threshold > minThreshold)
            {
                int currentMaxThreshold = 0;

                //Check whether there are any points matching this threshold
                for (int x = 0; x<width; x++)
                {
                    for (int y = 0; y<height; y++)
                    {
                        //Check whether this is a maximum threshold (for later use)
                        if (neigbourmap[x, y, 0] > currentMaxThreshold)
                            currentMaxThreshold = neigbourmap[x, y, 0];

                        //Check whether the pixel reach the current Threshold
                        if (neigbourmap[x, y, 0] >= threshold)
                        {
                            //Add this point to the list
                            ObjectPoint p = new ObjectPoint(x, y, neigbourmap[x, y, 0], neigbourmap[x, y, 1]);
                            pointlist.Add(p);

                            //Set the neigbours of the points within this point's Rectangle to 0, as they are part of the same object
                            //(so they aren't recognized anymore)
                            SetPointsToZero(ref neigbourmap, width, height, p);
                        }
                    }
                }

                //now decrease the threshold by 1 for the next run
                threshold--;
                //if the threshold was never reached, set it to the maximum threshold that occured in the neigbourmap
                if (threshold > currentMaxThreshold)
                    threshold = currentMaxThreshold;
            }

            return pointlist;
        }

        /// <summary>
        /// Sets all neigbourvalues around a given point (in "his" rectangle) to zero
        /// </summary>
        /// <param name="neigbourmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="point"></param>
        private void SetPointsToZero(ref int[,,] neigbourmap, int width, int height, ObjectPoint point)
        {
            //Create Rectangle for point
            TRectangle workrectancle = new TRectangle(point.X, point.Y, point.RectSize, true,
                                                      new TRectangle(0, 0, width-1, height-1));

            //Set every neigbourvalue to 0
            for (int x = workrectancle.X; x <= workrectancle.X2; x++)
            {
                for (int y = workrectancle.Y; y <= workrectancle.Y2; y++)
                {
                    neigbourmap[x, y, 0] = 0;
                }
            }
        }

        /// <summary>
        /// Creates a neighbourmap - an int[x,y,n] array, where the int stores the count of the whitepixel-neighbours of the object (n=0) and the size of the circle/rect used 
        /// to count these neighbours (n=1). The circle/rect is scaled up until a defined percentage of the pixels is false, meaning no part of the object. the neigbourmap is only created around true rasterpoints to be eficcent
        /// </summary>
        /// <param name="boolmap">The boolmap</param>
        /// <param name="image">Depth Image</param>
        /// <param name="rasterpoints">The calculated Rasterpoints</param>
        /// <returns></returns>
        private static int[,,] CreateNeighbourMap(bool[,] boolmap, DepthImage image, List<TPoint> rasterpoints)
        {
            int[,,] neighbourmap = new int[image.Width,image.Height,2];

            //Do this for each region arount the true rasterpoints
            foreach (TPoint rasterpoint in rasterpoints)
            {
                //calculate the area: the Gridsize after the points Position
                int xmin = rasterpoint.DepthX;
                int ymin = rasterpoint.DepthY;

                int xmax = rasterpoint.DepthX + SettingsManager.RecognitionSet.ObjectRecognitionGridSpacing;
                if (xmax >= image.Width)
                    xmax = image.Width - 1;

                int ymax = rasterpoint.DepthY + SettingsManager.RecognitionSet.ObjectRecognitionGridSpacing;
                if (ymax >= image.Height)
                    ymax = image.Height - 1;

                TRectangle area = new TRectangle(xmin, ymin, xmax, ymax);

                //Now calculate the values for every point in the area
                CalculateNeigbourValues(boolmap, area, ref neighbourmap, image.Width, image.Height);
            }

            return neighbourmap;
        }
        
        /// <summary>
        /// Calculates the neigbour-values for a given
        /// </summary>
        /// <param name="boolmap"></param>
        /// <param name="area"></param>
        /// <param name="neigbourmap"></param>
        /// <param name="ImageWidth"></param>
        /// <param name="ImageHeight"></param>
        private static void CalculateNeigbourValues(bool[,] boolmap, TRectangle area, ref int[,,] neigbourmap, int ImageWidth, int ImageHeight)
        {
            //For every Point in the given Area
            for (int x = area.X; x <= area.X2; x++)
            {
                for (int y = area.Y; y <= area.Y2; y++)
                {
                    bool work = true;
                    int rectangleSize = 0;
                    while (work)
                    {
                        //Expand the rectangle
                        if (rectangleSize > 0)
                            rectangleSize += SettingsManager.RecognitionSet.ObjectRecognitionRectIncrease;
                        else
                            rectangleSize = SettingsManager.RecognitionSet.ObjectMinimalRadius;

                        TRectangle workrectancle = new TRectangle(x, y, rectangleSize, true, new TRectangle(0, 0, ImageWidth-1, ImageHeight-1));

                        //Count the neigbours
                        int true_n = 0;
                        int false_n = 0;

                        for (int xn = workrectancle.X; xn <= workrectancle.X2; xn++)
                        {
                            for (int yn = workrectancle.Y; yn <= workrectancle.Y2; yn++)
                            {
                                if (boolmap[xn, yn])
                                    true_n++;
                                else
                                    false_n++;
                            }
                        }

                        //Calculate the percentage of false-neigbours
                        double false_percentage = (100/(true_n + false_n))*false_n;
                        false_percentage = false_percentage / 100.0;

                        //break if the percentage is reached or the maximal object size is reached (or a fixed programmed value - for
                        if (false_percentage >= SettingsManager.RecognitionSet.ObjectRecognitionNeighbourcountThreshold ||
                            workrectancle.Width >= SettingsManager.RecognitionSet.ObjectMaximalRadius )
                        {
                            work = false;
                            //write the values to the array
                            neigbourmap[x, y, 0] = true_n;
                            neigbourmap[x, y, 1] = workrectancle.Width; // == Height

                            //Special case: object is too small (not enought neigbours with the lowest rectangle size) --> don't save values
                            if (rectangleSize == SettingsManager.RecognitionSet.ObjectMinimalRadius)
                                neigbourmap[x, y, 0] = 0;
                        }
                    }
                }
            }

            
        }

        /// <summary>
        /// Check in a raster whether there are any true pixels
        /// </summary>
        /// <param name="prepared_boolmap"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private static List<TPoint> GetTrueRasterPoints(bool[,] prepared_boolmap, DepthImage image)
        {
            List<TPoint> TrueRasterPoints = new List<TPoint>();

            for (int x = 0; x < image.Width; x = x + SettingsManager.RecognitionSet.ObjectRecognitionGridSpacing)
            {
                for (int y = 0; y < image.Height; y = y + SettingsManager.RecognitionSet.ObjectRecognitionGridSpacing)
                {
                    if (prepared_boolmap[x, y])
                        TrueRasterPoints.Add(new TPoint(x, y, TPoint.PointCreationType.depth));
                }
            }
            return TrueRasterPoints;
        }
    }
}
