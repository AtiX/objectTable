using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition
{
    class HandRecognizer
    {
        /// <summary>
        /// Recognizes Hands/Arms by checking the position of hand-pixels on the screen. the pixel that is nearest to the image's center is the point where the hand points at
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public HandObject RecognizeHands(ref bool[,] boolmap, int width, int height)
        {
            //First: Check whether there are any hand-pixels
            if (CheckForHandPixels(boolmap, width, height) == false)
                return null;

            //Now go from the center of the screen (in circles)
            return CheckWithCircles(boolmap, width, height);
        }

        private HandObject CheckWithCircles(bool[,] boolmap, int width, int height)
        {
            //The middle of the image
            int MiddleX = width/2;
            int MiddleY = height/2;

            int radius = 0;
            //The maximal radius: the diagonal-length / 2
            int maxRadius = (int)Math.Round(Math.Sqrt(width*width + height*height) / 2);

            for (radius = 0; radius <= maxRadius; radius ++)
            {
                //Now check every point on the current circle (around the middle point)
                //every point means to calculate the coords for every angle
                for (double angle = 0; angle < 2*Math.PI; angle = angle + SettingsManager.RecognitionSet.HandRecognitionDeltaAngle)
                {
                    int pointX = (int) Math.Round(Math.Sin(angle)*radius);
                    int pointY = (int) Math.Round(Math.Cos(angle)*radius);

                    //Check whether the coords are valid
                    if (!(pointX > width || pointX < 0 || pointY > height || pointY < 0))
                    {
                        //if true, return as a hand object
                        if (boolmap[pointX,pointY])
                        {
                            HandObject hand = new HandObject();
                            hand.PointsAt = new TPoint(pointX, pointY, TPoint.PointCreationType.depth);
                            return hand;
                        }
                    }
                }
            }

            return null; //in case no point is found
        }

        private bool CheckForHandPixels(bool[,] boolmap, int width, int height)
        {
            for (int x=0; x< width; x++)
            {
                for (int y=0; y < height; y++)
                {
                    if (boolmap[x, y])
                        return true;
                }
            }
            return false;
        }
    }
}
