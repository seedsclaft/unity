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

            _view.InitActors();
            _view.InitResultList(GetListData(_model.ResultCommand()));
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            var bgm = await _model.GetBgmData(_model.TacticsBgmKey());
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.SetEvent((type) => UpdateCommand(type));

            CommandStartStrategy();
            _busy = false;
        }

        private void UpdateCommand(StrategyViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case CommandType.StartStrategy:
                CommandStartStrategy();
                break;
                case CommandType.EndAnimation:
                CommandEndAnimation();
                break;
                case CommandType.CallEnemyInfo:
                CommandCallEnemyInfo();
                break;
                case CommandType.PopupSkillInfo:
                CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                break;
                case CommandType.ResultClose:
                CommandResultClose((SystemData.CommandData)viewEvent.template);
                break;
                case CommandType.EndLvUpAnimation:
                    NextSeekResult();
                break;
                case CommandType.LvUpNext:
                    CommandLvUpNext();
                    break;
                case CommandType.SelectAlcanaList:
                    CommandSelectAlcanaList((SkillInfo)viewEvent.template);
                    break;
            }
        }

        private void CommandStartStrategy()
        {
            if (_model.BattleResult)
            {
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
                SeekStrategyState();
            } else
            {
                EndStrategy();
            }
        }

        private void SeekStrategyState()
        {
            if (_model.BattleResult)
            {
                var battledResultActors = _model.BattleResultActors();
                var bonusList = new List<bool>();
                foreach (var item in battledResultActors)
                {
                    bonusList.Add(false);
                }
                // 勝利時
                if (_model.BattleResultVictory)
                {
                    _model.MakeLvUpData();
                    _model.MakeSelectRelicData();
                    _model.MakeResult();
                    _view.SetTitle(DataSystem.GetText(14030));
                    _view.StartResultAnimation(_model.MakeListData(battledResultActors),bonusList);
                    if (_model.LevelUpData.Count > 0)
                    {
                        _view.StartLvUpAnimation();
                        _view.HideResultList();
                    }
                } else
                {
                    _view.SetTitle(DataSystem.GetText(14030));
                    _view.StartResultAnimation(_model.MakeListData(battledResultActors),bonusList);
                }
            } else
            {
                var tacticsActors = _model.TacticsActors();
                _model.MakeResult();
                var bonusList = new List<bool>();
                foreach (var item in tacticsActors)
                {
                    bonusList.Add(_model.IsBonusTactics(item.ActorId));
                }
                _view.SetTitle(DataSystem.GetText(14020));
                _view.StartResultAnimation(_model.MakeListData(tacticsActors),bonusList);
            }
        }

        private void CommandEndAnimation()
        {
            if (_model.BattleResult)
            {
                if (_model.BattleResultVictory)
                {
                    if (_model.LevelUpData.Count == 0)
                    {
                        ShowResultList();
                    }
                } else
                {
                    ShowResultList();
                }
            } else
            {
                NextSeekResult();
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
                learnSkillInfo.SetToValue(_model.LevelUpData[0].Evaluate());
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        _model.RemoveLevelUpData();
                        NextSeekResult();
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            } else
            {
                _model.RemoveLevelUpData();
                NextSeekResult();
            }
        }

        private void NextSeekResult()
        {
            if (_model.LevelUpData.Count > 0)
            {
                CommandEndLvUpAnimation();
                return;
            }
            if (_model.RelicData.Count > 0)
            {
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.RelicData));
                return;
            }
            ShowResultList();
        }

        private void ShowResultList()
        {
            _view.ShowResultList(GetListData(_model.ResultViewInfos),_model.BattleSaveHumanResultInfo(),_model.BattleResultTurn(),_model.BattleResultScore());
        }

        private void CommandResultClose(SystemData.CommandData commandData)
        {
            if (commandData.Key == "Yes")
            {
                if (_model.BattleResult)
                {
                    var battledMembers = _model.BattleResultActors();
                    if (battledMembers != null && battledMembers.Count > 0)
                    {
                        _model.ClearBattleData(battledMembers);
                        _model.ClearSceneParam();
                    }
                } else
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
                if (_model.BattleResult && _model.BattleResultVictory == false)
                {
                    _model.ReturnTempBattleMembers(); 
                    _view.CommandChangeViewToTransition(null);  
                    // ボス戦なら
                    if (_model.CurrentSelectRecord().SymbolType == SymbolType.Boss)
                    {
                        //SoundManager.Instance.FadeOutBgm();
                        PlayBossBgm();
                    } else
                    {
                        var bgmData = DataSystem.Data.GetBGM(_model.TacticsBgmKey());
                        if (bgmData.CrossFade != "" && SoundManager.Instance.CrossFadeMode)
                        {
                            SoundManager.Instance.ChangeCrossFade();
                        } else
                        {
                            PlayTacticsBgm();
                        }
                    }      
                    _view.CommandGotoSceneChange(Scene.Battle);
                } else
                {
                    ShowStatus();
                }
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }


        private void CommandSelectAlcanaList(SkillInfo skillInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11140),(a) => UpdateSelectAlcana((ConfirmCommandType)a),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // アルカナ選択
                var alcanaSelect = _view.AlcanaSelectSkillInfo();
                _model.MakeSelectRelic(alcanaSelect.Id);
                _view.HideAlcanaList();
                NextSeekResult();
            }
        }
        
        private void CommandContinue()
        {
            // ロスト判定
            var lostMembers = _model.LostMembers();
            // コンテニュー判定
            if (_model.BattleResultVictory == true && lostMembers.Count == 0)
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
            var confirmInfo = new ConfirmInfo(continuePopupTitle,(a) => UpdatePopupContinueCommand((ConfirmCommandType)a));
                        
            confirmInfo.SetSelectIndex(1);
            if (needAdsContinue)
            {
                //popupInfo.SetDisableIds(new List<int>(){1});
                confirmInfo.SetCommandTextIds(_model.SaveAdsCommandTextIds());
            } else
            {
            }
            _view.CommandCallConfirm(confirmInfo);
            //_view.ChangeUIActive(false);
        }

        private void UpdatePopupContinueCommand(ConfirmCommandType confirmCommandType)
        {
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
                            var confirmInfo = new ConfirmInfo(savePopupTitle,(a) => UpdatePopupContinueCommand((ConfirmCommandType)a));
                            _view.CommandCallConfirm(confirmInfo);
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
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetText(14010));
            });
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo("",(a) => {});
            confirmInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandCallEnemyInfo()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);  
            var enemyIndex = _model.CurrentStage.CurrentSeekIndex;
            var enemyInfos = _model.TacticsSymbols()[enemyIndex].SymbolInfo.BattlerInfos();
            _busy = true;
            CommandEnemyInfo(enemyInfos,false,() => 
            { 
                SetHelpInputSkipEnable();
                _view.SetHelpText(DataSystem.GetText(14010));
                _busy = false;
            });  
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
            if (_model.PartyInfo.ReturnSymbol != null)
            {
                _model.SetSelectSymbol();
                //_model.EndStrategy();
                if (_model.CurrentStage.ParallelStage)
                {
                    _model.CommitCurrentParallelResult();
                } else
                {
                    _model.CommitCurrentResult();
                }
                _view.CommandGotoSceneChange(Scene.Tactics);
            } else
            if (_model.BattleResult && _model.BattleResultVictory == false)
            {
                // 敗北して戻る
                _model.ReturnTempBattleMembers();
                _model.EndStrategy();
                var tacticsSceneInfo = new TacticsSceneInfo
                {
                    ReturnBeforeBattle = true,
                    SeekIndex = _model.CurrentStage.CurrentSeekIndex
                };
                _view.CommandGotoSceneChange(Scene.Tactics,tacticsSceneInfo);
            } else
            {
                // レコード新規保存
                _model.SetSelectSymbol();
                _model.EndStrategy();
                if (_model.RemainTurns == 1)
                {
                    _model.EndStage();
                    _view.CommandGotoSceneChange(Scene.MainMenu);
                } else
                {
                    _model.SeekStage();
                    var tacticsSceneInfo = new TacticsSceneInfo
                    {
                        ReturnNextBattle = true,
                        SeekIndex = _model.CurrentStage.CurrentSeekIndex
                    };
                    _view.CommandGotoSceneChange(Scene.Tactics,tacticsSceneInfo);
                }
            }
            //_model.TempInfo.ClearTempGetItemInfos();
        }
    }
}