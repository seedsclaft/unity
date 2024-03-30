using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SymbolInfo
    {
        private StageSymbolData _stageSymbolData;
        public StageSymbolData StageSymbolData => _stageSymbolData;
        public SymbolType SymbolType => _stageSymbolData.SymbolType;
        private TroopInfo _troopInfo;
        public TroopInfo TroopInfo => _troopInfo;
        public void SetTroopInfo(TroopInfo troopInfo)
        {
            _troopInfo = troopInfo;
        }
        private List<GetItemInfo> _getItemInfos = new ();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        private bool _selected;
        public bool Selected => _selected;
        public void SetSelected(bool selected)
        {
            _selected = selected;
        }
        private bool _lastSelected;
        public bool LastSelected => _lastSelected;
        public void SetLastSelected(bool lastSelected)
        {
            _lastSelected = lastSelected;
        }
        private bool _past;
        public bool Past => _past;
        public void SetPast(bool past)
        {
            _past = past;
        }
        private bool _cleared;
        public bool Cleared => _cleared;
        public void SetCleared(bool cleared)
        {
            _cleared = cleared;
        }
        public SymbolInfo(StageSymbolData symbol = null)
        {
            if (symbol == null)
            {
                return;
            }
            _stageSymbolData = symbol;
        }

        public void CopyData(SymbolInfo symbolInfo)
        {
            _stageSymbolData = symbolInfo.StageSymbolData;
            _troopInfo = symbolInfo.TroopInfo;
            _getItemInfos = symbolInfo.GetItemInfos;
            _selected = symbolInfo.Selected;
            _cleared = symbolInfo.Cleared;
        }

        public List<BattlerInfo> BattlerInfos()
        {
            return _troopInfo.BattlerInfos;
        }

        public void SetGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            _getItemInfos = getItemInfos;
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

        public bool IsBattleSymbol()
        {
            return SymbolType == SymbolType.Battle || SymbolType == SymbolType.Boss;
        }

        public bool IsActorSymbol()
        {
            return SymbolType == SymbolType.Actor || SymbolType == SymbolType.SelectActor;
        }
    }
}