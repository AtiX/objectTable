using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// Seperates various Objects on a Table from the depth image
    /// </summary>
    class ObjectSeperator
    {
        public List<TableObject> SeperateObjects(ref DepthImage image, out bool[,] boolmap_object, out int[,,] neighbourmap)
        {
            //Create 2 seperate depth-images, remove the objects (for hand recognition) and the hand (for object recognition)
            DepthImage HandImage = image.Clone();
            DepthImage ObjectImage = image.Clone();
            SplitUpDepthImages(ref HandImage, ref ObjectImage);

            //For object recognition, create a BooleanXY-Map (true for all objects with a height > 0)
            bool[,] boolmap_hand = CreateBoolMap(HandImage);
            boolmap_object = CreateBoolMap(ObjectImage);

            if (SettingsManager.RecognitionSet.SaveDebugMaps)
            {
                Bitmap dm_obj = MapVisualizer.VisualizeDepthImage(ObjectImage, false, false);
                dm_obj.Save("ObjectDepthImage.bmp");
                Bitmap dm_h = MapVisualizer.VisualizeDepthImage(HandImage, false, false);
                dm_h.Save("HandDepthImage.bmp");
                Bitmap bobj = MapVisualizer.VisualizeBoolMap(boolmap_hand, image.Width, image.Height);
                bobj.Save("boolmap_hand.bmp");
                Bitmap bmpo = MapVisualizer.VisualizeBoolMap(boolmap_object, image.Width, image.Height);
                bmpo.Save("boolmap_object.bmp");
                Bitmap ci = MapVisualizer.VisualizeDepthImage(image, false, false);
                ci.Save("normalized_depthimage.bmp");
            }

            //Run the Hand Recognition
            HandRecognizer hrec = new HandRecognizer();
            HandObject hand = hrec.RecognizeHands(ref boolmap_hand, image.Width, image.Height);

            //RunTheObjectRecognition
            ObjectRecognizer orec = new ObjectRecognizer();
            List<TableObject> tableObjects = orec.RecognizeObjects(boolmap_object, image, out neighbourmap);

            //If there is a hand, add it to the TableObjects
            if (hand != null)
                tableObjects.Add(hand);

            //Return the ObjectList
            return tableObjects; 
        }

        private void SplitUpDepthImages(ref DepthImage HandImage, ref DepthImage objectImage)
        {
            for (int x=0;x<HandImage.Width;x++)
            {
                for(int y=0;y<HandImage.Height;y++)
                {
                    int heigt = HandImage.Data[x, y];
                    if (HandImage.Data[x, y] > SettingsManager.RecognitionSet.HandMaximalHeight || HandImage.Data[x, y] <= SettingsManager.RecognitionSet.ObjectMaximalHeight)
                        HandImage.Data[x, y] = 0;

                    if (objectImage.Data[x, y] > SettingsManager.RecognitionSet.ObjectMaximalHeight)
                        objectImage.Data[x, y] = 0;
                }
            }
        }

        /// <summary>
        /// Creates a boolmap out of an depth image. Every pixel that has an height > 0 is set to true
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private bool[,] CreateBoolMap(DepthImage source)
        {
            bool[,] boolmap = new bool[source.Width,source.Height];

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (source.Data[x,y] > 0)
                    {
                        boolmap[x, y] = true;
                    }
                    else
                    {
                        boolmap[x, y] = false;
                    }
                }
            }

            return boolmap;
        }
    }
}
