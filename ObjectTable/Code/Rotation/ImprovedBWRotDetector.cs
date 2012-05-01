using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;
using Point = System.Drawing.Point;

namespace ObjectTable.Code.Rotation
{
    class ImprovedBWRotDetector : RotationDetector
    {

        private enum EPixelColor
        {
            Black,
            InBetween,
            White
        };

        public override List<TableObject> DetectRotation(List<TableObject> ObjectList)
        {
            DateTime start = DateTime.Now;

            //No bitmaps -> no rotation detection
            if (ObjectList.Where(o => o.ExtractedBitmap != null).Count() == 0)
                return ObjectList;

            List<TableObject> ObjectsWithBitmap = ObjectList.Where(o => o.ExtractedBitmap != null).ToList();
            int objects = ObjectsWithBitmap.Count();
            int index = 0;

            int _threadcount = 0;
            ManualResetEvent[] resetEvents;

            //Start 2 threads, only if enough objects (starting threads is very expensive)
            if (objects >= SettingsManager.RotationRecSet.MaxObjectsPerThread)
            {
                _threadcount = 2;
                resetEvents = new ManualResetEvent[2];

                for (int i = _threadcount; i > 0; i--)
                {
                    Thread t1 = new Thread(new ParameterizedThreadStart(Work));
                    t1.Name = "ObjectTable ImprovedRotationDetector Thread #" + i.ToString();
                    //Copy 1/i*objectcount of the objects to the new thread
                    int amount = (int)Math.Round((1.0 / i) * objects);
                    TableObject[] threadobjects = new TableObject[amount];
                    ObjectsWithBitmap.CopyTo(index, threadobjects, 0, amount);
                    objects -= amount;
                    index += amount;

                    resetEvents[i - 1] = new ManualResetEvent(false);
                    t1.Start(new object[] { threadobjects, resetEvents[i - 1] });
                }

                WaitHandle.WaitAll(resetEvents);
            }
            else
            {
                //Single thread - do not start a new thread
                _threadcount = 1;

                //ManualResetEvent manualReset = new ManualResetEvent(false);

                //Thread t = new Thread(new ParameterizedThreadStart(Work));
                //t.Name = "ObjectTable ImprovedRotationDetector Thread -";
                //t.Start(new object[] { ObjectsWithBitmap.ToArray(), manualReset });
                Work(new object[] { ObjectsWithBitmap.ToArray(), null });
                //manualReset.WaitOne();
            }

            //Performance 
            this.RotationDetectionDuration = (int)Math.Round((DateTime.Now - start).TotalMilliseconds);
            return ObjectList;
        }

        //The thread and rotation detection
        public void Work(object data)
        {
            TableObject[] objects = (TableObject[])((object[])data)[0];
            ManualResetEvent mreset = (ManualResetEvent)((object[])data)[1];

            foreach (TableObject obj in objects)
            {
                //First, copy the lumiosity-values to a short array 
                int average;
                var greyArray = ConvertToGreyArray(obj, out average);
                average = (int)Math.Round((double)average / (obj.ExtractedBitmap.Width * obj.ExtractedBitmap.Height));

                //calculate thresholds for white / black
                int thresholdWhite = (int)Math.Round((double)average + SettingsManager.RotationRecSet.whiteThresholdPercentage * average);
                int thresholdBlack = (int)Math.Round((double)average - SettingsManager.RotationRecSet.blackThresholdPercentage * average);

                //DEBUG: save the picture image, with thresholds applied
                if (SettingsManager.RecognitionSet.SaveDebugRotationBitmaps)
                {
                    SaveDebugBitmap(thresholdBlack, greyArray, thresholdWhite, obj);
                }

                //Calculate DirectionVectors
                List<Vector> verticalVectors = GenerateVectors(greyArray, obj.ExtractedBitmap.Width,obj.ExtractedBitmap.Height, true, thresholdBlack,thresholdWhite);
                List<Vector> horizontalVectors = GenerateVectors(greyArray, obj.ExtractedBitmap.Width, obj.ExtractedBitmap.Height, false, thresholdBlack, thresholdWhite);

                verticalVectors.AddRange(horizontalVectors);

                //Only calculate if at least 4 vectors
                if (verticalVectors.Count > 0)
                {
                    //Calculate the "average"
                    Vector averageVector = CreateAverageVector(verticalVectors);
                    //set the length to 1
                    averageVector = ScreenMathHelper.RescaleVector(averageVector, 1.0);
                    obj.DirectionVector = averageVector;
                    obj.RotationDefined = true;
                }
            }

            //The thread has finished - signalize this to the main thread
            if (mreset != null)
                mreset.Set();
        }

        private List<Vector> GenerateVectors(short[,] greyArray, int width, int height, bool Vertical, int blackThreshold, int whiteThreshold)
        {
            //The Distance of the two vector creating lines:
            int maxLineDistance = 6;
            int minLineDistance = 3;

            List<Vector> vectorlist = new List<Vector>();

            //image too small?
            if ((width < maxLineDistance) || (height < maxLineDistance))
                return vectorlist;

            int positionMax = 0;
            if (Vertical)
                positionMax = width - maxLineDistance;
            else
                positionMax = height - maxLineDistance;

            int maxValue = 0;
            if (Vertical)
                maxValue = height - 1;
            else
                maxValue = width - 1;

            for (int position = 0; position < positionMax; position++)
            {
                for (int position2 = position + minLineDistance; position2 < position + maxLineDistance; position2++)
                {
                    //Get two transitionPoints
                    bool blackFirst1;
                    Point p1 = GetLineTransitionPoint(greyArray, position, maxValue, Vertical, blackThreshold,
                                                      whiteThreshold, out blackFirst1);
                    bool blackFirst2;
                    Point p2 = GetLineTransitionPoint(greyArray, position2, maxValue, Vertical,
                                                      blackThreshold,
                                                      whiteThreshold, out blackFirst2);

                    //Two points that have transitions in the same direction? if not, skip the vector creation
                    if (!((!p1.Equals(new Point(0, 0)) && !p2.Equals(new Point(0, 0))) && (blackFirst1 == blackFirst2)))
                        continue;

                    Vector v;

                    //create vector depending on the transition direction
                    if (blackFirst1)
                    {
                        v = new Vector(p1.X - p2.X, p1.Y - p2.Y);
                    }
                    else
                    {
                        v = new Vector(p2.X - p1.X, p2.Y - p1.Y);
                    }

                    //if vertical, the vector has to be inverted 
                    if (Vertical)
                        v = v*(-1);

                    //add to list 
                    vectorlist.Add(v);
                }
            }

            return vectorlist;
        }

        private static void SaveDebugBitmap(int thresholdBlack, short[,] greyArray, int thresholdWhite, TableObject obj)
        {
            int x,y;
            Bitmap bmp = new Bitmap(obj.ExtractedBitmap.Width, obj.ExtractedBitmap.Height);
            for (x = 0; x < bmp.Width; x++)
            {
                for (y = 0; y < bmp.Width; y++)
                {
                    if (greyArray[x, y] >= thresholdWhite)
                        bmp.SetPixel(x, y, Color.White);
                    else if (greyArray[x, y] <= thresholdBlack)
                        bmp.SetPixel(x, y, Color.Black);
                    else
                        bmp.SetPixel(x, y, Color.Lime);
                }
            }
            bmp.Save("bwrotation_" + obj.ObjectID.ToString() + ".bmp");
        }

        private static short[,] ConvertToGreyArray(TableObject obj, out int average)
        {
            short[,] greyArray = new short[obj.ExtractedBitmap.Width,obj.ExtractedBitmap.Height];
            average = 0;
            int x, y;
            for (x = 0; x < obj.ExtractedBitmap.Width; x++)
            {
                for (y = 0; y < obj.ExtractedBitmap.Height; y++)
                {
                    Color c = obj.ExtractedBitmap.GetPixel(x, y);
                    greyArray[x, y] = (short) Math.Round(0.299*c.R + 0.587*c.G + 0.144*c.B);
                    average += (short) Math.Round(0.299*c.R + 0.587*c.G + 0.144*c.B);
                }
            }
            return greyArray;
        }

        private Point GetLineTransitionPoint(short[,] greyArray, int startValue, int maxValue, bool Vertical, int blackThreshold, int whiteThreshold, out bool blackFirst)
        {
            //Set coords
            int x = 0, y = 0;
            if (Vertical)//set x, increase y
                x = startValue;
            else//Horizontal: set y, increase x
                y = startValue;

            blackFirst = false;
            bool countStarted = false;
            bool secondHalf = false;
            Point transitionPoint = new Point(0,0);
            int countA = 0;
            int countMiddle = 0;
            int countB = 0;

            int currentPosition = 0;

            while (currentPosition <= maxValue)
            {
                //Get the pixel color
                EPixelColor pixelColor;
                //is the pixel black?
                if (greyArray[x, y] <= blackThreshold)
                    pixelColor = EPixelColor.Black;
                else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                {   //Settings for improved recognition on white backgrounds
                    if (SettingsManager.RotationRecSet.whiteBackground)
                        pixelColor = EPixelColor.White;
                    else
                        pixelColor = EPixelColor.InBetween;
                }
                else
                    pixelColor = EPixelColor.White;

                //increase pixel position for next round
                currentPosition++;
                if (Vertical)
                    y++;
                else
                    x++;

                //start of new counting process: (skip all undefined pixel) 
                if (!countStarted)
                {
                    if (pixelColor == EPixelColor.Black)
                    {
                        blackFirst = true; countA++; countStarted = true;
                    }
                    else if (pixelColor == EPixelColor.White)
                    {
                        blackFirst = false; countA++; countStarted = true;
                    }
                    continue;
                }

                //We are in a counting process!
                //check for black or white
                if (!secondHalf)
                {
                    //firsthalf
                    //count increase
                    if ((blackFirst && pixelColor == EPixelColor.Black)||(!blackFirst && pixelColor == EPixelColor.White))
                        countA++;
                    //change of color
                    else if ((blackFirst && pixelColor == EPixelColor.White) || (!blackFirst && pixelColor == EPixelColor.Black))
                    {
                        secondHalf = true;
                        countB++;
                    }
                    else //Transition begins
                        countMiddle++;
                }
                else
                {
                    //secondhalf   
                    //count increase
                    if ((blackFirst && pixelColor == EPixelColor.White) || (!blackFirst && pixelColor == EPixelColor.Black))
                        countB++;
                    else
                    {
                        //transition again! - check whether this is enough for a transition
                        if (CheckCriteria(countMiddle, countB, countA))
                        {
                            //calculate point position
                            transitionPoint = GetTransitionPoint(Vertical, countB, countMiddle, y, currentPosition, x);
                            //exit loop and return point
                            break;
                        }
                        else
                        {
                            //no valid transition - reset values to start searching for another transition again
                            countA = 0; countB = 0; countStarted = false; secondHalf = false; countMiddle = 0;
                        }
                    }
                }
            } // while

            //Check whether current data suffices for a point
            if (CheckCriteria(countMiddle, countB, countA))
            {
                //calculate point position
                transitionPoint = GetTransitionPoint(Vertical, countB, countMiddle, y, currentPosition, x);
            }

            return transitionPoint;
        }

        private static bool CheckCriteria(int countMiddle, int countB, int countA)
        {
            return (countA >= 5) && (countB >= 5) && (countMiddle <= 2);
        }

        private static Point GetTransitionPoint(bool Vertical, int countB, int countMiddle, int y, int currentPosition, int x)
        {
            Point p = new Point();
            if (Vertical) //increasing y
            {
                p.X = x;
                p.Y = currentPosition - (countB + countMiddle/2);
            }
            else //increasing x
            {
                p.X = currentPosition - (countB + countMiddle/2);
                p.Y = y;
            }
            return p;
        }

        /// <summary>
        /// Creates the average vector for the direction
        /// </summary>
        /// <param name="vectorList"></param>
        /// <returns></returns>
        private Vector CreateAverageVector(List<Vector> vectorList)
        {
            //First, remove all vectors with (0,0)
            List<Vector> cleanedlist = new List<Vector>();
            foreach (Vector v in vectorList)
            {
                if (v.Length > 0.001)
                    cleanedlist.Add(v);
            }
            vectorList = cleanedlist;

            //First, calculate the average vector
            return ScreenMathHelper.Average(vectorList);

            //surprisingly, it is more exact with bad values calculated...
            //Calculate the standard deviation
            //double stdDeviation = StdDeviationInDegree(vectorList, average);

            
            /*//throw out every vector that doesn't fit into the StdDeviation
            List<Vector> cleanedVectors = new List<Vector>();
            foreach (Vector test in vectorList)
            {
                double diff = DegreeDifference(average, test);
                //if (diff <= stdDeviation)
                //{
                cleanedVectors.Add(test);
                //}
            }*/

            //Finally, return the average of the cleaned vectors
            //return Average(cleanedVectors);
        }

        private double StdDeviationInDegree(List<Vector> vectorList, Vector average)
        {
            double sum = 0.0;
            foreach (Vector t in vectorList)
            {
                double difference = DegreeDifference(t, average);
                sum += difference * difference;
            }

            return Math.Sqrt(sum / vectorList.Count);
        }

        private double DegreeDifference(Vector a, Vector b)
        {
            double d1 = ScreenMathHelper.VectorToDegree(a);
            double d2 = ScreenMathHelper.VectorToDegree(b);

            //d1 has to be the "higher" degree value
            if (d1 < d2)
            {
                double tmp = d1;
                d1 = d2;
                d2 = tmp;
            }
            else if (Math.Abs(d1 - d2) < 0.0001)
            {
                return 0.0;
            }

            //calculate both differences (going left and right on the degree-circle)
            double diff1 = 360 - d1;
            diff1 += d2;

            double diff2 = d1 - d2;

            if (diff2 > diff1)
            {
                return diff1;
            }
            else
            {
                return diff2;
            }
        }
    }
}
