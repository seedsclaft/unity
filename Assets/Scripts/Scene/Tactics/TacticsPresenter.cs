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

        Initialize();
    }

    private async void Initialize()
    {
        _model.RefreshTacticsEnable();

        _view.SetHelpWindow();
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
            _view.SetActiveUi(false);
            return;
        }
        
        var isAbort = CheckAdvStageEvent(EventTiming.BeforeTactics,() => {
            _view.CommandSceneChange(Scene.Tactics);
        },_model.CurrentStage.SelectActorIdsClassId(0));
        if (isAbort)
        {
            _view.SetActiveUi(false);
            return;
        }
        
        var isReborn = CheckRebornEvent(EventTiming.BeforeTactics,() => {
            _view.CommandSceneChange(Scene.RebornResult);
        });
        if (isReborn)
        {
            _view.SetActiveUi(false);
            return;
        }
        _view.SetUIButton();
        _view.SetActiveBack(false);
        _view.SetEvent((type) => updateCommand(type));
        _view.SetSideMenu(_model.SideMenu());
        _view.SetActors(_model.StageMembers(),_model.ConfirmCommand(),_model.CommandRankInfo());
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetEnemies(_model.TacticsTroops());

        _view.SetTacticsCommand(_model.TacticsCommand());
        _view.HideCommandList();
        CommandRefresh();
        //Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        // イベントチェック
        var stageEvents = _model.StageEvents(EventTiming.StartTactics);
        if (stageEvents.Count > 0)
        {
            for (int i = 0;i < stageEvents.Count;i++)
            {
                if (stageEvents[i].Type == StageEventType.CommandDisable)
                {
                    _view.RefreshListData(_model.ChangeEnableCommandData(stageEvents[i].Param,false));
                }
                if (stageEvents[i].Type == StageEventType.TutorialBattle)
                {
                    _view.SetEnemies(_model.TutorialTroopData());
                }
                if (stageEvents[i].Type == StageEventType.NeedAllTactics)
                {
                    _model.SetNeedAllTacticsCommand(true);
                    var listData = _model.ChangeEnableCommandData((int)TacticsComandType.Turnend - 1, !_model.CheckNonBusy());
                    _view.RefreshListData(listData);
                }
                if (stageEvents[i].Type == StageEventType.IsSubordinate)
                {
                    _model.SetIsSubordinate(stageEvents[i].Param == 1);
                    if (stageEvents[i].Param == 1)
                    {        
                        CommandRefresh();
                    }
                    _model.AddEventReadFlag(stageEvents[i]);
                }
                if (stageEvents[i].Type == StageEventType.IsAlcana)
                {
                    _model.SetIsAlcana(stageEvents[i].Param == 1);
                    if (stageEvents[i].Param == 1)
                    {        
                        CommandRefresh();
                    }
                    _model.AddEventReadFlag(stageEvents[i]);
                }
                if (stageEvents[i].Type == StageEventType.SelectAddActor)
                {
                    var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(11050).Text,(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmComandType)menuCommandInfo));
                    popupInfo.SetIsNoChoise(true);
                    popupInfo.SetSelectIndex(0);
                    _view.CommandCallConfirm(popupInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    _view.SetActiveUi(false);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.SaveCommand)
                {
                    var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(11080).Text,(menuCommandInfo) => UpdatePopupSaveCommand((ConfirmComandType)menuCommandInfo));
                    popupInfo.SetSelectIndex(1);
                    _view.CommandCallConfirm(popupInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    _view.SetActiveUi(false);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.SetDefineBossIndex)
                {
                    _model.SetDefineBossIndex(stageEvents[i].Param);
                    _view.SetEnemies(_model.TacticsTroops());
                }
                if (stageEvents[i].Type == StageEventType.SetRouteSelectParam)
                {
                    _view.CommandSetRouteSelect();
                    _model.AddEventReadFlag(stageEvents[i]);
                }
                if (stageEvents[i].Type == StageEventType.AbortStage)
                {
                    isAbort = true;
                    _model.StageClaer();
                    _model.AddEventReadFlag(stageEvents[i]);
                    _view.CommandSceneChange(Scene.MainMenu);
                }
                if (stageEvents[i].Type == StageEventType.ChangeRouteSelectStage)
                {
                    _model.ChangeRouteSelectStage(stageEvents[i].Param);
                    _model.AddEventReadFlag(stageEvents[i]);
                    _view.CommandSceneChange(Scene.Tactics);
                }
                if (stageEvents[i].Type == StageEventType.RouteSelectBattle)
                {
                    _view.SetEnemies(_model.RouteSelectTroopData());
                    _model.AddEventReadFlag(stageEvents[i]);
                }
            }
        }
        if (isAbort)
        {
            return;
        }
        isAbort = CheckAdvStageEvent(EventTiming.StartTactics,() => {
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

    private void updateCommand(TacticsViewEvent viewEvent)
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
            CommandTacticsCommand((TacticsComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor)
        {
            var tacticsActorInfo = (TacticsActorInfo)viewEvent.templete;
            var actorId = tacticsActorInfo.ActorInfo.ActorId;
            if (CheckBusyOther(actorId,tacticsActorInfo.TacticsComandType)) return;
            if (tacticsActorInfo.TacticsComandType == TacticsComandType.Train)
            {
                CommandSelectActorTrain(actorId);
            } else
            if (tacticsActorInfo.TacticsComandType == TacticsComandType.Alchemy)
            {
                CommandSelectActorAlchemy(actorId);
            } else
            if (tacticsActorInfo.TacticsComandType == TacticsComandType.Resource)
            {
                CommandSelectActorResource(actorId);
            } else
            if (tacticsActorInfo.TacticsComandType == TacticsComandType.Recovery)
            {
                CommandSelectActorRecovery(actorId);
            } else
            if (tacticsActorInfo.TacticsComandType == TacticsComandType.Battle)
            {
                CommandSelectActorBattle(actorId);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommandClose)
        {
            if (_model.CommandType == TacticsComandType.Battle)
            {
                CommandTacticsCommand(TacticsComandType.Battle);
            } else
            {
                CommandTacticsCommandClose((ConfirmComandType)viewEvent.templete);
            }
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorAlchemy)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Alchemy)) return;
            CommandSelectActorAlchemy(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SkillAlchemy)
        {
            CommandSkillAlchemy((SkillInfo)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectAlchemyClose)
        {
            CommandTacticsCommand(TacticsComandType.Alchemy);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryPlus)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Recovery)) return;
            CommandSelectRecoveryPlus(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryMinus)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Recovery)) return;
            CommandSelectRecoveryMinus(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectBattleEnemy)
        {
            CommandSelectBattleEnemy((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.PopupSkillInfo)
        {
            CommandPopupSkillInfo((GetItemInfo)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.EnemyClose)
        {
            CommandEnemyClose();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorResource)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Resource)) return;
            CommandSelectActorResource(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.ShowUi)
        {
            CommandShowUi();
        }
        if (viewEvent.commandType == Tactics.CommandType.HideUi)
        {
            CommandHideUi();
        }
        if (viewEvent.commandType == Tactics.CommandType.OpenAlcana)
        {
            CommandOpenAlcana();
        }
        if (viewEvent.commandType == Tactics.CommandType.CallEnemyInfo)
        {
            CommandCallEnemyInfo((int)viewEvent.templete);
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
            CommandSelectSideMenu((SystemData.CommandData)viewEvent.templete);
        }
        if (_model.NeedAllTacticsCommand)
        {
            var listData = _model.ChangeEnableCommandData((int)TacticsComandType.Turnend - 1, !_model.CheckNonBusy());
            _view.RefreshListData(listData);
        }
    }

    private bool CheckBusyOther(int actorId,TacticsComandType tacticsComandType)
    {
        bool IsBusyOther = _model.IsOtherBusy(actorId,tacticsComandType);
        if (IsBusyOther == true)
        {
            _model.CurrentActorId = actorId;
            _model.CommandType = tacticsComandType;
            TextData mainTextData = DataSystem.System.GetTextData(1030);
            TextData textData = DataSystem.System.GetTextData((int)_model.TacticsActor(actorId).TacticsComandType);
            string mainText = mainTextData.Text.Replace("\\d",textData.Text);
            var popupInfo = new ConfirmInfo(_model.TacticsActor(actorId).Master.Name + mainText,(a) => UpdatePopup((ConfirmComandType)a));
            _view.CommandCallConfirm(popupInfo);
            _view.DeactivateTacticsCommand();
            return true;
        }
        return false;
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            int actorId = _model.CurrentActorId;
            if (_model.CommandType == TacticsComandType.Train)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorTrain(actorId);
            }
            if (_model.CommandType == TacticsComandType.Alchemy)
            {
                _model.ResetTacticsCost(actorId);
                CommandSelectActorAlchemy(actorId);
            }
            if (_model.CommandType == TacticsComandType.Recovery)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorRecovery(actorId);
            }
            if (_model.CommandType == TacticsComandType.Battle)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorBattle(actorId);
            }
            if (_model.CommandType == TacticsComandType.Resource)
            {
                _model.ResetTacticsCost(actorId);
                _view.ActivateTacticsCommand();
                CommandSelectActorResource(actorId);
            }
            if (_model.CommandType == TacticsComandType.Turnend)
            {
                _model.TurnEnd();
                _model.InitInBattle();
                _view.CommandSceneChange(Scene.Strategy);
            }
        } else{
            if (_model.CommandType == TacticsComandType.Turnend)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.ShowCommandList();
            }
        }
        if (confirmComandType == ConfirmComandType.No && _model.CommandType != TacticsComandType.Turnend)
        {        
            _view.ActivateTacticsCommand();
        }
        _view.CommandConfirmClose();
    }

    private void UpdatePopupOpenAlcana(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.OpenAlcana();
            CommandCheckAlcana();
        } else{
            _view.ActivateCommandList();
            _view.CommandConfirmClose();
        }
    }

    private void UpdatePopupUseAlcana(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            CommandUseAlcana();
        } else{
            CommandDeleteAlcana();
        }
        _view.ActivateCommandList();
        _view.CommandConfirmClose();
    }

    private void UpdatePopupDropout(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.SetResumeStageFalse();
            _view.CommandSceneChange(Scene.MainMenu);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        //_view.ActivateCommandList();
        _view.CommandConfirmClose();
    }


    private void UpdatePopupSelectAddActor(ConfirmComandType confirmComandType)
    {
        _model.SetSelectAddActor();
        _view.CommandConfirmClose();
        StatusViewInfo statusViewInfo = new StatusViewInfo(null);
        statusViewInfo.SetDisplayDecideButton(true);
        statusViewInfo.SetDisplayBackButton(false);
        statusViewInfo.SetDisableStrength(true);
        _view.CommandCallStatus(statusViewInfo);
    }

    private void UpdatePopupSaveCommand(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SaveSystem.SaveStart(GameSystem.CurrentData);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.CommandSceneChange(Scene.Tactics);
    }

    private void UpdatePopupSkillInfo(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
    }

    private void CommandBack()
    {
        var eventData = new TacticsViewEvent(_backCommand);
        if (_backCommand == Tactics.CommandType.SelectActorAlchemy)
        {
            eventData.templete = _model.CurrentActorId;
        } else{
            eventData.templete = _model.CommandType;
        }
        updateCommand(eventData);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandAddAlcana()
    {
        _model.MakeAlcana();
        _view.AddAlcana();
        CommandRefresh();
    }

    private void CommandTacticsCommand(TacticsComandType tacticsComandType)
    {
        _model.CommandType = tacticsComandType;
        if (tacticsComandType == TacticsComandType.Train)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowSelectCharacter(_model.TacticsCharacterData(),1,"");
            _view.ActivateTacticsCommand();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            CommandRefresh();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowSelectCharacter(_model.TacticsCharacterData(),1,"");
            _view.ActivateTacticsCommand();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            _view.HideAttributeList();
            _view.ShowSelectCharacterCommand();
            CommandRefresh();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Recovery)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowSelectCharacter(_model.TacticsCharacterData(),1,"");
            _view.ActivateTacticsCommand();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            CommandRefresh();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Battle)
        {
            _model.SetTempData(tacticsComandType);
            _view.SetActiveBack(true);
            _view.HideCommandList();
            _view.HideSelectCharacter();
            _view.ShowEnemyList();
            CommandRefresh();
            _backCommand = Tactics.CommandType.EnemyClose;
        }
        if (tacticsComandType == TacticsComandType.Resource)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowSelectCharacter(_model.TacticsCharacterData(),1,"");
            _view.ActivateTacticsCommand();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            CommandRefresh();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Status)
        {
            SaveSystem.SaveStart(GameSystem.CurrentData);
            _model.SetStageActor();
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                _view.SetNuminous(_model.Currency);
                CommandShowUi();
            });
            CommandHideUi();
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
        }
        if (tacticsComandType == TacticsComandType.Turnend)
        {
            TextData textData = DataSystem.System.GetTextData(1040);
            TextData subData = DataSystem.System.GetTextData(1050);
            string mainText = textData.Text;
            if (_model.CheckNonBusy())
            {
                mainText += "\n" + subData.Text;
            }
            var popupInfo = new ConfirmInfo(mainText,(a) => UpdatePopup((ConfirmComandType)a));
            popupInfo.SetSelectIndex(_model.CheckNonBusy() ? (int)ConfirmComandType.Yes : (int)ConfirmComandType.No);
            _view.CommandCallConfirm(popupInfo);
            _view.HideCommandList();
        }
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
            _view.SetActiveBack(false);
            _backCommand = Tactics.CommandType.SelectActorAlchemy;
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectActorTrain(int actorId)
    {
        _model.SelectActorTrain(actorId);
        CommandRefresh();
    }

    private void CommandTacticsCommandClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _model.ResetTempData(_model.CommandType);
        }
        _view.ShowCommandList();
        _view.HideSelectCharacter();
        CommandRefresh();
        if (_model.IsBusyAll())
        {
            CommandTacticsCommand(TacticsComandType.Turnend);
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
            _view.SetActiveBack(true);
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
        _model.SetTempData(TacticsComandType.Battle);
        _model.CurrentEnemyIndex = enemyIndex;
        _view.HideEnemyList();
        _view.ShowSelectCharacter(_model.TacticsCharacterData(),1,"");
        _view.ActivateTacticsCommand();
        _view.SetActiveBack(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        ConfirmInfo popupInfo = new ConfirmInfo("",(menuCommandInfo) => UpdatePopupSkillInfo((ConfirmComandType)menuCommandInfo));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoise(true);
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
        _view.SetActiveBack(false);
        _view.ShowCommandList();
        if (_model.IsBusyAll())
        {
            CommandTacticsCommand(TacticsComandType.Turnend);
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
        
        _view.CommandRefresh(_model.CommandType);
    }

    private void CommandShowUi()
    {
        _view.SetActiveUi(true);
    }

    private void CommandHideUi()
    {
        _view.SetActiveUi(false);
    }

    private void CommandOpenAlcana()
    {
        if (_model.CheckIsAlcana())
        {
            _view.DeactivateCommandList();
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(1070).Text,(menuCommandInfo) => UpdatePopupOpenAlcana((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void CommandCallEnemyInfo(int enemyIndex)
    {
        if (_model.TacticsTroops().Count < enemyIndex)
        {
            return;
        }
        List<BattlerInfo> enemyInfos = _model.TacticsTroops()[enemyIndex].BattlerInfos;
        
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.SetActiveUi(true);
        });
        statusViewInfo.SetEnemyInfos(enemyInfos,false);
        _view.CommandCallEnemyInfo(statusViewInfo);
        _view.SetActiveUi(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandCheckAlcana()
    {
        _view.DeactivateCommandList();
        var alcana = _model.CurrentAlcana.CurrentSelectAlcana();
        TextData textData = DataSystem.System.GetTextData(1080);
        var popupInfo = new ConfirmInfo(alcana.Name + alcana.Help + textData.Text,(menuCommandInfo) => UpdatePopupUseAlcana((ConfirmComandType)menuCommandInfo));
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
        var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(1100).Text,(a) => UpdatePopupDropout((ConfirmComandType)a));
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
