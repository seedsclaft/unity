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
            //CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.MapCommandType)
            {
                case CommandType.BattleStart:
                    CommandBattleStart();
                    break;
                case CommandType.CallStatus:
                    CommandCallStatus();
                    break;
            }
        }

        private void CommandBattleStart()
        {
            _view.ClearMap();
            var enemyInfos = new List<BattlerInfo>();
            var enemyData = DataSystem.Enemies[3];
            enemyInfos.Add( new BattlerInfo(enemyData,1,0,LineType.Front,false));
            enemyInfos.Add( new BattlerInfo(enemyData,1,1,LineType.Front,false));
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = _model.PartyMembers(),
                EnemyInfos = enemyInfos
            };
            _view.CommandGotoSceneChange(Scene.Battle,battleSceneInfo);
        }

        private void CommandCallStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                //_view.SetHelpText(DataSystem.GetText(20020));
            });
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("TITLE");
        }
    }
}