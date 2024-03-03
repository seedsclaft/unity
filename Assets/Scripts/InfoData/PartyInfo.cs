using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo 
    {
        public PartyInfo()
        {
            ClearData();
        }

        public void ClearData()
        {
            _actorInfos.Clear();
            _currency = 0;
            _battleResultVictory = false;
            _battleResultScore = 0;
            _symbolRecordList.Clear();
            _alcanaInfo = new AlcanaInfo();
        }
        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;

        
        public List<ActorInfo> CurrentActorInfos(int stageId,int seek)
        {
            return _actorInfos.FindAll(a => CurrentActorIdList(stageId,seek).Contains(a.ActorId));
        }
        
        public List<int> CurrentActorIdList(int stageId,int seek)
        {
            var actorIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.StageId <= stageId && a.Seek <= seek && a.SymbolInfo.SymbolType == SymbolType.Actor && a.Selected);
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    actorIdList.Add(getItemInfo.Param1);
                }
            }
            return actorIdList;
        }
        
        private int _currency = 0;
        public int Currency => _currency;
        private int _parallelCount = 1;

        public List<int> CurrentAlchemyIdList(int stageId,int seek)
        {
            var alchemyIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.StageId <= stageId && a.Seek <= seek && a.SymbolInfo.SymbolType == SymbolType.Alcana && a.Selected);
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    alchemyIdList.Add(getItemInfo.Param1);
                }
            }
            return alchemyIdList;
        }
        private AlcanaInfo _alcanaInfo;
        public AlcanaInfo AlcanaInfo => _alcanaInfo;

        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;
        private int _battleResultScore = 0;
        public int BattleResultScore => _battleResultScore;

        private int _clearTroopCount;
        public int ClearTroopCount => _clearTroopCount;

        private List<int> _lastBattlerIdList = new();
        public List<int> LastBattlerIdList => _lastBattlerIdList;
        public void SetLastBattlerIdList(List<int> lastBattlerIdList)
        {
            _lastBattlerIdList = lastBattlerIdList;
        }

        // 生成したステージシンボル
        private List<SymbolInfo> _stageSymbolInfos = new();
        public List<SymbolInfo> StageSymbolInfos => _stageSymbolInfos;
        public void SetStageSymbolInfos(List<SymbolInfo> symbolInfos)
        {
            _stageSymbolInfos = symbolInfos;
        }
        public List<SymbolInfo> CurrentSymbolInfos(int seek) => _stageSymbolInfos.FindAll(a => a.StageSymbolData.Seek == seek);
        
        // ステージシンボルの結果
        private List<SymbolResultInfo> _symbolRecordList = new ();
        public List<SymbolResultInfo> SymbolRecordList => _symbolRecordList;
        public void SetSymbolResultInfo(SymbolResultInfo symbolResultInfo)
        {
            var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo));
            if (findIndex < 0)
            {
            } else{
                _symbolRecordList.RemoveAt(findIndex);
            }
            _symbolRecordList.Add(symbolResultInfo);
            _symbolRecordList.Sort((a,b) => a.Seek - b.Seek > 0 ? 1 : -1);
        }

        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }

        public void UpdateActorInfo(ActorInfo actorInfo)
        {
            var findIndex = _actorInfos.FindIndex(a => a.ActorId == actorInfo.ActorId);
            if (findIndex > -1)
            {
                _actorInfos[findIndex] = actorInfo;
            } else
            {
                _actorInfos.Add(actorInfo);
            }
        }

        public void InitActorInfos()
        {
            ClearActorInfos();
            ClearActorIds();
        }

        public void ClearActorInfos()
        {
            _actorInfos.Clear();
        }

        // アクター加入
        public void AddActorId(int actorId)
        {
        }

        // アクター離脱
        public void RemoveActor(int actorId)
        {
        }

        public void ClearActorIds()
        {
        }

        public void ChangeCurrency(int currency)
        {
            _currency = currency;
        }

        public void SetBattleResultVictory(bool isVictory)
        {
            _battleResultVictory = isVictory;
        }

        public void SetBattleScore(int score)
        {
            _battleResultScore = score;
        }

        public void AddAlchemy(int skillId)
        {
        }

        public void RemoveAlchemy(int skillId)
        {
        }

        public void ClearAlchemy()
        {
        }

        public int TotalScore()
        {
            var score = 0;
            foreach (var record in SymbolRecordList)
            {
                score += record.BattleScore;
            }
            return score;
        }

        public int ParallelCost()
        {
            return _parallelCount * _parallelCount;
        }

        public void GainParallelCount()
        {
            _parallelCount++;
        }
    }
}