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

        public int GetCurrency(int stageId,int seek,WorldType worldType)
        {
            var currency = 0;
            var records = EnableResultInfos(stageId,seek,worldType);
            foreach (var resultInfo in records)
            {
                var getItemInfos = resultInfo.SymbolInfo.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.Numinous);
                foreach (var getItemInfo in getItemInfos)
                {
                    currency += getItemInfo.ResultParam;
                }
            }
            var consume = 0;
            var actorInfos = CurrentActorInfos(stageId,seek,worldType);
            foreach (var actorInfo in actorInfos)
            {
                foreach (var levelUpInfo in actorInfo.LevelUpInfos)
                {
                    if (levelUpInfo.IsEnableStage(stageId,seek,worldType))
                    {
                        consume += levelUpInfo.Currency;
                    }
                }
            }
            return currency - consume;
        }
        public int ParallelCount => _scorePrizeInfos.FindAll(a => a.Used == false && a.EnableParallel()).Count;
        private int _stageStockCount = 99;
        public int StageStockCount => _stageStockCount;
        // スコア報酬リスト
        private List<ScorePrizeInfo> _scorePrizeInfos = new ();
        public List<ScorePrizeInfo> ScorePrizeInfos => _scorePrizeInfos;
        public void UpdateScorePrizeInfos(WorldType worldType)
        {
            _scorePrizeInfos.ForEach(a => a.UpdateGetFlag(TotalScore(worldType)));
        }
        
        public List<ScorePrizeInfo> CheckGainScorePrizeInfos()
        {
            return _scorePrizeInfos.FindAll(a => a.CheckFlag());
        }

        public bool RemakeHistory()
        {
            return true;//_scorePrizeInfos.Find(a => a.RemakeHistory()) != null; 
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

        // 戻り先の始点のシンボル
        private StageSymbolData _brunchSymbol = null;
        public StageSymbolData BrunchSymbol => _brunchSymbol;
        private StageSymbolData _brunchBaseSymbol = null;
        public StageSymbolData BrunchBaseSymbol => _brunchBaseSymbol;
        public void SetBrunchStageIdSeek(int stageId,int seek,bool writeBase) 
        {
            var symbolData = new StageSymbolData
            {
                StageId = stageId,
                Seek = seek
            };
            _brunchSymbol = symbolData;
            if (writeBase)
            {
                _brunchBaseSymbol = symbolData;
            }
        }

        public bool NeedEndBrunch()
        {
            if (_returnSymbol == null) return false;
            if (_brunchSymbol == null) return false;

            return _brunchSymbol.StageId == _returnSymbol.StageId && _brunchSymbol.Seek == _returnSymbol.Seek;
        }

        public void ClearBrunch() 
        {
            _returnSymbol = null;
            _brunchSymbol = null;
            _brunchBaseSymbol = null;
        }

        private List<int> _lastBattlerIdList = new();
        public List<int> LastBattlerIdList => _lastBattlerIdList;
        public void SetLastBattlerIdList(List<int> lastBattlerIdList)
        {
            //_lastBattlerIdList = lastBattlerIdList;
        }

        public List<SymbolResultInfo> CurrentRecordInfos(int stageId,int seek,WorldType worldNo) => _symbolRecordList.FindAll(a => a.StageId == stageId && a.Seek == seek && a.WorldNo == worldNo);
        
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

        /// <summary>
        /// ブランチをメインにマージ
        /// </summary>
        /// <param name="symbolResultInfo"></param>
        public void MergeBrunch(SymbolResultInfo symbolResultInfo)
        {
            var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo,WorldType.Main));
            if (findIndex > -1)
            {
                var main = _symbolRecordList[findIndex];
                main.CopyParamData(symbolResultInfo);
                symbolResultInfo.ResetParamData();
                // 元の成長データを削除
                var actorInfos = CurrentActorInfos(symbolResultInfo.StageId,symbolResultInfo.Seek,WorldType.Brunch);
                foreach (var actorInfo in actorInfos)
                {
                    actorInfo.RemoveParamData(symbolResultInfo.StageId,symbolResultInfo.Seek,WorldType.Main);
                }
                // ブランチの成長データをマージ
                foreach (var actorInfo in actorInfos)
                {
                    actorInfo.MargeLevelUpInfo(symbolResultInfo.StageId,symbolResultInfo.Seek,WorldType.Brunch);
                }
            }
        }

        /// <summary>
        /// ブランチのデータリセット
        /// </summary>
        /// <param name="symbolResultInfo"></param>
        public void ReverseBrunch(SymbolResultInfo symbolResultInfo)
        {
            var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo,WorldType.Main));
            if (findIndex > -1)
            {
                symbolResultInfo.ResetParamData();
                // 成長データを削除
                var actorInfos = CurrentActorInfos(symbolResultInfo.StageId,symbolResultInfo.Seek,symbolResultInfo.WorldNo);
                foreach (var actorInfo in actorInfos)
                {
                    actorInfo.RemoveParamData(symbolResultInfo.StageId,symbolResultInfo.Seek,WorldType.Brunch);
                }
            }
        }

        public void ResetBrunchData()
        {
            var resultInfos = SymbolRecordList.FindAll(a => a.WorldNo == WorldType.Brunch);
            foreach (var resultInfo in resultInfos)
            {
                resultInfo.ResetParamData();
            }
        }

        /// <summary>
        /// ブランチのデータリセット
        /// </summary>
        /// <param name="symbolResultInfo"></param>
        public void ResetCurrentLevelUpInfo(int stageId,int seek,WorldType worldType)
        {
            var findIndex = _symbolRecordList.FindIndex(a => a.IsSameStageSeek(stageId,seek,worldType));
            if (findIndex > -1)
            {
                // 成長データを削除
                var actorInfos = CurrentActorInfos(stageId,seek,worldType);
                foreach (var actorInfo in actorInfos)
                {
                    actorInfo.RemoveParamData(stageId,seek,worldType);
                }
            }
        }

        public void SetSelectSymbol(SymbolResultInfo symbolResultInfo,bool isSelect)
        {
            var record = _symbolRecordList.Find(a => a.IsSameSymbol(symbolResultInfo));
            record.SetSelected(isSelect);
            SetSymbolResultInfo(record);
        }

        private List<SymbolResultInfo> EnableResultInfos(int stageId,int seek,WorldType worldNo)
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
        
        public List<ActorInfo> CurrentActorInfos(int stageId,int seek,WorldType worldNo)
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
            var find = records.Find(a => a.SymbolInfo.GetItemInfos.Find(b => b.GetFlag && b.ResultParam == actorInfo.ActorId) != null);
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

        public List<int> CurrentActorIdList(int stageId,int seek,WorldType worldNo)
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
                        var actorId = getItemInfo.ResultParam;
                        if (!actorIdList.Contains(actorId))
                        {
                            actorIdList.Add(actorId);
                        }
                    }
                }
            }
            return actorIdList;
        }
        
        public List<int> PastActorIdList(int stageId,int seek,WorldType worldNo)
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
                        var actorId = getItemInfo.ResultParam;
                        if (!actorIdList.Contains(actorId))
                        {
                            actorIdList.Add(actorId);
                        }
                    }
                }
            }
            return actorIdList;
        }

        public List<int> CurrentAlchemyIdList(int stageId,int seek,WorldType worldNo)
        {
            var alchemyIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.Selected);
            foreach (var record in records)
            {
                var getItemInfos = record.SymbolInfo.GetItemInfos.FindAll(a => a.GetFlag && a.IsSkill());
                foreach (var getItemInfo in getItemInfos)
                {
                    if (DataSystem.FindSkill(getItemInfo.ResultParam).Rank < RankType.RelicRank1)
                    {
                        alchemyIdList.Add(getItemInfo.ResultParam);
                    }
                }
            }
            return alchemyIdList;
        }

        public List<int> CurrentAlcanaIdList(int stageId,int seek,WorldType worldNo)
        {
            var alcanaIdList = new List<int>();
            var records = EnableResultInfos(stageId,seek,worldNo);
            records = records.FindAll(a => a.Selected);
            foreach (var record in records)
            {
                var getItemInfos = record.SymbolInfo.GetItemInfos.FindAll(a => a.GetFlag && a.IsSkill());
                foreach (var getItemInfo in getItemInfos)
                {
                    if (DataSystem.FindSkill(getItemInfo.ResultParam).Rank >= RankType.RelicRank1)
                    {
                        alcanaIdList.Add(getItemInfo.ResultParam);
                    }
                }
            }
            return alcanaIdList;
        }

        // 所持している魔法全てのId
        public List<int> CurrentAllSkillIds(int stageId,int seek,WorldType worldNo)
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
                    if (symbolResultInfo.StageId > stageId)
                    {
                        stageId = symbolResultInfo.StageId;
                    }
                }
            }
            var currentTurn = 0;
            foreach (var symbolResultInfo in _symbolRecordList)
            {
                if (symbolResultInfo.Selected)
                {
                    if (symbolResultInfo.StageId == stageId)
                    {
                        if (symbolResultInfo.Seek > currentTurn)
                        {
                            currentTurn = symbolResultInfo.Seek;
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
        }

        public float TotalScore(WorldType worldType)
        {
            var score = 0f;
            var resultInfos = SymbolRecordList.FindAll(a => a.Selected && a.WorldNo == worldType);
            foreach (var record in resultInfos)
            {
                var saveHuman = record.SymbolInfo.GetItemInfos.Find(a => a.GetItemType == GetItemType.BattleScoreBonus);
                if (saveHuman?.ResultParam > 0)
                {
                    score += saveHuman.ResultParam * saveHuman.Param1 * 0.01f;
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