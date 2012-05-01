using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// This class assigns an extract of the videobitmap to the Tableobjects
    /// </summary>
    class ObjectVideoBitmapAssigner
    {
        public List<TableObject>AssignVideoBitmap(List<TableObject> objects, Bitmap videoBitmap)
        {
            Bitmap localbitmap = new Bitmap(videoBitmap);
            foreach (TableObject obj in objects.Where(obj => obj.CenterDefined))
            {
                //Calculate the position of the Color-Coords
                obj.Center = PositionMapper.GetColorCoordinatesfromDepth(obj.Center, true,
                                                                         SettingsManager.RecognitionSet.TableDistance - obj.Height);
                //Cut out an extract of the videoImage
                Bitmap extract = new Bitmap(obj.Radius + SettingsManager.RecognitionSet.ObjectVideoBitmapEnlargement, obj.Radius + SettingsManager.RecognitionSet.ObjectVideoBitmapEnlargement);

                //Create area rectangle
                int width = 0, height = 0;
                SettingsManager.KinectSet.GetVideoResolution(out width, out height);

                TRectangle area = new TRectangle(obj.Center.ColorX, obj.Center.ColorY, obj.Radius + SettingsManager.RecognitionSet.ObjectVideoBitmapEnlargement, true,
                                                 new TRectangle(0, 0, width-1, height-1));

                //Copy Color pixels
                for (int x = area.X; x < area.X2; x++)
                {
                    for (int y = area.Y; y < area.Y2; y++)
                    {
                        Color col = localbitmap.GetPixel(x, y);
                        extract.SetPixel(x - area.X, y - area.Y, col);
                    }
                }

                //Assign bitmap
                obj.ExtractedBitmap = extract;
            }

            return objects;
        }
    }
}
