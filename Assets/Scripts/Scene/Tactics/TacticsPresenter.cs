using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsPresenter :BasePresenter
    {
        TacticsModel _model = null;
        TacticsView _view = null;

        private bool _busy = true;
        private bool _eventBusy = false;
        private bool _alcanaSelectBusy = false;

        private Tactics.CommandType _backCommand = Tactics.CommandType.None;
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
            if (CheckRebornEvent())
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

        private void CheckStageEvent()
        {
            // イベントチェック
            var stageEvents = _model.StageEvents(EventTiming.StartTactics);
            foreach (var stageEvent in stageEvents)
            {
                if (_eventBusy)
                {
                    continue;
                }
                switch (stageEvent.Type)
                {
                    case StageEventType.CommandDisable:
                        _model.SetTacticsCommandEnables((TacticsCommandType)stageEvent.Param + 1,false);
                        break;
                    case StageEventType.NeedAllTactics:
                        break;
                    case StageEventType.IsSubordinate:
                        break;
                    case StageEventType.IsAlcana:
                        //_model.SetIsAlcana(stageEvent.Param == 1);
                        break;
                    case StageEventType.SelectAddActor:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        var selectAddActor = new ConfirmInfo(DataSystem.GetText(11050),(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmCommandType)menuCommandInfo));
                        selectAddActor.SetIsNoChoice(true);
                        selectAddActor.SetSelectIndex(0);
                        _view.CommandCallConfirm(selectAddActor);
                        _view.ChangeUIActive(false);
                        PlayTacticsBgm();
                        
                        break;
                    case StageEventType.SaveCommand:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        CommandSave(true);
                        break;
                    case StageEventType.SetDefineBossIndex:
                        break;
                    case StageEventType.SetRouteSelectParam:
                        _view.CommandSetRouteSelect();
                        break;
                    case StageEventType.ClearStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _model.StageClear();
                        _view.CommandGotoSceneChange(Scene.MainMenu);
                        break;
                    case StageEventType.ChangeRouteSelectStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _view.CommandGotoSceneChange(Scene.Tactics);
                        break;
                    case StageEventType.SetDisplayTurns:
                        break;
                    case StageEventType.MoveStage:
                        break;
                    case StageEventType.SetDefineBoss:
                        //_model.SetDefineBoss(stageEvent.Param);
                        break;
                    case StageEventType.SurvivalMode:
                        _model.SetSurvivalMode();
                        break;
                }
            }
        }

        private bool CheckAdvEvent()
        {
            var StartTacticsAdvData = _model.StartTacticsAdvData();
            if (StartTacticsAdvData != null)
            {
                var advInfo = new AdvCallInfo();
                advInfo.SetLabel(_model.GetAdvFile(StartTacticsAdvData.Id));
                advInfo.SetCallEvent(() => {
                    if (StartTacticsAdvData.EndJump != Scene.None)
                    {
                        _view.CommandSceneChange(StartTacticsAdvData.EndJump);
                    }   
                });
                _view.CommandCallAdv(advInfo);
                _view.ChangeUIActive(false);
            }
            return StartTacticsAdvData != null;
        }

        private bool CheckBeforeTacticsAdvEvent()
        {
            var isAbort = CheckAdvStageEvent(EventTiming.BeforeTactics,() => {
                _view.CommandGotoSceneChange(Scene.Tactics);
            },_model.CurrentStage.SelectActorIdsClassId(0));
            if (isAbort)
            {
                _view.ChangeUIActive(false);
            }
            return isAbort;
        }

        private bool CheckRebornEvent()
        {
            var isReborn = CheckRebornEvent(EventTiming.BeforeTactics,() => {
                _view.CommandGotoSceneChange(Scene.RebornResult);
            });
            if (isReborn)
            {
                _view.ChangeUIActive(false);
            }
            return isReborn;
        }

        private void Initialize()
        {
            InitializeView();
            var isAbort = CheckAdvStageEvent(EventTiming.StartTactics,() => {
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


        private void UpdateCommand(TacticsViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            Debug.Log(viewEvent.commandType);
            if (_model.CheckTutorial(viewEvent))
            {
                _model.SeekTutorial();
                if (_model.CurrentStageTutorialDates.Count == 0)
                {
                    _view.CommandCloseTutorialFocus();
                } else{            
                    _view.CommandCallTutorialFocus(_model.CurrentStageTutorialDates[0]);
                }
            }
            switch (viewEvent.commandType)
            {
                case Tactics.CommandType.TacticsCommand:
                    CommandTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
                case Tactics.CommandType.TacticsCommandClose:
                    if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
                    {
                        CommandSelectEnemyClose((ConfirmCommandType)viewEvent.template);
                    }
                    break;
                case Tactics.CommandType.SelectSymbol:
                    CommandSelectSymbol((SymbolInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.SymbolClose:
                    CommandSymbolClose();
                    break;
                case Tactics.CommandType.CallEnemyInfo:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.SelectRecord:
                    CommandSelectRecord((int)viewEvent.template);
                    break;
                case Tactics.CommandType.CancelSymbolRecord:
                    if (_alcanaSelectBusy == true)
                    {
                        return;
                    }
                    CommandCancelSymbolRecord();
                    break;
                case Tactics.CommandType.CancelSelectSymbol:
                    CommandCancelSelectSymbol();
                    break;
                case Tactics.CommandType.Back:
                    CommandBack();
                    break;
                case Tactics.CommandType.DecideRecord:
                    CommandDecideRecord();
                    break;
                case Tactics.CommandType.Parallel:
                    CommandParallel();
                    break;
                case Tactics.CommandType.SelectAlcanaList:
                    if (_alcanaSelectBusy == false)
                    {
                        return;
                    }
                    CommandSelectAlcanaList((SkillInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.HideAlcanaList:
                    CommandHideAlcanaList();
                    break;
            }
            if (viewEvent.commandType == Tactics.CommandType.SelectSideMenu)
            {
                CommandSelectSideMenu();
            }
            if (viewEvent.commandType == Tactics.CommandType.StageHelp)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandStageHelp();
            }
            if (viewEvent.commandType == Tactics.CommandType.CancelRecordList)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandCancelRecordList();
            }
            if (viewEvent.commandType == Tactics.CommandType.CommandHelp)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandCommandHelp();
            }
            
            if (viewEvent.commandType == Tactics.CommandType.AlcanaCheck)
            {
                CommandAlcanaCheck();
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(11084),(a) => {
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

        private void CommandSelectRecord(int seek)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //Seekに対応したシンボルを表示
            _view.SetSymbols(_model.StageSymbolInfos(seek));
            _view.ShowSymbolList();
            _view.ChangeSymbolBackCommandActive(true);
            // 過去
            if (seek < _model.CurrentStage.CurrentTurn)
            {
                // 過去の中ではさらに過去に戻らない
                if (_model.CurrentStage.ReturnSeek < 0)
                {
                    _view.ShowParallelList();
                }
            }
            //_backCommand = Tactics.CommandType.TacticsCommand;
        }

        private void CommandCancelSymbolRecord()
        {
            _view.HideRecordList();
            _view.HideSymbolRecord();
            _view.ChangeBackCommandActive(false);
            _view.ChangeSymbolBackCommandActive(false);
            _backCommand = Tactics.CommandType.None;
        }

        private void CommandBack()
        {
            var eventData = new TacticsViewEvent(_backCommand);
            eventData.template = _model.TacticsCommandType;
            UpdateCommand(eventData);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandTacticsCommand(TacticsCommandType tacticsCommandType)
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
                    _view.SetSymbols(_model.StageSymbolInfos(_model.CurrentStage.CurrentTurn));
                    _backCommand = Tactics.CommandType.TacticsCommandClose;
                    break;
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
            _view.SetPositionSymbolRecords(_model.SymbolRecords());
            _view.ChangeBackCommandActive(true);
            _view.ChangeSymbolBackCommandActive(true);
            _backCommand = Tactics.CommandType.CancelSymbolRecord;
        }

        private void CommandSelectSymbol(SymbolInfo symbolInfo)
        {
            _model.SetSymbolInfo(symbolInfo);
            if (symbolInfo.StageSymbolData.Seek == _model.CurrentStage.CurrentTurn)
            {
                // 現在
                CommandCurrentSelectSymbol(symbolInfo);
            }
        }

        private void CommandCurrentSelectSymbol(SymbolInfo symbolInfo)
        {
            _view.HideSymbolRecord();
            _model.SetStageSeekIndex(symbolInfo.StageSymbolData.SeekIndex);
            _view.HideRecordList();
            // 回路解析
            switch (symbolInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    _view.ChangeBackCommandActive(true);
                    _view.ChangeSymbolBackCommandActive(true);
                    _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                    _view.ActivateTacticsCommand();
                    _view.ShowConfirmCommand();
                    CommandRefresh();
                    _backCommand = Tactics.CommandType.CancelSelectSymbol;
                    break;
                case SymbolType.Recover:
                    CheckRecoverSymbol(symbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.Actor:
                    CheckActorSymbol(symbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.SelectActor:
                    CheckSelectActorSymbol(symbolInfo.GetItemInfos);
                    break;
                case SymbolType.Alcana:
                    CheckAlcanaSymbol(symbolInfo.GetItemInfos);
                    break;
                case SymbolType.Resource:
                    CheckResourceSymbol(symbolInfo.GetItemInfos[0]);
                    break;
                case SymbolType.Rebirth:
                    CheckRebirthSymbol(symbolInfo.GetItemInfos[0]);
                    break;
            }
        }

        private void CommandCancelSelectSymbol()
        {
            _view.ShowSymbolList();
            _view.HideSelectCharacter();
            _view.ShowSymbolRecord();
            _backCommand = Tactics.CommandType.TacticsCommand;
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
                var selectSeek = _view.RecordSeekIndex();
                if (selectSeek > -1)
                {
                    // 過去のステージを作る
                    _model.MakeSymbolRecordStage(selectSeek);
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
                CommandDecideRecord();
                return;
            }
            if (_model.CanParallel())
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23020,_model.ParallelCost().ToString()),(a) => UpdatePopupCheckParallelRecord((ConfirmCommandType)a));
                _view.CommandCallConfirm(popupInfo);
            } else
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23030,_model.ParallelCost().ToString()),(a) => UpdatePopupNoParallelRecord());
                popupInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(popupInfo);
            }
        }

        private void UpdatePopupCheckParallelRecord(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var selectSeek = _view.RecordSeekIndex();
                if (selectSeek > -1)
                {
                    // 過去のステージを作る
                    _model.MakeSymbolRecordStage(selectSeek);
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
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,""),(a) => UpdateSelectAlcana((ConfirmCommandType)a),ConfirmType.SkillDetail);
            popupInfo.SetSkillInfo(new List<SkillInfo>(){skillInfo});
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdateSelectAlcana(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // アルカナ選択
                var getItemInfos = _model.CurrentSelectSymbol().GetItemInfos;
                var alcanaSelect = _view.AlcanaSelectSkillInfo();
                getItemInfos = getItemInfos.FindAll(a => a.Param1 == alcanaSelect.Id);
                GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else{
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
                var getItemInfos = _model.CurrentSelectSymbol().GetItemInfos;
                var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
                GotoStrategyScene(getItemInfos,actorInfos);
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
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
                if (_model.CurrentSelectSymbol().StageSymbolData.Param2 == 0)
                {
                    _model.SetTempAddSelectActorStatusInfos();
                } else
                {
                    _model.SetTempAddSelectActorGetItemInfoStatusInfos(_model.CurrentSelectSymbol().GetItemInfos);
                }
                var statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                statusViewInfo.SetDisplayCharacterList(false);
                statusViewInfo.SetDisplayDecideButton(true);
                statusViewInfo.SetDisplayBackButton(false);
                _view.CommandCallStatus(statusViewInfo);
                _view.ChangeUIActive(false);
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
            }
        }

        private void CheckRecoverSymbol(GetItemInfo getItemInfo)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11130,getItemInfo.ResultName),(a) => UpdatePopupRecoverSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupRecoverSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentSymbol = _model.CurrentSelectSymbol();
                GotoStrategyScene(currentSymbol.GetItemInfos,_model.StageMembers());
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
            }
        }

        private void CheckAlcanaSymbol(List<GetItemInfo> getItemInfos)
        {
            /*
            _view.ShowLeaningList(_model.AlcanaMagicSkillInfos(getItemInfos));
            _view.ChangeBackCommandActive(true);
            _backCommand = Tactics.CommandType.TacticsCommand; 
            */
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
                var getItemInfos = _model.CurrentSelectSymbol().GetItemInfos;
                _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.AlcanaMagicSkillInfos(getItemInfos)));
                //GotoStrategyScene(getItemInfos,_model.StageMembers());
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
            }
        }

        private void CheckResourceSymbol(GetItemInfo getItemInfo)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,getItemInfo.Param1 + DataSystem.GetText(1000)),(a) => UpdatePopupResourceSymbol((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currentSymbol = _model.CurrentSelectSymbol();
                GotoStrategyScene(currentSymbol.GetItemInfos,_model.StageMembers());
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
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
                var currentSymbol = _model.CurrentSelectSymbol();
                GotoStrategyScene(currentSymbol.GetItemInfos,_model.StageMembers());
            } else{
                CommandTacticsCommand(_model.TacticsCommandType);
            }
        }


        private void CommandSelectEnemyClose(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                if (_model.BattleMembers().Count > 0)
                {
                    _model.SaveTempBattleMembers();
                    _model.SetStatusActorInfos();
                    _view.CommandChangeViewToTransition(null);
                    // ボス戦なら
                    if (_model.CurrentSelectSymbol().SymbolType == SymbolType.Boss)
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
                    _view.SetActiveBackGround(false);
                    _model.SetPartyBattlerIdList();
                    SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                    _view.CommandSceneChange(Scene.Battle);
                } else
                {
                    CheckBattleMember();
                }
            } else{
                _view.ShowSymbolList();
                _view.HideSelectCharacter();
                CommandRefresh();
            }
        }

        private void CheckBattleMember()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(DataSystem.GetText(11160));
            _view.CommandCallCaution(cautionInfo);
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
            var confirmInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillGetItemInfos(getItemInfos));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSymbolClose()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.HideRecordList();
        }

        private void CommandRefresh()
        {
            _view.SetTurns(_model.RemainTurns);
            _view.SetStageInfo(_model.CurrentStage);
            _view.SetAlcanaInfo(_model.AlcanaSkillInfos());
            _view.SetTacticsCharaLayer(_model.StageMembers());
            _view.SetEvaluate(_model.PartyEvaluate(),_model.TroopEvaluate());
            _view.CommandRefresh();
                    
        }

        private void CommandCallEnemyInfo(SymbolInfo symbolInfo)
        {
            switch (symbolInfo.SymbolType)
            {
                case SymbolType.Battle:
                case SymbolType.Boss:
                    var enemyInfos = symbolInfo.BattlerInfos();
                    
                    var enemyViewInfo = new StatusViewInfo(() => {
                        _view.CommandGameSystem(Base.CommandType.CloseStatus);
                        _view.ChangeUIActive(true);
                    });
                    enemyViewInfo.SetEnemyInfos(enemyInfos,false);
                    _view.CommandCallEnemyInfo(enemyViewInfo);
                    _view.ChangeUIActive(false);
                    break;
                case SymbolType.Alcana:
                    CommandPopupSkillInfo(symbolInfo.GetItemInfos);
                    break;
                case SymbolType.Actor:
                    _model.SetTempAddActorStatusInfos(symbolInfo.GetItemInfos[0].Param1);
                    var statusViewInfo = new StatusViewInfo(() => {
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
                        _model.SetTempAddSelectActorGetItemInfoStatusInfos(symbolInfo.GetItemInfos);
                    }
                    var statusViewInfo2 = new StatusViewInfo(() => {
                        _view.CommandGameSystem(Base.CommandType.CloseStatus);
                        _view.ChangeUIActive(true);
                    });
                    statusViewInfo2.SetDisplayCharacterList(false);
                    _view.CommandCallStatus(statusViewInfo2);
                    _view.ChangeUIActive(false);
                    break;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }


        private void CommandSelectSideMenu()
        {
            var sideMenuViewInfo = new SideMenuViewInfo();
            sideMenuViewInfo.EndEvent = () => {

            };
            sideMenuViewInfo.CommandLists = _model.SideMenu();
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }

        private void CommandStageHelp()
        {
            _view.CommandHelpList(DataSystem.HelpText("Tactics"));
        }

        private void CommandCancelRecordList()
        {
            _view.HideRecordList();
            _view.HideParallelList();
            _view.ChangeSymbolBackCommandActive(false);
            _backCommand = Tactics.CommandType.CancelSymbolRecord;
        }

        private void CommandCommandHelp()
        {
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                _view.CommandHelpList(DataSystem.HelpText("Battle"));
                return;
                case TacticsCommandType.Train:
                _view.CommandHelpList(DataSystem.HelpText("LevelUp"));
                return;
                case TacticsCommandType.Alchemy:
                _view.CommandHelpList(DataSystem.HelpText("Alchemy"));
                return;
                /*
                case TacticsCommandType.Recovery:
                _view.CommandHelpList(DataSystem.HelpText("Recovery"));
                return;
                */
            }
            
        }

        private void CommandAlcanaCheck()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.SetAlcanaSelectInfos(ListData.MakeListData(_model.AlcanaSkillInfos()));
            _backCommand = Tactics.CommandType.HideAlcanaList;
        }

        private void CommandHideAlcanaList()
        {
            _view.HideAlcanaList();
            _view.ChangeBackCommandActive(false);
            _view.ChangeSymbolBackCommandActive(false);
            _backCommand = Tactics.CommandType.None;
        }
    }
}