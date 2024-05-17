using System;
using System.Collections.Generic;
using UnityEngine;

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
        // バトルで1度クリアしたことがある
        private bool _cleared;
        public bool Cleared => _cleared;
        public void SetCleared(bool cleared)
        {
            _cleared = cleared;
        }
        private int _battleScore;
        public int BattleScore => _battleScore;
        public void SetBattleScore(int battleScore)
        {
            _battleScore = battleScore;
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
    }
}