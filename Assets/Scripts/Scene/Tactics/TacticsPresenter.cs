using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class TacticsPresenter : BasePresenter
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
            if (_model.BattleResultVictory() == false && GameSystem.SceneStackManager.LastScene == Scene.Strategy)
            {
                // 敗北して戻ってきたとき
                var currentRecord = _model.CurrentSelectRecord();
                if (currentRecord != null)
                {
                    CommandTacticsCommand(TacticsCommandType.Paradigm);
                    CommandStageSymbol();
                    CommandSelectRecordSeek(currentRecord.StageSymbolData.Seek);
                    CommandSelectRecord(currentRecord);
                }
            }
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
                } else
                {            
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
                    CommandSelectRecord((SymbolResultInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.SymbolClose:
                    CommandSymbolClose();
                    break;
                case Tactics.CommandType.CallEnemyInfo:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolResultInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                    break;
                case Tactics.CommandType.SelectRecord:
                    CommandSelectRecordSeek((int)viewEvent.template);
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
                case Tactics.CommandType.ScorePrize:
                    CommandScorePrize();
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

        private void CommandSelectRecordSeek(int seek)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //Seekに対応したシンボルを表示
            _view.SetSymbols(_model.StageRecords(seek));
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
                    _view.SetSymbols(_model.StageRecords(_model.CurrentStage.CurrentTurn));
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

        private void CommandSelectRecord(SymbolResultInfo recordInfo)
        {
            var currentTurn = _model.CurrentStage.CurrentTurn;
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
                    _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                    _view.ActivateTacticsCommand();
                    _view.ShowConfirmCommand();
                    CommandRefresh();
                    _backCommand = Tactics.CommandType.CancelSelectSymbol;
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
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
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
                var getItemInfos = _model.CurrentSelectRecord().SymbolInfo.GetItemInfos;
                var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
                GotoStrategyScene(getItemInfos,actorInfos);
            } else
            {
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
                if (_model.CurrentSelectRecord().StageSymbolData.Param2 == 0)
                {
                    _model.SetTempAddSelectActorStatusInfos();
                } else
                {
                    _model.SetTempAddSelectActorGetItemInfoStatusInfos(_model.CurrentSelectRecord().SymbolInfo.GetItemInfos);
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
            } else
            {
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
                var currentSymbol = _model.CurrentSelectRecord();
                GotoStrategyScene(currentSymbol.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
                CommandTacticsCommand(_model.TacticsCommandType);
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
                var currentRecord = _model.CurrentSelectRecord();
                GotoStrategyScene(currentRecord.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
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
                var currentSymbol = _model.CurrentSelectRecord();
                GotoStrategyScene(currentSymbol.SymbolInfo.GetItemInfos,_model.StageMembers());
            } else
            {
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
                    
                    var enemyViewInfo = new StatusViewInfo(() => {
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
                        _model.SetTempAddSelectActorGetItemInfoStatusInfos(symbolInfo.SymbolInfo.GetItemInfos);
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
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {
                },
                CommandLists = _model.SideMenu()
            };
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
            }
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