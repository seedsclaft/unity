using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RebornResult;

public class RebornResultPresenter : BasePresenter
{
    RebornResultModel _model = null;
    RebornResultView _view = null;

    private bool _busy = true;

    private bool _isRankingEnd = false;
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
        _view.SetUiView();
        _view.SetResultList(_model.RebornResultCommand());
        _view.SetActors(_model.RebornMembers());
        var bgm = await _model.GetBgmData("TACTICS1");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => updateCommand(type));
        _busy = true;

        _view.StartAnimation();
        _view.StartRebornResultAnimation(_model.RebornMembers());
        _busy = false;
    }

    private void updateCommand(RebornResultViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == CommandType.RebornResultClose)
        {
            CommandRebornResultClose((ConfirmComandType)viewEvent.templete);
        }
    }

    private void CommandEndAnimation()
    {
        _view.ShowResultList(_model.ResultGetItemInfos());
    }

    private void CommandRebornResultClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
        } else
        {
            CommandEndReborn();
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandEndReborn()
    {
        _view.CommandSceneChange(Scene.Tactics);
    }
}
