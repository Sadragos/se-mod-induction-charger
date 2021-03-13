using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace InductionCharger
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class ConfigAdaptor : MySessionComponentBase
    {
        public override void LoadData()
        {
            base.LoadData();
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();

            foreach (var componenet in allDefs.OfType<MyBatteryBlockDefinition>())
            {
                if(componenet.BlockPairName == "InductionBattery")
                {
                    componenet.RequiredPowerInput = Config.Instance.MaxIOMW;
                    componenet.MaxPowerOutput = Config.Instance.MaxIOMW;
                    componenet.MaxStoredPower = Config.Instance.StoredMW;
                    break;
                }
            }
        }
    }
}
