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
        _view.SetActors(_model.Actors(),_model.ConfirmCommand());
        _view.SetEnemies(_model.Enemies());

        _view.SetTacticsCommand(_model.TacticsCommand);
        _view.SetAttributeTypes(_model.AttributeTypes());
        CommandRefresh();
        var bgm = await _model.BgmData();
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(TacticsViewEvent viewEvent)
    {
        if (_busy){
            return;
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
        if (viewEvent.commandType == Tactics.CommandType.LeftActor)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == Tactics.CommandType.RightActor)
        {
            CommandRightActor();
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorTrain)
        {
            CommandSelectActorTrain((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.TrainClose)
        {
            CommandTrainClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectActorAlchemy)
        {
            CommandSelectActorAlchemy((int)viewEvent.templete);
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
            CommandSelectActorRecovery((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryPlus)
        {
            CommandSelectRecoveryPlus((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.SelectRecoveryMinus)
        {
            CommandSelectRecoveryMinus((int)viewEvent.templete);
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
            CommandSelectActorBattle((int)viewEvent.templete);
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
            CommandSelectActorResource((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.ResourceClose)
        {
            CommandResourceClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Tactics.CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandBack()
    {
        var eventData = new TacticsViewEvent(_backCommand);
        eventData.templete = _model.CommandType;
        updateCommand(eventData);
    }

    private void CommandTacticsCommand(TacticsComandType tacticsComandType)
    {
        _model.CommandType = tacticsComandType;
        if (tacticsComandType == TacticsComandType.Train)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowTrainList();
            _view.SetActiveBack(false);
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowAlchemyList();
            _view.HideSkillAlchemyList();
            _view.SetActiveBack(false);
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Recovery)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowRecoveryList();
            _view.SetActiveBack(false);
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Battle)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowEnemyList();
            _view.HideBattleList();
            _view.SetActiveBack(true);
            _backCommand = Tactics.CommandType.EnemyClose;
        }
        if (tacticsComandType == TacticsComandType.Resource)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowResourceList();
            _view.SetActiveBack(false);
            _backCommand = Tactics.CommandType.None;
        }
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SelectActorAlchemy(_model.CurrentActor.ActorId,attributeType);
        _view.ShowSkillAlchemyList(skillInfos);
    }

    private void CommandDecideActor()
    {

    }
    
    private void CommandLeftActor()
    {
         _model.ChangeActorIndex(-1);
        _view.SetActorInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private void CommandRightActor()
    {
         _model.ChangeActorIndex(1);
        _view.SetActorInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
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
        } else{
            _model.ResetTempData(TacticsComandType.Train);
        }
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
            _model.ChangeActorIndex(_model.ActorIndex(actorId));
            CommandAttributeType(_model.CurrentAttributeType);
            _view.HideAlchemyList();
            _view.SetActiveBack(true);
            _backCommand = Tactics.CommandType.TacticsCommand;
        }
        CommandRefresh();
    }

    private void CommandSkillAlchemy(int skillId)
    {
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
        } else{
            _model.ResetTempData(TacticsComandType.Alchemy);
        }
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
        } else{
            _model.ResetTempData(TacticsComandType.Recovery);
        }
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
        } else{
            _model.ResetTempData(TacticsComandType.Battle);
            _view.HideBattleList();
            _view.ShowEnemyList();
            _view.SetActiveBack(true);
            _backCommand = Tactics.CommandType.EnemyClose;
        }
        CommandRefresh();
    }

    private void CommandEnemyClose()
    {
        _view.HideEnemyList();
        _view.SetActiveBack(false);
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
        } else{
            _model.ResetTempData(TacticsComandType.Resource);
        }
        _view.HideResourceList();
        CommandRefresh();
    }

    private void CommandRefresh()
    {
        _view.SetNuminous(_model.Currency);
        _view.CommandRefresh();
    }
}
