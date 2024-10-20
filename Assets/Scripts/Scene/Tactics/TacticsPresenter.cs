using System;
using System.Collections.Generic;
using UnityEngine;
using Tactics;

namespace Ryneus
{
    public partial class TacticsPresenter : BasePresenter
    {
        TacticsModel _model = null;
        TacticsView _view = null;

        private bool _busy = true;
        private bool _eventBusy = false;
        private bool _alcanaSelectBusy = false;
        private bool _shopSelectBusy = false;

        private CommandType _backCommand = CommandType.None;
        public TacticsPresenter(TacticsView view)
        {
            _view = view;
            SetView(_view);
            _model = new TacticsModel();
            SetModel(_model);

            if (CheckAdvEvent())
            {
                return;
            }
            if (CheckBeforeTacticsAdvEvent())
            {
                return;
            }
            CheckStageEvent();
            if (_eventBusy)
            {
                return;
            }
            Initialize();
        }

        private void Initialize()
        {
            InitializeView();
            var isAbort = CheckAdvStageEvent(EventTiming.StartTactics,() => 
            {
                _view.CommandGotoSceneChange(Scene.Tactics);
            },_model.CurrentStage.SelectActorIdsClassId(0));
            if (isAbort)
            {
                return;
            }
            _busy = false;
        }

        private void InitializeView()
        {
            _view.ChangeUIActive(false);
            //_model.AssignBattlerIndex();
            _view.SetHelpWindow();
            _view.ChangeBackCommandActive(false);
            _view.SetEvent((type) => UpdateCommand(type));
            
            _view.SetStageInfo(_model.CurrentStage);
            _view.SetTacticsCommand(_model.TacticsCommand());
            //_view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));
            _view.SetUIButton();
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            _view.SetSymbolRecords(_model.SymbolRecords());
            _view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            _view.SetNuminous(_model.Currency);
            CommandRefresh();
            PlayTacticsBgm();
            _view.ChangeUIActive(true);
            _view.StartAnimation();
            // チュートリアル確認
            //CheckTutorialState();
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 100)
                {
                    // マス一覧を初めて開く
                    checkFlag = _view.SymbolRecordListActive;
                }
                if (tutorialData.Param1 == 200)
                {
                    // 編成を初めて開く
                    checkFlag = commandType == CommandType.SelectSymbol;
                }
                if (tutorialData.Param1 == 300)
                {
                    // トレジャーのマスを初めて開く
                    checkFlag = _model.CurrentStage.Seek == 2;
                }
                if (tutorialData.Param1 == 400)
                {
                    // Seek３の編成を初めて開く
                    checkFlag = _model.CurrentStage.Seek == 3 && commandType == CommandType.SelectSymbol;
                }
                if (tutorialData.Param1 == 900)
                {
                    // 仲間加入のマスを初めて開く
                    checkFlag = _view.SymbolRecordListActive && _model.CurrentStage.Seek == 7;
                }
                if (tutorialData.Param1 == 1100)
                {
                    // ステージ2の最初
                    checkFlag = _model.CurrentStage.Id == 2;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.Tactics,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }

        public void CommandReturnStrategy()
        {
            // マージリクエストが必要なら
            if (_model.BrunchMode && _model.NeedEndBrunch)
            {
                CommandNeedEndBrunch();
                return;
            }
            if (_model.SceneParam != null && _model.SceneParam.ReturnBeforeBattle)
            {
                // 敗北して戻ってきたとき
                _model.SetStageSeekIndex(_model.SceneParam.SeekIndex);
                var currentRecord = _model.CurrentSelectRecord();
                if (currentRecord != null)
                {
                    CommandSelectTacticsCommand(TacticsCommandType.Paradigm);
                    CommandStageSymbol();
                    CommandSelectRecordSeek(currentRecord);
                    CommandSelectRecord(currentRecord);
                }
            } else
            if (_model.SceneParam != null && _model.SceneParam.ReturnNextBattle)
            {
                // 勝利して戻ってきたとき
                _model.SetStageSeekIndex(_model.SceneParam.SeekIndex);
                var currentRecord = _model.CurrentSelectRecord();
                if (currentRecord != null)
                {
                    CommandSelectTacticsCommand(TacticsCommandType.Paradigm);
                }
            }
            CheckTutorialState();
        }

        private void CommandNeedEndBrunch()
        {
            // コンフリクト確認
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.CheckConflict,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }


        private void UpdateCommand(TacticsViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            _view.UpdateInputKeyActive(viewEvent,_model.TacticsCommandType);
            //Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case CommandType.SelectTacticsCommand:
                    CommandSelectTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
                case CommandType.SelectSymbol:
                    CommandSelectRecord((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CallEnemyInfo:
                    //if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CallAddActorInfo:
                    CommandCallAddActorInfo((SymbolResultInfo)viewEvent.template,false);
                    break;
                case CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((List<GetItemInfo>)viewEvent.template);
                    break;
                case CommandType.SelectRecord:
                    CommandSelectRecordSeek((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CancelSymbolRecord:
                    if (_alcanaSelectBusy || _shopSelectBusy)
                    {
                        return;
                    }
                    CommandCancelSymbolRecord();
                    break;
                case CommandType.CancelSelectSymbol:
                    CommandCancelSelectSymbol();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.DecideRecord:
                    CommandDecideRecord();
                    break;
                case CommandType.Parallel:
                    CommandParallel();
                    break;
                case CommandType.SelectAlcanaList:
                    CommandSelectAlcanaList((SkillInfo)viewEvent.template);
                    break;
                case CommandType.EndShopSelect:
                    CommandEndShopSelect();
                    break;
                case CommandType.HideAlcanaList:
                    CommandHideAlcanaList();
                    break;
                case CommandType.ScorePrize:
                    CommandScorePrize();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.StageHelp:
                    CommandStageHelp();
                    break;
                case CommandType.CancelRecordList:
                    CommandCancelRecordList();
                    break;
                case CommandType.AlcanaCheck:
                    CommandAlcanaCheck();
                    break;
                case CommandType.NormalWorld:
                    CommandNormalWorld();
                    break;
                case CommandType.AnotherWorld:
                    CommandAnotherWorld();
                    break;
                case CommandType.SelectCharaLayer:
                    CommandSelectCharaLayer((int)viewEvent.template);
                    break;
                case CommandType.MargeRequest:
                    CommandMargeRequest();
                    break;
            }
            // チュートリアル確認
            CheckTutorialState(viewEvent.commandType);
        }

        private void UpdatePopupSelectAddActor(ConfirmCommandType confirmCommandType)
        {
        }

        private void UpdatePopupSaveCommand(ConfirmCommandType confirmCommandType,bool isReturnScene)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var saveNeedAds = _model.NeedAdsSave();
                if (saveNeedAds)
                {
                    // ロード表示
                    _view.CommandGameSystem(Base.CommandType.CallLoading);
    #if UNITY_ANDROID
                    AdMobController.Instance.PlayRewardedAd(() => 
                    {
                        SuccessSave(isReturnScene);
                    },
                    () => {
                        // ロード非表示
                        _view.CommandGameSystem(Base.CommandType.CloseLoading);
                        // 失敗した時
                        var savePopupTitle = _model.FailedSavePopupTitle();
                        var confirmInfo = new ConfirmInfo(savePopupTitle,(q) => UpdatePopupSaveCommand((ConfirmCommandType)q,isReturnScene));
                        _view.CommandCallConfirm(confirmInfo);
                        _view.ChangeUIActive(false);
                    });
    #endif
                } else
                {
                    SuccessSave(isReturnScene);
                }
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                if (isReturnScene)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _view.ChangeUIActive(true);
                }
            }
        }

        private void SuccessSave(bool isReturnScene)
        {
            // ロード非表示
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _model.GainSaveCount();
            _model.SavePlayerStageData(true);
            // 成功表示
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19500),(a) => 
            {
                if (isReturnScene)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {        
                    _view.ChangeUIActive(true);
                }
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandSelectRecordSeek(SymbolResultInfo symbolResultInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //Symbolに対応したシンボルを表示
            _view.SetSymbols(_model.StageResultInfos(symbolResultInfo));
            _view.ShowRecordList();
            _view.ShowSymbolRecord();
            _view.ChangeSymbolBackCommandActive(true);
            _view.CommandRefresh();
        }

        private void CommandCancelSymbolRecord()
        {
            _view.HideRecordList();
            _view.HideSymbolRecord();
            _view.ChangeBackCommandActive(false);
            _view.ChangeSymbolBackCommandActive(false);
            _view.CommandRefresh();
            _backCommand = CommandType.None;
        }

        private void CommandBack()
        {
            if (_alcanaSelectBusy)
            {
                return;
            }
            if (_backCommand != CommandType.None)
            {
                var eventData = new TacticsViewEvent(_backCommand)
                {
                    template = _model.TacticsCommandType
                };
                UpdateCommand(eventData);
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _backCommand = CommandType.None;
            }
        }

        private void CommandSelectTacticsCommand(TacticsCommandType tacticsCommandType)
        {
            _model.SetTacticsCommandType(tacticsCommandType);
            switch (tacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                    CommandStageSymbol();
                    return;
                case TacticsCommandType.Train:
                case TacticsCommandType.Alchemy:
                case TacticsCommandType.Status:
                    CommandStatus();
                    break;
            }
        }

        private void CommandStageSymbol()
        {
            _view.HideRecordList();
            _view.ShowSymbolRecord();
            _view.SetPositionSymbolRecords(_model.FirstRecordIndex);
            _view.ChangeBackCommandActive(true);
            _view.ChangeSymbolBackCommandActive(true);
            _view.EndStatusCursor();
            _view.CommandRefresh();
            _backCommand = CommandType.CancelSymbolRecord;
        }

        private void CommandStatus(int startIndex = -1)
        {
            int actorId = -1;
            if (startIndex != -1)
            {
                // actorIdに変換
                var actor = _model.TacticsActor();
                if (actor != null)
                {
                    actorId = actor.ActorId;
                }
            }

            CommandStatusInfo(_model.PastActorInfos(),false,true,true,false,actorId,() => 
            {
                _view.SetNuminous(_model.Currency);
                CommandRefresh();
            });
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSelectRecord(SymbolResultInfo recordInfo)
        {
            var currentTurn = _model.CurrentStage.Seek;
            var currentStage = _model.CurrentStage.Id;
            if (recordInfo.Seek == currentTurn && recordInfo.StageId == currentStage)
            {
                // 現在
                CommandCurrentSelectRecord(recordInfo);
            } else
            if (recordInfo.Seek > currentTurn && recordInfo.StageId == currentStage)
            {
                // 未来
                CommandCautionInfo(DataSystem.GetText(19340));
            } else
            if (recordInfo.StageId < currentStage || recordInfo.Seek < currentTurn && recordInfo.StageId == currentStage)
            {
                // ブランチを作成
                if (_model.CurrentStage.WorldType == WorldType.Main)
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(19300),(a) =>
                    {
                        if (a == ConfirmCommandType.Yes)
                        {
                            _model.MakeBrunch(recordInfo);
                            _model.CommandAnotherWorld();
                            _view.CommandGotoSceneChange(Scene.Tactics);
                        }
                    });
                    _view.CommandCallConfirm(confirmInfo);
                }
            }
        }

        private void CommandCurrentSelectRecord(SymbolResultInfo recordInfo)
        {
            _view.HideSymbolRecord();
            _model.SetStageSeekIndex(recordInfo.SeekIndex);
            _view.HideRecordList();
            // 回路解析
            switch (recordInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    _view.ChangeBackCommandActive(true);
                    _view.ChangeSymbolBackCommandActive(true);
                    /*
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers(),_model.SkillActionListData(_model.TacticsActor()));

                    _view.ShowConfirmCommand();
                    _view.ShowBattleReplay(recordInfo.SaveBattleReplayStage());
                    _view.ShowSelectCharacter();
                    */
                    CommandRefresh();
                    _busy = true;
                    var popupInfo = new PopupInfo
                    {
                        PopupType = PopupType.BattleParty,
                        EndEvent = () =>
                        {
                            _busy = false;
                            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                            CommandCancelSelectSymbol();
                            _view.SetNuminous(_model.Currency);
                        }
                    };
                    _view.CommandCallPopup(popupInfo);
                    _backCommand = CommandType.CancelSelectSymbol;
                    break;
                case SymbolType.Recover:
                    CheckRecoverSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.Actor:
                    CheckActorSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.SelectActor:
                    CheckSelectActorSymbol();
                    break;
                case SymbolType.Shop:
                    CheckShopStageSymbol();
                    break;
                case SymbolType.Alcana:
                    CheckAlcanaSymbol(recordInfo.SymbolInfo.GetItemInfos);
                    break;
                case SymbolType.Resource:
                    CheckResourceSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.Rebirth:
                    CheckRebirthSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
            }
        }

        private void CommandCancelSelectSymbol()
        {
            //_model.ResetRecordStage();
            _model.SetFirstBattleActorId();
            CommandRefresh();
            _view.ShowRecordList();
            _view.ShowSymbolRecord();
            _backCommand = CommandType.SelectTacticsCommand;
        }

        private void CancelSelectSymbol()
        {
            CommandRefresh();
            CommandSelectRecordSeek(_model.CurrentSelectRecord());
            //_model.ResetRecordStage();
        }

        private void CommandDecideRecord()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19300),(a) => UpdatePopupCheckStartRecord((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupCheckStartRecord(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var symbolResultInfo = _view.SymbolResultInfo();
                if (symbolResultInfo != null)
                {
                    // 過去のステージを作る
                    _model.SetReturnRecordStage(symbolResultInfo);
                    //_view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }

        private void CommandParallel()
        {
        }

        private void CommandRefreshShop()
        {
            _view.SetNuminous(_model.Currency - _model.LearningShopMagicCost());
            var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
            _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
        }

        private void CommandSelectAlcanaList(SkillInfo skillInfo)
        {
            var symbolType = _model.CurrentSelectRecord().SymbolType;
            if (symbolType == SymbolType.Alcana && _alcanaSelectBusy)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19250,skillInfo.Master.Name),(a) => UpdateSelectAlcana(a),ConfirmType.SkillDetail);
                confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                _view.CommandCallConfirm(confirmInfo);
            } else
            if (symbolType == SymbolType.Shop && _shopSelectBusy)
            {
                if (_model.IsSelectedShopMagic(skillInfo))
                {
                    // 既に選択済み
                    _model.CancelShopCurrency(skillInfo);
                    CommandRefreshShop();
                } else
                if (_model.EnableShopMagic(skillInfo))
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19260,_model.ShopLearningCost(skillInfo).ToString()) + DataSystem.GetReplaceText(19250,skillInfo.Master.Name),(a) => UpdateShop(a,skillInfo),ConfirmType.SkillDetail);
                    confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                    _view.CommandCallConfirm(confirmInfo);
                } else
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(19410),(a) => 
                    {
                    });
                    confirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(confirmInfo);
                }
            }
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var alcanaSelect = _view.AlcanaSelectSkillInfo();
                getItemInfos = getItemInfos.FindAll(a => a.Param1 == alcanaSelect.Id);
                GotoStrategyScene(getItemInfos,_model.StageMembers());
                _model.MakeSelectRelic(alcanaSelect.Id);
            }
        }

        private void UpdateShop(ConfirmCommandType confirmCommandType,SkillInfo skillInfo)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // 魔法入手、Nu消費
                _model.PayShopCurrency(skillInfo);
                CommandRefreshShop();
            }
        }

        private void CommandEndShopSelect()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19230),(a) => UpdatePopupEndShopSelect((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupEndShopSelect(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var getItemInfos = _model.LearningShopMagics();
                GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else
            {
                _backCommand = CommandType.EndShopSelect;
            }
        }

        private void CheckActorSymbol(GetItemInfo getItemInfo)
        {
            var actorData = DataSystem.FindActor(getItemInfo.Param1);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19270,actorData.Name),(a) => UpdatePopupActorSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupActorSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
                GotoStrategyScene(getItemInfos,actorInfos);
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckSelectActorSymbol()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19210),(a) => UpdatePopupSelectActorSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupSelectActorSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                CommandCallAddActorInfo(_model.CurrentSelectRecord(),true);
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckShopStageSymbol()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19220),(a) => UpdatePopupShopSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupShopSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _shopSelectBusy = true;            
                _backCommand = CommandType.EndShopSelect;
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckRecoverSymbol(GetItemInfo getItemInfo)
        {
        }

        private void UpdatePopupRecoverSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentSymbol = _model.CurrentSelectRecord();
                GotoStrategyScene(currentSymbol.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CommandSelectTacticsCommand(_model.TacticsCommandType);
            }
        }

        private void CheckAlcanaSymbol(List<GetItemInfo> getItemInfos)
        {
            CheckAlcanaSymbol(_model.AlcanaMagicSkillInfos(getItemInfos));
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        
        private void CheckAlcanaSymbol(List<SkillInfo> skillInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19200,""),(a) => UpdatePopupAlcanaSymbol(a),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(skillInfos);
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupAlcanaSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _alcanaSelectBusy = true;
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.AlcanaMagicSkillInfos(getItemInfos)));
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckResourceSymbol(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19280,getItemInfo.Param1.ToString()),(a) => UpdatePopupResourceSymbol(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentRecord = _model.CurrentSelectRecord();
                GotoStrategyScene(currentRecord.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void GotoStrategyScene(List<GetItemInfo> getItemInfos,List<ActorInfo> actorInfos)
        {
            var strategySceneInfo = new StrategySceneInfo
            {
                GetItemInfos = getItemInfos.FindAll(a => !a.GetFlag),
                ActorInfos = actorInfos,
                InBattle = false
            };
            _model.ResetBattlerIndex();
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
        }

        private void CheckRebirthSymbol(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo("ロストしたキャラを復活しますか？",(a) => UpdatePopupRebirthSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupRebirthSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentSymbol = _model.CurrentSelectRecord();
                GotoStrategyScene(currentSymbol.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CommandSelectTacticsCommand(_model.TacticsCommandType);
            }
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
        }

        private void CommandPopupSkillInfo(List<GetItemInfo> getItemInfos)
        {
            if (getItemInfos.Count == 1)
            {
                CallPopupSkillDetail("",_model.BasicSkillInfos(getItemInfos[0]));
            } else
            {
                CallPopupSkillDetail(DataSystem.GetText(19200),_model.BasicSkillGetItemInfos(getItemInfos));
            }
        }

        private void CommandRefresh()
        {
            _view.SetSaveScore(_model.TotalScore);
            _view.SetStageInfo(_model.CurrentStage);
            _view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            _view.SetTacticsCharaLayer(_model.StageMembers());
            _view.SetPastMode(_model.CurrentStage.WorldType == WorldType.Brunch);
            _view.SetWorldMove(_model.BrunchMode,_model.CurrentStage.WorldType);
            _view.CommandRefresh();
        }

        private void CommandCallEnemyInfo(SymbolResultInfo symbolResultInfo)
        {
            switch (symbolResultInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    var enemyInfos = symbolResultInfo.SymbolInfo.BattlerInfos();
                    _busy = true;
                    CommandEnemyInfo(enemyInfos,false,() => 
                    {
                        _busy = false;
                        _view.CommandRefresh();
                    });
                    break;
                case SymbolType.Alcana:
                    CallPopupSkillDetail(DataSystem.GetText(19200),_model.BasicSkillGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos));
                    break;
                case SymbolType.Actor:
                    CommandStatusInfo(_model.AddActorInfos(symbolResultInfo.SymbolInfo.GetItemInfos[0].Param1),false,true,false,false,-1,() => 
                    {
                        _view.CommandRefresh();
                    });
                    break;
                case SymbolType.SelectActor:
                    CommandCallAddActorInfo(symbolResultInfo,false);
                    break;
                case SymbolType.Shop:
                    CallPopupSkillDetail(DataSystem.GetText(19240),_model.BasicSkillGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos));
                    break;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandCallAddActorInfo(SymbolResultInfo symbolResultInfo,bool addCommand)
        {
            List<ActorInfo> actorInfos;
            if (symbolResultInfo.StageSymbolData.Param2 == 0 && symbolResultInfo.SymbolInfo.GetItemInfos.Find(a => a.GetItemType == GetItemType.AddActor) == null)
            {
                actorInfos = _model.AddSelectActorInfos();
            } else
            {
                actorInfos = _model.AddSelectActorGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos);
            }
            if (addCommand)
            {
                // 加入する用
                CommandStatusInfo(actorInfos,false,false,false,true,-1,() => 
                {

                });
            } else
            {
                var selectActorId = -1;
                var getItemInfo = _view.SymbolGetItemInfo;
                if (getItemInfo != null)
                {
                    selectActorId = getItemInfo.Param1;
                }
                // 確認する用
                CommandStatusInfo(actorInfos,false,true,false,false,selectActorId,() => 
                {

                });
            }
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(_model.SideMenu(),() => 
            {
                _busy = false;
            });
        }

        private void CallPopupSkillDetail(string title,List<SkillInfo> skillInfos)
        {
            var confirmInfo = new ConfirmInfo(title,(a) => 
            {
                CloseConfirm();
                _view.CommandRefresh();
            },ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(skillInfos);
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandStageHelp()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "Tactics",
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCancelRecordList()
        {
            //_model.ResetRecordStage();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.HideRecordList();
            _view.ChangeSymbolBackCommandActive(false);
            _view.CommandRefresh();
            _backCommand = CommandType.CancelSymbolRecord;
        }

        private void CommandScorePrize()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ScorePrize,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandAlcanaCheck()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.SetAlcanaSelectInfos(_model.MakeListData(_model.AlcanaSkillInfos(),0));
            _backCommand = CommandType.HideAlcanaList;
        }

        private void CommandNormalWorld()
        {
            _model.CommandNormalWorld();
            _view.CommandGotoSceneChange(Scene.Tactics);
        }

        private void CommandAnotherWorld()
        {
            _model.CommandAnotherWorld();
            _view.CommandGotoSceneChange(Scene.Tactics);
        }

        private void CommandHideAlcanaList()
        {
            _view.HideAlcanaList();
            _view.ChangeBackCommandActive(false);
            _view.ChangeSymbolBackCommandActive(false);
            _backCommand = CommandType.None;
        }

        private void CommandSelectCharaLayer(int actorId)
        {
            _busy = true;
            _view.CommandSelectCharaLayer(actorId);
            CommandTacticsStatusInfo(_model.StageMembers(),false,true,true,false,actorId,() => 
            {
                _view.WaitFrame(12,() => 
                {
                    _busy = false;
                });
                _view.EndStatus();
                _view.SetHelpText(DataSystem.GetText(20020));
                _view.SetNuminous(_model.Currency);
                CommandRefresh();
            },(a) => 
            {
                _view.CommandSelectCharaLayer(a);
            });
        }

        private void CommandMargeRequest()
        {
            var confirmInfo = new ConfirmInfo("マージしますか？",(a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    CommandNeedEndBrunch();
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }
    }
}