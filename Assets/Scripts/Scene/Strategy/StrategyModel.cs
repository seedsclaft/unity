using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyModel : BaseModel
    {
        private StrategySceneInfo _sceneParam;
        public StrategySceneInfo SceneParam => _sceneParam;

        private bool _battleResult = false;
        public bool BattleResult => _battleResult;
        public StrategyModel()
        {
            _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            _battleResult = _sceneParam.ActorInfos.FindAll(a => a.BattleIndex >= 0).Count > 0;
        }
        public void ClearSceneParam()
        {
            _sceneParam = null;
        }
        private List<TacticsResultInfo> _resultInfos = new();
        

        private List<ActorInfo> _levelUpData = new();
        public List<ActorInfo> LevelUpData => _levelUpData;
        private List<LearnSkillInfo> _learnSkillInfo = new();
        public List<LearnSkillInfo> LearnSkillInfo => _learnSkillInfo;
        public List<ListData> LevelUpActorStatus(int index)
        {
            var list = new List<ListData>();
            var listData = new ListData(_levelUpData[0]);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            list.Add(listData);
            return list;
        }

        private List<ListData> _resultItemInfos = new();
        public List<ListData> ResultGetItemInfos => _resultItemInfos;

        public List<ActorInfo> TacticsActors()
        {
            if (SceneParam != null)
            {
                return SceneParam.ActorInfos.FindAll(a => a.BattleIndex == -1);
            }
            return null;
        }

        public void SetLvUp()
        {
            if (_levelUpData.Count > 0) return;
            if (CurrentStage.ReturnSeek > 0) return;
            //var lvUpActorInfos = TacticsActors().FindAll(a => a.TacticsCommandType == TacticsCommandType.Train);
            var lvUpList = new List<ActorInfo>();
            // 結果出力
            foreach (var lvUpActorInfo in BattleMembers())
            {
                // 新規魔法取得があるか
                var skills = lvUpActorInfo.LearningSkills(1);
                var from = lvUpActorInfo.Evaluate();
                lvUpActorInfo.LevelUp(0);
                var to = lvUpActorInfo.Evaluate();
                if (skills.Count > 0)
                {
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    _learnSkillInfo.Add(learnSkillInfo);
                } else
                {
                    _learnSkillInfo.Add(null);
                }
                lvUpList.Add(lvUpActorInfo);
            }
            _levelUpData = lvUpList;
        }

        public void MakeResult()
        {
            var getItemInfos = SceneParam.GetItemInfos;
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
            var beforeRecord = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
            
            foreach (var getItemInfo in getItemInfos)
            {
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        if (beforeRecord == null || (beforeRecord != null && beforeRecord.Cleared == false))
                        {
                            PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                        }
                        break;
                    case GetItemType.Skill:
                        break;
                    case GetItemType.Regeneration:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.ChangeHp(stageMember.CurrentHp + getItemInfo.Param1);
                                stageMember.ChangeMp(stageMember.CurrentMp + getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.Demigod:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.GainDemigod(getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.StatusUp:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.LevelUp(0);
                            }
                        }
                        break;
                    case GetItemType.AddActor:
                        break;
                    case GetItemType.SaveHuman:
                        
                        var rate = PartyInfo.BattleResultScore * 0.01f;
                        rate *= getItemInfo.Param1;
                        getItemInfo.SetParam2((int)rate);
                        getItemInfo.MakeTextData();
                        record.SetBattleScore((int)rate);
                        break;
                }
            }
            // クリアフラグを立てる
            record.SetCleared(true); 
            
            if (CurrentStage.ReturnSeek > 0)
            {
                foreach (var actorInfo in TempInfo.TempRecordActors)
                {
                    // 強さ差分を適用
                    var recordActor = Actors().Find(a => a.ActorId == actorInfo.ActorId);
                    if (recordActor != null)
                    {
                        var levelUpCost = recordActor.LevelUpCost;
                        actorInfo.GainLevelUpCost(levelUpCost);
                        var levelUpByCost = actorInfo.LevelUpByCost();
                        if (levelUpByCost > 0)
                        {
                            actorInfo.LevelUp(levelUpByCost-1);
                            var getItemData = new GetItemData();
                            var getItemInfo = new GetItemInfo(getItemData);
                            getItemInfo.MakeActorLvUpResult(actorInfo.Master.Name,actorInfo.Level);
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }

                _resultItemInfos = ListData.MakeListData(getItemInfos);
            } else
            {
                _resultItemInfos = ListData.MakeListData(getItemInfos);
            }
        }

        public void DecideStrength()
        {
        }

        public void RemoveLevelUpData()
        {
            _levelUpData.RemoveAt(0);
            _learnSkillInfo.RemoveAt(0);
        }

        public bool BattleResultVictory()
        {
            return PartyInfo.BattleResultVictory;
        }

        public List<ListData> BattleResultInfos()
        {
            return MakeListData(SceneParam.GetItemInfos);
        }

        public List<ActorInfo> BattleResultActors()
        {
            return SceneParam.ActorInfos.FindAll(a => a.BattleIndex >= 0);
        }

        public void ClearBattleData(List<ActorInfo> actorInfos)
        {
            foreach (var actorInfo in actorInfos)
            {
                if (actorInfo.BattleIndex >= 0)
                {
                    actorInfo.SetBattleIndex(-1);
                }
            }
        }

        public List<ActorInfo> LostMembers()
        {
            return BattleResultActors().FindAll(a => a.BattleIndex >= 0 && a.CurrentHp == 0);
        }

        public List<ListData> ResultCommand()
        {
            if (_battleResult && BattleResultVictory() == false)
            {
                return MakeListData(BaseConfirmCommand(3040,3054)); // 再戦
            }
            return MakeListData(BaseConfirmCommand(3040,4));
        }

        public bool IsBonusTactics(int actorId)
        {
            var result = _resultInfos.Find(a => a.ActorId == actorId);
            if (result != null)
            {
                return result.IsBonus;
            }
            return false;
        }
        
        public void EndStrategy()
        {
            foreach (var actorInfo in StageMembers())
            {
                actorInfo.ChangeTacticsCostRate(1);
            }
            CurrentStage.SetSeekIndex(0);
        }

        public void SeekStage()
        {
            CurrentStage.SeekStage();
        }

        public void SetSelectSymbol()
        {
            // レコード作成
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectSymbol()));
            record.SetSelected(true);
            PartyInfo.SetSymbolResultInfo(record);
        }

        public void CommitResult()
        {
            CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
            SavePlayerData();
            SavePlayerStageData(false);
        }

        public void CommitCurrentResult()
        {
            // 新たに選択したシンボル
            var newSymbolInfo = CurrentSelectSymbol();
            
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
            foreach (var record in records)
            {
                if (record.IsSameSymbol(newSymbolInfo))
                {
                    record.SetSelected(true);
                }
            }
            newSymbolInfo.SetSelected(true);
            // 選択から外れたシンボル
            var removeSymbolInfos = records.FindAll(a => a.Selected && !a.IsSameSymbol(newSymbolInfo));
            foreach (var removeSymbolInfo in removeSymbolInfos)
            {
                removeSymbolInfo.SetSelected(false);
            }

            // 復帰処理
            PartyInfo.InitActorInfos();
            foreach (var actorInfo in TempInfo.TempRecordActors)
            {
                PartyInfo.UpdateActorInfo(actorInfo);
            }

            TempInfo.ClearRecordActors();
            CurrentStage.SetCurrentTurn(CurrentStage.ReturnSeek);
            CurrentStage.SetReturnSeek(-1);
            //SavePlayerData();
            //SavePlayerStageData(false);
        }

        public void CommitCurrentParallelResult()
        {
            var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
            
            var addAlchemyIdList = new List<int>();
            var removeAlchemyIdList = new List<int>();

            var addActorIdList = new List<int>();
            var removeActorIdList = new List<int>();
            // 差分確認
            foreach (var beforeRecord in beforeRecords)
            {
                var currentRecord = records.Find(a => a.IsSameSymbol(beforeRecord));
                if (beforeRecord.Selected != currentRecord.Selected)
                {
                    var symbol = PartyInfo.CurrentSymbolInfos(currentRecord.Seek)[currentRecord.SeekIndex];
                    if (symbol.SymbolType == SymbolType.Alcana)
                    {
                        foreach (var getItemInfo in symbol.GetItemInfos)
                        {
                            if (currentRecord.Selected)
                            {
                                // 増える
                                addAlchemyIdList.Add(getItemInfo.Param1);
                            } else
                            {
                                //removeAlchemyIdList.Add(getItemInfo.Param1);
                            }
                        }
                    }
                    if (symbol.SymbolType == SymbolType.Actor)
                    {
                        foreach (var getItemInfo in symbol.GetItemInfos)
                        {
                            if (currentRecord.Selected)
                            {
                                // 増える
                                addActorIdList.Add(getItemInfo.Param1);
                            } else
                            {
                                //removeActorIdList.Add(getItemInfo.Param1);
                            }
                        }
                    }
                }
            }

            // データ復元
            records.Sort((a,b)=> a.SeekIndex > b.SeekIndex ? 1 : -1);
            foreach (var symbolResultInfo in records)
            {
                var beforeRecord = beforeRecords.Find(a => a.IsSameSymbol(symbolResultInfo));
                if (beforeRecord != null)
                {
                    if (beforeRecord.Cleared)
                    {
                        symbolResultInfo.SetCleared(true);
                    }
                    if (beforeRecord.Selected)
                    {
                        symbolResultInfo.SetSelected(true);
                    }
                }
                PartyInfo.SetSymbolResultInfo(symbolResultInfo);
            }

            PartyInfo.InitActorInfos();
            foreach (var actorInfo in TempInfo.TempRecordActors)
            {
                PartyInfo.UpdateActorInfo(actorInfo);
            }
            
            // 後のレコードを書き換え
            var afterRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId >= CurrentStage.Id && a.Seek > CurrentStage.CurrentTurn);
            foreach (var symbolResultInfo in afterRecords)
            {
                PartyInfo.SetSymbolResultInfo(symbolResultInfo);
            }

            TempInfo.ClearRecordActors();
            SavePlayerData();
            SavePlayerStageData(false);
        }

        public bool ChainParallelMode()
        {
            var chain = false;
            var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
            foreach (var record in beforeRecords)
            {
                if (record.Selected == false)
                {
                    chain = true;
                }
            }
            return chain;
        }

        public bool EnableBattleSkip()
        {
            // スキップ廃止
            return false;
            //return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
        }



        public void ReturnTempBattleMembers()
        {
            foreach (var tempActorInfo in TempInfo.TempActorInfos)
            {
                tempActorInfo.SetBattleIndex(-1);
                PartyInfo.UpdateActorInfo(tempActorInfo);
            }
            TempInfo.ClearBattleActors();
        }
    }

    public class StrategySceneInfo
    {
        public List<GetItemInfo> GetItemInfos;
        public List<ActorInfo> ActorInfos;
    }
}