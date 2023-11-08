using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPresenter 
{
    StatusModel _model = null;
    StatusView _view = null;
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
        _view.SetHelpWindow(_model.HelpText());
        _view.SetUIButton();
        _view.SetEvent((type) => updateCommand(type));

        _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());
        CommandRefresh();
        if (_model.StatusActors().Count == 1) _view.HideArrows();
    }

    private void updateCommand(StatusViewEvent viewEvent)
    {
        if (_view.Busy){
            return;
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
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandDecideActor()
    {
        ActorInfo actorInfo = _model.CurrentActor;
        var text = _model.SelectAddActorConfirmText(actorInfo.Master.Name);
        ConfirmInfo confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(confirmInfo);
        _popupCommandType = Status.CommandType.DecideStage;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void updatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();

        if (_popupCommandType == Status.CommandType.SelectSkillAction)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                CommandRefresh();
            }
            _view.ActivateSkillActionList();
        }


        if (_popupCommandType == Status.CommandType.DecideStage)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                _model.SelectAddActor();
                _view.CommandStatusClose();
                var NeedReborn = _model.NeedReborn();
                if (_model.StageMembers().Count == 1 && NeedReborn)
                {
                    _view.CommandSceneChange(Scene.Reborn);
                } else
                {
                    _view.CommandSceneChange(Scene.Tactics);
                }
            } else{
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
        }
    }
    
    private void CommandLeftActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        SaveSelectedSkillId();
        _model.ChangeActorIndex(-1);
        _view.MoveActorLeft(() => {
            _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());
            CommandRefresh();
        });
    }

    private void CommandRightActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        SaveSelectedSkillId();
        _model.ChangeActorIndex(1);
        _view.MoveActorRight(() => {
            _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());
            CommandRefresh();
        });
    }

    private void CommandRefresh()
    {
        _view.CommandRefresh();
        var skillInfos = _model.SkillActionList();
        var lastSelectIndex = skillInfos.FindIndex(a => ((SkillInfo)a.Data).Id == _model.CurrentActor.LastSelectSkillId);
        if (lastSelectIndex == -1)
        {
            lastSelectIndex = 0;
        }
        _view.CommandRefreshStatus(skillInfos,_model.CurrentActor,_model.PartyMembers(),lastSelectIndex);
    }

    private void SaveSelectedSkillId()
    {
        var selectedSkillId = _view.SelectedSkillId();
        if (selectedSkillId > -1)
        {
            _model.SetActorLastSkillId(selectedSkillId);
        }
    }
}
