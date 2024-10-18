using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ranking;

namespace Ryneus
{
    public class RankingPresenter : BasePresenter
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
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.RankingOpen:
                    CommandRankingOpen((int)viewEvent.template);
                    break;
                case CommandType.Detail:
                    CommandDetail((int)viewEvent.template);
                    break;
            }
        }

        private void CommandRankingOpen(int stageId)
        {
            _busy = true;
            _view.CommandGameSystem(Base.CommandType.CallLoading);
            _model.RankingInfos(stageId,(res) => 
            {
                _view.CommandGameSystem(Base.CommandType.CloseLoading);
                _view.SetRankingInfo(res);
                _busy = false;
            });
        }

        private void CommandDetail(int listIndex)
        {
            CommandStatusInfo(_model.RankingActorInfos(listIndex),false,true,false,false,-1,() => 
            {
            });
            _model.RankingActorInfos(listIndex);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
    }
}