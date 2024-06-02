using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ScorePrizeInfo
    {
        public ScorePrizeData Master => DataSystem.ScorePrizes.Find(a => a.Id == _scorePrizeId);
        public int _scorePrizeId;
        public int PrizeSetId => Master.PriseSetId;
        public int Score => Master.Score;
        public string ConditionName => Master.ConditionName;
        public bool _getFlag = false;
        public ScorePrizeInfo(int scorePrizeId)
        {
            _scorePrizeId = scorePrizeId;
            //_prizeSetId = prizeSetId;
            _getFlag = false;
        }


    }
}
