using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ranking;

public class RankingPresenter 
{
    RankingModel _model = null;
    RankingView _view = null;

    private bool _busy = true;
    public RankingPresenter(RankingView view)
    {
        _view = view;
        _model = new RankingModel();

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));
        var res = await _model.RankingInfos();
        _view.SetRankingInfo(res);
        _busy = false;
    }

    private void updateCommand(RankingViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
    }
}
