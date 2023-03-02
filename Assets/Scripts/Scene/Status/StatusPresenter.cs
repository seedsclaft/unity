using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPresenter 
{
    StatusModel _model = null;
    StatusView _view = null;

    private bool _busy = true;

    private Status.CommandType _backCommandType = Status.CommandType.None;
    private Status.CommandType _popupCommandType = Status.CommandType.None;
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

        _view.SetStrengthInfo(_model.CurrentActor,_model.ConfirmCommand());

        _view.SetStatusCommand(_model.StatusCommand);
        _view.SetAttributeTypes(_model.AttributeTypes());
        //var bgm = await _model.BgmData();
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        CommandRefresh();
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
        if (viewEvent.commandType == Status.CommandType.SelectStrengthPlus)
        {
            StatusParamType statusId = (StatusParamType)viewEvent.templete;
            CommandSelectStrengthPlus(statusId);
        }
        if (viewEvent.commandType == Status.CommandType.SelectStrengthMinus)
        {
            StatusParamType statusId = (StatusParamType)viewEvent.templete;
            CommandSelectStrengthMinus(statusId);
        }
        if (viewEvent.commandType == Status.CommandType.StrengthClose)
        {
            CommandStrengthClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == Status.CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandBack()
    {
        if (_backCommandType == Status.CommandType.None)
        {
            _view.CommandBack();
        }
        if (_backCommandType == Status.CommandType.StatusCommand)
        {
            _backCommandType = Status.CommandType.None;
            _view.HideSkillActionList();
            if (_model.StatusActors().Count > 1) _view.ShowArrows();
            _view.ShowCommandList();
            _view.ShowDecideButton();
        }
    }

    private void CommandStatusCommand(StatusComandType statusComandType)
    {
        if (statusComandType == StatusComandType.Strength)
        {
            _view.ShowStrength();
            _view.SetActiveBack(false);
        }
        if (statusComandType == StatusComandType.SkillActionList)
        {
            _backCommandType = Status.CommandType.StatusCommand;
            _view.SetActiveBack(true);
            _view.ShowSkillActionList();
            CommandAttributeType(_model.CurrentAttributeType);
        }
        _view.HideDecideButton();
        _view.HideArrows();
        _view.HideCommandList();
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
    }

    private void CommandDecideActor()
    {
        ActorInfo actorInfo = _model.CurrentActor;
        var popupInfo = new ConfirmInfo(actorInfo.Master.Name + "をつれてシナリオを開始しますか？",(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(popupInfo);
        _popupCommandType = Status.CommandType.DecideStage;
    }

    private void updatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
            if (_popupCommandType == Status.CommandType.DecideStage)
            {
                _model.MakeStageData();
                _view.CommandStatusClose();
                _view.CommandSceneChange(Scene.Tactics);
            }
            if (_popupCommandType == Status.CommandType.StrengthClose)
            {
                _model.DecideStrength();
                _view.SetActorInfo(_model.CurrentActor);
                CommandRefresh();
            }
        }
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

    private void CommandSelectStrengthPlus(StatusParamType statusId)
    {
        bool enableParamUp = _model.EnableParamUp(statusId);
        if (enableParamUp == true)
        {
            _model.ChangeParameter(statusId,1);
        } else
        {

        }
        CommandRefresh();
    }

    private void CommandSelectStrengthMinus(StatusParamType statusId)
    {
        
        bool enableParamMinus = _model.EnableParamMinus(statusId);
        if (enableParamMinus == true)
        {
            _model.ChangeParameter(statusId,-1);
        } else
        {

        }
        CommandRefresh();
    }

    private void CommandStrengthClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _popupCommandType = Status.CommandType.StrengthClose;
            TextData textData = DataSystem.System.SystemTextData.Find(a => a.Id == 2000);
            ConfirmInfo confirmInfo = new ConfirmInfo(textData.Text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(confirmInfo);
        } else{
            if (_model.StatusActors().Count > 1) _view.ShowArrows();
            _view.ShowCommandList();
            _view.ShowDecideButton();
            _view.HideStrength();
            _view.SetActiveBack(true);
        }
    }

    private void CommandRefresh()
    {
        _view.CommandRefresh(_model.CurrentActor.Sp);
    }
}
