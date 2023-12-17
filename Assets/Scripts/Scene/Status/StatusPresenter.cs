public class StatusPresenter : BasePresenter
{
    private StatusModel _model = null;
    private StatusView _view = null;
    private Status.CommandType _popupCommandType = Status.CommandType.None;
    public StatusPresenter(StatusView view)
    {
        _view = view;
        SetView(_view);
        _model = new StatusModel();
        SetModel(_model);
        Initialize();
    }

    private void Initialize()
    { 
        _view.SetHelpWindow(_model.HelpText());
        _view.SetUIButton();
        _view.SetEvent((type) => UpdateCommand(type));

        CommandRefresh();
        if (_model.StatusActors().Count == 1) _view.HideArrows();
    }

    private void UpdateCommand(StatusViewEvent viewEvent)
    {
        if (_view.Busy){
            return;
        }
        UnityEngine.Debug.Log(viewEvent.commandType);
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
        if (viewEvent.commandType == Status.CommandType.CharacterList)
        {
            CommandCharacterList();
        }
    }

    private void CommandBack()
    {
        _view.CommandBack();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandCharacterList()
    {
        var characterListInfo = new CharacterListInfo((a) => {
            _view.CommandConfirmClose();
            _model.SelectActor(a);
            CommandRefresh();
        });
        _view.CommandCallCharacterList(characterListInfo);
    }

    private void CommandDecideActor()
    {
        var actorInfo = _model.CurrentActor;
        var text = _model.SelectAddActorConfirmText(actorInfo.Master.Name);
        var confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
        _view.CommandCallConfirm(confirmInfo);
        _popupCommandType = Status.CommandType.DecideStage;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void UpdatePopup(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();

        if (_popupCommandType == Status.CommandType.SelectSkillAction)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                CommandRefresh();
            }
            _view.ActivateSkillActionList();
        }


        if (_popupCommandType == Status.CommandType.DecideStage)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _model.SelectAddActor();
                _view.CommandStatusClose();
                var NeedReborn = _model.NeedReborn();
                if (_model.StageMembers().Count == 1 && NeedReborn)
                {
                    _view.CommandSceneChange(Scene.Reborn);
                } else
                {
                    _model.SetResumeStageTrue();
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
        CommandRefresh();
    }

    private void CommandRightActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        SaveSelectedSkillId();
        _model.ChangeActorIndex(1);
        CommandRefresh();
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
