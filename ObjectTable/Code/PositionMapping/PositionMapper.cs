using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.PositionMapping
{
    /// <summary>
    /// This class is a helperclass. It provides methods to map the different XY-Coordinates (depth / color / screen) and convert them to each other
    /// </summary>
    public static class PositionMapper
    {
        private static KinectController _kinectController; 

        public static void AssignKinectController(KinectController kinect)
        {
            _kinectController = kinect;
        }

        public static TPoint GetColorCoordinatesfromDepth(TPoint depth_point, bool CutIntoBounds = false, int distanceFromSensor = -1)
        {
            //if the distance is not set (=-1), use the Table distance as an approximation to the real distance
            if (distanceFromSensor == -1)
                distanceFromSensor = SettingsManager.RecognitionSet.TableDistance;

            //if no runtime is assigned, return null
            if (_kinectController == null)
                return null;

            //Calculate the approx. Coordinates with help of the kinect
            try
            {
                _kinectController.GetColorPixelCoordinatesFromDepthPixel(depth_point.DepthX, depth_point.DepthY,
                                                                         (short) distanceFromSensor,
                                                                         out depth_point.ColorX,
                                                                         out depth_point.ColorY);
            }
            catch (Exception)
            {
                //TODO: adequate catch
            }

            //Move accordingly to the settings
            depth_point.ColorX = depth_point.ColorX + SettingsManager.ScreenMappingSet.ColorMoveX;
            depth_point.ColorY = depth_point.ColorY + SettingsManager.ScreenMappingSet.ColorMoveY;

            //If true, check whether the coordinates exeed the bounds
            if (CutIntoBounds)
            {
                int xmax=0, ymax=0;
                SettingsManager.KinectSet.GetVideoResolution(out xmax, out ymax);

                if (depth_point.ColorX > xmax - 1)
                    depth_point.ColorX = xmax - 1;

                if (depth_point.ColorY > ymax - 1)
                    depth_point.ColorY = ymax - 1;

                if (depth_point.ColorX < 0)
                    depth_point.ColorX = 0;
                if (depth_point.ColorY < 0)
                    depth_point.ColorY = 0;
            }

            return depth_point;
        }

        public static TPoint GetScreenCoordsfromDepth(TPoint depth_point)
        {
            //First, scale the coordinates
            double tmpX = depth_point.DepthX*SettingsManager.ScreenMappingSet.ScaleX;
            double tmpY = depth_point.DepthY*SettingsManager.ScreenMappingSet.ScaleY;

            //Move them accoding to the settings
            depth_point.ScreenX = (int) Math.Round(tmpX + SettingsManager.ScreenMappingSet.MoveX);
            depth_point.ScreenY = (int) Math.Round(tmpY + SettingsManager.ScreenMappingSet.MoveY);

            return depth_point;
        }

        public static TPoint GetDepthCoordsfromScreen(TPoint screenPoint)
        {
            //Rewind the move
            double X = screenPoint.ScreenX - SettingsManager.ScreenMappingSet.MoveX;
            double Y = screenPoint.ScreenY - SettingsManager.ScreenMappingSet.MoveY;

            //Re-scale
            int x = (int)Math.Round(X / SettingsManager.ScreenMappingSet.ScaleX);
            int y = (int)Math.Round(Y / SettingsManager.ScreenMappingSet.ScaleY);

            return new TPoint(x,y,TPoint.PointCreationType.depth);
        }
    }
}
