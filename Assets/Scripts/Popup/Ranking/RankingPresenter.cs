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

    private void Initialize()
    {
        _busy = false;
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetHelpInputInfo("RANKING");
    }

    private void UpdateCommand(RankingViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.RankingOpen)
        {
            CommandRankingOpen((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.Detail)
        {
            CommandDetail((int)viewEvent.template);
        }
    }

    private void CommandRankingOpen(int stageId)
    {
        _busy = true;
        _view.CommandCallLoading();
        _model.RankingInfos(stageId,(res) => {
            _view.CommandLoadingClose();
            _view.SetRankingInfo(res);
            _busy = false;
        });
    }

    private void CommandDetail(int listIndex)
    {
        _model.MakeDetailPartyInfo(listIndex);
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            //_view.SetHelpText(DataSystem.GetTextData(14010).Text);
            _view.ChangeUIActive(true);
            _view.CommandSceneShowUI();
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.ChangeUIActive(false);
        _view.CommandSceneHideUI();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
}
