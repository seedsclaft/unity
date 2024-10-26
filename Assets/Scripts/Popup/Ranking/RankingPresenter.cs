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
            SetView(_view);
            SetModel(_model);

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
                    CommandDetail((List<ActorInfo>)viewEvent.template);
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

        private void CommandDetail(List<ActorInfo> actorInfos)
        {
            _busy = true;
            CommandStatusInfo(actorInfos,false,true,false,false,actorInfos[0].ActorId,() => 
            {
                _busy = false;
            },true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
    }
}