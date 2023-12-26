using AlcanaResult;

public class AlcanaResultPresenter : BasePresenter
{
    private AlcanaResultModel _model = null;
    private AlcanaResultView _view = null;
    private bool _busy = false;
    public AlcanaResultPresenter(AlcanaResultView view)
    {
        _view = view;
        SetView(_view);
        _model = new AlcanaResultModel();
        SetModel(_model);
        Initialize();
    }

    private void Initialize()
    { 
        _view.SetHelpWindow();
        _view.SetResultList(_model.RebornResultCommand());
        _view.SetActors(_model.AlcanaMembers());
        _view.SetEvent((type) => UpdateCommand(type));
        _busy = true;

        _view.StartAnimation();
        _view.StartRebornResultAnimation(_model.MakeListData(_model.AlcanaMembers()));
        _busy = false;
    }

    private void UpdateCommand(AlcanaResultViewEvent viewEvent)
    {
        if (_view.Busy){
            return;
        }
        switch (viewEvent.commandType)
        {
            case CommandType.EndAnimation:
                CommandEndAnimation();
                break;
            case CommandType.ResultClose:
                CommandRebornResultClose((ConfirmCommandType)viewEvent.template);
                break;
        }
    }

    private void CommandEndAnimation()
    {
        _view.ShowResultList(_model.ResultGetItemInfos());
    }

    private void CommandRebornResultClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            CommandEndReborn();
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandEndReborn()
    {
        _model.ReleaseAlcana();
        _view.CommandSceneChange(Scene.Tactics);
    }
}
