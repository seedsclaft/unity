using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Ryneus
{
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
                    CommandOnClickSymbol();
                    break;
                case CommandType.OnCancelSymbol:
                    CommandOnCancelSymbol();
                    break;
                case CommandType.OnSelectSymbolIndex:
                    CommandOnSelectSymbolIndex((int)viewEvent.template);
                    break;
                case CommandType.OnSelectSymbolList:
                    CommandOnSelectSymbolList((List<SymbolInfo>)viewEvent.template);
                    break;
            }
        }

        private void CommandBattleStart()
        {
            _view.ClearMap();
            var currentSymbol = _model.SelectSymbolInfo();
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyMembers(),
                EnemyInfos = currentSymbol.TroopInfo.BattlerInfos
            };
            _view.CommandGotoSceneChange(Scene.Battle,battleSceneInfo);
        }

        private void CommandCallStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                _view.SetBusy(false);
                //_view.SetHelpText(DataSystem.GetText(20020));
            });
            _view.SetBusy(true);
        }

        private void CommandCallSymbol()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.SetSymbolList(_model.StageSymbolInfos());
            _view.SetPositionSymbolRecords(_model.PartyInfo.Seek);
            _view.SetBusy(true);
        }

        private void CommandOnClickSymbol()
        {
            var currentSymbol = _model.SelectSymbolInfo();
            if (currentSymbol != null && _model.IsCurrentSeekSymbolInfo())
            {
                switch (currentSymbol.Master.SymbolType)
                {
                    case SymbolType.Resource:
                        _model.EndSymbolInfo(currentSymbol);
                        CommandNextSeek();
                        return;
                }
            }
        }

        private void CommandOnCancelSymbol()
        {
            _view.SetBusy(false);
        }

        private void CommandOnSelectSymbolIndex(int selectIndex)
        {
            _model.GainSeekIndex(selectIndex);
            _view.UpdateSelectSymbolList(_model.SelectSymbolInfo());
        }

        private void CommandOnSelectSymbolList(List<SymbolInfo> symbolInfos)
        {
            _model.SetCurrentSymbolInfos(symbolInfos);
            _view.UpdateSelectSymbolList(_model.SelectSymbolInfo());
        }

        private void CommandNextSeek()
        {
            _model.SeekNext();
            _view.SetSymbolList(_model.StageSymbolInfos());
            _view.SetPositionSymbolRecords(_model.PartyInfo.Seek);
        }
    }
}