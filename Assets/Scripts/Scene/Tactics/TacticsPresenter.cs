using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsPresenter 
{
    TacticsModel _model = null;
    TacticsView _view = null;

    private bool _busy = true;


    private Tactics.CommandType _backCommand = Tactics.CommandType.None;
    public TacticsPresenter(TacticsView view)
    {
        _view = view;
        _model = new TacticsModel();

        Initialize();
    }

    private async void Initialize()
    {
        
        _model.RefreshTacticsEnable();


        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetActiveBack(false);
        _view.SetEvent((type) => updateCommand(type));

        //List<ActorInfo> actorInfos = _model.Actors();
        //_view.SetActorInfo(_model.CurrentActor);
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetActors(_model.Actors(),_model.ConfirmCommand());
        _view.SetEnemies(_model.TacticsTroops());

        _view.SetTacticsCommand(_model.TacticsCommand);
        _view.ShowCommandList();
        _view.SetAttributeTypes(_model.AttributeTypes());
        CommandRefresh();
        var bgm = await _model.GetBgmData("TACTICS1");
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        // イベントチェック
        var stageEvents = _model.StageEvents(EventTiming.StartTactics);
        if (stageEvents.Count > 0)
        {
            for (int i = 0;i < stageEvents.Count;i++)
            {
                if (stageEvents[i].Type == StageEventType.CommandDisable)
                {
                    _view.SetCommandDisable(stageEvents[i].Param);
                }
                if (stageEvents[i].Type == StageEventType.TutorialBattle)
                {
                    _view.SetEnemies(_model.ResetTroopData());
                }
                if (stageEvents[i].Type == StageEventType.NeedAllTactics)
                {
                    _model.SetNeedAllTacticsCommand(true);
                    if (_model.CheckNonBusy())
                    {
                        _view.SetCommandDisable(DataSystem.TacticsCommand.Count-1);
                        
                    } else{
                        _view.SetCommandAble(DataSystem.TacticsCommand.Count-1);
                    }
                }
                if (stageEvents[i].Type == StageEventType.IsSubordinate)
                {
                    _model.SetIsSubordinate(stageEvents[i].Param == 1);
                    if (stageEvents[i].Param == 1)
                    {        
                        CommandRefresh();
                    }
                }
                if (stageEvents[i].Type == StageEventType.IsAlcana)
                {
                    _model.SetIsAlcana(stageEvents[i].Param == 1);
                    if (stageEvents[i].Param == 1)
                    {        
                        CommandRefresh();
                    }
                }
            }
        }

        // アルカナ配布
        if (_model.CheckIsAlcana())
        {
            CommandAddAlcana();
        }
        _busy = false;
    }

    private void updateCommand(TacticsViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Tactics.CommandType.AddAlcana)
        {
            CommandAddAlcana();
        }
        if (viewEvent.commandType == Tactics.CommandType.TacticsCommand)
        {
            CommandTacticsCommand((TacticsComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.AttributeType)
        {
            CommandAttributeType((AttributeType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.DecideActor)
        {
            CommandDecideActor();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorTrain)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Train)) return;
            CommandSelectActorTrain(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.TrainClose)
        {
            CommandTrainClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorAlchemy)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Alchemy)) return;
            CommandSelectActorAlchemy(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.SkillAlchemy)
        {
            CommandSkillAlchemy((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.AlchemyClose)
        {
            CommandAlchemyClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorRecovery)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Recovery)) return;
            CommandSelectActorRecovery(actorId);
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
        if (viewEvent.commandType == Tactics.CommandType.RecoveryClose)
        {
            CommandRecoveryClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectBattleEnemy)
        {
            CommandSelectBattleEnemy((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorBattle)
        {
            int actorId = (int)viewEvent.templete;
            if (CheckBusyOther(actorId,TacticsComandType.Battle)) return;
            CommandSelectActorBattle(actorId);
        }
        if (viewEvent.commandType == Tactics.CommandType.BattleClose)
        {
            CommandBattleClose((ConfirmComandType)viewEvent.templete);
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
        if (viewEvent.commandType == Tactics.CommandType.ResourceClose)
        {
            CommandResourceClose((ConfirmComandType)viewEvent.templete);
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
        if (viewEvent.commandType == Tactics.CommandType.Back)
        {
            CommandBack();
        }
        if (_model.NeedAllTacticsCommand)
        {
            if (_model.CheckNonBusy())
            {
                _view.SetCommandDisable(DataSystem.TacticsCommand.Count-1);
                
            } else{
                _view.SetCommandAble(DataSystem.TacticsCommand.Count-1);
            }
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
            TextData textData = DataSystem.System.GetTextData((int)tacticsComandType);
            string mainText = mainTextData.Text.Replace("\\d",textData.Text);
            var popupInfo = new ConfirmInfo(_model.TacticsActor(actorId).Master.Name + mainText,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
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
                CommandSelectActorRecovery(actorId);
            }
            if (_model.CommandType == TacticsComandType.Battle)
            {
                _model.ResetTacticsCost(actorId);
                CommandSelectActorBattle(actorId);
            }
            if (_model.CommandType == TacticsComandType.Resource)
            {
                _model.ResetTacticsCost(actorId);
                CommandSelectActorResource(actorId);
            }
            if (_model.CommandType == TacticsComandType.Turnend)
            {
                _model.TurnEnd();
                _view.CommandSceneChange(Scene.Strategy);
            }
        } else{
            if (_model.CommandType == TacticsComandType.Turnend)
            {
                _view.ShowCommandList();
            }
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

    private void CommandBack()
    {
        var eventData = new TacticsViewEvent(_backCommand);
        eventData.templete = _model.CommandType;
        updateCommand(eventData);
        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
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
            _view.ShowTrainList();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowAlchemyList();
            _view.HideSkillAlchemyList();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Recovery)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowRecoveryList();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Battle)
        {
            _model.SetTempData(tacticsComandType);
            _view.HideBattleList();
            _view.SetActiveBack(true);
            _view.HideCommandList();
            _view.ShowEnemyList();
            _backCommand = Tactics.CommandType.EnemyClose;
        }
        if (tacticsComandType == TacticsComandType.Resource)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowResourceList();
            _view.SetActiveBack(false);
            _view.HideCommandList();
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Status)
        {
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                CommandShowUi();
            });
            CommandHideUi();
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
            SaveSystem.SaveStart(GameSystem.CurrentData);
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
            var popupInfo = new ConfirmInfo(mainText,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
            _view.HideCommandList();
        }
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SelectActorAlchemy(_model.CurrentActor.ActorId,attributeType);
        _view.ShowSkillAlchemyList(skillInfos);
    }

    private void CommandDecideActor()
    {

    }
    


    private void CommandSelectActorTrain(int actorId)
    {
        _model.SelectActorTrain(actorId);
        CommandRefresh();
    }

    private void CommandTrainClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _model.ResetTempData(TacticsComandType.Train);
        }
        _view.ShowCommandList();
        _view.HideTrainList();
        CommandRefresh();
    }

    private void CommandSelectActorAlchemy(int actorId)
    {
        bool IsCheckAlchemy = _model.IsCheckAlchemy(actorId);
        if (IsCheckAlchemy)
        {

        } else
        {
            _model.CurrentActorId = actorId;
            CommandAttributeType(_model.CurrentAttributeType);
            _view.HideAlchemyList();
            _view.SetActiveBack(true);
            _backCommand = Tactics.CommandType.TacticsCommand;
        }
        CommandRefresh();
    }

    private void CommandSkillAlchemy(int skillId)
    {
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.SelectAlchemy(skillId);
        _view.HideSkillAlchemyList();
        _view.SetActiveBack(false);
        _view.ShowAlchemyList();
        CommandRefresh();
    }

    private void CommandAlchemyClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            _model.ResetTempData(TacticsComandType.Alchemy);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.ShowCommandList();
        _view.HideAlchemyList();
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

    private void CommandRecoveryClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            _model.ResetTempData(TacticsComandType.Recovery);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.ShowCommandList();
        _view.HideRecoveryList();
        CommandRefresh();
    }

    private void CommandSelectBattleEnemy(int enemyIndex)
    {
        _model.SetTempData(TacticsComandType.Battle);
        _model.CurrentEnemyIndex = enemyIndex;
        _view.ShowBattleList();
        _view.HideEnemyList();
        _view.SetActiveBack(false);
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectActorBattle(int actorId)
    {
        _model.SelectActorBattle(actorId);
        CommandRefresh();
    }

    private void CommandBattleClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            _model.ResetTempData(TacticsComandType.Battle);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.HideBattleList();
        _view.ShowEnemyList();
        _view.SetActiveBack(true);
        _backCommand = Tactics.CommandType.EnemyClose;
        CommandRefresh();
    }

    private void CommandEnemyClose()
    {
        _view.HideEnemyList();
        _view.SetActiveBack(false);
        _view.ShowCommandList();
    }

    private void CommandSelectActorResource(int actorId)
    {
        _model.SelectActorResource(actorId);
        CommandRefresh();
    }

    private void CommandResourceClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else{
            _model.ResetTempData(TacticsComandType.Resource);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.ShowCommandList();
        _view.HideResourceList();
        CommandRefresh();
    }

    private void CommandRefresh()
    {
        _view.SetTurns(_model.Turns);
        _view.SetNuminous(_model.Currency);
        _view.SetStageInfo(_model.CurrentStage);
        _view.CommandRefresh();
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
            var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(1070).Text,(menuCommandInfo) => UpdatePopupOpenAlcana((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
        }
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
    }

    private void CommandDeleteAlcana()
    {
        _model.DeleteAlcana();
    }
}
