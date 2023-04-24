using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loading;

public class LoadingPresenter 
{
    LoadingModel _model = null;
    LoadingView _view = null;

    private bool _busy = true;
    public LoadingPresenter(LoadingView view)
    {
        _view = view;
        _model = new LoadingModel();

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));
        CommandRefresh();
        _busy = false;
    }

    private void updateCommand(LoadingViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
    }

    private async void CommandRefresh()
    {
        _model.RefreshTips();
        var sprite = await _model.TipsImage();
        _view.SetTips(sprite,_model.TipsText());
    }
}
