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
            _view.CommandMapChange(MapType.Battle);

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
        }
    }
}