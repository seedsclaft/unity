using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SymbolResultInfo
    {
        private StageSymbolData _stageSymbolData;
        public StageSymbolData StageSymbolData => _stageSymbolData;
        public int StageId => StageSymbolData.StageId;
        public int Seek => StageSymbolData.Seek;
        public int SeekIndex => StageSymbolData.SeekIndex;
        public SymbolType SymbolType => StageSymbolData.SymbolType;

        public int _currency;
        public int Currency => _currency;
        public bool _selected;
        public bool Selected => _selected;
        public void SetSelected(bool isSelected)
        {
            _selected = isSelected;
        }
        // 選択式のシンボルの行動結果
        public int _selectedIndex = 0;
        public int SelectedIndex => _selectedIndex;
        public void SetSelectedIndex(int selectedIndex)
        {
            _selectedIndex = selectedIndex;
        }
        private int _battleScore;
        public int BattleScore => _battleScore;
        public void SetBattleScore(int battleScore)
        {
            _battleScore = battleScore;
        }

        // 確定しているか
        public bool _endFlag = false;
        public bool EndFlag => _endFlag;
        public void SetEndFlag(bool endFlag)
        {
            _endFlag = endFlag;
        }

        private SymbolInfo _symbolInfo;
        public SymbolInfo SymbolInfo => _symbolInfo;

        public SymbolResultInfo(SymbolInfo symbolInfo,StageSymbolData stageSymbolData,int currency)
        {
            _symbolInfo = symbolInfo;
            _stageSymbolData = stageSymbolData;
            _currency = currency;
            _selected = false;
        }

        public bool IsSameSymbol(SymbolResultInfo symbolResultInfo)
        {
            return symbolResultInfo.StageId == StageId && symbolResultInfo.Seek == Seek && symbolResultInfo.SeekIndex == SeekIndex;
        }

        public bool IsSameSymbol(int stageId,int seek,int seekIndex)
        {
            return StageId == stageId && Seek == seek && SeekIndex == seekIndex;
        }

        public bool IsSameStageSeek(int stageId,int seek)
        {
            return StageId == stageId && Seek == seek;
        }

        public bool SaveBattleReplayStage()
        {
            if (_symbolInfo?.TroopInfo != null)
            {
                return _symbolInfo?.TroopInfo.RandomTroop == false;
            }
            return false;
        }

        public int SortKey()
        {
            return StageId*1000 + Seek*100 + SeekIndex;
        }

        public bool EnableStage(int stageId,int seek)
        {
            return StageId == stageId && Seek < seek || StageId < stageId;
        }
    }
}