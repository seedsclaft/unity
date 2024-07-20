using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ryneus
{
    public class TroopDates : ScriptableObject
    {
        public List<TroopData> Data = new();
    }

    [Serializable]
    public class TroopData
    {   
        public int TroopId;
        public List<TroopEnemyData> TroopEnemies;
        public int StageTurn;
        //public int PrizeSetId;
    }

    [Serializable]
    public class TroopEnemyData
    {   
        public int Id;
        public int TroopId;
        public int EnemyId;
        public int Lv;
        public bool BossFlag;
        public LineType Line;
        public int StageLv;
        //public int PrizeSetId;
    }
}