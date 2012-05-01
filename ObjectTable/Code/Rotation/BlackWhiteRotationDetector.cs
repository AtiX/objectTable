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
    class BlackWhiteRotationDetector : RotationDetector
    {

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

            //Start 4 threads, only if >= 4 threads
            if (objects >= 4)
            {
                _threadcount = 4;
                resetEvents = new ManualResetEvent[4];

                for (int i=4; i > 0; i--)
                {
                    Thread t1 = new Thread(new ParameterizedThreadStart(Work));
                    t1.Name = "ObjectTable RotationDetection Thread #" + i.ToString();
                    //Copy 1/i*objectcount of the objects to the new thread
                    int amount = (int) Math.Round((1.0/i)*objects);
                    TableObject[] threadobjects = new TableObject[amount];
                    ObjectsWithBitmap.CopyTo(index, threadobjects, 0, amount);
                    objects -= amount;
                    index += amount;

                    resetEvents[i-1] = new ManualResetEvent(false);
                    t1.Start(new object[] {threadobjects,resetEvents[i-1]});
                }

                WaitHandle.WaitAll(resetEvents);
            }
            else
            {
                //Single thread
                _threadcount = 1;

                ManualResetEvent manualReset = new ManualResetEvent(false);

                Thread t = new Thread(new ParameterizedThreadStart(Work));
                t.Name = "ObjectTable RotationDetection Thread -";
                t.Start(new object[] {ObjectsWithBitmap.ToArray(),manualReset});

                manualReset.WaitOne();
            }

            //Performance 
            this.RotationDetectionDuration = (int) Math.Round((DateTime.Now - start).TotalMilliseconds);
            return ObjectList;
        }

        //The thread and rotation detection
        public void Work(object data)
        {
            TableObject[] objects = (TableObject[])((object[])data)[0];
            ManualResetEvent mreset = (ManualResetEvent) ((object[]) data)[1];

            foreach (TableObject obj in objects)
            {
                //First, copy the lumiosity-values to a short array 
                short[,] greyArray = new short[obj.ExtractedBitmap.Width,obj.ExtractedBitmap.Height];
                int average = 0;
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
                average = (int) Math.Round((double) average/(obj.ExtractedBitmap.Width*obj.ExtractedBitmap.Height));

                //calculate thresholds for white / black
                int thresholdWhite = (int) Math.Round((double)average + 0.3*average);
                int thresholdBlack = (int) Math.Round((double)average - 0.3*average);

                //DEBUG: save the picture image, with thresholds applied
                if (SettingsManager.RecognitionSet.SaveDebugRotationBitmaps)
                {
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

                //Calculate DirectionVectors
                List<Vector> vectorlist = LinesVertically(greyArray, obj.ExtractedBitmap.Width, obj.ExtractedBitmap.Height, thresholdBlack, thresholdWhite);
                List<Vector> vectorlist2 = LinesHorizontally(greyArray, obj.ExtractedBitmap.Width, obj.ExtractedBitmap.Height, thresholdBlack, thresholdWhite);
                vectorlist.AddRange(vectorlist2);

                //Only calculate if at least 4 vectors
                if (vectorlist.Count > 0)
                {
                    //Calculate the "average"
                    Vector averageVector = CreateAverageVector(vectorlist);
                    //set the length to 1
                    averageVector = ScreenMathHelper.RescaleVector(averageVector, 1.0);
                    obj.DirectionVector = averageVector;
                    obj.RotationDefined = true;
                }
            }

            //The thread has finished - signalize this to the main thread
            mreset.Set();
        }

        public List<Vector> LinesVertically(short[,] greyArray, int Width, int Height, int blackThreshold, int whiteThreshold)
        {
            //Create a List for the vectors
            List<Vector> vectorList = new List<Vector>();
            Point lastPoint = new Point(0,0);

            //lines must have a certain distance:
            const int lineDistance = 5;

            //how many lines fit in the image
            int lineCount = (int)Math.Floor((double)(Width-1)/lineDistance);

            for (int iLine = 0; iLine <= lineCount; iLine++)
            {
                int x = iLine*lineDistance;

                bool blackFirst = false;
                bool countStarted = false;
                bool secondHalf = false;
                int TransitionPositionY = 0;
                int countA = 0;
                int countMiddle = 0;
                int countB = 0;

                for (int y=0; y<Height;y++)
                {
                    //start of new counting process: 
                    if (!countStarted)
                    {
                        if (greyArray[x, y] <= blackThreshold)
                        {
                            blackFirst = true;
                            countA++;
                            countStarted = true;
                        }
                        else if (greyArray[x, y] >= whiteThreshold)
                        {
                            blackFirst = false;
                            countA++;
                            countStarted = true;
                        }
                    }
                    else
                    {
                            //first or second half?
                            if (!secondHalf)
                            {
                                //first half
                                //did we start with black
                                if (blackFirst)
                                {
                                    //is the pixel black?
                                    if (greyArray[x, y] <= blackThreshold)
                                        countA++;
                                    else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                        countMiddle++;
                                    else
                                    {
                                        //it's white -> switch to second half
                                        countB++;
                                        secondHalf = true;
                                        TransitionPositionY = y - countMiddle/2;
                                    }
                                }
                                else
                                {
                                    //we did start with white pixels
                                    if (greyArray[x, y] >= whiteThreshold)
                                        countA++;
                                    else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                        countMiddle++;
                                    else
                                    {
                                        //it's black -> switch to second half
                                        countB++;
                                        secondHalf = true;
                                        TransitionPositionY = y - countMiddle / 2;
                                    }
                                }
                            }
                            else
                            {
                                //we are in the second half
                                //did we start with black
                                if (blackFirst)
                                {
                                    //we started with black pixels
                                    //is the pixel white
                                    if (greyArray[x, y] >= whiteThreshold)
                                        countB++;
                                    else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                    {
                                        //change again, check whether there are enought pixels to create point
                                        if (CheckAndCreatePoint(x, TransitionPositionY, vectorList, ref lastPoint, countA, countMiddle, countB,blackFirst,false)) break;
                                        //set the position back to the begin of the second color (so that a following transition can be recognized
                                        y = y - countB;
                                        //reset 
                                        countA = 0;
                                        countB = 0;
                                        countStarted = false;
                                        secondHalf = false;
                                        countMiddle = 0;
                                    }
                                    else if (greyArray[x,y] <= blackThreshold)
                                    {
                                        //change again, check whether there are enought pixels to create point
                                        if (CheckAndCreatePoint(x, TransitionPositionY, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst, false)) break;
                                        //set the position back to the begin of the second color (so that a following transition can be recognized
                                        y = y - countB;
                                        //reset
                                        countA = 0;
                                        countB = 0;
                                        countStarted = false;
                                        secondHalf = false;
                                        countMiddle = 0;
                                    }
                                }
                                else
                                {
                                    //we did start with white
                                    //is the pixel black?
                                    if (greyArray[x, y] <= blackThreshold)
                                        countB++;
                                    else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                    {
                                        //change again, check whether there are enought pixels to create point
                                        if (CheckAndCreatePoint(x, TransitionPositionY, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst,false)) break;
                                        //set the position back to the begin of the second color (so that a following transition can be recognized
                                        y = y - countB;
                                        //reset
                                        countA = 0;
                                        countB = 0;
                                        countStarted = false;
                                        secondHalf = false;
                                        countMiddle = 0;
                                    }
                                    else if (greyArray[x, y] >= whiteThreshold)
                                    {
                                        //change again, check whether there are enought pixels to create point
                                        if (CheckAndCreatePoint(x, TransitionPositionY, vectorList, ref lastPoint, countA, countMiddle, countB,blackFirst,false)) break;
                                        //set the position back to the begin of the second color (so that a following transition can be recognized
                                        y = y - countB;
                                        //reset
                                        countA = 0;
                                        countB = 0;
                                        countStarted = false;
                                        secondHalf = false;
                                        countMiddle = 0;
                                    }
                                }
                            }
                    }
                }

                //Check last pixels (if there wasn't any check before)
                if (secondHalf)
                    CheckAndCreatePoint(x, TransitionPositionY, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst, false);
            }
            return vectorList;
        }

        public List<Vector> LinesHorizontally(short[,] greyArray, int Width, int Height, int blackThreshold, int whiteThreshold)
        {
            //Create a List for the vectors
            List<Vector> vectorList = new List<Vector>();
            Point lastPoint = new Point(0, 0);

            //lines must have a certain distance:
            int lineDistance = 5;

            //how many lines fit in the image
            int lineCount = (int)Math.Floor((double)(Height-1) / lineDistance);

            for (int iLine = 0; iLine <= lineCount; iLine++)
            {
                int y = iLine * lineDistance;

                bool blackFirst = false;
                bool countStarted = false;
                bool secondHalf = false;
                int TransitionPositionX = 0;
                int countA = 0;
                int countMiddle = 0;
                int countB = 0;

                for (int x = 0; x < Width; x++)
                {
                    //start of new counting process: 
                    if (!countStarted)
                    {
                        if (greyArray[x, y] <= blackThreshold)
                        {
                            blackFirst = true;
                            countStarted = true;
                        }
                        else if (greyArray[x, y] >= whiteThreshold)
                        {
                            blackFirst = false;
                            countStarted = true;
                        }
                    }
                    else
                    {
                        //first or second half?
                        if (!secondHalf)
                        {
                            //first half
                            //did we start with black
                            if (blackFirst)
                            {
                                //is the pixel black?
                                if (greyArray[x, y] <= blackThreshold)
                                    countA++;
                                else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                    countMiddle++;
                                else
                                {
                                    //it's white -> switch to second half
                                    countB++;
                                    secondHalf = true;
                                    TransitionPositionX = x - countMiddle / 2;
                                }
                            }
                            else
                            {
                                //we did start with white pixels
                                if (greyArray[x, y] >= whiteThreshold)
                                    countA++;
                                else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                    countMiddle++;
                                else
                                {
                                    //it's black -> switch to second half
                                    countB++;
                                    secondHalf = true;
                                    TransitionPositionX = x - countMiddle / 2;
                                }
                            }
                        }
                        else
                        {
                            //we are in the second half
                            //did we start with black
                            if (blackFirst)
                            {
                                //we started with black pixels
                                //is the pixel white
                                if (greyArray[x, y] >= whiteThreshold)
                                    countB++;
                                else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                {
                                    //change again, check whether there are enought pixels to create point
                                    if (CheckAndCreatePoint(TransitionPositionX, y,vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst,true)) break;
                                    //set the position back to the begin of the second color (so that a following transition can be recognized
                                    x = x - countB;
                                    //reset
                                    countA = 0;
                                    countB = 0;
                                    countStarted = false;
                                    secondHalf = false;
                                    countMiddle = 0;
                                }
                                else if (greyArray[x, y] <= blackThreshold)
                                {
                                    //change again, check whether there are enought pixels to create point
                                    if (CheckAndCreatePoint(TransitionPositionX, y, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst,true )) break;
                                    //set the position back to the begin of the second color (so that a following transition can be recognized
                                    x = x - countB;
                                    //reset
                                    countA = 0;
                                    countB = 0;
                                    countStarted = false;
                                    secondHalf = false;
                                    countMiddle = 0;
                                }
                            }
                            else
                            {
                                //we did start with white
                                //is the pixel black?
                                if (greyArray[x, y] <= blackThreshold)
                                    countB++;
                                else if ((greyArray[x, y] > blackThreshold) && (greyArray[x, y] < whiteThreshold))
                                {
                                    //change again, check whether there are enought pixels to create point
                                    if (CheckAndCreatePoint(TransitionPositionX, y, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst,true )) break;
                                    //set the position back to the begin of the second color (so that a following transition can be recognized
                                    x = x - countB;
                                    //reset
                                    countA = 0;
                                    countB = 0;
                                    countStarted = false;
                                    secondHalf = false;
                                    countMiddle = 0;
                                }
                                else if (greyArray[x, y] >= whiteThreshold)
                                {
                                    //change again, check whether there are enought pixels to create point
                                    if (CheckAndCreatePoint(TransitionPositionX, y, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst, true)) break;
                                    //set the position back to the begin of the second color (so that a following transition can be recognized
                                    x = x - countB;
                                    //reset
                                    countA = 0;
                                    countB = 0;
                                    countStarted = false;
                                    secondHalf = false;
                                    countMiddle = 0;
                                }
                            }
                        }
                    }
                }

                //Check last pixels (if there wasn't any check before)
                if (secondHalf)
                    CheckAndCreatePoint(TransitionPositionX,y, vectorList, ref lastPoint, countA, countMiddle, countB, blackFirst, true);
            }
            return vectorList;
        }

        private static bool CheckAndCreatePoint(int x, int TransitionPositionY, List<Vector> vectorlist,ref Point lastPoint, int countA, int countMiddle,
                                                int countB, bool blackFirst, bool inverse)
         {
            //Transition does fit, and one point does already exist
            if ((countA >= 5) && (countMiddle <= 2) && (countB >= 5)&&(!lastPoint.Equals(new Point(0,0))))
            {
                //Create vector
                Point p = new Point(x, TransitionPositionY);
                if (!inverse)
                {
                    //The vector is aligned, so that the white side is on the left (looking in the vectors direction)
                    if (blackFirst)
                    {
                        Vector v = new Vector(p.X - lastPoint.X, p.Y - lastPoint.Y);
                        vectorlist.Add(v);
                    }
                    else
                    {
                        Vector v = new Vector(lastPoint.X - p.X, lastPoint.Y - p.Y);
                        vectorlist.Add(v);
                    }
                }
                else
                {
                    //Inverse the alingnement (because the direction would be the opposite with the vertical methods
                    if (blackFirst)
                    {
                        Vector v = new Vector(lastPoint.X - p.X, lastPoint.Y - p.Y);
                        vectorlist.Add(v);
                    }
                    else
                    {
                       Vector v = new Vector(p.X - lastPoint.X, p.Y - lastPoint.Y);
                       vectorlist.Add(v);
                    }
                }
                lastPoint = new Point(x, TransitionPositionY);
                return true;
            }
            else if ((countA >= 5) && (countMiddle <= 2) && (countB >= 5))
            {
                //The transition does fit, but no point exists: create point
                lastPoint = new Point(x, TransitionPositionY);
            }
            return false;
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
            Vector average = Average(vectorList);

            //Calculate the standard deviation
            double stdDeviation = StdDeviationInDegree(vectorList, average);

            //throw out every vector that doesn't fit into the StdDeviation
            List<Vector> cleanedVectors = new List<Vector>();
            foreach (Vector test in vectorList)
            {
                double diff = DegreeDifference(average, test);
                //if (diff <= stdDeviation)
                //{
                    cleanedVectors.Add(test);
                //}
            }

            //Finally, return the average of the cleaned vectors
            return Average(cleanedVectors);
        }

        private Vector Average(List<Vector> vectorList)
        {
            Vector v = new Vector();
            foreach (Vector a in vectorList)
            {
                //Equal length
                Vector tmp = ScreenMathHelper.RescaleVector(a, 1.0);
                v.X += tmp.X;
                v.Y += tmp.Y;
            }
            v.X = v.X/vectorList.Count();
            v.Y = v.Y/vectorList.Count();
            return v;
        }

        private double StdDeviationInDegree(List<Vector> vectorList, Vector average)
        {
            double sum = 0.0;
            foreach (Vector t in vectorList)
            {
                double difference = DegreeDifference(t, average);
                sum += difference * difference;
            }

            return Math.Sqrt(sum/vectorList.Count);
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
