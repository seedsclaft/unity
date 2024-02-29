using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TestBattleData : ScriptableObject
    {
        [SerializeField] public List<TestBattlerData> TestBattleDates = new();
        [SerializeField] public List<TestActionData> TestActionDates = new();
    }

    [Serializable]
    public class TestBattlerData 
    {   
        public int BattlerId;
        public bool IsActor;
        public int Level;
        public bool IsFront;
    }

    [Serializable]
    public class TestActionData 
    {   
        public int BattlerIndex;
        public int SkillId;
    }
}