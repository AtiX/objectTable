using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ObjectTable.Code.Kinect;

namespace ProgrammingTable.Code.General
{
    static class LocalSettingsManager
    {
        public static ProgrammingTableSettings PrgTblSet = new ProgrammingTableSettings();

        public static string Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\defaultSettings\\";
        /// <summary>
        /// Loads the settings 
        /// </summary>
        /// <param name="path">The path of the directory. With backslash at the end!</param>
        /// <returns></returns>
        public static bool LoadSettings(string path)
        {
            //Check whether the path and the files exist
            if (!Directory.Exists(path))
                return false;

            if (!File.Exists(path + "ProgrammingTableSettings.xml"))
                return false;

            //Now load them
            try
            {
                PrgTblSet = (ProgrammingTableSettings)Deserialize(typeof(ProgrammingTableSettings), path + "ProgrammingTableSettings.xml");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
            if (!Serialize(PrgTblSet, path + "ProgrammingTableSettings.xml"))
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
