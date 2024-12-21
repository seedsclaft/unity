using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Map;
    public class MapPresenter : BasePresenter
    {
        MapModel _model = null;
        MapView _view = null;
        private bool _busy = true;
        public MapPresenter(MapView view)
        {
            _view = view;
            SetView(_view);
            _model = new MapModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;

            _view.SetEvent((type) => UpdateCommand(type));
            _view.CommandMapChange(MapType.Default);

            _view.CreateMapLeaderActor(_model.LeaderActorPrefab());
            //_view.SetSymbolList(_model.StageSymbolInfos());
            //_view.SetPositionSymbolRecords(_model.PartyInfo.Seek);
            //CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            /*
            Debug.Log(viewEvent.ViewCommandType.MapCommandType);
            switch (viewEvent.ViewCommandType.MapCommandType)
            {
                case CommandType.BattleStart:
                    CommandBattleStart();
                    break;
                case CommandType.CallStatus:
                    CommandCallStatus();
                    break;
                case CommandType.CallSymbol:
                    CommandCallSymbol();
                    break;
                case CommandType.OnClickSymbol:
                    CommandOnClickSymbol((SymbolInfo)viewEvent.template);
                    break;
                case CommandType.OnCancelSymbol:
                    CommandOnCancelSymbol();
                    break;
            }
            */
        }

        private void CommandBattleStart()
        {
            var currentSymbol = _view.SelectSymbolInfo;
            if (currentSymbol != null)
            {
                _view.ClearMap();
                var battleSceneInfo = new BattleSceneInfo
                {
                    ActorInfos = _model.PartyMembers(),
                    EnemyInfos = currentSymbol.TroopInfo.BattlerInfos
                };
                _view.CommandGotoSceneChange(Scene.Battle,battleSceneInfo);
            }
        }

        private void CommandCallStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                _view.SetViewBusy(false);
                //_view.SetHelpText(DataSystem.GetText(20020));
            });
            _view.SetViewBusy(true);
        }

        private void CommandCallSymbol()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.SetSymbolList(_model.StageSymbolInfos(),_model.PartyInfo.SeekIndex,_model.PartyInfo.Seek);
            _view.UpdatePartyInfo(_model.PartyInfo);
            _view.SetViewBusy(true);
        }

        private void CommandOnClickSymbol(SymbolInfo symbolInfo)
        {
            var currentSymbol = symbolInfo;
            if (currentSymbol != null && _model.IsCurrentSeekSymbolInfo(symbolInfo))
            {
                switch (currentSymbol.Master.SymbolType)
                {
                    case SymbolType.Battle:
                        CommandBattleStart();
                        return;
                    case SymbolType.Resource:
                        _model.EndSymbolInfo(currentSymbol);
                        CommandNextSeek();
                        return;
                }
            }
        }

        private void CommandOnCancelSymbol()
        {
            _view.SetViewBusy(false);
        }

        private void CommandNextSeek()
        {
            _model.SeekNext();
            _view.SetSymbolList(_model.StageSymbolInfos(),_model.PartyInfo.SeekIndex,_model.PartyInfo.Seek);
            _view.UpdatePartyInfo(_model.PartyInfo);
        }
    }
}