using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    [Serializable]
    public class SymbolResultInfo
    {
        public StageSymbolData StageSymbolData => SymbolInfo.Master;
        public int StageId => SymbolInfo.Master.StageId;
        public int Seek => SymbolInfo.Master.Seek;
        public int SeekIndex => SymbolInfo.Master.SeekIndex;
        public SymbolType SymbolType => SymbolInfo.Master.SymbolType;

        public bool _selected;
        public bool Selected => _selected;
        public void SetSelected(bool isSelected)
        {
            _selected = isSelected;
        }
        private int _battleScore;
        public int BattleScore => _battleScore;
        public void SetBattleScore(int battleScore)
        {
            _battleScore = battleScore;
        }

        private SymbolInfo _symbolInfo;
        public SymbolInfo SymbolInfo => _symbolInfo;

        public SymbolResultInfo(SymbolInfo symbolInfo)
        {
            _symbolInfo = symbolInfo;
            _selected = false;
        }

        public void CopyParamData(SymbolResultInfo symbolResultInfo)
        {
            _selected = symbolResultInfo.Selected;
            _battleScore = symbolResultInfo.BattleScore;
            _symbolInfo.CopyData(symbolResultInfo.SymbolInfo);
        }

        public void ResetParamData()
        {
            _selected = false;
            _battleScore = 0;
            _symbolInfo.ResetParamData();
        }

        public bool IsSameSymbol(SymbolResultInfo symbolResultInfo)
        {
            return symbolResultInfo.StageId == StageId && symbolResultInfo.Seek == Seek && symbolResultInfo.SeekIndex == SeekIndex;
        }

/*
        public bool IsSameSymbol(int stageId,int seek,int seekIndex)
        {
            return StageId == stageId && Seek == seek && SeekIndex == seekIndex;
        }
*/
        public bool IsSameStageSeek(int stageId,int seek)
        {
            return StageId == stageId && Seek == seek;
        }

        public bool IsBeforeStageSeek(int stageId,int seek)
        {
            return (StageId == stageId && Seek < seek || StageId < stageId);
        }

        public bool IsAfterStageSeek(int stageId,int seek)
        {
            return (StageId == stageId && Seek >= seek || StageId > stageId);
        }

        public bool SaveBattleReplayStage()
        {
            if (_symbolInfo?.TroopInfo != null)
            {
                return _symbolInfo?.TroopInfo.NeedReplayData == false;
            }
            return false;
        }

        public int SortKey()
        {
            return (int)StageId*1000 + Seek*100 + SeekIndex;
        }

        public bool EnableStage(int stageId,int seek)
        {
            return (StageId == stageId && Seek < seek || StageId < stageId);
        }
    }
}