using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyModel : BaseModel
    {
        private StrategySceneInfo _sceneParam;
        public StrategySceneInfo SceneParam => _sceneParam;
        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;

        private bool _inBattleResult = false;
        public bool InBattleResult => _inBattleResult;
        public StrategyModel()
        {
            _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            _inBattleResult = _sceneParam.InBattle;
            _battleResultVictory = _sceneParam.BattleResultVictory;
        }
        public void ClearSceneParam()
        {
            _sceneParam = null;
        }
        private List<StrategyResultViewInfo> _resultInfos = new();
        public List<StrategyResultViewInfo> ResultViewInfos => _resultInfos;

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
                return SceneParam.ActorInfos;
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
                var levelUpInfo = lvUpActorInfo.LevelUp(0,CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.CurrentSeekIndex,CurrentStage.WorldNo);
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
            var selectRelicInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectRelic);
            if (selectRelicInfo != null)
            {
                var relicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
                _relicData = new List<SkillInfo>();
                foreach (var relicInfo in relicInfos)
                {
                    var skillInfo = new SkillInfo(relicInfo.Param1);
                    skillInfo.SetEnable(true);
                    _relicData.Add(skillInfo);
                }
            }
        }

        public void MakeResult()
        {
            var getItemInfos = SceneParam.GetItemInfos;
            var record = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentSelectRecord()));
            
            var recordScore = SceneParam.BattleResultScore * 0.01f;
            //record.ClearSelectedIndex();
            foreach (var getItemInfo in getItemInfos)
            {
                var resultInfo = new StrategyResultViewInfo();
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        getItemInfo.SetGetFlag(true);
                        if (_inBattleResult)
                        { 
                            var beforeGain = getItemInfo.Param2;
                            var gainCurrency = (int)Math.Round(getItemInfo.Param1 * recordScore);
                            
                            if (beforeGain == 0)
                            {
                                // 初クリア
                                getItemInfo.SetParam2(gainCurrency);
                                PartyInfo.ChangeCurrency(Currency + gainCurrency);
                            } else 
                            if (gainCurrency > beforeGain)
                            {
                                getItemInfo.SetParam2(gainCurrency);
                                PartyInfo.ChangeCurrency(Currency + gainCurrency - beforeGain);
                                //getItemInfo.MakeTextData();
                            }
                            
                            // 獲得+Nu
                            resultInfo.SetTitle("+" + getItemInfo.Param1.ToString() + DataSystem.GetText(1000));
                            _resultInfos.Add(resultInfo);
                            // 損失-Nu
                            var minusCurrency = getItemInfo.Param1 - gainCurrency;
                            if (minusCurrency > 0)
                            {
                                var minusResultInfo = new StrategyResultViewInfo();
                                minusResultInfo.SetTitle(DataSystem.GetText(20050) + minusCurrency.ToString() + DataSystem.GetText(1000));
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
                        resultInfo.SetTitle(DataSystem.GetReplaceText(20200,actorData.Name));
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
                        /*
                        // アルカナ選択の時は既にFlagを変えておく
                        if (getItemInfo.GetFlag == true)
                        {
                            var relicData = DataSystem.FindSkill(getItemInfo.Param1);
                            resultInfo.SetSkillId(relicData.Id);
                            resultInfo.SetTitle(relicData.Name);
                            _resultInfos.Add(resultInfo);
                        }
                        */
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
                            resultInfo.SetTitle(DataSystem.GetText(20100));
                            _resultInfos.Add(resultInfo);
                            break;
                        case GetItemType.ParallelHistory:
                            getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                            resultInfo.SetTitle(DataSystem.GetText(20110));
                            _resultInfos.Add(resultInfo);
                            break;
                        case GetItemType.Multiverse:
                            getItemInfos.Add(new GetItemInfo(prizeMaster.GetItem));
                            resultInfo.SetTitle(DataSystem.GetText(20120));
                            _resultInfos.Add(resultInfo);
                            break;
                    }
                }
            }
        }

        public void MakeSelectRelic(int skillId)
        {
            var getItemInfos = SceneParam.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
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

        public string BattleSaveHumanResultInfo()
        {
            if (!_inBattleResult)
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
            if (!_inBattleResult)
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
            if (!_inBattleResult)
            {
                return null;
            }
            var recordScore = SceneParam.BattleResultScore;
            if (recordScore >= 0)
            {
                return recordScore.ToString() + "%";
            }
            return null;
        }

        public List<ActorInfo> BattleResultActors()
        {
            return SceneParam.ActorInfos;
        }

        public void ClearBattleData(List<ActorInfo> actorInfos)
        {
            foreach (var actorInfo in actorInfos)
            {
                if (actorInfo.BattleIndex >= 0)
                {
                    //actorInfo.SetBattleIndex(-1);
                }
            }
        }

        public List<ActorInfo> LostMembers()
        {
            return BattleResultActors().FindAll(a => a.BattleIndex >= 0 && a.CurrentHp == 0);
        }

        public List<SystemData.CommandData> ResultCommand()
        {
            if (_inBattleResult && _battleResultVictory == false)
            {
                return BaseConfirmCommand(3040,3054); // 再戦
            }
            return BaseConfirmCommand(3040,19040
);
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
            // バトルに敗北しているときは更新しない
            if (InBattleResult && _battleResultVictory == false)
            {

            } else
            {
                // 新たに選択したシンボル
                var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo));
                
                PartyInfo.SetSelectSymbol(CurrentSelectRecord(),true);
                // 選択から外れたシンボル
                var removeSymbolInfos = records.FindAll(a => a.Selected && !a.IsSameSymbol(CurrentSelectRecord()));
                foreach (var removeSymbolInfo in removeSymbolInfos)
                {
                    removeSymbolInfo.SetSelected(false);
                    // 選択から外れた救命人数を初期化
                    foreach (var getItemInfo in removeSymbolInfo.SymbolInfo.GetItemInfos)
                    {
                        if (getItemInfo.GetItemType == GetItemType.SaveHuman)
                        {
                            getItemInfo.SetParam2(0);
                        }
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

        public void CommitCurrentParallelResult()
        {
            // バトルに敗北しているときは更新しない
            if (InBattleResult && _battleResultVictory == false)
            {

            } else
            {
                // 新たに選択したシンボル
                var records = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo));
                
                PartyInfo.SetSelectSymbol(CurrentSelectRecord(),true);
                // 並行世界化回数を増やす
                PartyInfo.UseParallel();
            }

            // 復帰処理
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(-1);
            PartyInfo.ClearReturnStageIdSeek();
            SetStageSeek();
            //PartyInfo.SetStageSymbolInfos(SelectedSymbolInfos(CurrentStage.Id));
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
        public bool InBattle;
        public int BattleResultScore;
        public bool BattleResultVictory;
    }
}