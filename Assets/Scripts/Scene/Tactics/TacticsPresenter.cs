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
            _backCommand = Tactics.CommandType.None;
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            _model.SetTempData(tacticsComandType);
            _view.ShowAlchemyList();
            _view.HideSkillAlchemyList();
            _backCommand = Tactics.CommandType.None;
            
        }        
        _view.SetActiveBack(false);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
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
            List<SkillInfo> skillInfos = _model.SelectActorAlchemy(actorId);
            _view.ShowSkillAlchemyList(skillInfos);
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

    private void CommandRefresh()
    {
        _view.SetNuminous(_model.Currency);
        _view.CommandRefresh();
    }
}
