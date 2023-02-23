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
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetActiveBack(false);
        _view.SetEvent((type) => updateCommand(type));

        //List<ActorInfo> actorInfos = _model.Actors();
        //_view.SetActorInfo(_model.CurrentActor);
        _view.SetActors(_model.Actors());

        _view.SetTacticsCommand(_model.TacticsCommand);
        _view.SetAttributeTypes(_model.AttributeTypes());
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
        if (viewEvent.commandType == Tactics.CommandType.SelectTrain)
        {
            CommandSelectTrain((int)viewEvent.templete);
        }
    }

    private void CommandTacticsCommand(TacticsComandType tacticsComandType)
    {
        if (tacticsComandType == TacticsComandType.Train)
        {
            _view.ShowTrainList();
            _backCommand = Tactics.CommandType.None;
        }
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

    private void CommandRefresh()
    {
        _view.CommandRefresh();
    }

    private void CommandSelectTrain(int actorId)
    {
        _model.SelectTrain(actorId);
        CommandRefresh();
    }

}
