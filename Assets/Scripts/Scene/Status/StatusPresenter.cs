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

        //List<ActorInfo> actorInfos = _model.Actors();
        _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());

        _view.SetStrengthInfo(_model.ConfirmCommand());
        _view.SetStatusCommand(_model.StatusCommand);
        _model.ChangeSkillAttributeType();
        _view.SetAttributeTypes(_model.AttributeTypes(),_model.CurrentAttributeType);
        if (_model.StatusActors().Count == 1) _view.HideArrows();
        CommandRefresh();
    }

    private void updateCommand(StatusViewEvent viewEvent)
    {
        if (_view.Busy){
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
        if (viewEvent.commandType == Status.CommandType.SelectSkillAction)
        {
            CommandSelectSkillAction((SkillInfo) viewEvent.templete);
        }
        if (viewEvent.commandType == Status.CommandType.SelectSkillLearning)
        {
            CommandSelectSkillLearning((SkillInfo) viewEvent.templete);
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
        
        if (viewEvent.commandType == Status.CommandType.SelectStrengthReset)
        {
            CommandSelectStrengthReset();
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
            SaveSelectedSkillId();
            _view.HideSkillActionList();
            _view.ActivateActorList();
            if (_model.StatusActors().Count > 1) _view.ShowArrows();
            _view.ShowCommandList();
            _view.ShowDecideButton();
            if (_view.IsDisplayBack == false)
            {
                _view.SetActiveBack(false);
            }
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandStatusCommand(StatusComandType statusComandType)
    {
        if (statusComandType == StatusComandType.Strength)
        {
            _backCommandType = Status.CommandType.StatusCommand;
            _model.ClearStrength();
            _view.ActivateStrengthList();
            _view.ShowStrength();
            _view.DeactivateActorList();
            _view.SetActiveBack(false);
        }
        if (statusComandType == StatusComandType.SkillActionList)
        {
            _backCommandType = Status.CommandType.StatusCommand;
            _view.SetActiveBack(true);
            _view.ShowSkillActionList();
            _view.DeactivateActorList();
            CommandAttributeType(_model.CurrentAttributeType);
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.HideDecideButton();
        _view.HideArrows();
        _view.HideCommandList();
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        var lastSelectIndex = skillInfos.FindIndex(a => a.Id == _model.CurrentActor.LastSelectSkillId);
        if (lastSelectIndex == -1)
        {
            lastSelectIndex = 0;
        }
        _view.RefreshSkillActionList(skillInfos,_model.AttributeTypes(),attributeType,lastSelectIndex);
    }

    private void CommandDecideActor()
    {
        ActorInfo actorInfo = _model.CurrentActor;
        var text = _model.SelectAddActorConfirmText(actorInfo.Master.Name);
        ConfirmInfo confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(confirmInfo);
        _view.DeactivateCommandList();
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
                _model.ForgetSkill();
                _view.ActivateSkillActionList();
                CommandRefresh();
            } else
            {
                _view.ActivateSkillActionList();
            }
        }

        if (_popupCommandType == Status.CommandType.SelectSkillLearning)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                //_model.LearnSkillInfo();
                //_view.ActivateSkillActionList();
                CommandRefresh();
            } else
            {
                _view.ActivateSkillActionList();
            }
        }

        if (_popupCommandType == Status.CommandType.SelectStrengthReset)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                _model.StrengthReset();
                _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());
                CommandRefresh();
                _view.ActivateStrengthList();
            } else
            {
                _view.ActivateStrengthList();
            }
        }
        
        if (_popupCommandType == Status.CommandType.StrengthClose)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                _model.DecideStrength();
                _view.SetActorInfo(_model.CurrentActor,_model.StatusActors());
                CommandRefresh();
                _view.ActivateStrengthList();
            } else
            {
                _view.ActivateStrengthList();
            }
        }

        if (_popupCommandType == Status.CommandType.DecideStage)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                _model.SelectAddActor();
                _view.CommandStatusClose();
                _view.CommandSceneChange(Scene.Reborn);
            } else{
                _view.ActivateCommandList();
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
            CommandAttributeType(_model.CurrentAttributeType);
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
            CommandAttributeType(_model.CurrentAttributeType);
            CommandRefresh();
        });
    }

    private void CommandSelectSkillAction(SkillInfo skillInfo)
    {
        //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectSkillLearning(SkillInfo skillInfo)
    {
        if (skillInfo.LearningState == LearningState.Notlearned)
        {
            _model.SetLearnSkillInfo(skillInfo);
            var leariningSkillList = _model.LearningSkillList(skillInfo.Attribute);
            _view.RefreshSkillActionList(leariningSkillList,_model.AttributeTypes(),_model.CurrentAttributeType,0);
            _view.HideAttributeList();
        }
        if (skillInfo.LearningState == LearningState.SelectLearn)
        {
            _model.LearnSkillInfo(skillInfo);
            _view.ShowAttributeList();
            CommandAttributeType(_model.CurrentAttributeType);
        }
        //_model.SetActorLastSkillId(skillInfo.Id);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandSelectStrengthPlus(StatusParamType statusId)
    {
        bool enableParamUp = _model.EnableParamUp(statusId);
        if (enableParamUp == true)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeParameter(statusId,-1);
        } else
        {

        }
        CommandRefresh();
    }

    private void CommandSelectStrengthReset()
    {
        string text = DataSystem.System.GetTextData(2010).Text;
        ConfirmInfo confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(confirmInfo);
        _view.DeactivateStrengthList();
        _popupCommandType = Status.CommandType.SelectStrengthReset;
    }

    private void CommandStrengthClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _popupCommandType = Status.CommandType.StrengthClose;
            TextData textData = DataSystem.System.GetTextData(2000);
            ConfirmInfo confirmInfo = new ConfirmInfo(textData.Text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(confirmInfo);
            _view.DeactivateCommandList();
            _view.DeactivateStrengthList();
        } else{
            _backCommandType = Status.CommandType.None;
            if (_model.StatusActors().Count > 1) _view.ShowArrows();
            _view.ShowCommandList();
            _view.ShowDecideButton();
            _view.HideStrength();
            _view.DeactivateStrengthList();
            _view.ActivateActorList();
            _view.ActivateCommandList();
            _view.SetActiveBack(true);
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandRefresh()
    {
        _view.RefreshActor(_model.CurrentActor);
        _view.CommandRefresh(_model.CurrentActor.Sp,_model.StrengthNuminous());
        List<SkillInfo> skillInfos = _model.SkillActionList(_model.CurrentAttributeType);
        var lastSelectIndex = skillInfos.FindIndex(a => a.Id == _model.CurrentActor.LastSelectSkillId);
        if (lastSelectIndex == -1)
        {
            lastSelectIndex = 0;
        }
        _view.RefreshSkillActionList(skillInfos,_model.AttributeTypes(),_model.CurrentAttributeType,lastSelectIndex);
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
