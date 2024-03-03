using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class SymbolResultInfo
    {
        public int StageId => _symbolInfo.StageSymbolData.StageId;
        public int Seek => _symbolInfo.StageSymbolData.Seek;
        public int SeekIndex => _symbolInfo.StageSymbolData.SeekIndex;

        public int _currency;
        public int Currency => _currency;
        public bool _selected;
        public bool Selected => _selected;
        public void SetSelected(bool isSelected)
        {
            _selected = isSelected;
        }
        // バトルで1度クリアしたことがある
        public bool _cleared;
        public bool Cleared => _cleared;
        public void SetCleared(bool cleared)
        {
            _cleared = cleared;
        }
        public int _battleScore;
        public int BattleScore => _battleScore;
        public void SetBattleScore(int battleScore)
        {
            _battleScore = battleScore;
        }
        private List<int> _alchemyIdList = new();
        public List<int> AlchemyIdList => _alchemyIdList;
        public void AddAlchemyId(int alchemyId)
        {
            if (!_alchemyIdList.Contains(alchemyId))
            {
                _alchemyIdList.Add(alchemyId);
            }
        }
        public void RemoveAlchemyId(int alchemyId)
        {
            if (_alchemyIdList.Contains(alchemyId))
            {
                _alchemyIdList.Remove(alchemyId);
            }
        }
        public void SetAlchemyIdList(List<int> alchemyIdList)
        {
            _alchemyIdList = alchemyIdList;
        }


        public List<ActorInfo> _actorInfos = new ();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            foreach (var actorInfo in actorInfos)
            {
                var recordActorInfo = new ActorInfo(actorInfo.Master);
                recordActorInfo.CopyData(actorInfo);		
                _actorInfos.Add(recordActorInfo);
            }
        }
        private SymbolInfo _symbolInfo;
        public SymbolInfo SymbolInfo => _symbolInfo;

        public SymbolResultInfo(SymbolInfo symbolInfo,int currency)
        {
            _symbolInfo = symbolInfo;
            _currency = currency;
            _selected = false;
        }


        public bool IsSameSymbol(SymbolResultInfo symbolResultInfo)
        {
            return symbolResultInfo.StageId == StageId && symbolResultInfo.Seek == Seek && symbolResultInfo.SeekIndex == SeekIndex;
        }

        public bool IsSameSymbol(SymbolInfo symbolInfo)
        {
            return symbolInfo.StageSymbolData.StageId == StageId && symbolInfo.StageSymbolData.Seek == Seek && symbolInfo.StageSymbolData.SeekIndex == SeekIndex;
        }

        public bool IsSameSymbol(int stageId,int seek,int seekIndex)
        {
            return StageId == stageId && Seek == seek && SeekIndex == seekIndex;
        }
    }
}