using System.Collections.Generic;
using UnityEngine;
using Strategy;
using Ryneus;

namespace Ryneus
{
    public class StrategyPresenter : BasePresenter
    {
        StrategyModel _model = null;
        StrategyView _view = null;

        private bool _busy = true;


        private StrategyState _strategyState = StrategyState.None;
        public StrategyPresenter(StrategyView view)
        {
            _view = view;
            SetView(_view);
            _model = new StrategyModel();
            SetModel(_model);
            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;
            _view.SetHelpWindow();

            _view.SetActors(_model.SceneParam.ActorInfos);
            _view.SetResultList(_model.ResultCommand());
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            var bgm = await _model.GetBgmData(_model.TacticsBgmKey());
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.SetEvent((type) => UpdateCommand(type));

            CommandStartStrategy();
            _busy = false;
        }

        private void UpdateCommand(StrategyViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            Debug.Log(viewEvent.commandType);
            if (viewEvent.commandType == CommandType.StartStrategy)
            {
                CommandStartStrategy();
            }
            if (viewEvent.commandType == CommandType.EndAnimation)
            {
                CommandEndAnimation();
            }
            if (viewEvent.commandType == CommandType.CallEnemyInfo)
            {
                CommandCallEnemyInfo();
            }
            if (viewEvent.commandType == CommandType.PopupSkillInfo)
            {
                CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.ResultClose)
            {
                CommandResultClose((ConfirmCommandType)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.EndLvUpAnimation)
            {
                CommandEndLvUpAnimation();
            }
            if (viewEvent.commandType == CommandType.LvUpNext)
            {
                CommandLvUpNext();
            }
        }

        private void UpdatePopupSkillInfo(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.ClosePopup);
        }

        private void UpdatePopupLost(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            CommandContinue();
            //_model.LostActors(_model.LostMembers());
        }

        private void CommandStartStrategy(){
            var battledResultActors = _model.BattleResultActors();
            if (battledResultActors.Count > 0)
            {
                _strategyState = StrategyState.BattleResult;
                SeekStrategyState();
            } else
            {
                CheckTacticsActors();
            }
        }

        private void CheckTacticsActors()
        {
            var tacticsActors = _model.TacticsActors();
            if (tacticsActors != null && tacticsActors.Count > 0)
            {
                _strategyState = StrategyState.TacticsResult;
                SeekStrategyState();
            } else{
                EndStrategy();
            }
        }

        private void SeekStrategyState()
        {
            if (_strategyState == StrategyState.BattleResult)
            {
                var battledResultActors = _model.BattleResultActors();
                var bonusList = new List<bool>();
                foreach (var item in battledResultActors)
                {
                    bonusList.Add(false);
                }
                // 勝利時
                if (_model.BattleResultVictory())
                {
                    _model.SetLvUp();
                    _model.MakeResult();
                    _view.SetTitle(DataSystem.GetTextData(14030).Text);
                    _view.StartResultAnimation(_model.MakeListData(battledResultActors),bonusList);
                    if (_model.LevelUpData.Count > 0)
                    {
                        _view.StartLvUpAnimation();
                        _view.HideResultList();
                    }
                } else{
                    _view.SetTitle(DataSystem.GetTextData(14030).Text);
                    _view.StartResultAnimation(_model.MakeListData(battledResultActors),bonusList);
                }
            } else
            if (_strategyState == StrategyState.TacticsResult)
            {
                var tacticsActors = _model.TacticsActors();
                _model.MakeResult();
                var bonusList = new List<bool>();
                foreach (var item in tacticsActors)
                {
                    bonusList.Add(_model.IsBonusTactics(item.ActorId));
                }
                _view.SetTitle(DataSystem.GetTextData(14020).Text);
                _view.StartResultAnimation(_model.MakeListData(tacticsActors),bonusList);
            }
        }

        private void CommandEndAnimation()
        {
            if (_strategyState == StrategyState.BattleResult)
            {
                if (_model.BattleResultVictory())
                {
                    if (_model.LevelUpData.Count == 0)
                    {
                        _view.ShowResultList(_model.BattleResultInfos());
                    }
                } else
                {
                    _view.ShowResultList(_model.BattleResultInfos());
                }
            }
            if (_strategyState == StrategyState.TacticsResult)
            {
                if (_model.LevelUpData.Count == 0)
                {
                    _view.ShowResultList(_model.ResultGetItemInfos);
                }
            }
            var stageEvents = _model.StageEvents(EventTiming.StartStrategy);
            foreach (var stageEvent in stageEvents)
            {
                if (stageEvent.Type == StageEventType.CommandDisable)
                {
                    _model.AddEventReadFlag(stageEvent);
                }
                if (stageEvent.Type == StageEventType.NeedUseSp)
                {
                    _model.AddEventReadFlag(stageEvent);
                }
                if (stageEvent.Type == StageEventType.AdvStart)
                {
                    var advInfo = new AdvCallInfo();
                    advInfo.SetLabel(_model.GetAdvFile(stageEvent.Param));
                    advInfo.SetCallEvent(() => {   
                        _busy = false;
                    });
                    _view.CommandCallAdv(advInfo);
                    _model.AddEventReadFlag(stageEvent);
                    _busy = true;
                    break;
                }
            }
        }

        private void CommandEndLvUpAnimation()
        {
            _view.ShowLvUpActor(_model.LevelUpData[0],_model.LevelUpActorStatus(0));
        }

        private void CommandLvUpNext()
        {
            var learnSkillInfo = _model.LearnSkillInfo.Count > 0 ? _model.LearnSkillInfo[0] : null;
            if (learnSkillInfo != null && learnSkillInfo.SkillInfo != null)
            {
                _model.DecideStrength();
                learnSkillInfo.SetToValue(_model.LevelUpData[0].Evaluate());
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
                    
                var popupInfo = new PopupInfo();
                popupInfo.PopupType = PopupType.LearnSkill;
                popupInfo.EndEvent = () => 
                {
                    _model.RemoveLevelUpData();
                    if (_model.LevelUpData.Count > 0)
                    {
                        CommandEndLvUpAnimation();
                    } else
                    {
                        _view.ShowResultList(_model.ResultGetItemInfos);
                    }
                };
                popupInfo.template = learnSkillInfo;
                _view.CommandCallPopup(popupInfo);
            } else
            {
                _model.DecideStrength();
                _model.RemoveLevelUpData();
                if (_model.LevelUpData.Count > 0)
                {
                    CommandEndLvUpAnimation();
                } else
                {
                    _view.ShowResultList(_model.ResultGetItemInfos);
                }
            }
        }

        private void CommandResultClose(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                if (_strategyState == StrategyState.BattleResult)
                {
                    var battledMembers = _model.BattleResultActors();
                    if (battledMembers != null && battledMembers.Count > 0)
                    {
                        _model.ClearBattleData(battledMembers);
                        _model.ClearSceneParam();
                    }
                }
                if (_strategyState == StrategyState.TacticsResult)
                {
                    var tacticsActors = _model.TacticsActors();
                    if (tacticsActors != null && tacticsActors.Count > 0)
                    {
                        _model.ClearBattleData(tacticsActors);
                        _model.ClearSceneParam();
                    }
                }
                CheckTacticsActors();
            } else
            {
                ShowStatus();
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }



        private void CommandContinue()
        {
            // ロスト判定
            var lostMembers = _model.LostMembers();
            // コンテニュー判定
            if (_model.BattleResultVictory() == true && lostMembers.Count == 0)
            {
                return;
            }
            // コンテニュー不可
            if (!_model.EnableContinue())
            {
                _model.LostActors(lostMembers);
                return;
            }
            if (!_model.EnableUserContinue())
            {
                _model.LostActors(lostMembers);
                return;
            }
            var continuePopupTitle = _model.ContinuePopupTitle();
            var needAdsContinue = _model.NeedAdsContinue();
            var popupInfo = new ConfirmInfo(continuePopupTitle,(a) => UpdatePopupContinueCommand((ConfirmCommandType)a));
                        
            popupInfo.SetSelectIndex(1);
            if (needAdsContinue)
            {
                //popupInfo.SetDisableIds(new List<int>(){1});
                popupInfo.SetCommandTextIds(_model.SaveAdsCommandTextIds());
            } else
            {
            }
            _view.CommandCallConfirm(popupInfo);
            //_view.ChangeUIActive(false);
        }

        private void UpdatePopupContinueCommand(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var needAdsContinue = _model.NeedAdsContinue();
                if (needAdsContinue)
                {
                    // ロード表示
                    _view.CommandGameSystem(Base.CommandType.CallLoading);
    #if UNITY_ANDROID
                    AdMobController.Instance.PlayRewardedAd(() => 
                        {
                            SuccessContinue();
                        },
                        () => {
                            // ロード非表示
                            _view.CommandGameSystem(Base.CommandType.CloseLoading);
                            // 失敗した時
                            var savePopupTitle = _model.FailedSavePopupTitle();
                            var popupInfo = new ConfirmInfo(savePopupTitle,(a) => UpdatePopupContinueCommand((ConfirmCommandType)a));
                            _view.CommandCallConfirm(popupInfo);
                        }
                    );
    #endif
                } else
                {
                    SuccessContinue();
                }
            } else
            if (confirmCommandType == ConfirmCommandType.No)
            {
                _model.LostActors(_model.LostMembers());
            }
        }

        private void SuccessContinue()
        {
            // ロード非表示
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _model.GainContinueCount();
            // 復帰して結果をやり直し
            _model.ReturnTempBattleMembers();
            _view.CommandGotoSceneChange(Scene.Strategy);
        }

        private void ShowStatus()
        {
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetTextData(14010).Text);
                _view.ChangeUIActive(true);
            });
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
            var popupInfo = new ConfirmInfo("",(menuCommandInfo) => UpdatePopupSkillInfo((ConfirmCommandType)menuCommandInfo));
            popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(popupInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandCallEnemyInfo()
        {
            var enemyIndex = _model.CurrentStage.CurrentSeekIndex;
            var enemyInfos = _model.TacticsSymbols()[enemyIndex].BattlerInfos();
            
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetTextData(14010).Text);
            });
            statusViewInfo.SetEnemyInfos(enemyInfos,false);
            _view.CommandCallEnemyInfo(statusViewInfo);
            _view.ChangeUIActive(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);    
        }

        private void SetHelpInputSkipEnable()
        {
            if (_model.EnableBattleSkip())
            {
                _view.SetHelpInputInfo("STRATEGY_BATTLE_SKIP");
            } else
            {
                _view.SetHelpInputInfo("STRATEGY_BATTLE");
            }
        }

        private void EndStrategy()
        {
            _view.EndShinyEffect();
            if (_model.CurrentStage.ReturnSeek > 0)
            {
                _model.SetSelectSymbol();
                _model.EndStrategy(false);
                if (_model.CurrentStage.ParallelStage)
                {
                    _model.CommitCurrentParallelResult();
                    if (_model.ChainParallelMode())
                    {
                        _model.MakeSymbolRecordStage(_model.CurrentStage.CurrentTurn-1);
                        _model.SetParallelMode();
                        _view.CommandGotoSceneChange(Scene.Tactics);
                    } else
                    {
                        _view.CommandGotoSceneChange(Scene.Tactics);
                    }
                } else
                {
                    _model.CommitCurrentResult();
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            } else
            if (_strategyState == StrategyState.BattleResult &&_model.BattleResultVictory() == false)
            {
                _model.ReturnTempBattleMembers();
                _model.EndStrategy(false);
                _view.CommandGotoSceneChange(Scene.Tactics);
            } else
            if (_model.RemainTurns == 1)
            {
                _model.SetSelectSymbol();
                _model.EndStrategy(false);
                _model.CommitResult();
                _view.CommandGotoSceneChange(Scene.MainMenu);
            } else
            {
                // レコード新規保存
                _model.SetSelectSymbol();
                _model.EndStrategy(true);
                _view.CommandGotoSceneChange(Scene.Tactics);
            }
            //_model.TempInfo.ClearTempGetItemInfos();
        }

        private enum StrategyState{
            None = 0,
            BattleResult = 1,
            TacticsResult = 2,
        }
    }
}