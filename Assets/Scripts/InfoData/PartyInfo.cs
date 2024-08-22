using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo 
    {
        public PartyInfo()
        {
            ClearData();
            InitScorePrizeInfos();
            ClearStageClearCount();
            InitStageClearCount();
        }

        public void InitScorePrizeInfos()
        {
            foreach (var scorePrizeData in DataSystem.ScorePrizes)
            {
                if (_scorePrizeInfos.Find(a => a.Master.Id == scorePrizeData.Id) == null)
                {
                    var scorePrizeInfo = new ScorePrizeInfo(scorePrizeData.Id);
                    _scorePrizeInfos.Add(scorePrizeInfo);
                }
            }
        }

        private int _currency = 0;
        public int Currency => _currency;
        public int ParallelCount => _scorePrizeInfos.FindAll(a => a.Used == false && a.EnableParallel()).Count;
        private int _stageStockCount = 99;
        public int StageStockCount => _stageStockCount;
        // スコア報酬リスト
        private List<ScorePrizeInfo> _scorePrizeInfos = new ();
        public List<ScorePrizeInfo> ScorePrizeInfos => _scorePrizeInfos;
        public void UpdateScorePrizeInfos()
        {
            _scorePrizeInfos.ForEach(a => a.UpdateGetFlag(TotalScore()));
        }
        
        public List<ScorePrizeInfo> CheckGainScorePrizeInfos()
        {
            return _scorePrizeInfos.FindAll(a => a.CheckFlag());
        }

        public bool RemakeHistory()
        {
            return _scorePrizeInfos.Find(a => a.RemakeHistory()) != null; 
        }

        public bool ParallelHistory()
        {
            return _scorePrizeInfos.Find(a => a.ParallelHistory()) != null;
        }

        public bool EnableMultiverse()
        {
            return _scorePrizeInfos.Find(a => a.EnableMultiverse()) != null; 
        }

        public bool EnableLvLink()
        {
            return _scorePrizeInfos.Find(a => a.EnableLvLink()) != null; 
        }

        // 戻り先の1番目のシンボル
        private StageSymbolData _returnSymbol = null;
        public StageSymbolData ReturnSymbol => _returnSymbol;
        public void SetReturnStageIdSeek(int stageId,int seek) 
        {
            var symbolData = new StageSymbolData
            {
                StageId = stageId,
                Seek = seek
            };
            _returnSymbol = symbolData;
        }

        public void ClearReturnStageIdSeek() 
        {
            _returnSymbol = null;
        }

        private List<int> _lastBattlerIdList = new();
        public List<int> LastBattlerIdList => _lastBattlerIdList;
        public void SetLastBattlerIdList(List<int> lastBattlerIdList)
        {
            //_lastBattlerIdList = lastBattlerIdList;
        }

        public List<SymbolResultInfo> CurrentRecordInfos(int stageId,int seek,int worldNo) => _symbolRecordList.FindAll(a => a.StageSymbolData.StageId == stageId && a.StageSymbolData.Seek == seek && a.WorldNo == worldNo);
        
        // ステージシンボルの結果
        private List<SymbolResultInfo> _symbolRecordList = new ();
        public List<SymbolResultInfo> SymbolRecordList => _symbolRecordList;
        public void SetSymbolResultInfo(SymbolResultInfo symbolResultInfo,bool checkFindIndex = true)
        {
            if (checkFindIndex)
            {
                var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo));
                if (findIndex > -1)
                {
                    _symbolRecordList.RemoveAt(findIndex);
                }
            }
            _symbolRecordList.Add(symbolResultInfo);
            _symbolRecordList.Sort((a,b) => a.SortKey() - b.SortKey() > 0 ? 1 : -1);
        }
        public void SetSelectSymbol(SymbolResultInfo symbolResultInfo,bool isSelect)
        {
            var record = _symbolRecordList.Find(a => a.IsSameSymbol(symbolResultInfo));
            record.SetSelected(isSelect);
            SetSymbolResultInfo(record);
        }

        private List<SymbolResultInfo> EnableResultInfos(int stageId,int seek,int worldNo)
        {
            return _symbolRecordList.FindAll(a => a.EnableStage(stageId,seek,worldNo));
        }

        public void SetLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            var actorInfo = _actorInfos.Find(a => a.ActorId == levelUpInfo.ActorId);
            actorInfo?.SetLevelUpInfo(levelUpInfo);
        }

        private int LevelLinkValue(List<ActorInfo> actorInfos)
        {
            if (EnableLvLink() && actorInfos.Count > 5)
            {
                actorInfos.Sort((a,b) => a.Level - b.Level > 0 ? -1 : 1);
                var targetActor = actorInfos[4];
                return targetActor.Level;
            }
            return 0;
        }

        public void ClearData()
        {
            _actorInfos.Clear();
            _currency = 0;
            _symbolRecordList.Clear();
            _scorePrizeInfos.Clear();
        }

        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
        
        public List<ActorInfo> CurrentActorInfos(int stageId,int seek,int worldNo)
        {
            var actorIdList = CurrentActorIdList(stageId,seek,worldNo);
            var actorInfos = new List<ActorInfo>();
            foreach (var actorId in actorIdList)
            {
                var actorInfo = _actorInfos.Find(a => a.ActorId == actorId);
                if (actorInfo != null)
                {
                    actorInfo.SetAddTiming(AddTimingText(actorInfo));
                    actorInfos.Add(actorInfo);
                }
            }
            var levelLinkValue = LevelLinkValue(actorInfos);
            actorInfos.ForEach((ActorInfo a) => a.SetLevelLinked(false));
            if (actorInfos.Count > 5)
            {
                actorInfos.Sort((a,b) => a.Level - b.Level > 0 ? -1 : 1);
                for (int i = 0;i < actorInfos.Count;i++)
                {
                    actorInfos[i].SetLevelLinked(i > 4);
                }
            }
            foreach (var actorInfo in actorInfos)
            {
                actorInfo?.SetLevelLink(levelLinkValue);
            }
            return actorInfos;
        }

        private string AddTimingText(ActorInfo actorInfo)
        {
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol() && a.Selected);
            var find = records.Find(a => a.SymbolInfo.GetItemInfos.Find(b => b.GetFlag && b.Param2 == actorInfo.ActorId) != null);
            if (find != null)
            {
                if (find.StageId == 0)
                {
                    return DataSystem.GetText(390);
                }
                return find.StageId + "-" + find.Seek + "~";
            }
            return DataSystem.GetText(391);
        }

        public List<int> CurrentActorIdList(int stageId,int seek,int worldNo)
        {
            var actorIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.SymbolInfo.IsActorSymbol() && a.Selected);
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    if (getItemInfo.GetFlag)
                    {
                        var actorId = getItemInfo.Param2;
                        if (!actorIdList.Contains(actorId))
                        {
                            actorIdList.Add(actorId);
                        }
                    }
                }
            }
            return actorIdList;
        }
        
        public List<int> PastActorIdList(int stageId,int seek,int worldNo)
        {
            var actorIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.SymbolInfo.IsActorSymbol());
            foreach (var record in records)
            {
                foreach (var getItemInfo in record.SymbolInfo.GetItemInfos)
                {
                    if (getItemInfo.GetFlag)
                    {
                        var actorId = getItemInfo.Param2;
                        if (!actorIdList.Contains(actorId))
                        {
                            actorIdList.Add(actorId);
                        }
                    }
                }
            }
            return actorIdList;
        }

        public List<int> CurrentAlchemyIdList(int stageId,int seek,int worldNo)
        {
            var alchemyIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.Selected);
            foreach (var record in records)
            {
                var getItemInfos = record.SymbolInfo.GetItemInfos.FindAll(a => a.GetFlag && a.IsSkill());
                foreach (var getItemInfo in getItemInfos)
                {
                    if (DataSystem.FindSkill(getItemInfo.Param1).Rank < RankType.RelicRank1)
                    {
                        alchemyIdList.Add(getItemInfo.Param1);
                    }
                }
            }
            return alchemyIdList;
        }

        public List<int> CurrentAlcanaIdList(int stageId,int seek,int worldNo)
        {
            var alcanaIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.Selected);
            foreach (var record in records)
            {
                var getItemInfos = record.SymbolInfo.GetItemInfos.FindAll(a => a.GetFlag && a.IsSkill());
                foreach (var getItemInfo in getItemInfos)
                {
                    if (DataSystem.FindSkill(getItemInfo.Param1).Rank >= RankType.RelicRank1)
                    {
                        alcanaIdList.Add(getItemInfo.Param1);
                    }
                }
            }
            return alcanaIdList;
        }

        // 所持している魔法全てのId
        public List<int> CurrentAllSkillIds(int stageId,int seek,int worldNo)
        {
            var skillIds = new List<int>();
            var alchemyIds = CurrentAlchemyIdList(stageId,seek,worldNo);
            var actorInfos = CurrentActorInfos(stageId,seek,worldNo);
            foreach (var actorInfo in actorInfos)
            {
                var skillInfos = actorInfo.SkillActionList(alchemyIds);
                foreach (var skillInfo in skillInfos)
                {
                    skillIds.Add(skillInfo.Id);
                }
            }
            foreach (var alchemyId in alchemyIds)
            {
                skillIds.Add(alchemyId);
            }
            return skillIds;
        }

        public int ActorLevelReset(ActorInfo actorInfo)
        {
            return actorInfo.ActorLevelReset();
        }

        public (int,int) LastStageIdTurns()
        {
            var stageId = 0;
            foreach (var symbolResultInfo in _symbolRecordList)
            {
                if (symbolResultInfo.Selected)
                {
                    if (symbolResultInfo.StageSymbolData.StageId > stageId)
                    {
                        stageId = symbolResultInfo.StageSymbolData.StageId;
                    }
                }
            }
            var currentTurn = 0;
            foreach (var symbolResultInfo in _symbolRecordList)
            {
                if (symbolResultInfo.Selected)
                {
                    if (symbolResultInfo.StageSymbolData.StageId == stageId)
                    {
                        if (symbolResultInfo.StageSymbolData.Seek > currentTurn)
                        {
                            currentTurn = symbolResultInfo.StageSymbolData.Seek;
                        }
                    }
                }
            }
            return (stageId,currentTurn);
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

        public int TotalScore()
        {
            var score = 0;
            foreach (var record in SymbolRecordList)
            {
                if (record._selected)
                {
                    score += record.BattleScore;
                }
            }
            return score;
        }

        public void UseParallel()
        {
            var find = _scorePrizeInfos.Find(a => a.EnableParallel());
            if (find != null)
            {
                find.UseParallel();
            }
        }

        // クリア情報
        private Dictionary<int,int> _stageClearDict = new ();
        public Dictionary<int,int> StageClearDict => _stageClearDict;
        public int ClearCount(int stageId)
        {
            if (_stageClearDict.ContainsKey(stageId))
            {
                return _stageClearDict[stageId];
            }
            return 0;
        }

        private void ClearStageClearCount()
        {
            _stageClearDict.Clear();
        }

        private void InitStageClearCount()
        {
            var stageDates = DataSystem.Stages;
            foreach (var stageData in stageDates)
            {
                if (stageData.Selectable)
                {
                    if (!_stageClearDict.ContainsKey(stageData.Id))
                    {
                        _stageClearDict[stageData.Id] = 0;
                    }
                }
            }
        }

        public void StageClear(int stageId)
        {
            if (!_stageClearDict.ContainsKey(stageId))
            {
                _stageClearDict[stageId] = 0;
            }
            _stageClearDict[stageId]++;
        }
    }
}