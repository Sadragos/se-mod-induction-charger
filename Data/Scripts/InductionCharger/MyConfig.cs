using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Xml.Serialization;
using VRageMath;
using VRage.Game;
using System.Text;

namespace InductionCharger
{
    [ProtoContract]
    [Serializable]
    public class MyConfig
    {
        [ProtoMember(1)]
        public int StoredMW;
        [ProtoMember(2)]
        public int MaxIOMW;
        [ProtoMember(3)]
        public int MaxTargetBatteries;
        [ProtoMember(4)]
        public float Radius;
        [ProtoMember(5)]
        public float LossPercentPerM;
    }
}