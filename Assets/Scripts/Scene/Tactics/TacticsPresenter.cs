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
            if (_busy)
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
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
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
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            var statusViewInfo = new StatusViewInfo(null);
            statusViewInfo.SetDisplayDecideButton(true);
            statusViewInfo.SetDisplayBackButton(false);
            _view.CommandCallStatus(statusViewInfo);
        }

        private void UpdatePopupSaveCommand(ConfirmCommandType confirmCommandType,bool isReturnScene)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
                        var popupInfo = new ConfirmInfo(savePopupTitle,(q) => UpdatePopupSaveCommand((ConfirmCommandType)q,isReturnScene));
                        _view.CommandCallConfirm(popupInfo);
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
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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

        private void UpdatePopupSkillInfo()
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }

        private void CommandSelectRecordSeek(SymbolResultInfo symbolResultInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //Symbolに対応したシンボルを表示
            _view.SetSymbols(_model.StageResultInfos(symbolResultInfo));
            _view.ShowSymbolList();
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
            if (recordInfo.StageSymbolData.Seek == currentTurn)
            {
                // 現在
                CommandCurrentSelectRecord(recordInfo);
            } else
            if (recordInfo.StageSymbolData.Seek > currentTurn)
            {
                // 未来
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(23060));
                _view.CommandCallCaution(cautionInfo);
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
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());

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
            _view.ShowSymbolList();
            _view.HideSelectCharacter();
            _view.ShowSymbolRecord();
            _backCommand = CommandType.SelectTacticsCommand;
        }

        private void CommandDecideRecord()
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetText(23010),(a) => UpdatePopupCheckStartRecord((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupCheckStartRecord(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var symbolResultInfo = _view.SymbolResultInfo();
                if (symbolResultInfo != null)
                {
                    // 過去のステージを作る
                    _model.MakeSymbolRecordStage(symbolResultInfo);
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
                    var cautionInfo = new CautionInfo();
                    cautionInfo.SetTitle(DataSystem.GetText(23030));
                    _view.CommandCallCaution(cautionInfo);
                }
                return;
            }
            if (_model.ParallelHistory())
            {
                if (_model.PartyInfo.ParallelCount > 0)
                {
                    var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23020,_model.PartyInfo.ParallelCount.ToString()),(a) => UpdatePopupCheckParallelRecord(a));
                    _view.CommandCallConfirm(popupInfo);
                } else
                {
                    var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23021,_model.PartyInfo.ParallelCount.ToString()),(a) => UpdatePopupCheckParallelRecord(a));
                    popupInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(popupInfo);
                }
            } else
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(23030));
                _view.CommandCallCaution(cautionInfo);
            }
        }

        private void UpdatePopupCheckParallelRecord(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var symbolResultInfo = _view.SymbolResultInfo();
                if (symbolResultInfo != null)
                {
                    // 過去のステージを作る
                    _model.MakeSymbolRecordStage(symbolResultInfo);
                    _model.SetParallelMode();
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }

        private void UpdatePopupNoParallelRecord()
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }

        private void CommandSelectAlcanaList(SkillInfo skillInfo)
        {
            var symbolType = _model.CurrentSelectRecord().SymbolType;
            if (symbolType == SymbolType.Alcana && _alcanaSelectBusy)
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11142,skillInfo.Master.Name),(a) => UpdateSelectAlcana(a),ConfirmType.SkillDetail);
                popupInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                _view.CommandCallConfirm(popupInfo);
            } else
            if (symbolType == SymbolType.Shop && _shopSelectBusy)
            {
                if (_model.EnableShopMagic(skillInfo))
                {
                    var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,_model.ShopLearningCost(skillInfo).ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdateShop(a,skillInfo),ConfirmType.SkillDetail);
                    popupInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
                    _view.CommandCallConfirm(popupInfo);
                } else
                {
                    var popupInfo = new ConfirmInfo(DataSystem.GetText(11170),(a) => 
                    {
                        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                    });
                    popupInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(popupInfo);
                }
            }
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
            var popupInfo = new ConfirmInfo(DataSystem.GetText(11191),(a) => UpdatePopupEndShopSelect((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupEndShopSelect(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11120,actorData.Name),(a) => UpdatePopupActorSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupActorSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
                GotoStrategyScene(getItemInfos,actorInfos);
            } else
            {
                CommandSelectRecordSeek(_model.CurrentSelectRecord());
            }
        }

        private void CheckSelectActorSymbol(List<GetItemInfo> getItemInfos)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetText(11180),(a) => UpdatePopupSelectActorSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupSelectActorSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                if (_model.CurrentSelectRecord().StageSymbolData.Param2 == 0)
                {
                    _model.SetTempAddSelectActorStatusInfos();
                } else
                {
                    _model.SetTempAddSelectActorGetItemInfoStatusInfos(_model.CurrentSelectRecord().SymbolInfo.GetItemInfos);
                }
                var statusViewInfo = new StatusViewInfo(() => 
                {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                statusViewInfo.SetDisplayCharacterList(false);
                statusViewInfo.SetDisplayDecideButton(true);
                statusViewInfo.SetDisplayBackButton(false);
                _view.CommandCallStatus(statusViewInfo);
                _view.ChangeUIActive(false);
            } else
            {
                CommandSelectRecordSeek(_model.CurrentSelectRecord());
            }
        }

        private void CheckShopStageSymbol(List<GetItemInfo> getItemInfos)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetText(11190),(a) => UpdatePopupShopSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupShopSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _shopSelectBusy = true;            
                _backCommand = CommandType.EndShopSelect;
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.ShopMagicSkillInfos(getItemInfos)));
            } else
            {
                CommandSelectRecordSeek(_model.CurrentSelectRecord());
            }
        }

        private void CheckRecoverSymbol(GetItemInfo getItemInfo)
        {
            //var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11130,getItemInfo.ResultName),(a) => UpdatePopupRecoverSymbol((ConfirmCommandType)a));
            //_view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupRecoverSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,""),(a) => UpdatePopupAlcanaSymbol((ConfirmCommandType)a),ConfirmType.SkillDetail);
            popupInfo.SetSkillInfo(skillInfos);
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupAlcanaSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _alcanaSelectBusy = true;
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.AlcanaMagicSkillInfos(getItemInfos)));
                //GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else
            {
                CommandSelectRecordSeek(_model.CurrentSelectRecord());
            }
        }

        private void CheckResourceSymbol(GetItemInfo getItemInfo)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11141,getItemInfo.Param1.ToString()),(a) => UpdatePopupResourceSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentRecord = _model.CurrentSelectRecord();
                GotoStrategyScene(currentRecord.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CommandSelectRecordSeek(_model.CurrentSelectRecord());
            }
        }

        private void GotoStrategyScene(List<GetItemInfo> getItemInfos,List<ActorInfo> actorInfos)
        {
            var strategySceneInfo = new StrategySceneInfo
            {
                GetItemInfos = getItemInfos,
                ActorInfos = actorInfos
            };
            _model.ResetBattlerIndex();
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
        }

        private void CheckRebirthSymbol(GetItemInfo getItemInfo)
        {
            var popupInfo = new ConfirmInfo("ロストしたキャラを復活しますか？",(a) => UpdatePopupRebirthSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupRebirthSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
            var confirmInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandPopupSkillInfo(List<GetItemInfo> getItemInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11140),(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillGetItemInfos(getItemInfos));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandPopupShopInfo(List<GetItemInfo> getItemInfos)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11192),(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillGetItemInfos(getItemInfos));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
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
                    
                    var enemyViewInfo = new StatusViewInfo(() => 
                    {
                        _view.CommandGameSystem(Base.CommandType.CloseStatus);
                        _view.ChangeUIActive(true);
                    });
                    enemyViewInfo.SetEnemyInfos(enemyInfos,false);
                    _view.CommandCallEnemyInfo(enemyViewInfo);
                    _view.ChangeUIActive(false);
                    break;
                case SymbolType.Alcana:
                    CommandPopupSkillInfo(symbolInfo.SymbolInfo.GetItemInfos);
                    break;
                case SymbolType.Actor:
                    _model.SetTempAddActorStatusInfos(symbolInfo.SymbolInfo.GetItemInfos[0].Param1);
                    var statusViewInfo = new StatusViewInfo(() => 
                    {
                        _view.CommandGameSystem(Base.CommandType.CloseStatus);
                        _view.ChangeUIActive(true);
                    });
                    statusViewInfo.SetDisplayCharacterList(false);
                    _view.CommandCallStatus(statusViewInfo);
                    _view.ChangeUIActive(false);
                    break;
                case SymbolType.SelectActor:
                    if (symbolInfo.StageSymbolData.Param2 == 0)
                    {
                        _model.SetTempAddSelectActorStatusInfos();
                    } else
                    {
                        _model.SetTempAddSelectActorGetItemInfoStatusInfos(symbolInfo.SymbolInfo.GetItemInfos);
                    }
                    var statusViewInfo2 = new StatusViewInfo(() => 
                    {
                        _view.CommandGameSystem(Base.CommandType.CloseStatus);
                        _view.ChangeUIActive(true);
                    });
                    statusViewInfo2.SetDisplayCharacterList(false);
                    _view.CommandCallStatus(statusViewInfo2);
                    _view.ChangeUIActive(false);
                    break;
                case SymbolType.Shop:
                    CommandPopupShopInfo(symbolInfo.SymbolInfo.GetItemInfos);
                    break;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }


        private void CommandSelectSideMenu()
        {
            _busy = true;
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {
                    _busy = false;
                    _view.SetBusyTrain(false);
                },
                CommandLists = _model.SideMenu()
            };
            _view.CommandCallSideMenu(sideMenuViewInfo);
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
            _model.CurrentStage.SetWorldNo(0);
            _view.CommandGotoSceneChange(Scene.Tactics);
        }

        private void CommandAnotherWorld()
        {
            _model.CurrentStage.SetWorldNo(1);
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