using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPresenter 
{
    StatusModel _model = null;
    StatusView _view = null;

    private bool _busy = true;
    public StatusPresenter(StatusView view)
    {
        _view = view;
        _model = new StatusModel();

        Initialize();
    }

    private void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetEvent((type) => updateCommand(type));

        //List<ActorInfo> actorInfos = _model.Actors();
        _view.SetActorInfo(_model.CurrentActor);


        _view.SetStatusCommand(_model.StatusCommand);
        _view.SetAttributeTypes(_model.AttributeTypes());
        //var bgm = await _model.BgmData();
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(StatusViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Status.CommandType.StatusCommand)
        {
            CommandStatusCommand((StatusComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Status.CommandType.AttributeType)
        {
            CommandAttributeType((AttributeType)viewEvent.templete);
        }
        if (viewEvent.commandType == Status.CommandType.DecideActor)
        {
            CommandDecideActor();
        }
        if (viewEvent.commandType == Status.CommandType.LeftActor)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == Status.CommandType.RightActor)
        {
            CommandRightActor();
        }
    }

    private void CommandStatusCommand(StatusComandType statusComandType)
    {
        if (statusComandType == StatusComandType.SkillActionList)
        {
            _view.ShowSkillActionList();
            CommandAttributeType(_model.CurrentAttributeType);
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

}
