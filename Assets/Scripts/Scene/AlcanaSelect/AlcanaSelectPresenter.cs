public class AlcanaSelectPresenter : BasePresenter
{
    private AlcanaSelectModel _model = null;
    private AlcanaSelectView _view = null;
    public AlcanaSelectPresenter(AlcanaSelectView view)
    {
        _view = view;
        SetView(_view);
        _model = new AlcanaSelectModel();
        SetModel(_model);
        Initialize();
    }

    private void Initialize()
    { 
        _view.SetInitHelpText();
        _view.SetEvent((type) => UpdateCommand(type));

        CommandRefresh();
    }

    private void UpdateCommand(AlcanaSelectViewEvent viewEvent)
    {
        if (_view.Busy){
            return;
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.ChangeAlcana)
        {
            CommandChangeAlcana((SkillInfo)viewEvent.template);
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandChangeAlcana(SkillInfo skillInfo)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.ChangeSelectAlcana(skillInfo);
        CommandRefresh();
    }

    private void CommandBack()
    {
        var statusViewInfo = new StatusViewInfo(() => {
            // ステージセレクトに戻る
            _view.CommandStatusClose();
            _view.CommandSceneChange(Scene.MainMenu);
        });
        statusViewInfo.SetDisplayDecideButton(true);
        _view.CommandCallStatus(statusViewInfo);
        _view.ChangeUIActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandRefresh()
    {
        _view.CommandRefresh(_model.SelectedAlcanaList);
        var skillInfos = _model.SkillActionList();
        _view.RefreshMagicList(skillInfos);
        if (_model.CheckStageStart())
        {
            CommandStageStart();
        }
    }

    private void CommandStageStart()
    {
        var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(20030).Text,(a) => UpdatePopup((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopup(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SavePlayerStageData(true);
            _view.CommandSceneChange(Scene.Tactics);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
    }
}
