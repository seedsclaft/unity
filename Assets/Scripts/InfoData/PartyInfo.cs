using System.Collections.Generic;
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
            _alchemyIdList.Clear();
            _actorInfos.Clear();
            _actorIdList.Clear();
            _currency = 0;
            _battleResultVictory = false;
            _battleResultScore = 0;
            _symbolRecordList.Clear();
            _alcanaInfo = new AlcanaInfo();
        }
        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;

        // 現在使用可能なアクターIdリスト
        private List<int> _actorIdList = new();
        public List<int> ActorIdList => _actorIdList;

        public List<ActorInfo> CurrentActorInfos => _actorInfos.FindAll(a => _actorIdList.Contains(a.ActorId));
        private int _currency = 0;
        public int Currency => _currency;
        private int _parallelCount = 1;
        private List<int> _alchemyIdList = new();
        public List<int> AlchemyIdList => _alchemyIdList;

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
            if (_actorIdList.IndexOf(actorId) != -1)
            {
                return;
            }
            _actorIdList.Add(actorId);
        }

        // アクター離脱
        public void RemoveActor(int actorId)
        {
            if (_actorIdList.IndexOf(actorId) == -1)
            {
                return;
            }
            _actorIdList.Remove(actorId);
        }

        public void ClearActorIds()
        {
            _actorIdList.Clear();
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
            _alchemyIdList.Add(skillId);
        }

        public void RemoveAlchemy(int skillId)
        {
            var findIndex = _alchemyIdList.FindIndex(a => a == skillId);
            if (findIndex > -1)
            {
                _alchemyIdList.RemoveAt(findIndex);
            }
        }

        public void ClearAlchemy()
        {
            _alchemyIdList.Clear();
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