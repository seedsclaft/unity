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
        _view.StartAnimation();
        _view.SetInitHelpText();
        _view.SetEvent((type) => UpdateCommand(type));
        CommandRefresh();
    }

    private void UpdateCommand(AlcanaSelectViewEvent viewEvent)
    {
        if (_view.Busy){
            return;
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.ChangeAlcana)
        {
            CommandChangeAlcana((SkillInfo)viewEvent.template);
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.DeleteAlcana)
        {
            CommandDeleteAlcana((SkillInfo)viewEvent.template);
        }
        if (viewEvent.commandType == AlcanaSelect.CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandEndAnimation()
    {

    }

    private void CommandChangeAlcana(SkillInfo skillInfo)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.ChangeSelectAlcana(skillInfo);
        var selectIndex = _view.SkillListIndex;
        CommandRefresh(selectIndex);
    }

    private void CommandDeleteAlcana(SkillInfo skillInfo)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.SetDeleteAlcana(skillInfo);
        var selectIndex = _view.SkillListIndex;
        CommandRefresh(selectIndex);

        var confirmInfo = new ConfirmInfo(DataSystem.System.GetReplaceText(20050,skillInfo.Master.Name),(a) => UpdatePopupDeleteAlcana(a));
        _view.CommandCallConfirm(confirmInfo);
    }

    private void UpdatePopupDeleteAlcana(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _model.DeleteAlcana();
        CommandRefresh(0);
        _view.CommandConfirmClose();
    }

    private void CommandBack()
    {
        if (GameSystem.LastScene == Scene.MainMenu)
        {
            var statusViewInfo = new StatusViewInfo(() => {
                // ステージセレクトに戻る
                _view.CommandStatusClose();
                _view.CommandSceneChange(Scene.MainMenu);
            });
            _model.InitializeStageData(_model.CurrentStage.Id);
            statusViewInfo.SetDisplayDecideButton(true);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
        } else
        if (GameSystem.LastScene == Scene.Reborn)
        {
            _view.CommandSceneChange(Scene.Reborn);
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
    }

    private void CommandRefresh(int selectIndex = 0)
    {
        _view.CommandRefresh(_model.SelectedAlcanaList);
        var skillInfos = _model.SkillActionList();
        _view.RefreshMagicList(skillInfos,selectIndex);
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
            // アルカナをセット
            _model.SetStageAlcanaList();
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SavePlayerStageData(true);
            _view.CommandSceneChange(Scene.Tactics);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
    }
}
