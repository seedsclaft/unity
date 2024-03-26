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
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.SymbolType == SymbolType.Actor && a.Selected);
            records = records.FindAll(a => a.StageId == stageId && a.Seek < seek || a.StageId < stageId);
            foreach (var record in records)
            {
                actorIdList.Add(record.SymbolInfo.StageSymbolData.Param1);
            }
            return actorIdList;
        }
        
        public List<int> PastActorIdList(int stageId,int seek)
        {
            var actorIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.SymbolType == SymbolType.Actor);
            records = records.FindAll(a => a.StageId == stageId && a.Seek < seek || a.StageId < stageId);
            foreach (var record in records)
            {
                actorIdList.Add(record.SymbolInfo.StageSymbolData.Param1);
            }
            return actorIdList;
        }

        private int _currency = 0;
        public int Currency => _currency;
        private int _parallelCount = 1;

        public List<int> CurrentAlchemyIdList(int stageId,int seek)
        {
            var alchemyIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.Selected);
            records = records.FindAll(a => a.StageId == stageId && a.Seek < seek || a.StageId < stageId);
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    if (getItemInfo.GetItemType == GetItemType.Skill)
                    {
                        if (DataSystem.FindSkill(getItemInfo.Param1).SkillType != SkillType.UseAlcana)
                        {
                            alchemyIdList.Add(getItemInfo.Param1);
                        }
                    }
                }
            }
            return alchemyIdList;
        }

        public List<int> CurrentAlcanaIdList(int stageId,int seek)
        {
            var alcanaIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.Selected);
            records = records.FindAll(a => a.StageId == stageId && a.Seek < seek || a.StageId < stageId);
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    if (getItemInfo.GetItemType == GetItemType.Skill)
                    {
                        if (DataSystem.FindSkill(getItemInfo.Param1).SkillType == SkillType.UseAlcana)
                        {
                            alcanaIdList.Add(getItemInfo.Param1);
                        }
                    }
                }
            }
            return alcanaIdList;
        }
        private AlcanaInfo _alcanaInfo;
        public AlcanaInfo AlcanaInfo => _alcanaInfo;

        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;
        private int _battleResultScore = 0;
        public int BattleResultScore => _battleResultScore;

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
        public void SetSymbolResultInfo(SymbolResultInfo symbolResultInfo,bool checkFindIndex = true)
        {
            if (checkFindIndex)
            {
                var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo));
                if (findIndex < 0)
                {
                } else{
                    _symbolRecordList.RemoveAt(findIndex);
                }
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
        }

        public void ClearActorInfos()
        {
            _actorInfos.Clear();
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