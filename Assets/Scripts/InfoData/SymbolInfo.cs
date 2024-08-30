using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SymbolInfo
    {
        private SymbolType _symbolType;
        public SymbolType SymbolType => _symbolType;
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
        private bool _cleared;
        public bool Cleared => _cleared;
        public void SetCleared(bool cleared)
        {
            _cleared = cleared;
        }
        public SymbolInfo(SymbolType symbolType)
        {
            _symbolType = symbolType;
            //_stageSymbolData = symbol;
        }

        public void CopyData(SymbolInfo symbolInfo)
        {
            _troopInfo = symbolInfo._troopInfo;
            _getItemInfos = new List<GetItemInfo>();
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {
                var getItem = new GetItemInfo(null);
                getItem.CopyData(getItemInfo);
                _getItemInfos.Add(getItem);
            }
            _cleared = symbolInfo._cleared;
        }

        public void ResetParamData()
        {
            foreach (var getItemInfo in GetItemInfos)
            {
                getItemInfo.SetParam2(0);
                getItemInfo.SetGetFlag(false);
            }
            _cleared = false;
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
                    scoreMax += getItemInfo.Param1;
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