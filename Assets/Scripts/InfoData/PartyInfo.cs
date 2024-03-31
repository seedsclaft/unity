﻿using System.Collections.Generic;
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
            var actorIdList = CurrentActorIdList(stageId,seek);
            var actorInfos = new List<ActorInfo>();
            foreach (var actorId in actorIdList)
            {
                var actorInfo = _actorInfos.Find(a => a.ActorId == actorId);
                if (actorInfo != null)
                {
                    var levelUpInfos = _levelUpInfos.FindAll(a => a.Enable && a.ActorId == actorInfo.ActorId);
                    actorInfo.SetLevelUpInfo(levelUpInfos);
                }
                actorInfos.Add(actorInfo);
            }
            return actorInfos;
        }

        public List<LevelUpInfo> AssignedLevelUpInfos(int actorId)
        {
            var actorInfo = _actorInfos.Find(a => a.ActorId == actorId);
            return _levelUpInfos.FindAll(a => a.Enable && a.ActorId == actorInfo.ActorId);
        }
        
        public List<int> CurrentActorIdList(int stageId,int seek)
        {
            var actorIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol() && a.Selected);
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
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol());
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

        private List<LevelUpInfo> _levelUpInfos = new();
        public List<LevelUpInfo> LevelUpInfos => _levelUpInfos;
        public void SetLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            var findIndex = _levelUpInfos.FindIndex(a => a.IsSameLevelUpInfo(levelUpInfo));
            if (findIndex < 0)
            {
            } else{
                _levelUpInfos.RemoveAt(findIndex);
            }
            _levelUpInfos.Add(levelUpInfo);
        }

        public int ActorLevelReset(int actorId)
        {
            int currency = 0;
            var levelUpInfos = _levelUpInfos.FindAll(a => a.ActorId == actorId);
            var currencyIndexes = new List<int>();
            for (int i = 0;i < levelUpInfos.Count;i++)
            {
                if (levelUpInfos[i].Enable && levelUpInfos[i].Currency > 0)
                {
                    currency += levelUpInfos[i].Currency;
                    currencyIndexes.Add(i);
                }
            }
            for (int i = currencyIndexes.Count-1;i >= 0;i--)
            {
                _levelUpInfos.RemoveAt(currencyIndexes[i]);
            }
            return currency;
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
            _symbolRecordList.Sort((a,b) => a.StageId*100 + a.Seek - b.StageId*100 + b.Seek > 0 ? 1 : -1);
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