using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;

namespace ObjectTable.Code.Recognition
{
    /// <summary>
    /// Stores all the settings related to preprocessing
    /// </summary>
    public class PreprocessingSettings
    { 
        /// <summary>
        /// The Default DepthCorrectionMap
        /// </summary>
        public DepthCorrectionMap DefaultCorrectionMap = new DepthCorrectionMap(320,240);

        public bool Save(string path)
        {
            if (!Directory.Exists(path))
                return false;

            FileStream fstr = null;

            try
            {
                if (File.Exists(path + "CorrectionMap.bin"))
                    File.Delete(path + "CorrectionMap.bin");

                BinaryFormatter bfr = new BinaryFormatter();
                fstr = new FileStream(path + "CorrectionMap.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                bfr.Serialize(fstr, DefaultCorrectionMap);
                fstr.Flush();
                fstr.Close();
            }
            catch
            {
                if (fstr != null)
                {
                    fstr.Close();
                }
                return false;
            }
            return true;
        }

        public bool Load(string path)
        {
            if (!Directory.Exists(path))
                return false;

            FileStream fstr = null;

            try
            {
                fstr = new FileStream(path + "CorrectionMap.bin", FileMode.Open);
                BinaryFormatter bfr = new BinaryFormatter();
                object o = bfr.Deserialize(fstr);
                DefaultCorrectionMap = (DepthCorrectionMap)o;
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
    }
}
