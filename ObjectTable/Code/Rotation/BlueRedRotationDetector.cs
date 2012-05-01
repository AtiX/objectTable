using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Rotation
{
    class BlueRedRotationDetector : RotationDetector
    {
        public override List<TableObject> DetectRotation(List<TableObject> ObjectList)
        {
            foreach (TableObject obj in ObjectList.Where(o => o.ExtractedBitmap != null))
            {
                CalculateRotation(obj);
            }

            return ObjectList;
        }

        private void CalculateRotation(TableObject obj)
        {
            TPoint rPoint = null, bPoint = null;
            //calculate the average values
            int x = 0, y = 0;
            int avRed=0, avGreen=0, avBlue=0;
            for (x = 0; x < obj.ExtractedBitmap.Width - 2; x++)
            {
                for (y = 0; y < obj.ExtractedBitmap.Height - 2; y++)
                {
                    Color c = obj.ExtractedBitmap.GetPixel(x, y);
                    avRed += c.R;
                    avGreen += c.G;
                    avBlue += c.B;
                }
            }
            int Pixels = obj.ExtractedBitmap.Height*obj.ExtractedBitmap.Width;
            avRed = (int) Math.Round((double)avRed / Pixels);
            avGreen = (int)Math.Round((double)avGreen / Pixels);
            avBlue = (int)Math.Round((double)avBlue / Pixels);

            //check for every suitable point, whether the point itself and at least 7 direct neighbours are red
            x = 1;
            y = 1;

            //The Points at the border have not enought direct neighbours
            for (x = 1; x < obj.ExtractedBitmap.Width-2; x++)
            {
                for (y = 1; y < obj.ExtractedBitmap.Height - 2; y++ )
                {
                    if (rPoint == null)
                    {
                        bool red = true;
                        int count = CountPOints(obj, red, y, x,avRed,avBlue,avGreen);
                        if (count >= 7)
                            rPoint = new TPoint(x, y, TPoint.PointCreationType.screen);
                    }
                }
            }

            //Same for blue point
            for (x = 1; x < obj.ExtractedBitmap.Width - 2; x++)
            {
                for (y = 1; y < obj.ExtractedBitmap.Height - 2; y++)
                {
                    if (bPoint == null)
                    {
                        bool red = false;
                        int count = CountPOints(obj, red, y, x, avRed, avBlue, avGreen);
                        if (count >= 7)
                            bPoint = new TPoint(x, y, TPoint.PointCreationType.screen);
                    }
                }
            }

            //Were two points recognized?
            if ((rPoint == null) || (bPoint == null))
                return;

            //Calculate the vector
            x = rPoint.ScreenX - bPoint.ScreenX;
            y = rPoint.ScreenY - bPoint.ScreenY;
            obj.DirectionVector = new System.Windows.Vector(x, y);
            obj.RotationDefined = true;
        }

        private int CountPOints(TableObject obj, bool red, int y, int x, int avRed, int avBlue, int avGreen)
        {
            int count = 0;
            Color c;

            //Check 9 points (1 point and 8 neighbours)
            c = obj.ExtractedBitmap.GetPixel(x, y);
            if (CheckThreshold(c, red,avRed,avBlue,avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x + 1, y);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x - 1, y);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x, y + 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x, y - 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x - 1, y - 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x + 1, y + 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x - 1, y + 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;

            c = obj.ExtractedBitmap.GetPixel(x + 1, y - 1);
            if (CheckThreshold(c, red, avRed, avBlue, avGreen))
                count++;
            return count;
        }

        private bool CheckThreshold(Color cl, bool red, int avRed, int avBlue, int avGreen)
        {
            int r, g, b;

            r = cl.R;
            g = cl.G;
            b = cl.B;

            if (red)
            {
                if (g < 0.5 * avGreen)
                    return true;
            }
            else
            {
                //check blue
                if ((b > avBlue) && (r < 0.7*avRed) && (g < avGreen))
                    return true;
            }
            return false;

        }
    }
}
