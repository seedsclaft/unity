using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsPresenter :BasePresenter
{
    TacticsModel _model = null;
    TacticsView _view = null;

    private bool _busy = true;

    private Tactics.CommandType _backCommand = Tactics.CommandType.None;
    public TacticsPresenter(TacticsView view)
    {
        _view = view;
        SetView(_view);
        _model = new TacticsModel();
        SetModel(_model);

        if (CheckBeforeTacticsAdvEvent())
        {
            return;
        }
        if (CheckStageEvent())
        {
            return;
        }
        if (CheckAdvEvent())
        {
            return;
        }
        if (CheckRebornEvent())
        {
            return;
        }
        Initialize();
    }

    private bool CheckStageEvent()
    {
        var isEvent = false;
        // イベントチェック
        var stageEvents = _model.StageEvents(EventTiming.StartTactics);
        foreach (var stageEvent in stageEvents)
        {
            switch (stageEvent.Type)
            {
                case StageEventType.CommandDisable:
                    _model.SetTacticsCommandEnables((TacticsCommandType)stageEvent.Param,false);
                    break;
                case StageEventType.TutorialBattle:
                    _model.TutorialTroopData();
                    break;
                case StageEventType.NeedAllTactics:
                    _model.SetNeedAllTacticsCommand(true);
                    break;
                case StageEventType.IsSubordinate:
                    _model.ChangeSubordinate(stageEvent.Param == 1);
                    _model.AddEventReadFlag(stageEvent);
                    break;
                case StageEventType.IsAlcana:
                    _model.SetIsAlcana(stageEvent.Param == 1);
                    break;
                case StageEventType.SelectAddActor:
                    var selectAddActor = new ConfirmInfo(DataSystem.System.GetTextData(11050).Text,(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmCommandType)menuCommandInfo));
                    selectAddActor.SetIsNoChoice(true);
                    selectAddActor.SetSelectIndex(0);
                    _view.CommandCallConfirm(selectAddActor);
                    _view.ChangeUIActive(false);
                    _model.AddEventReadFlag(stageEvent);
                    isEvent = true;
                    break;
                case StageEventType.SaveCommand:
                    var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(11080).Text,(menuCommandInfo) => UpdatePopupSaveCommand((ConfirmCommandType)menuCommandInfo));
                    popupInfo.SetSelectIndex(1);
                    _view.CommandCallConfirm(popupInfo);
                    _view.ChangeUIActive(false);
                    _model.AddEventReadFlag(stageEvent);
                    isEvent = true;
                    break;
                case StageEventType.SetDefineBossIndex:
                    _model.SetDefineBossIndex(stageEvent.Param);
                    break;
                case StageEventType.SetRouteSelectParam:
                    _view.CommandSetRouteSelect();
                    _model.AddEventReadFlag(stageEvent);
                    break;
                case StageEventType.AbortStage:
                    _model.StageClear();
                    _model.AddEventReadFlag(stageEvent);
                    isEvent = true;
                    _view.CommandSceneChange(Scene.MainMenu);
                    break;
                case StageEventType.ChangeRouteSelectStage:
                    _model.ChangeRouteSelectStage(stageEvent.Param);
                    _model.AddEventReadFlag(stageEvent);
                    isEvent = true;
                    _view.CommandSceneChange(Scene.Tactics);
                    break;
                case StageEventType.RouteSelectBattle:
                    _model.RouteSelectTroopData();
                    _model.AddEventReadFlag(stageEvent);
                    break;
            }
            if (isEvent)
            {
                return true;
            }
        }
        return isEvent;
    }

    private bool CheckAdvEvent()
    {
        var StartTacticsAdvData = _model.StartTacticsAdvData();
        if (StartTacticsAdvData != null)
        {
            AdvCallInfo advInfo = new AdvCallInfo();
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

    private async void Initialize()
    {
        _model.RefreshTacticsEnable();

        _view.SetHelpWindow();
        
        
        _view.SetUIButton();
        _view.ChangeBackCommandActive(false);
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetSideMenu(_model.SideMenu());
        _view.SetSelectCharacter(_model.StageMembers(),_model.ConfirmCommand(),_model.CommandRankInfo());
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetEnemies(_model.TroopInfoListDates(_model.TacticsTroops()));

        _view.SetTacticsCommand(_model.TacticsCommand());
        _view.HideCommandList();
        CommandRefresh();
        //_view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
        var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        var isAbort = CheckAdvStageEvent(EventTiming.StartTactics,() => {
            _view.CommandSceneChange(Scene.Tactics);
        },_model.CurrentStage.SelectActorIdsClassId(0));
        if (isAbort)
        {
            return;
        }
        // アルカナ配布
        if (_model.CheckIsAlcana())
        {
            CommandAddAlcana();
        }
        DisableTacticsCommand();
        _view.ShowCommandList();
        _busy = false;
    }

    private void UpdateCommand(TacticsViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        Debug.Log(viewEvent.commandType);
        if (viewEvent.commandType == Tactics.CommandType.AddAlcana)
        {
            CommandAddAlcana();
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommand)
        {
            CommandTacticsCommand((TacticsCommandType)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor)
        {
            var tacticsActorInfo = (TacticsActorInfo)viewEvent.template;
            var actorId = tacticsActorInfo.ActorInfo.ActorId;
            if (CheckBusyOther(actorId,tacticsActorInfo.TacticsCommandType)) return;
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Train)
            {
                CommandSelectActorTrain(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                CommandSelectActorAlchemy(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Resource)
            {
                CommandSelectActorResource(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Recovery)
            {
                CommandSelectActorRecovery(actorId);
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Battle)
            {
                CommandSelectActorBattle(actorId);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommandClose)
        {
            if (_model.TacticsCommandType == TacticsCommandType.Battle)
            {
                CommandTacticsCommand(TacticsCommandType.Battle);
            } else
            {
                CommandTacticsCommandClose((ConfirmCommandType)viewEvent.template);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorAlchemy)
        {
            int actorId = (int)viewEvent.template;
            if (CheckBusyOther(actorId,TacticsCommandType.Alchemy)) return;
            CommandSelectActorAlchemy(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SkillAlchemy)
        {
            CommandSkillAlchemy((SkillInfo)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectAlchemyClose)
        {
            CommandTacticsCommand(TacticsCommandType.Alchemy);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryPlus)
        {
            int actorId = (int)viewEvent.template;
            if (CheckBusyOther(actorId,TacticsCommandType.Recovery)) return;
            CommandSelectRecoveryPlus(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryMinus)
        {
            int actorId = (int)viewEvent.template;
            if (CheckBusyOther(actorId,TacticsCommandType.Recovery)) return;
            CommandSelectRecoveryMinus(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectBattleEnemy)
        {
            CommandSelectBattleEnemy((int)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.PopupSkillInfo)
        {
            CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
        }
        if (viewEvent.commandType == Tactics.CommandType.EnemyClose)
        {
            CommandEnemyClose();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorResource)
        {
            int actorId = (int)viewEvent.template;
            if (CheckBusyOther(actorId,TacticsCommandType.Resource)) return;
            CommandSelectActorResource(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.OpenAlcana)
        {
            CommandOpenAlcana();
        }
        if (viewEvent.commandType == Tactics.CommandType.CallEnemyInfo)
        {
            CommandCallEnemyInfo((TroopInfo)viewEvent.template);
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
        if (viewEvent.commandType == Tactics.CommandType.OpenSideMenu)
        {
            CommandOpenSideMenu();
        }
        if (viewEvent.commandType == Tactics.CommandType.CloseSideMenu)
        {
            CommandCloseSideMenu();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.CommandData)viewEvent.template);
        }
        if (_model.NeedAllTacticsCommand)
        {
            var listData = _model.ChangeEnableCommandData((int)TacticsCommandType.TurnEnd - 1, !_model.CheckNonBusy());
            _view.RefreshListData(listData);
        }
    }

    private bool CheckBusyOther(int actorId,TacticsCommandType tacticsCommandType)
    {
        bool IsBusyOther = _model.IsOtherBusy(actorId,tacticsCommandType);
        if (IsBusyOther == true)
        {
            _model.CurrentActorId = actorId;
            _model.SetTacticsCommandType(tacticsCommandType);
            TextData mainTextData = DataSystem.System.GetTextData(1030);
            TextData textData = DataSystem.System.GetTextData((int)_model.TacticsActor(actorId).TacticsCommandType);
            string mainText = mainTextData.Text.Replace("\\d",textData.Text);
            var popupInfo = new ConfirmInfo(_model.TacticsActor(actorId).Master.Name + mainText,(a) => UpdatePopup((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
            _view.DeactivateTacticsCommand();
            return true;
        }
        return false;
    }

    private void UpdatePopup(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            int actorId = _model.CurrentActorId;
            if (_model.TacticsCommandType == TacticsCommandType.Train)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorTrain(actorId);
            }
            if (_model.TacticsCommandType == TacticsCommandType.Alchemy)
            {
                _model.ResetTacticsCost(actorId);
                CommandSelectActorAlchemy(actorId);
            }
            if (_model.TacticsCommandType == TacticsCommandType.Recovery)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorRecovery(actorId);
            }
            if (_model.TacticsCommandType == TacticsCommandType.Battle)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorBattle(actorId);
            }
            if (_model.TacticsCommandType == TacticsCommandType.Resource)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorResource(actorId);
            }
            if (_model.TacticsCommandType == TacticsCommandType.TurnEnd)
            {
                _model.TurnEnd();
                _model.InitInBattle();
                _view.CommandSceneChange(Scene.Strategy);
            }
        } else{
            if (_model.TacticsCommandType == TacticsCommandType.TurnEnd)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.SetHelpInputInfo("TACTICS");
                _view.ShowCommandList();
            }
        }
        if (confirmCommandType == ConfirmCommandType.No && _model.TacticsCommandType != TacticsCommandType.TurnEnd)
        {        
            _view.ActivateTacticsCommand();
        }
        _view.CommandConfirmClose();
    }

    private void UpdatePopupOpenAlcana(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _model.OpenAlcana();
            CommandCheckAlcana();
        } else{
            _view.ActivateCommandList();
            _view.CommandConfirmClose();
        }
    }

    private void UpdatePopupUseAlcana(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            CommandUseAlcana();
        } else{
            CommandDeleteAlcana();
        }
        _view.ActivateCommandList();
        _view.CommandConfirmClose();
    }

    private void UpdatePopupDropout(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _model.SetResumeStageFalse();
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
        StatusViewInfo statusViewInfo = new StatusViewInfo(null);
        statusViewInfo.SetDisplayDecideButton(true);
        statusViewInfo.SetDisplayBackButton(false);
        _view.CommandCallStatus(statusViewInfo);
    }

    private void UpdatePopupSaveCommand(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            SaveSystem.SaveStart(GameSystem.CurrentData);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.CommandSceneChange(Scene.Tactics);
    }

    private void UpdatePopupSkillInfo(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
    }

    private void CommandBack()
    {
        var eventData = new TacticsViewEvent(_backCommand);
        if (_backCommand == Tactics.CommandType.SelectActorAlchemy)
        {
            eventData.template = _model.CurrentActorId;
        } else{
            eventData.template = _model.TacticsCommandType;
        }
        UpdateCommand(eventData);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandAddAlcana()
    {
        _model.MakeAlcana();
        _view.AddAlcana();
        CommandRefresh();
    }

    private void CommandTacticsCommand(TacticsCommandType tacticsCommandType)
    {
        _model.SetTacticsCommandType(tacticsCommandType);
        _model.SetTempData(tacticsCommandType);
        switch (tacticsCommandType)
        {
            case TacticsCommandType.Train:
            case TacticsCommandType.Recovery:
            case TacticsCommandType.Resource:
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(false);
                _backCommand = Tactics.CommandType.None;
                break;
            case TacticsCommandType.Alchemy:
                _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(false);
                _backCommand = Tactics.CommandType.None;
                _view.HideAttributeList();
                _view.ShowSelectCharacterCommand();
                break;
            case TacticsCommandType.Battle:
                _view.ActivateTacticsCommand();
                _view.ChangeBackCommandActive(true);
                _view.HideSelectCharacter();
                _view.ShowEnemyList();
                _backCommand = Tactics.CommandType.EnemyClose;
                break;
        }
        if (tacticsCommandType == TacticsCommandType.Status)
        {
            SaveSystem.SaveStart(GameSystem.CurrentData);
            _model.SetStageActor();
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                CommandShowUi();
                _view.ShowCommandList();
                _view.SetHelpInputInfo("TACTICS");
            });
            CommandHideUi();
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
        }
        if (tacticsCommandType == TacticsCommandType.TurnEnd)
        {
            TextData textData = DataSystem.System.GetTextData(1040);
            TextData subData = DataSystem.System.GetTextData(1050);
            string mainText = textData.Text;
            if (_model.CheckNonBusy())
            {
                mainText += "\n" + subData.Text;
            }
            var popupInfo = new ConfirmInfo(mainText,(a) => UpdatePopup((ConfirmCommandType)a));
            popupInfo.SetSelectIndex(_model.CheckNonBusy() ? (int)ConfirmCommandType.Yes : (int)ConfirmCommandType.No);
            _view.CommandCallConfirm(popupInfo);
        }
        CommandRefresh();
        _view.HideCommandList();
        _view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSkillAlchemy(SkillInfo skillInfo)
    {
        if (_model.CheckCanSelectAlchemy(skillInfo.Attribute))
        {
            _model.SelectAlchemySkill(skillInfo.Id);
            _view.HideAttributeList();
            _view.ShowSelectCharacterCommand();
            CommandRefresh();
            _view.ChangeBackCommandActive(false);
            _backCommand = Tactics.CommandType.SelectActorAlchemy;
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectActorTrain(int actorId)
    {
        _model.SelectActorTrain(actorId);
        CommandRefresh();
    }

    private void CommandTacticsCommandClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _model.ResetTempData(_model.TacticsCommandType);
        }
        _view.ShowCommandList();
        _view.HideSelectCharacter();
        CommandRefresh();
        if (_model.IsBusyAll())
        {
            CommandTacticsCommand(TacticsCommandType.TurnEnd);
        }
    }

    private void CommandSelectActorAlchemy(int actorId)
    {
        bool IsCheckAlchemy = _model.IsCheckAlchemy(actorId);
        if (IsCheckAlchemy)
        {

        } else
        {
            /*
            CommandAttributeType(_model.CurrentAttributeType);
            */
            _model.CurrentActorId = actorId;
            _view.ShowAttributeList(_model.CurrentActor, _model.SelectActorAlchemy(actorId));
            _view.HideSelectCharacterCommand();
            //_view.HideSkillAlchemyList();
            _view.ChangeBackCommandActive(true);
            _backCommand = Tactics.CommandType.SelectAlchemyClose;        
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        CommandRefresh();
    }

    private void CommandSelectActorRecovery(int actorId)
    {
        _model.SelectActorRecovery(actorId);
        CommandRefresh();
    }

    private void CommandSelectRecoveryPlus(int actorId)
    {
        _model.SelectRecoveryPlus(actorId);
        CommandRefresh();
    }

    private void CommandSelectRecoveryMinus(int actorId)
    {
        _model.SelectRecoveryMinus(actorId);
        CommandRefresh();
    }


    private void CommandSelectBattleEnemy(int enemyIndex)
    {
        _model.SetTempData(TacticsCommandType.Battle);
        _model.CurrentEnemyIndex = enemyIndex;
        _view.HideEnemyList();
        _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
        _view.ActivateTacticsCommand();
        _view.ChangeBackCommandActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        ConfirmInfo popupInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo((ConfirmCommandType)a));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallConfirm(popupInfo);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectActorBattle(int actorId)
    {
        _model.SelectActorBattle(actorId);
        CommandRefresh();
    }

    private void CommandEnemyClose()
    {
        _view.HideEnemyList();
        _view.ChangeBackCommandActive(false);
        _view.ShowCommandList();
        if (_model.IsBusyAll())
        {
            CommandTacticsCommand(TacticsCommandType.TurnEnd);
        }
    }

    private void CommandSelectActorResource(int actorId)
    {
        _model.SelectActorResource(actorId);
        CommandRefresh();
    }


    private void CommandRefresh()
    {
        _view.SetTurns(_model.Turns);
        _view.SetNuminous(_model.Currency);
        _view.SetStageInfo(_model.CurrentStage);
        
        _view.CommandRefresh(_model.TacticsCommandType);
    }

    private void CommandShowUi()
    {
        _view.ChangeUIActive(true);
    }

    private void CommandHideUi()
    {
        _view.ChangeUIActive(false);
    }

    private void CommandOpenAlcana()
    {
        if (_model.CheckIsAlcana())
        {
            _view.DeactivateCommandList();
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(1070).Text,(menuCommandInfo) => UpdatePopupOpenAlcana((ConfirmCommandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void CommandCallEnemyInfo(TroopInfo troopInfo)
    {
        var enemyInfos = troopInfo.BattlerInfos;
        
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.ChangeUIActive(true);
        });
        statusViewInfo.SetEnemyInfos(enemyInfos,false);
        _view.CommandCallEnemyInfo(statusViewInfo);
        _view.ChangeUIActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandCheckAlcana()
    {
        _view.DeactivateCommandList();
        var alcana = _model.CurrentAlcana.CurrentSelectAlcana();
        TextData textData = DataSystem.System.GetTextData(1080);
        var popupInfo = new ConfirmInfo(alcana.Name + alcana.Help + textData.Text,(menuCommandInfo) => UpdatePopupUseAlcana((ConfirmCommandType)menuCommandInfo));
        _view.CommandCallConfirm(popupInfo);
    }

    private void CommandUseAlcana()
    {
        _model.UseAlcana();
        _view.UseAlcana();
        CommandRefresh();
        DisableTacticsCommand();
    }

    private void CommandDeleteAlcana()
    {
        _model.DeleteAlcana();
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
        var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(1100).Text,(a) => UpdatePopupDropout((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void DisableTacticsCommand()
    {
        if (_model.CurrentAlcana.IsDisableTactics())
        {
            _model.ResetTacticsCostAll();
            for (int i = 0;i < 5;i++)
            {
                var listData = _model.ChangeEnableCommandData(i, false);
                _view.RefreshListData(listData);
            }
        }
    }

    private void CommandOpenSideMenu()
    {
        _view.CommandOpenSideMenu();
    }

    private void CommandCloseSideMenu()
    {
        _view.CommandCloseSideMenu();
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
}
