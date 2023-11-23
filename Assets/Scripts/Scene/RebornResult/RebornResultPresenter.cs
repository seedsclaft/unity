using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RebornResult;

public class RebornResultPresenter : BasePresenter
{
    RebornResultModel _model = null;
    RebornResultView _view = null;

    private bool _busy = true;
    public RebornResultPresenter(RebornResultView view)
    {
        _view = view;
        SetView(_view);
        _model = new RebornResultModel();
        SetModel(_model);
        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetResultList(_model.RebornResultCommand());
        _view.SetActors(_model.RebornMembers());
        var bgm = await _model.GetBgmData("TACTICS1");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => UpdateCommand(type));
        _busy = true;

        _view.StartAnimation();
        _view.StartRebornResultAnimation(_model.CastActorInfos(_model.RebornMembers()));
        _busy = false;
    }

    private void UpdateCommand(RebornResultViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        switch (viewEvent.commandType)
        {
            case CommandType.EndAnimation:
            CommandEndAnimation();
            break;
            case CommandType.RebornResultClose:
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
        _model.SetResumeStageTrue();
        _view.CommandSceneChange(Scene.Tactics);
    }
}
