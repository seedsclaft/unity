using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class ScorePrizeInfo
    {
        public ScorePrizeData Master => DataSystem.ScorePrizes.Find(a => a.Id == _scorePrizeId);
        public int _scorePrizeId;
        public int PrizeSetId => Master.PriseSetId;
        public List<PrizeSetData> PrizeMaster => DataSystem.PrizeSets.FindAll(a => a.Id == Master.PriseSetId);
        public int Score => Master.Score;
        public string Title => Master.Title;
        public string Help => Master.Help;
        public bool _getFlag = false;
        public bool _isCheck = false;
        public bool _used = false;
        public bool Used => _used;
        public ScorePrizeInfo(int scorePrizeId)
        {
            _scorePrizeId = scorePrizeId;
            //_prizeSetId = prizeSetId;
            _getFlag = false;
            _used = false;
        }

        public void UpdateGetFlag(int point)
        {
            if (_getFlag == false)
            {
                _isCheck = true;
            }
            _getFlag = point >= Score;
            if (_getFlag == false)
            {
                _isCheck = false;
            }
        }        
        
        public bool CheckFlag()
        {
            if (_isCheck == true)
            {
                _isCheck = false;
                return true;
            }
            return false;
        }

        public bool RemakeHistory()
        {
            return _getFlag && PrizeMaster.Find(a => a.GetItem.Type == GetItemType.RemakeHistory) != null;
        }

        public bool ParallelHistory()
        {
            return _getFlag && PrizeMaster.Find(a => a.GetItem.Type == GetItemType.ParallelHistory) != null;
        }

        public bool EnableParallel()
        {
            return _used == false && _getFlag && PrizeMaster.Find(a => a.GetItem.Type == GetItemType.ParallelHistory) != null;
        }

        public void UseParallel()
        {
            _used = true;
        }

        public bool EnableMultiverse()
        {
            return _getFlag && PrizeMaster.Find(a => a.GetItem.Type == GetItemType.Multiverse) != null;
        }
    }
}
