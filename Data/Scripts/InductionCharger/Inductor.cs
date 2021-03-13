using Sandbox.Game;
using VRage.ObjectBuilders;
using VRageMath;
using ProtoBuf;
using System;
using Sandbox.ModAPI.Weapons;
using System.Collections.Generic;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Utils;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using VRage;
using System.Linq;
using Sandbox.Game.Entities;

namespace InductionCharger
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), true, new string[] { "InductionCharger" })]
    public class Inductor : MyGameLogicComponent
    {
        bool _init = false;
        MyBatteryBlock Battery;
        IMyTerminalBlock TerminalBlock;
        Vector3D Position;
        List<Target> Targets;
        IEnumerator<bool> Runtime;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (!_init)
            {
                Battery = Entity as MyBatteryBlock;
                TerminalBlock = Entity as IMyTerminalBlock;
                Targets = new List<Target>();

                TerminalBlock.AppendingCustomInfo += AppendingCustomInfo;
                Runtime = Run();
                _init = true;
            }
            if (TerminalBlock.IsFunctional && (TerminalBlock as IMyFunctionalBlock).Enabled)
            {
                if (Runtime == null) return;
                Runtime.MoveNext();
                if (Position == null || Targets.Count == 0) return;
                Charge();
                TerminalBlock.RefreshCustomInfo();
            }
        }

        public float CurrentOutput
        {
            get
            {
                float res = 0;
                foreach (Target t in Targets)
                {
                    res += t.Chargerate;
                }
                return res;
            }
        }

        void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder stringBuilder)
        {
            try
            {
                stringBuilder.Clear();
                stringBuilder
                    .Append("\nInduction Power:\n")
                    .Append(CurrentOutput.ToString("0.0"))
                    .Append(" / ")
                    .Append(Config.Instance.MaxIOMW.ToString("0.0"))
                    .Append(" MWh\n");
                if (Targets.Count > Config.Instance.MaxTargetBatteries)
                {
                    stringBuilder.Append("OVERLOAD!\n");
                }

                Targets.Sort((a, b) => a.Distance.CompareTo(b.Distance));

                foreach (Target bat in Targets)
                {
                    stringBuilder
                        .Append(bat.IBattery.CustomName)
                        .Append(" (")
                        .Append(bat.Distance.ToString("0"))
                        .Append("m): ");
                    if (bat.ToFull)
                    {
                        stringBuilder.Append("To Full\n");
                    }
                    else if (bat.Chargerate > 0)
                    {
                        stringBuilder
                        .Append(bat.Chargerate.ToString("0.0"))
                        .Append(" MWh (")
                        .Append((bat.Loss * 100f).ToString("0.0"))
                        .Append("% loss)\n");
                    }
                    else
                    {
                        stringBuilder.Append("OVERLOAD\n");
                    }
                }
                TerminalBlock.CustomData = stringBuilder.ToString();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("InductionCharger: ERROR " + e);
            }
        }

        public void Charge()
        {
            try
            {
                double filled = Battery.CurrentStoredPower / Battery.MaxStoredPower;
                Targets.ShuffleList();
                int c = 0;
                int amount = Math.Min(Config.Instance.MaxTargetBatteries, Targets.Count);
                float charge = Config.Instance.MaxIOMW / amount;
                foreach (Target bat in Targets)
                {
                    if ((bat.IBattery.CurrentStoredPower / bat.IBattery.MaxStoredPower) >= filled)
                    {
                        bat.ToFull = true;
                        continue;
                    }
                    float cAmount = Math.Min(charge, bat.IBattery.MaxInput);
                    bat.Chargerate = cAmount * (1 - bat.Loss);


                    bat.Battery.CurrentStoredPower += (cAmount / 3600f) * (1 - bat.Loss) * (100f / 60f);
                    Battery.CurrentStoredPower -= (cAmount / 3600f) * (100f / 60f);


                    if (++c >= Config.Instance.MaxTargetBatteries) return;
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("InductionCharger: ERROR " + e);
            }
        }

        public IEnumerator<bool> Run()
        {
            while (true)
            {
                // Get Position
                Position = Entity.GetPosition();
                yield return true;

                // Get Entities
                Targets.Clear();
                List<long> entities = MyVisualScriptLogicProvider.GetEntitiesInSphere(Position, Config.Instance.Radius);
                foreach (long entityId in entities)
                {
                    if (entityId == Entity.EntityId) continue;

                    IMyEntity entity = MyVisualScriptLogicProvider.GetEntityById(entityId);
                    if (entity is IMyBatteryBlock)
                    {
                        IMyBatteryBlock bat = entity as IMyBatteryBlock;
                        if (bat.IsSameConstructAs(Battery)) continue;
                        if (!bat.Enabled) continue;
                        if (bat.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Discharge) continue;
                        Target targ = new Target()
                        {
                            IBattery = bat,
                            Distance = (float)Vector3D.Distance(entity.GetPosition(), Position)
                        };
                        targ.Loss = targ.Distance * Config.Instance.LossPercentPerM;
                        Targets.Add(targ);
                    }
                }
                yield return true;
                yield return true;
            }
        }
    }

    public class Target
    {
        public IMyBatteryBlock IBattery;
        public float Chargerate;
        public float Distance;
        public float Loss;
        public bool ToFull;

        public MyBatteryBlock Battery { get { return IBattery as MyBatteryBlock; } }
    }
}