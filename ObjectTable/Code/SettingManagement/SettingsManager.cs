using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition;
using ObjectTable.Code.Rotation;
using ObjectTable.Code.Tracking;

namespace ObjectTable.Code.SettingManagement
{
    /// <summary>
    /// Supplies the Application's Settings as well as methods to save/load them
    /// </summary>
    public static class SettingsManager
    {
        public static KinectSettings KinectSet = new KinectSettings();
        public static ScreenMappingSettings ScreenMappingSet = new ScreenMappingSettings();
        public static TrackingSettings TrackingSet = new TrackingSettings();
        public static RecognitionSettings RecognitionSet = new RecognitionSettings();
        public static PreprocessingSettings PreprocessingSet = new PreprocessingSettings();
        public static RotationRecognitionSettings RotationRecSet = new RotationRecognitionSettings();

        public static string Path = System.IO.Path.GetDirectoryName(System.Reflection. Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\defaultSettings\\";
        /// <summary>
        /// Loads the settings 
        /// </summary>
        /// <param name="path">The path of the directory. With backslash at the end!</param>
        /// <returns></returns>
        public static bool LoadSettings(string path)
        {
            //Override default value
            if (path != "")
            {
                Path = path;
            }

            //Check whether the path and the files exist
            if (!Directory.Exists(path))
                return false;

            if ((!File.Exists(path + "KinectSettings.xml"))||
                (!File.Exists(path + "ScreenMappingSettings.xml"))||
                (!File.Exists(path + "RecognitionSettings.xml"))||
                (!File.Exists(path + "RotationRecognitionSettings.xml")) ||
                (!File.Exists(path + "TrackingSettings.xml")))
                return false;

            //Now load them
            try
            {
                KinectSet = (KinectSettings) Deserialize(typeof (KinectSettings), path + "KinectSettings.xml");
                ScreenMappingSet = (ScreenMappingSettings)Deserialize(typeof(ScreenMappingSettings), path + "ScreenMappingSettings.xml");
                TrackingSet = (TrackingSettings) Deserialize(typeof (TrackingSettings), path + "TrackingSettings.xml");
                RecognitionSet =(RecognitionSettings) Deserialize(typeof (RecognitionSettings), path + "RecognitionSettings.xml");
                RotationRecSet = (RotationRecognitionSettings)Deserialize(typeof(RotationRecognitionSettings), path + "RotationRecognitionSettings.xml");
            }
            catch (Exception)
            {
                return false;
            }

            //Load depth correction map
            return PreprocessingSet.Load(path);
        }

        public static bool LoadSettings()
        {
            return LoadSettings(Path);
        }

        /// <summary>
        /// Saves the settings (
        /// </summary>
        /// <param name="path">The path of the directory. With backslash at the end! If the directory doesn't exist, it will be created</param>
        /// <returns></returns>
        public static bool SaveSettings(string path)
        {
            //If the path doesn't exist, create it
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            //TryToSave
            if (!Serialize(KinectSet, path + "KinectSettings.xml"))
                return false;
            if (!Serialize(ScreenMappingSet, path + "ScreenMappingSettings.xml"))
                return false;
            if (!Serialize(TrackingSet, path + "TrackingSettings.xml"))
                return false;
            if (!Serialize(RecognitionSet, path + "RecognitionSettings.xml"))
                return false;
            if (!Serialize(RotationRecSet, path + "RotationRecognitionSettings.xml"))
                return false;
            if (!PreprocessingSet.Save(path))
                return false;

            //Success
            return true;
        }

        public static bool SaveSettings()
        {
            return SaveSettings(Path);
        }

        private static bool Serialize(object obj, string filename)
        {
            //Delete file if it already exists
            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                    return false;
                }
            }

            //Try to serialize and save
            FileStream fstr = null;
            try
            {
                fstr = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite);
                XmlSerializer xser = new XmlSerializer(obj.GetType());
                xser.Serialize(fstr, obj);
                fstr.Flush();
                fstr.Close();
            }
            catch
            {
                if (fstr != null)
                    fstr.Close();

                return false;
            }
            return true;
        }

        private static object Deserialize(Type type, string filename)
        {
            FileStream fstr = null;
            XmlSerializer xser = new XmlSerializer(type);
            try
            {
                fstr = new FileStream(filename, FileMode.Open);
                object obj = xser.Deserialize(fstr);
                fstr.Close();
                return obj;
            }
            catch
            {
                if (fstr != null)
                {
                    fstr.Close();
                }
                throw new Exception();
            }
        }
    }
}
