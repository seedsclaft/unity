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
                    _model.ChangeSubordinate(stageEvent.Param == 1);
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
                    var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
                    Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
                    
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
                    _model.SetDisplayTurns();
                    break;
                case StageEventType.MoveStage:
                    _eventBusy = true;
                    _model.AddEventReadFlag(stageEvent);
                    _model.MoveStage(stageEvent.Param);
                    _view.CommandSceneChange(Scene.Tactics);
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

    private async void InitializeView()
    {
        _model.RefreshTacticsEnable();
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.ChangeBackCommandActive(false);
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetSideMenu(_model.SideMenu());
        _view.SetSelectCharacter(_model.TacticsCharacterData(),_model.NoChoiceConfirmCommand(),_model.CommandRankInfo());
        _view.SetTacticsCharaLayer(_model.StageMembers());
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetSymbols(ListData.MakeListData(_model.TacticsSymbols()));

        _view.SetTacticsCommand(_model.TacticsCommand());
        _view.HideCommandList();
        CommandRefresh();
        //_view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
        var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        Ryneus.SoundManager.Instance.PlayCrossFadeBgm(bgm,1.0f);
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
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Train)
            {
                CommandSelectActorTrain(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                _model.SetSelectAlchemyActorId(actorId);
                CommandSelectActorAlchemy();
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Recovery)
            {
                CommandSelectRecoveryPlus(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                CommandSelectActorParadigm(actorId);
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
            _model.SetSelectAlchemyActorId(actorId);
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
                // 魔法装備
                CommandSkillEquip((SkillInfo)viewEvent.template);  
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
        _view.CommandConfirmClose();
    }


    private void UpdatePopupSelectAddActor(ConfirmCommandType confirmCommandType)
    {
        _model.SetSelectAddActor();
        _view.CommandConfirmClose();
        var statusViewInfo = new StatusViewInfo(null);
        statusViewInfo.SetDisplayDecideButton(true);
        statusViewInfo.SetDisplayBackButton(false);
        _view.CommandCallStatus(statusViewInfo);
    }

    private void UpdatePopupSaveCommand(ConfirmCommandType confirmCommandType,bool isReturnScene)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var saveNeedAds = _model.NeedAdsSave();
            if (saveNeedAds)
            {
                // ロード表示
                _view.CommandCallLoading();
#if UNITY_ANDROID
                AdMobController.Instance.PlayRewardedAd(() => 
                {
                    SuccessSave(isReturnScene);
                },
                () => {
                    // ロード非表示
                    _view.CommandLoadingClose();
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
        _view.CommandLoadingClose();
        _model.GainSaveCount();
        _model.SavePlayerStageData(true);
        // 成功表示
        var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(11084).Text,(a) => {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandConfirmClose();
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
        _view.CommandPopupClose();
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
                _view.ShowTrainCharacter(_model.StageMembers()[0],_model.StageMembers());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(true);
                _backCommand = Tactics.CommandType.TacticsCommandClose;
                break;
            case TacticsCommandType.Alchemy:
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ShowAttributeList(_model.SelectActorAlchemy());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(true);

                //_view.HideAttributeList();
                _view.ShowSelectCharacterCommand();
                _backCommand = Tactics.CommandType.TacticsCommandClose;
                break;
        }
        if (tacticsCommandType == TacticsCommandType.Status)
        {
            _model.SetStageActor();
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
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


    private void CommandSelectActorTrain(int actorId)
    {
        _model.SelectActorTrain(actorId);
        _view.ShowTrainCharacter(_model.TacticsActor(actorId),_model.StageMembers());
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
        // 装備中なら
        if (_model.SkillEquipmentActor())
        {
            //CheckRemoveAlchemy();
            ///return;
        }
        _view.ShowAttributeList(_model.SelectActorAlchemy());
        _view.ChangeBackCommandActive(true);
        _view.HideSelectCharacterCommand();
        _backCommand = Tactics.CommandType.SelectAlchemyClose;
    }

    private void CheckRemoveAlchemy()
    {
        var popupInfo = new ConfirmInfo(_model.SelectAlchemyActorInfo().Master.Name + "の加護をはずしますか？\n習得進捗は初期化されます！",(a) => UpdatePopupRemoveSkill((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupRemoveSkill(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.RemoveAlchemy();
            CommandSelectActorAlchemy();
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        CommandRefresh();
    }

    private void CommandSelectActorRecovery(int actorId)
    {
    }

    private void CommandSelectActorParadigm(int actorId)
    {
        _model.SelectActorParadigm(actorId);
        CommandRefresh();
    }

    private void CommandSelectRecoveryPlus(int actorId)
    {
        _model.SelectRecoveryPlus(actorId);
        CommandRefresh();
    }


    private void CommandSelectSymbol(int enemyIndex)
    {
        _model.CurrentEnemyIndex = enemyIndex;
        _model.SetBattleEnemyIndex();
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
                _view.ActivateTacticsCommand();
                _view.ShowConfirmCommand();
                _backCommand = Tactics.CommandType.TacticsCommand;
                break;
            case SymbolType.Recover:
                CheckRecoverSymbol(currentSymbol.GetItemInfos[0]);
                break;
            case SymbolType.Actor:
                CheckActorSymbol(currentSymbol.GetItemInfos);
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

    private void CheckActorSymbol(List<GetItemInfo> getItemInfos)
    {
        var popupInfo = new ConfirmInfo("仲間を選びますか？",(a) => UpdatePopupActorSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupActorSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var selectAddActor = new ConfirmInfo(DataSystem.GetTextData(11050).Text,(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmCommandType)menuCommandInfo));
            selectAddActor.SetIsNoChoice(true);
            selectAddActor.SetSelectIndex(0);
            _view.CommandCallConfirm(selectAddActor);
            _view.ChangeUIActive(false);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CheckRecoverSymbol(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo(getItemInfo.ResultName + "を発動しますか？",(a) => UpdatePopupRecoverSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupRecoverSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempData.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempData.SetTempResultActorInfos(_model.StageMembers());
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CheckAlcanaSymbol(List<GetItemInfo> getItemInfos)
    {
        _view.ShowAttributeList(_model.AlcanaMagicSkillInfos(getItemInfos));
        _view.ChangeBackCommandActive(true);
        _backCommand = Tactics.CommandType.TacticsCommand;        
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSkillAlchemy(SkillInfo skillInfo)
    {
        _model.SelectAlchemySkill(skillInfo.Id);
        _view.HideAttributeList();
        //_view.ShowSelectCharacterCommand();
        CommandRefresh();
        _view.ChangeBackCommandActive(false);
        //_backCommand = Tactics.CommandType.SelectActorAlchemy;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        CheckAlcanaSymbol(skillInfo);
    }

    
    private void CheckAlcanaSymbol(SkillInfo skillInfo)
    {
        var popupInfo = new ConfirmInfo(skillInfo.Master.Name + "を取得しますか？",(a) => UpdatePopupAlcanaSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupAlcanaSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var skill = _model.CurrentStage.CurrentSelectSymbol().GetItemInfos.FindAll(a => a.Param1 == _model.SelectAlcanaSkillId);
            _model.TempData.SetTempGetItemInfos(skill);
            _model.TempData.SetTempResultActorInfos(_model.StageMembers());
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CheckResourceSymbol(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo(getItemInfo.Param1 + "(Nu)を取得しますか？",(a) => UpdatePopupResourceSymbol((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupResourceSymbol(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempData.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempData.SetTempResultActorInfos(_model.StageMembers());
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
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
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var currentSymbol = _model.CurrentStage.CurrentSelectSymbol();
            _model.TempData.SetTempGetItemInfos(currentSymbol.GetItemInfos);
            _model.TempData.SetTempResultActorInfos(_model.StageMembers());
            _view.CommandSceneChange(Scene.Strategy);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            CommandTacticsCommand(_model.TacticsCommandType);
        }
    }

    private void CommandSkillEquip(SkillInfo skillInfo)
    {
        _model.SetSkillEquip(skillInfo.Id);
        CommandTacticsCommand(_model.TacticsCommandType);
    }

    private void CommandSelectEnemyClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetInBattle(true);
            _model.SaveTempBattleMembers();
            _view.CommandChangeViewToTransition(null);
            Ryneus.SoundManager.Instance.ChangeCrossFade();
            _view.CommandSceneChange(Scene.Battle);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.ShowSymbolList();
            _view.HideSelectCharacter();
            CommandRefresh();
        }
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
        
        _view.CommandRefresh();
                
    }

    private void CommandCallEnemyInfo(int troopInfoIndex)
    {
        var enemyInfos = _model.TacticsSymbols()[troopInfoIndex].BattlerInfos();
        
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.ChangeUIActive(true);
        });
        statusViewInfo.SetEnemyInfos(enemyInfos,false);
        _view.CommandCallEnemyInfo(statusViewInfo);
        _view.ChangeUIActive(false);
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
