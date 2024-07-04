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

        public bool ChangeInitActor()
        {
            return _scorePrizeInfos.Find(a => a.ChangeInitActor()) != null;
        }

        private bool _inReplay = false;
        public bool InReplay => _inReplay;
        public void SetInReplay(bool inReplay)
        {
            _inReplay = inReplay;
        }

        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;
        public void SetBattleResultVictory(bool isVictory)
        {
            _battleResultVictory = isVictory;
        }

        private int _battleResultScore = 0;
        public int BattleResultScore => _battleResultScore;
        public void SetBattleScore(int score)
        {
            _battleResultScore = score;
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
            _lastBattlerIdList = lastBattlerIdList;
        }

        public List<SymbolResultInfo> CurrentRecordInfos(int stageId,int seek) => _symbolRecordList.FindAll(a => a.StageSymbolData.StageId == stageId && a.StageSymbolData.Seek == seek);
        
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

        private List<LevelUpInfo> _levelUpInfos = new();
        public List<LevelUpInfo> LevelUpInfos => _levelUpInfos;
        public void SetLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            var findIndex = _levelUpInfos.FindIndex(a => a.IsSameLevelUpInfo(levelUpInfo));
            if (findIndex > -1)
            {
                _levelUpInfos.RemoveAt(findIndex);
            }
            _levelUpInfos.Add(levelUpInfo);
        }

        public void ClearData()
        {
            _actorInfos.Clear();
            _currency = 0;
            _battleResultVictory = false;
            _battleResultScore = 0;
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
        
        public List<ActorInfo> CurrentActorInfos(int stageId,int seek)
        {
            var actorIdList = CurrentActorIdList(stageId,seek);
            var actorInfos = new List<ActorInfo>();
            foreach (var actorId in actorIdList)
            {
                var actorInfo = _actorInfos.Find(a => a.ActorId == actorId);
                if (actorInfo != null)
                {
                    var levelUpInfos = _levelUpInfos.FindAll(a => a.Enable && a.ActorId == actorInfo.ActorId && a.StageId < stageId);
                    var sameStage = _levelUpInfos.FindAll(a => a.Enable && a.ActorId == actorInfo.ActorId && a.StageId == stageId && a.Seek <= seek);
                    levelUpInfos.AddRange(sameStage);
                    actorInfo.SetLevelUpInfo(levelUpInfos);
                    actorInfo.SetAddTiming(AddTimingText(actorInfo));
                    actorInfos.Add(actorInfo);
                }
            }
            return actorInfos;
        }

        private string AddTimingText(ActorInfo actorInfo)
        {
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol() && a.Selected);
            var find = records.Find(a => a.SelectedIndex == actorInfo.ActorId);
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

        public List<LevelUpInfo> AssignedLevelUpInfos(int actorId)
        {
            var actorInfo = _actorInfos.Find(a => a.ActorId == actorId);
            return _levelUpInfos.FindAll(a => a.Enable && a.ActorId == actorInfo.ActorId);
        }
        
        public List<int> CurrentActorIdList(int stageId,int seek)
        {
            var actorIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol() && a.Selected);
            records = records.FindAll(a => a.EnableStage(stageId,seek));
            foreach (var record in records)
            {
                var actorId = record.SelectedIndex;
                if (!actorIdList.Contains(actorId))
                {
                    actorIdList.Add(actorId);
                }
            }
            return actorIdList;
        }
        
        public List<int> PastActorIdList(int stageId,int seek)
        {
            var actorIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.SymbolInfo.IsActorSymbol());
            records = records.FindAll(a => a.EnableStage(stageId,seek));
            foreach (var record in records)
            {
                var actorId = record.SelectedIndex;
                if (!actorIdList.Contains(actorId))
                {
                    actorIdList.Add(actorId);
                }
            }
            return actorIdList;
        }

        public List<int> CurrentAlchemyIdList(int stageId,int seek)
        {
            var alchemyIdList = new List<int>();
            var records = _symbolRecordList.FindAll(a => a.Selected);
            records = records.FindAll(a => a.EnableStage(stageId,seek));
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
            records = records.FindAll(a => a.EnableStage(stageId,seek) && a.SelectedIndex > 0);
            foreach (var record in records)
            {
                var getItemInfos = record.SymbolInfo.GetItemInfos.FindAll(a => a.Param1 == record.SelectedIndex);
                foreach (var getItemInfo in getItemInfos)
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

        public int ActorLevelReset(ActorInfo actorInfo)
        {
            var currency = 0;
            // リセットされる数
            var levelUpDates = _levelUpInfos.FindAll(a => a.IsTrainData() && a.ActorId == actorInfo.ActorId);
            var resetLv = levelUpDates.Count;
            for (int i = levelUpDates.Count-1;i >= 0;i--)
            {
                currency += levelUpDates[i].Currency;
                _levelUpInfos.Remove(levelUpDates[i]);
            }
            return currency;
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
                score += record.BattleScore;
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
    }
}