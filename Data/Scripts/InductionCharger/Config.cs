using System;
using System.IO;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game;
using VRage.Utils;

namespace InductionCharger
{

    public class Config
    {

        private static MyConfig _Instance;

        public static MyConfig Instance
        {
            get
            {
                if (_Instance == null) Load();
                return _Instance;
            }
        }

        public static void Load()
        {
            // Load config xml
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("InductionChargerConfig.xml", typeof(MyConfig)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("InductionChargerConfig.xml", typeof(MyConfig));
                    var xmlData = reader.ReadToEnd();
                    _Instance = MyAPIGateway.Utilities.SerializeFromXML<MyConfig>(xmlData);
                    reader.Dispose();
                    MyLog.Default.WriteLine("InductionCharger: found and loaded");
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine("InductionCharger: loading failed, generating new Config");
                }
            }

            if (_Instance == null)
            {
                MyLog.Default.WriteLine("InductionCharger: No Loot Config found, creating New");
                // Create default values
                _Instance = new MyConfig()
                {
                    LossPercentPerM = 0.005f,
                    MaxIOMW = 20,
                    MaxTargetBatteries = 20,
                    Radius = 25,
                    StoredMW = 2
                };

                Write();
            }

        }


        public static void Write()
        {
            if (_Instance == null) return;

            try
            {
                MyLog.Default.WriteLine("InductionCharger: Serializing to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML<MyConfig>(_Instance);
                MyLog.Default.WriteLine("InductionCharger: Writing to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("InductionChargerConfig.xml", typeof(MyConfig));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("InductionCharger: Error saving XML!" + e.StackTrace);
            }
        }
    }
}