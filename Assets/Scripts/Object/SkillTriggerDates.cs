using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SkillTriggerDates : ScriptableObject
    {
        public List<SkillTriggerData> Data = new();
    }

    [Serializable]
    public class SkillTriggerData
    {   
        public int Id;
        public string Name;
        public string Help;
        public int Category;
        public int Priority;
        public TargetType TargetType;
        public TriggerType TriggerType;
        public int Param1;
        public int Param2;
        public int Param3;
    }
}