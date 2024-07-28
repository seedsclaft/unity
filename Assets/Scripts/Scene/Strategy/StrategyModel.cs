using System;
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
        private List<StrategyResultViewInfo> _resultInfos = new();
        public List<ListData> ResultViewInfos => MakeListData(_resultInfos);

        private string _saveHumanText = "";
        
        private List<SkillInfo> _relicData = new();
        public List<SkillInfo> RelicData => _relicData;

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
            //list.Add(listData);
            return list;
        }


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

        public void MakeLvUpData()
        {
            if (_levelUpData.Count > 0) return;
            if (PartyInfo.ReturnSymbol != null) return;
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
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

        public void MakeSelectRelicData()
        {
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
            if (record != null && record.SymbolInfo.Cleared) return;
            
            var getItemInfos = SceneParam.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectRelic);
            if (selectRelicInfos.Count > 0)
            {
                _relicData = new List<SkillInfo>();
                foreach (var selectRelicInfo in selectRelicInfos)
                {
                    var skillInfo = new SkillInfo(selectRelicInfo.Param1);
                    skillInfo.SetEnable(true);
                    _relicData.Add(skillInfo);
                }
            }
        }

        public void MakeResult()
        {
            var getItemInfos = SceneParam.GetItemInfos;
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
            
            var recordScore = TempInfo.BattleResultScore * 0.01f;
            //record.ClearSelectedIndex();
            foreach (var getItemInfo in getItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        getItemInfo.SetGetFlag(true);
                        if (_battleResult)
                        { 
                            var beforeGain = getItemInfo.Param2;
                            var gainCurrency = (int) Math.Round(getItemInfo.Param1 * recordScore);
                            if (gainCurrency > beforeGain)
                            {
                                getItemInfo.SetParam2(gainCurrency);
                                //getItemInfo.MakeTextData();
                            }
                            PartyInfo.ChangeCurrency(Currency + gainCurrency - beforeGain);
                            
                            // 獲得+Nu
                            resultInfo.SetTitle("+" + getItemInfo.Param1.ToString() + DataSystem.GetText(1000));
                            _resultInfos.Add(resultInfo);
                            // 損失-Nu
                            var minusCurrency = getItemInfo.Param1 - gainCurrency;
                            if (minusCurrency > 0)
                            {
                                var minusResultInfo = new StrategyResultViewInfo();
                                minusResultInfo.SetTitle("評価損失: -" + minusCurrency.ToString() + DataSystem.GetText(1000));
                                _resultInfos.Add(minusResultInfo);
                            }
                        } else
                        {
                            // 獲得+Nu
                            resultInfo.SetTitle("+" + getItemInfo.Param1.ToString() + DataSystem.GetText(1000));
                            _resultInfos.Add(resultInfo);
                            PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                        }

                        break;
                    case GetItemType.Skill:
                        getItemInfo.SetGetFlag(true);
                        // 魔法取得
                        var skillData = DataSystem.FindSkill(getItemInfo.Param1);
                        resultInfo.SetSkillId(skillData.Id);
                        resultInfo.SetTitle(skillData.Name);
                        _resultInfos.Add(resultInfo);
                        break;
                    case GetItemType.Regeneration:
                        /*
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.ChangeHp(stageMember.CurrentHp + getItemInfo.Param1);
                                stageMember.ChangeMp(stageMember.CurrentMp + getItemInfo.Param1);
                            }
                        }
                        */
                        break;
                    case GetItemType.Demigod:
                        break;
                    case GetItemType.StatusUp:
                        break;
                    case GetItemType.AddActor:
                    case GetItemType.SelectAddActor:
                        getItemInfo.SetGetFlag(true);
                        getItemInfo.SetParam2(getItemInfo.Param1);
                        //record.AddSelectedIndex(getItemInfo.Param1);
                        // キャラ加入
                        var actorData = DataSystem.FindActor(getItemInfo.Param1);
                        resultInfo.SetTitle(DataSystem.GetReplaceText(14120,actorData.Name));
                        _resultInfos.Add(resultInfo);
                        break;
                    case GetItemType.SaveHuman:
                        getItemInfo.SetGetFlag(true);
                        var beforeSave = getItemInfo.Param2;
                        var saveHuman = (int)Math.Round(getItemInfo.Param1 * recordScore);
                        if (saveHuman > beforeSave)
                        {
                            getItemInfo.SetParam2(saveHuman);
                            record.SetBattleScore(saveHuman);
                            // 救命人数
                            _saveHumanText = DataSystem.GetReplaceDecimalText(beforeSave) + "→" + DataSystem.GetReplaceDecimalText(saveHuman) + "/" + DataSystem.GetReplaceDecimalText(getItemInfo.Param1);
                        } else
                        {
                            // 救命人数
                            _saveHumanText = DataSystem.GetReplaceDecimalText((int)recordScore) + "/" + DataSystem.GetReplaceDecimalText(getItemInfo.Param1);
                        }
                        break;
                    case GetItemType.SelectRelic:
                        // アルカナ選択の時は既にFlagを変えておく
                        if (getItemInfo.GetFlag == true)
                        {
                            var relicData = DataSystem.FindSkill(getItemInfo.Param1);
                            resultInfo.SetSkillId(relicData.Id);
                            resultInfo.SetTitle(relicData.Name);
                            _resultInfos.Add(resultInfo);
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
                    var resultInfo = new StrategyResultViewInfo();
                    switch(prizeMaster.GetItem.Type)
                    {
                        case GetItemType.RemakeHistory:
                            getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                            resultInfo.SetTitle(DataSystem.GetText(16010));
                            _resultInfos.Add(resultInfo);
                            break;
                        case GetItemType.ParallelHistory:
                            getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                            resultInfo.SetTitle(DataSystem.GetText(16020));
                            _resultInfos.Add(resultInfo);
                            break;
                        case GetItemType.Multiverse:
                            getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                            resultInfo.SetTitle(DataSystem.GetText(16030));
                            _resultInfos.Add(resultInfo);
                            break;
                    }
                }
            }
        }

        public void MakeSelectRelic(int skillId)
        {
            var getItemInfos = SceneParam.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectRelic);
            // 魔法取得
            var selectRelic = selectRelicInfos.Find(a => a.Param1 == skillId);
            foreach (var selectRelicInfo in selectRelicInfos)
            {
                selectRelicInfo.SetGetFlag(false);
            }
            selectRelic.SetGetFlag(true);
            var resultInfo = new StrategyResultViewInfo();
            resultInfo.SetSkillId(skillId);
            resultInfo.SetTitle(DataSystem.FindSkill(skillId).Name);
            _resultInfos.Add(resultInfo);
            _relicData.Clear();
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
            if (!_battleResult)
            {
                return null;
            }
            if (_saveHumanText != "")
            {
                return _saveHumanText;
            }
            return null;
        }

        public string BattleResultTurn()
        {
            if (!_battleResult)
            {
                return null;
            }
            var turn = SceneParam.BattleTurn;
            if (turn > 0)
            {
                return turn.ToString() + "ターン";
            }
            return null;
        }

        public string BattleResultScore()
        {
            if (!_battleResult)
            {
                return null;
            }
            var recordScore = TempInfo.BattleResultScore;
            if (recordScore >= 0)
            {
                return recordScore.ToString() + "%";
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
            /*
            var result = _resultInfos.Find(a => a.ActorId == actorId);
            if (result != null)
            {
                return result.IsBonus;
            }
            */
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
            SetStageSeek();
        }

        public void EndStage()
        {
            StageClear();
            var (stageId,currentTurn) = PartyInfo.LastStageIdTurns();
            if (currentTurn == DataSystem.FindStage(stageId).Turns)
            {
                currentTurn += 1;
            }
            CurrentSaveData.MakeStageData(stageId);
            CurrentStage.SetCurrentTurn(currentTurn);
            SetStageSeek();
        }

        public void SetSelectSymbol()
        {
            PartyInfo.SetSelectSymbol(CurrentSelectRecord(),true);
        }


        public void CommitCurrentResult()
        {
            // 新たに選択したシンボル
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo));
            
            /*foreach (var record in records)
            {
                if (record.IsSameSymbol(newSymbolInfo))
                {
                    record.SetSelected(true);
                }
            }
            */
            PartyInfo.SetSelectSymbol(CurrentSelectRecord(),true);
            // 選択から外れたシンボル
            var removeSymbolInfos = records.FindAll(a => a.Selected && !a.IsSameSymbol(CurrentSelectRecord()));
            foreach (var removeSymbolInfo in removeSymbolInfos)
            {
                removeSymbolInfo.SetSelected(false);
                // 選択から外れたシンボルがバトルならレベルアップを無効にする
                /*
                var levelUpInfos = PartyInfo.LevelUpInfos.FindAll(a => a.StageId == removeSymbolInfo.StageId && a.Seek == removeSymbolInfo.Seek && a.SeekIndex == removeSymbolInfo.SeekIndex);
                foreach (var levelUpInfo in levelUpInfos)
                {
                    levelUpInfo.SetEnable(false);
                }
                */
                // 選択から外れた救命人数を初期化
                foreach (var getItemInfo in removeSymbolInfo.SymbolInfo.GetItemInfos)
                {
                    if (getItemInfo.GetItemType == GetItemType.SaveHuman)
                    {
                        getItemInfo.SetParam2(0);
                    }
                }
            }


            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(-1);
            PartyInfo.ClearReturnStageIdSeek();
            SetStageSeek();
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
            
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo));
            /*foreach (var record in records)
            {
                if (record.IsSameSymbol(newSymbolInfo))
                {
                    record.SetSelected(true);
                }
            }*/
            PartyInfo.SetSelectSymbol(CurrentSelectRecord(),true);
            // 並行世界化回数を増やす
            PartyInfo.UseParallel();

            // 復帰処理
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(-1);
            PartyInfo.ClearReturnStageIdSeek();
            SetStageSeek();
            //PartyInfo.SetStageSymbolInfos(SelectedSymbolInfos(CurrentStage.Id));
        }

        public bool ChainParallelMode()
        {
            var chain = false;
            var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo));
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