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
                var actorInfos = SceneParam.ActorInfos.FindAll(a => a.BattleIndex == -1);
                var list = new List<ActorInfo>();
                for (int i = 0;i < 5;i++)
                {
                    if (actorInfos.Count > i)
                    {
                        list.Add(actorInfos[i]);
                    }
                }
                return list;
            }
            return null;
        }

        public void SetLvUp()
        {
            if (_levelUpData.Count > 0) return;
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentSaveData.CurrentStage.CurrentSeekIndex));
            if (record != null && record.SymbolInfo.Cleared) return;
            //var lvUpActorInfos = TacticsActors().FindAll(a => a.TacticsCommandType == TacticsCommandType.Train);
            var lvUpList = new List<ActorInfo>();
            // 結果出力
            foreach (var lvUpActorInfo in BattleMembers())
            {
                // 新規魔法取得があるか
                var skills = lvUpActorInfo.LearningSkills(1);
                var from = lvUpActorInfo.Evaluate();
                var levelUpInfo = lvUpActorInfo.LevelUp(0,CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.CurrentSeekIndex);
                PartyInfo.SetLevelUpInfo(levelUpInfo);
                var to = lvUpActorInfo.Evaluate();
                if (skills.Count > 0)
                {
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    _learnSkillInfo.Add(learnSkillInfo);
                    foreach (var skill in skills)
                    {
                        // 作戦項目に追加
                        lvUpActorInfo.AddSkillTriggerSkill(skill.Id);  
                    }
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
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentSaveData.CurrentStage.CurrentSeekIndex));
            var beforeRecord = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentSaveData.CurrentStage.CurrentSeekIndex));
            
            foreach (var getItemInfo in getItemInfos)
            {
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        if (beforeRecord == null || (beforeRecord != null && beforeRecord.SymbolInfo.Cleared == false))
                        {
                            PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                        }
                        break;
                    case GetItemType.Skill:
                        record.SetSelectedIndex(getItemInfo.Param1);
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
                        break;
                    case GetItemType.StatusUp:
                        break;
                    case GetItemType.SelectAddActor:
                        record.SetSelectedIndex(getItemInfo.Param1);
                        break;
                    case GetItemType.SaveHuman:
                        var recordScore = PartyInfo.BattleResultScore * 0.01f;
                        recordScore *= getItemInfo.Param1;
                        getItemInfo.SetParam2((int)recordScore);
                        getItemInfo.MakeTextData();
                        if (beforeRecord != null && beforeRecord.BattleScore < (int)recordScore)
                        {
                            getItemInfo.MakeSaveHumanTextData(beforeRecord.BattleScore);
                            record.SetBattleScore((int)recordScore);
                        } else
                        {
                            record.SetBattleScore((int)recordScore);
                        }
                        break;
                }
            }
            // クリアフラグを立てる
            record.SymbolInfo.SetCleared(true); 
            // スコア報酬を更新
            PartyInfo.UpdateScorePrizeInfos();
            var nexScorePrizeInfos = PartyInfo.CheckGainScorePrizeInfos();
            foreach (var nexScorePrizeInfo in nexScorePrizeInfos)
            {
                foreach (var prizeMaster in nexScorePrizeInfo.PrizeMaster)
                {
                    switch(prizeMaster.GetItem.Type)
                    {
                        case GetItemType.RemakeHistory:
                        case GetItemType.ParallelHistory:
                        getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                        break;
                    }
                }
            }
            _resultItemInfos = ListData.MakeListData(getItemInfos);
        }

        public void RemoveLevelUpData()
        {
            _levelUpData.RemoveAt(0);
            _learnSkillInfo.RemoveAt(0);
        }

        public List<ListData> BattleResultInfos()
        {
            return MakeListData(SceneParam.GetItemInfos.FindAll(a => a.GetItemType != GetItemType.SaveHuman));
        }

        public string BattleSaveHumanResultInfo()
        {
            var result = SceneParam.GetItemInfos.Find(a => a.GetItemType == GetItemType.SaveHuman);
            if (result != null)
            {
                return result.Param2.ToString();
            }
            return null;
        }

        public string BattleResultTurn()
        {
            var result = SceneParam.BattleTurn;
            if (result > 0)
            {
                return result.ToString();
            }
            return null;
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

        public void EndStage()
        {
            var (stageId,currentTurn) = PartyInfo.LastStageIdTurns();
            if (currentTurn == DataSystem.FindStage(stageId).Turns)
            {
                currentTurn += 1;
            }
            CurrentSaveData.MakeStageData(stageId);
            CurrentStage.SetCurrentTurn(currentTurn);
        }

        public void SetSelectSymbol()
        {
            // レコード作成
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
            record.SetSelected(true);
            PartyInfo.SetSymbolResultInfo(record);
        }

        public void CommitResult()
        {
            CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
            //SavePlayerData();
            //SavePlayerStageData(false);
        }

        public void CommitCurrentResult()
        {
            // 新たに選択したシンボル
            var newSymbolInfo = CurrentSelectRecord();
            
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek));
            foreach (var record in records)
            {
                if (record.IsSameSymbol(newSymbolInfo))
                {
                    record.SetSelected(true);
                }
            }
            // 選択から外れたシンボル
            var removeSymbolInfos = records.FindAll(a => a.Selected && !a.IsSameSymbol(newSymbolInfo));
            foreach (var removeSymbolInfo in removeSymbolInfos)
            {
                removeSymbolInfo.SetSelected(false);
                // 選択から外れたシンボルがバトルならレベルアップを無効にする
                var levelUpInfos = PartyInfo.LevelUpInfos.FindAll(a => a.StageId == removeSymbolInfo.StageId && a.Seek == removeSymbolInfo.Seek && a.SeekIndex == removeSymbolInfo.SeekIndex);
                foreach (var levelUpInfo in levelUpInfos)
                {
                    levelUpInfo.SetEnable(false);
                }
            }


            // 復帰処理
            PartyInfo.InitActorInfos();
            foreach (var actorInfo in TempInfo.TempRecordActors)
            {
                PartyInfo.UpdateActorInfo(actorInfo);
            }

            TempInfo.ClearRecordActors();
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(-1);
            PartyInfo.ClearReturnStageIdSeek();
            //PartyInfo.SetStageSymbolInfos(SelectedSymbolInfos(CurrentStage.Id));
        }

        public List<SymbolInfo> SelectedSymbolInfos(int stageId)
        {
            var list = new List<SymbolInfo>();
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageSymbolData.StageId == stageId);
            foreach (var record in records)
            {
                list.Add(record.SymbolInfo);
            }
            return list;
        }

        public void CommitCurrentParallelResult()
        {
            // 新たに選択したシンボル
            var newSymbolInfo = CurrentSelectRecord();
            
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek));
            foreach (var record in records)
            {
                if (record.IsSameSymbol(newSymbolInfo))
                {
                    record.SetSelected(true);
                }
            }
            // 選択から外れたシンボル
            var removeSymbolInfos = records.FindAll(a => a.Selected && !a.IsSameSymbol(newSymbolInfo));
            foreach (var removeSymbolInfo in removeSymbolInfos)
            {
                //removeSymbolInfo.SetSelected(false);
            }
            // 並行世界化回数を増やす
            PartyInfo.UseParallel();

            // 復帰処理
            PartyInfo.InitActorInfos();
            foreach (var actorInfo in TempInfo.TempRecordActors)
            {
                PartyInfo.UpdateActorInfo(actorInfo);
            }

            TempInfo.ClearRecordActors();
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(-1);
            PartyInfo.ClearReturnStageIdSeek();
            //PartyInfo.SetStageSymbolInfos(SelectedSymbolInfos(CurrentStage.Id));
        }

        public bool ChainParallelMode()
        {
            var chain = false;
            var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek));
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
                //tempActorInfo.SetBattleIndex(-1);
                PartyInfo.UpdateActorInfo(tempActorInfo);
            }
            //TempInfo.ClearBattleActors();
        }
    }

    public class StrategySceneInfo
    {
        public int BattleTurn;
        public List<GetItemInfo> GetItemInfos;
        public List<ActorInfo> ActorInfos;
    }
}