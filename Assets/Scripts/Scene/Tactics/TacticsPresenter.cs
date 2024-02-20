using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TacticsPresenter :BasePresenter
{
    TacticsModel _model = null;
    TacticsView _view = null;

    private bool _busy = true;
    private bool _eventBusy = false;

    private Tactics.CommandType _backCommand = Tactics.CommandType.None;
    public TacticsPresenter(TacticsView view)
    {
        _view = view;
        SetView(_view);
        _model = new TacticsModel();
        SetModel(_model);

        if (CheckBeforeAlcanaEvent())
        {
            InitializeView();
            _busy = true;
            _view.StartAlcanaAnimation(() => 
            {
                _view.CommandSceneChange(Scene.AlcanaResult);
            });
            return;
        }
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
        if (CheckCurrentAlcanaEvent())
        {
            InitializeView();
            _busy = true;
            _view.StartAlcanaAnimation(() => 
            {
                _view.CommandSceneChange(Scene.AlcanaResult);
            });
            return;
        }
        Initialize();
    }

    private async void CheckStageEvent()
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
                    _model.SetNeedAllTacticsCommand(true);
                    break;
                case StageEventType.IsSubordinate:
                    break;
                case StageEventType.IsAlcana:
                    _model.SetIsAlcana(stageEvent.Param == 1);
                    break;
                case StageEventType.SelectAddActor:
                    _eventBusy = true;
                    _model.AddEventReadFlag(stageEvent);
                    var selectAddActor = new ConfirmInfo(DataSystem.GetTextData(11050).Text,(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmCommandType)menuCommandInfo));
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
                    _model.SetDefineBossIndex(stageEvent.Param);
                    break;
                case StageEventType.SetRouteSelectParam:
                    _view.CommandSetRouteSelect();
                    break;
                case StageEventType.ClearStage:
                    _eventBusy = true;
                    _model.AddEventReadFlag(stageEvent);
                    _model.StageClear();
                    _view.CommandSceneChange(Scene.MainMenu);
                    break;
                case StageEventType.ChangeRouteSelectStage:
                    _eventBusy = true;
                    _model.AddEventReadFlag(stageEvent);
                    _model.ChangeRouteSelectStage(stageEvent.Param);
                    _view.CommandSceneChange(Scene.Tactics);
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

    private bool CheckBeforeAlcanaEvent()
    {
        var results = _model.CheckAlcanaSkillInfos(TriggerTiming.BeforeTacticsTurn);
        _model.SetAlcanaSkillInfo(results);
        return results.Count > 0;
    }

    private bool CheckCurrentAlcanaEvent()
    {
        var results = _model.CheckAlcanaSkillInfos(TriggerTiming.CurrentTacticsTurn);
        _model.SetAlcanaSkillInfo(results);
        return results.Count > 0;
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
            _view.CommandSceneChange(Scene.Tactics);
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
            _view.CommandSceneChange(Scene.RebornResult);
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
            _view.CommandSceneChange(Scene.Tactics);
        },_model.CurrentStage.SelectActorIdsClassId(0));
        if (isAbort)
        {
            return;
        }
        _busy = false;
    }

    private void InitializeView()
    {
        _model.RefreshTacticsEnable();
        _model.AssignBattlerIndex();
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.ChangeBackCommandActive(false);
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetSideMenu(_model.SideMenu());
        _view.SetSelectCharacter(_model.TacticsCharacterData(),_model.NoChoiceConfirmCommand());
        
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));

        _view.SetTacticsCommand(_model.TacticsCommand());
        _view.HideCommandList();
        _view.SetBackGround(_model.CurrentStage.Master.BackGround);
        CommandRefresh();
        PlayTacticsBgm();
        //_view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
        _view.ShowCommandList();
        if (_model.SetStageTutorials(EventTiming.StartTactics))
        {
            _view.CommandCallTutorialFocus(_model.CurrentStageTutorialDates[0]);
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
            } else{            
                _view.CommandCallTutorialFocus(_model.CurrentStageTutorialDates[0]);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommand)
        {
            CommandTacticsCommand((TacticsCommandType)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor)
        {
            var tacticsActorInfo = (TacticsActorInfo)viewEvent.template;
            var actorId = tacticsActorInfo.ActorInfo.ActorId;
            _model.SetSelectActorId(actorId);
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Train)
            {
                CommandSelectActorTrain();
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                CommandSelectActorAlchemy();
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Recovery)
            {
                CommandSelectRecoveryPlus();
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                CommandSelectActorParadigm();
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommandClose)
        {
            if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                CommandSelectEnemyClose((ConfirmCommandType)viewEvent.template);
            } else
            {
                CommandTacticsCommandClose((ConfirmCommandType)viewEvent.template);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorAlchemy)
        {
            int actorId = (int)viewEvent.template;
            _model.SetSelectActorId(actorId);
            CommandSelectActorAlchemy();
        }
        if (viewEvent.commandType == Tactics.CommandType.SkillAlchemy)
        {
            if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                CommandSkillAlchemy((SkillInfo)viewEvent.template);  
            } else
            if (_model.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                // 魔法習得
                CommandLearnSkill((SkillInfo)viewEvent.template);  
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectAlchemyClose)
        {
            CommandTacticsCommand(TacticsCommandType.Alchemy);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectFrontBattleIndex)
        {
            if (_model.CurrentStageTutorialDates.Count > 0) return;
            int actorId = (int)viewEvent.template;
            CommandSelectFrontBattleIndex(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectBackBattleIndex)
        {
            if (_model.CurrentStageTutorialDates.Count > 0) return;
            int actorId = (int)viewEvent.template;
            CommandSelectBackBattleIndex(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectSymbol)
        {
            CommandSelectSymbol((int)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.PopupSkillInfo)
        {
            CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.SymbolClose)
        {
            CommandSymbolClose();
        }
        if (viewEvent.commandType == Tactics.CommandType.CallEnemyInfo)
        {
            if (_model.CurrentStageTutorialDates.Count > 0) return;
            CommandCallEnemyInfo((int)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.Rule)
        {
            CommandRule();
        }
        if (viewEvent.commandType == Tactics.CommandType.Dropout)
        {
            CommandDropout();
        }
        if (viewEvent.commandType == Tactics.CommandType.Option)
        {
            CommandOption();
        }
        if (viewEvent.commandType == Tactics.CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.CommandData)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.AlcanaCheck)
        {
            CommandAlcanaCheck();
        }
        if (viewEvent.commandType == Tactics.CommandType.ChangeSelectTacticsActor)
        {
            _model.SetSelectActorId((int)viewEvent.template);
            if (_model.TacticsCommandType == TacticsCommandType.Train)
            {
                _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
            } else
            if (_model.TacticsCommandType == TacticsCommandType.Recovery)
            {
                _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
            } else
            if (_model.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                _view.ShowLeaningList(_model.SelectActorLearningMagicList());
            } else
            if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
            }
        }
        if (_model.NeedAllTacticsCommand)
        {
        }
    }

    private void UpdatePopupDropout(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _model.SavePlayerStageData(false);
            _view.CommandSceneChange(Scene.MainMenu);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        //_view.ActivateCommandList();
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
    }


    private void UpdatePopupSelectAddActor(ConfirmCommandType confirmCommandType)
    {
        _model.SetSelectAddActor();
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
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (isReturnScene)
            {
                _view.CommandSceneChange(Scene.Tactics);
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
        var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(11084).Text,(a) => {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (isReturnScene)
            {
                _view.CommandSceneChange(Scene.Tactics);
            } else
            {        
                _view.ChangeUIActive(true);
            }
        });
        confirmInfo.SetIsNoChoice(true);
        _view.CommandCallConfirm(confirmInfo);
    }

    private void UpdatePopupSkillInfo(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.ClosePopup);
    }

    private void CommandBack()
    {
        var eventData = new TacticsViewEvent(_backCommand);
        if (_backCommand == Tactics.CommandType.SelectActorAlchemy)
        {
        } else{
            eventData.template = _model.TacticsCommandType;
        }
        UpdateCommand(eventData);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandTacticsCommand(TacticsCommandType tacticsCommandType)
    {
        _model.SetTacticsCommandType(tacticsCommandType);
        _view.HideConfirmCommand();
        if (tacticsCommandType == TacticsCommandType.Paradigm)
        {
            _view.ChangeBackCommandActive(true);
            _backCommand = Tactics.CommandType.SymbolClose;
            _view.ShowSymbolList();
            _view.ShowCommandList();
            _view.HideSelectCharacter();
            _view.HideAttributeList();
            return;
        }

        switch (tacticsCommandType)
        {
            case TacticsCommandType.Train:
            case TacticsCommandType.Recovery:
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ShowCharacterDetail(_model.StageMembers()[0],_model.StageMembers());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(true);
                _backCommand = Tactics.CommandType.TacticsCommandClose;
                break;
            case TacticsCommandType.Alchemy:
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ShowLeaningList(_model.SelectActorLearningMagicList());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(true);

                //_view.HideAttributeList();
                _view.ShowSelectCharacterCommand();
                _backCommand = Tactics.CommandType.TacticsCommandClose;
                break;
        }
        if (tacticsCommandType == TacticsCommandType.Status)
        {
            _model.SetStatusActorInfos();
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                _view.ShowCommandList();
                _view.SetHelpInputInfo("TACTICS");
            });
            _view.ChangeUIActive(false);
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
        }
        //CommandRefresh();
        _view.HideCommandList();
        _view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
    }


    private void CommandSelectActorTrain()
    {
        _model.SelectActorTrain();
        _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
        CommandRefresh();
    }

    private void CommandTacticsCommandClose(ConfirmCommandType confirmCommandType)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _view.ChangeBackCommandActive(false);
        _view.ShowCommandList();
        _view.HideSelectCharacter();
    }

    private void CommandSelectActorAlchemy()
    {
    }

    private void CommandSelectActorParadigm()
    {
        _model.SetInBattle();
        CommandRefresh();
    }

    private void CommandSelectRecoveryPlus()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.SelectRecoveryPlus();
        _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
        CommandRefresh();
    }


    private void CommandSelectSymbol(int symbolIndex)
    {
        _model.SetStageSeekIndex(symbolIndex);
        _view.HideSymbolList();
        // 回路解析
        var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
        switch (currentSymbol.SymbolType)
        {
            case SymbolType.Battle:
            case SymbolType.Boss:
                _view.ChangeBackCommandActive(true);
                _view.HideCommandList();
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                _view.ActivateTacticsCommand();
                _view.ShowConfirmCommand();
                _backCommand = Tactics.CommandType.TacticsCommand;
                break;
            case SymbolType.Recover:
                CheckRecoverSymbol(currentSymbol.GetItemInfos[0]);
                break;
            case SymbolType.Actor:
                CheckActorSymbol(currentSymbol.GetItemInfos[0]);
                break;
            case SymbolType.Alcana:
                CheckAlcanaSymbol(currentSymbol.GetItemInfos);
                break;
            case SymbolType.Resource:
                CheckResourceSymbol(currentSymbol.GetItemInfos[0]);
                break;
            case SymbolType.Rebirth:
                CheckRebirthSymbol(currentSymbol.GetItemInfos[0]);
                break;
        }
        
    }

    private void CommandSelectFrontBattleIndex(int actorId)
    {
        _model.ChangeBattleLineIndex(actorId,true);
        CommandRefresh();
    }

    private void CommandSelectBackBattleIndex(int actorId)
    {
        _model.ChangeBattleLineIndex(actorId,false);
        CommandRefresh();
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
            var getItemInfos = _model.CurrentStage.CurrentSelectSymbol().GetItemInfos;
            var actorInfos = _model.PartyInfo.ActorInfos.FindAll(a => a.ActorId == getItemInfos[0].Param1);
            _model.TempInfo.SetTempGetItemInfos(getItemInfos);
            _model.TempInfo.SetTempResultActorInfos(actorInfos);
            _model.ResetBattlerIndex();
            _view.CommandSceneChange(Scene.Strategy);
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
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempInfo.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempInfo.SetTempResultActorInfos(_model.StageMembers());
            _model.ResetBattlerIndex();
            _view.CommandSceneChange(Scene.Strategy);
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
        CheckAlcanaSymbol(_model.AlcanaMagicSkillInfos(getItemInfos)[0]);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSkillAlchemy(SkillInfo skillInfo)
    {
        /*
        _model.AddAlchemySkill(skillInfo.Id);
        _view.HideAttributeList();
        //_view.ShowSelectCharacterCommand();
        CommandRefresh();
        _view.ChangeBackCommandActive(false);
        //_backCommand = Tactics.CommandType.SelectActorAlchemy;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        CheckAlcanaSymbol(skillInfo);
        */
    }

    
    private void CheckAlcanaSymbol(SkillInfo skillInfo)
    {
        var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,skillInfo.Master.Name),(a) => UpdatePopupAlcanaSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupAlcanaSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var getItemInfos = _model.CurrentStage.CurrentSelectSymbol().GetItemInfos;
            _model.TempInfo.SetTempGetItemInfos(getItemInfos);
            _model.TempInfo.SetTempResultActorInfos(_model.StageMembers());
            _model.ResetBattlerIndex();
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CheckResourceSymbol(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11140,getItemInfo.Param1 + DataSystem.GetTextData(1000).Text),(a) => UpdatePopupResourceSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempInfo.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempInfo.SetTempResultActorInfos(_model.StageMembers());
            _model.ResetBattlerIndex();
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            CommandTacticsCommand(_model.TacticsCommandType);
        }
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
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempInfo.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempInfo.SetTempResultActorInfos(_model.StageMembers());
            _model.ResetBattlerIndex();
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CommandLearnSkill(SkillInfo skillInfo)
    {
        var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,skillInfo.LearningCost.ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var skillInfo = _view.SelectMagic;
            _model.LearnMagic(skillInfo.Id);
            CommandTacticsCommand(_model.TacticsCommandType);
            CommandRefresh();
        }
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
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
                if (_model.CurrentStage.CurrentSelectSymbol().SymbolType == SymbolType.Boss)
                {
                    Ryneus.SoundManager.Instance.StopBgm();
                } else
                {
                    var bgmData = DataSystem.Data.GetBGM(_model.TacticsBgmKey());
                    if (bgmData.CrossFade != "" && Ryneus.SoundManager.Instance.CrossFadeMode)
                    {
                        Ryneus.SoundManager.Instance.ChangeCrossFade();
                    } else
                    {
                        PlayTacticsBgm();
                    }
                }
                _view.SetActiveBackGround(false);
                _model.SetPartyBattlerIdList();
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
        var popupInfo = new ConfirmInfo(DataSystem.GetTextData(11160).Text,(a) => UpdatePopupBattleMember());
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupBattleMember()
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
    }

    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo((ConfirmCommandType)a));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallSkillDetail(popupInfo);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSymbolClose()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _view.HideSymbolList();
        //_view.SetActiveEnemyListClose(false);
        _view.ShowCommandList();
    }

    private void CommandRefresh()
    {
        _view.SetTurns(_model.RemainTurns);
        _view.SetNuminous(_model.Currency);
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetAlcanaInfo(_model.StageAlcana);
        _view.SetTacticsCharaLayer(_model.StageMembers());
        _view.CommandRefresh();
                
    }

    private void CommandCallEnemyInfo(int symbolIndex)
    {
        var symbolInfo = _model.TacticsSymbols()[symbolIndex];
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
                CommandPopupSkillInfo(symbolInfo.GetItemInfos[0]);
                break;
            case SymbolType.Actor:
                _model.SetTempAddActorStatusInfos(symbolInfo.GetItemInfos[0].Param1);
                var statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                _view.CommandCallStatus(statusViewInfo);
                _view.ChangeUIActive(false);
                break;
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandRule()
    {
        _busy = true;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.SetHelpInputInfo("RULING");
        _view.CommandCallRuling(() => {
            _busy = false;
            _view.SetHelpInputInfo("OPTION");
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
    }

    private void CommandDropout()
    {  
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        var popupInfo = new ConfirmInfo(DataSystem.GetTextData(1100).Text,(a) => UpdatePopupDropout((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void CommandSave(bool isReturnScene)
    {
#if UNITY_ANDROID
        var savePopupTitle = _model.SavePopupTitle();
        var saveNeedAds = _model.NeedAdsSave();
        var popupInfo = new ConfirmInfo(savePopupTitle,(a) => UpdatePopupSaveCommand((ConfirmCommandType)a,isReturnScene));
        
        popupInfo.SetSelectIndex(1);
        if (saveNeedAds)
        {
            //popupInfo.SetDisableIds(new List<int>(){1});
            popupInfo.SetCommandTextIds(_model.SaveAdsCommandTextIds());
        } else
        {
        }
        _view.CommandCallConfirm(popupInfo);
        _view.ChangeUIActive(false);
#elif UNITY_WEBGL
        SuccessSave(isReturnScene);
#endif
    }

    private void CommandSelectSideMenu(SystemData.CommandData sideMenu)
    {
        if (sideMenu.Key == "Retire")
        {
            CommandDropout();
        }
        if (sideMenu.Key == "Help")
        {
            CommandRule();
        }
        if (sideMenu.Key == "Save")
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandSave(false);
        }
    }

    public void CommandOption()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallOption(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }

    private void CommandAlcanaCheck()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.CommandCallAlcanaList(() => {
            _view.ChangeUIActive(true);
            _busy = false;
        });
        _busy = true;
        _view.ChangeUIActive(false);
    }
}
