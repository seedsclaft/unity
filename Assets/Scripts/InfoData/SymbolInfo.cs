using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SymbolInfo
    {
        private StageSymbolData _stageSymbolData;
        public StageSymbolData Master => _stageSymbolData;
        public SymbolType SymbolType => Master.SymbolType;
        private TroopInfo _troopInfo;
        public TroopInfo TroopInfo => _troopInfo;
        public void SetTroopInfo(TroopInfo troopInfo)
        {
            _troopInfo = troopInfo;
        }
        private List<GetItemInfo> _getItemInfos = new ();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        public void SetGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            _getItemInfos = getItemInfos;
        }
        private bool _lastSelected;
        public bool LastSelected => _lastSelected;
        public void SetLastSelected(bool lastSelected)
        {
            _lastSelected = lastSelected;
        }
        public SymbolInfo(StageSymbolData stageSymbolData)
        {
            _stageSymbolData = stageSymbolData;
        }

        public void CopyData(SymbolInfo symbolInfo)
        {
            _troopInfo = symbolInfo._troopInfo;
            _getItemInfos = new List<GetItemInfo>();
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {
                var getItem = new GetItemInfo(getItemInfo.Master);
                getItem.CopyData(getItemInfo);
                _getItemInfos.Add(getItem);
            }
        }

        public void ResetParamData()
        {
            foreach (var getItemInfo in GetItemInfos)
            {
                getItemInfo.SetResultParam(0);
                getItemInfo.SetGetFlag(false);
            }
        }

        public List<BattlerInfo> BattlerInfos()
        {
            return _troopInfo.BattlerInfos;
        }

        public int BattleEvaluate()
        {
            if (_troopInfo != null)
            {
                var evaluate = 0;
                foreach (var battlerInfo in _troopInfo.BattlerInfos)
                {
                    evaluate += battlerInfo.Evaluate();
                }
                return evaluate;
            }
            return 0;
        }

        public int ScoreMax()
        {
            var scoreMax = 0;
            foreach (var getItemInfo in GetItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.SaveHuman)
                {
                    scoreMax += getItemInfo.ResultParam;
                }
            }
            return scoreMax;
        }

        public bool IsActorSymbol()
        {
            return SymbolType == SymbolType.Actor || SymbolType == SymbolType.SelectActor;
        }

        public bool IsBattleSymbol()
        {
            return SymbolType == SymbolType.Battle || SymbolType == SymbolType.Boss;
        }


    }
}