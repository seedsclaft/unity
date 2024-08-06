using System.Collections;
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
            _model.AssignBattlerIndex();
            _view.SetHelpWindow();
            _view.ChangeBackCommandActive(false);
            _view.SetEvent((type) => UpdateCommand(type));
            
            _view.SetStageInfo(_model.CurrentStage);
            _view.SetParallelCommand(_model.ParallelCommand());
            _view.SetMultiverse(_model.PartyInfo.EnableMultiverse(),_model.CurrentStage.WorldNo);
            _view.SetTacticsCommand(_model.TacticsCommand());
            //_view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));
            _view.SetUIButton();
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            _view.SetSymbolRecords(_model.SymbolRecords());
            _view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            if (_model.PartyInfo.ReturnSymbol != null)
            {
                _view.SetPastMode();
            }
            CommandRefresh();
            PlayTacticsBgm();
            //_view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
            if (_model.SetStageTutorials(EventTiming.StartTactics))
            {
                _view.CommandCallTutorialFocus(_model.CurrentStageTutorialDates[0]);
            }
            _view.ChangeUIActive(true);
            _view.StartAnimation();
        }

        public void CommandReturnStrategy()
        {
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
            }
        }


        private void UpdateCommand(TacticsViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            _view.UpdateInputKeyActive(viewEvent,_model.TacticsCommandType);
            Debug.Log(viewEvent.commandType);
            if (_model.CheckTutorial(viewEvent))
            {
                _model.SeekTutorial();
                if (_model.CurrentStageTutorialDates.Count == 0)
                {
                    _view.CommandCloseTutorialFocus();
                } else
                {            
                    _view.CommandCallTutorialFocus(_model.CurrentStageTutorialDates[0]);
                }
            }
            switch (viewEvent.commandType)
            {
                case CommandType.SelectTacticsCommand:
                    CommandSelectTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
                case CommandType.SelectSymbol:
                    CommandSelectRecord((SymbolResultInfo)viewEvent.template);
                    break;
                case CommandType.CallEnemyInfo:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolResultInfo)viewEvent.template);
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
            }
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11084),(a) => 
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
            _view.ShowSymbolList();
            _view.HideSelectCharacter();
            _view.ShowSymbolRecord();
            _view.ChangeSymbolBackCommandActive(true);
            // 過去
            if (symbolResultInfo.StageId < _model.CurrentStage.Id || symbolResultInfo.Seek < _model.CurrentStage.CurrentSeek)
            {
                // 過去の中ではさらに過去に戻らない
                if (_model.PartyInfo.ReturnSymbol == null)
                {
                    _view.ShowParallelList();
                }
            }
            //_backCommand = CommandType.TacticsCommand;
        }

        private void CommandCancelSymbolRecord()
        {
            _view.HideRecordList();
            _view.HideSymbolRecord();
            _view.ChangeBackCommandActive(false);
            _view.ChangeSymbolBackCommandActive(false);
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
            _view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
            _view.CallTrainCommand(tacticsCommandType);
            switch (tacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                    CommandStageSymbol();
                    return;
                case TacticsCommandType.Train:
                case TacticsCommandType.Alchemy:
                case TacticsCommandType.Status:
                    break;
            }
        }

        private void CommandStageSymbol()
        {
            _view.HideParallelList();
            _view.HideSelectCharacter();
            _view.HideRecordList();
            _view.ShowSymbolRecord();
            _view.SetPositionSymbolRecords(_model.FirstRecordIndex);
            _view.ChangeBackCommandActive(true);
            _view.ChangeSymbolBackCommandActive(true);
            _backCommand = CommandType.CancelSymbolRecord;
        }

        private void CommandSelectRecord(SymbolResultInfo recordInfo)
        {
            var currentTurn = _model.CurrentStage.CurrentSeek;
            var currentStage = _model.CurrentStage.Id;
            if (recordInfo.StageSymbolData.Seek == currentTurn && recordInfo.StageSymbolData.StageId == currentStage)
            {
                // 現在
                CommandCurrentSelectRecord(recordInfo);
            } else
            if (recordInfo.StageSymbolData.Seek > currentTurn && recordInfo.StageSymbolData.StageId == currentStage)
            {
                // 未来
                CommandCautionInfo(DataSystem.GetText(23060));
            } else
            if (recordInfo.StageSymbolData.StageId < currentStage || recordInfo.StageSymbolData.Seek < currentTurn && recordInfo.StageSymbolData.StageId == currentStage)
            {
                // 過去
                if (_model.RemakeHistory())
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(23010),(a) =>
                    {
                        if (a == ConfirmCommandType.Yes)
                        {
                            _model.SetReturnRecordStage(recordInfo);
                            CommandCurrentSelectRecord(recordInfo);
                        }
                    });
                    _view.CommandCallConfirm(confirmInfo);
                }
            }
        }

        private void CommandCurrentSelectRecord(SymbolResultInfo recordInfo)
        {
            _view.HideSymbolRecord();
            _model.SetStageSeekIndex(recordInfo.StageSymbolData.SeekIndex);
            _view.HideRecordList();
            // 回路解析
            switch (recordInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    _view.ChangeBackCommandActive(true);
                    _view.ChangeSymbolBackCommandActive(true);
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers(),_model.SkillActionList(_model.TacticsActor()));

                    _view.ShowConfirmCommand();
                    _view.ShowBattleReplay(recordInfo.SaveBattleReplayStage());
                    _view.ShowSelectCharacter();
                    CommandRefresh();
                    _backCommand = CommandType.CancelSelectSymbol;
                    break;
                case SymbolType.Recover:
                    CheckRecoverSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.Actor:
                    CheckActorSymbol(recordInfo.SymbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.SelectActor:
                    CheckSelectActorSymbol(recordInfo.SymbolInfo.GetItemInfos);
                    break;
                case SymbolType.Shop:
                    CheckShopStageSymbol(recordInfo.SymbolInfo.GetItemInfos);
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
            _model.ResetRecordStage();
            CommandRefresh();
            _view.ShowSymbolList();
            _view.HideSelectCharacter();
            _view.ShowSymbolRecord();
            _backCommand = CommandType.SelectTacticsCommand;
        }

        private void CancelSelectSymbol()
        {
            _model.ResetRecordStage();
            CommandRefresh();
            CommandSelectRecordSeek(_model.CurrentSelectRecord());
        }

        private void CommandDecideRecord()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(23010),(a) => UpdatePopupCheckStartRecord((ConfirmCommandType)a));
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
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }

        private void CommandParallel()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var ParallelIndex = _view.ParallelListIndex;
            if (ParallelIndex == 0)
            {
                if (_model.RemakeHistory())
                {
                    CommandDecideRecord();
                } else
                {
                    CommandCautionInfo(DataSystem.GetText(23030));
                }
                return;
            }
            if (_model.ParallelHistory())
            {
                if (_model.PartyInfo.ParallelCount > 0)
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(23020,_model.PartyInfo.ParallelCount.ToString()),(a) => UpdatePopupCheckParallelRecord(a));
                    _view.CommandCallConfirm(confirmInfo);
                } else
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(23021,_model.PartyInfo.ParallelCount.ToString()),(a) => UpdatePopupCheckParallelRecord(a));
                    confirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(confirmInfo);
                }
            } else
            {
                CommandCautionInfo(DataSystem.GetText(23030));
            }
        }

        private void UpdatePopupCheckParallelRecord(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var symbolResultInfo = _view.SymbolResultInfo();
                if (symbolResultInfo != null)
                {
                    // 過去のステージを作る
                    _model.SetReturnRecordStage(symbolResultInfo);
                    _model.SetParallelMode();
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }

        private void UpdatePopupNoParallelRecord()
        {
        }

        private void CommandSelectAlcanaList(SkillInfo skillInfo)
        {
            var symbolType = _model.CurrentSelectRecord().SymbolType;
            if (symbolType == SymbolType.Alcana && _alcanaSelectBusy)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11142,skillInfo.Master.Name),(a) => UpdateSelectAlcana(a),ConfirmType.SkillDetail);
                confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                _view.CommandCallConfirm(confirmInfo);
            } else
            if (symbolType == SymbolType.Shop && _shopSelectBusy)
            {
                if (_model.EnableShopMagic(skillInfo))
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,_model.ShopLearningCost(skillInfo).ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdateShop(a,skillInfo),ConfirmType.SkillDetail);
                    confirmInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                    _view.CommandCallConfirm(confirmInfo);
                } else
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(11170),(a) => 
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
                _model.MakeSelectRelic(alcanaSelect.Id);
                GotoStrategyScene(getItemInfos,_model.StageMembers());
            }
        }

        private void UpdateShop(ConfirmCommandType confirmCommandType,SkillInfo skillInfo)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // 魔法入手、Nu消費
                _model.PayShopCurrency(skillInfo,_view.AlcanaListIndex);
                _view.SetNuminous(_model.Currency);
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
            }
        }

        private void CommandEndShopSelect()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11191),(a) => UpdatePopupEndShopSelect((ConfirmCommandType)a));
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11120,actorData.Name),(a) => UpdatePopupActorSymbol((ConfirmCommandType)a));
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

        private void CheckSelectActorSymbol(List<GetItemInfo> getItemInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11180),(a) => UpdatePopupSelectActorSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupSelectActorSymbol(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                List<ActorInfo> actorInfos;
                if (_model.CurrentSelectRecord().StageSymbolData.Param2 == 0)
                {
                    actorInfos = _model.AddSelectActorInfos();
                } else
                {
                    actorInfos = _model.AddSelectActorGetItemInfos(_model.CurrentSelectRecord().SymbolInfo.GetItemInfos);
                }
                CommandStatusInfo(actorInfos,false,false,false,true,-1,() => 
                {

                });
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckShopStageSymbol(List<GetItemInfo> getItemInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11190),(a) => UpdatePopupShopSymbol((ConfirmCommandType)a));
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
            //var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11130,getItemInfo.ResultName),(a) => UpdatePopupRecoverSymbol((ConfirmCommandType)a));
            //_view.CommandCallConfirm(popupInfo);
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,""),(a) => UpdatePopupAlcanaSymbol((ConfirmCommandType)a),ConfirmType.SkillDetail);
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
                //GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else
            {
                CancelSelectSymbol();
            }
        }

        private void CheckResourceSymbol(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11141,getItemInfo.Param1.ToString()),(a) => UpdatePopupResourceSymbol(a));
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
                ActorInfos = actorInfos
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
                CallPopupSkillDetail(DataSystem.GetText(11140),_model.BasicSkillGetItemInfos(getItemInfos));
            }
        }

        private void CommandRefresh()
        {
            _view.SetSaveScore(_model.TotalScore);
            _view.SetStageInfo(_model.CurrentStage);
            _view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            _view.SetTacticsCharaLayer(_model.StageMembers());
            _view.SetEvaluate(_model.PartyEvaluate(),_model.TroopEvaluate());
            _view.CommandRefresh();
        }

        private void CommandCallEnemyInfo(SymbolResultInfo symbolInfo)
        {
            switch (symbolInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    var enemyInfos = symbolInfo.SymbolInfo.BattlerInfos();
                    _busy = true;
                    CommandEnemyInfo(enemyInfos,false,() => {_busy = false;});
                    break;
                case SymbolType.Alcana:
                    CallPopupSkillDetail(DataSystem.GetText(11140),_model.BasicSkillGetItemInfos(symbolInfo.SymbolInfo.GetItemInfos));
                    break;
                case SymbolType.Actor:
                    CommandStatusInfo(_model.AddActorInfos(symbolInfo.SymbolInfo.GetItemInfos[0].Param1),false,true,false,false,-1,() => 
                    {

                    });
                    break;
                case SymbolType.SelectActor:
                    List<ActorInfo> actorInfos;
                    if (symbolInfo.StageSymbolData.Param2 == 0)
                    {
                        actorInfos = _model.AddSelectActorInfos();
                    } else
                    {
                        actorInfos = _model.AddSelectActorGetItemInfos(symbolInfo.SymbolInfo.GetItemInfos);
                    }
                    CommandStatusInfo(actorInfos,false,true,false,false,-1,() => 
                    {

                    });
                    break;
                case SymbolType.Shop:
                    CallPopupSkillDetail(DataSystem.GetText(11192),_model.BasicSkillGetItemInfos(symbolInfo.SymbolInfo.GetItemInfos));
                    break;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(_model.SideMenu(),() => 
            {
                _busy = false;
                _view.SetBusyTrain(false);
            });
        }

        private void CallPopupSkillDetail(string title,List<SkillInfo> skillInfos)
        {
            var confirmInfo = new ConfirmInfo(title,(a) => CloseConfirm(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(skillInfos);
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandStageHelp()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandHelpList(DataSystem.HelpText("Tactics"));
        }

        private void CommandCancelRecordList()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.HideRecordList();
            _view.HideParallelList();
            _view.ChangeSymbolBackCommandActive(false);
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
                    _view.SetBusyTrain(false);
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
    }
}