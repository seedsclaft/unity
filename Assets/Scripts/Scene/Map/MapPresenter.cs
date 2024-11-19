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

        private void UpdateCommand(MapViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.SelectMap:
                    CommandSelectMap();
                    break;
                case CommandType.Ranking:
                    CommandRanking();
                    break;
                case CommandType.BattleStart:
                    CommandBattleStart();
                    break;
            }
        }

        private void CommandRanking()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Ranking,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            var rankingViewInfo = new RankingViewInfo
            {
                StageId = 1,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallRanking(rankingViewInfo);
        }

        private void CommandSelectMap()
        {
            var loadFile = SaveSystem.ExistsLoadPlayerFile();
            if (loadFile)
            {
                CommandContinue();
            } else
            {
                CommandNewGame();
            }
        }

        private void CommandNewGame()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.PlayStart);
            _view.WaitFrame(60,() => 
            {
                _model.InitSaveInfo();
                _view.CommandGotoSceneChange(Scene.NameEntry);
            });
        }

        private void CommandContinue()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var loadSuccess = SaveSystem.LoadPlayerInfo();
            if (loadSuccess == false)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(13330),(a) => UpdatePopup(a));
                //SaveSystem.DeletePlayerData();
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                return;
            }
            // プレイヤーネームを設定しなおし
            _view.CommandDecidePlayerName(GameSystem.CurrentData.PlayerInfo.PlayerName);
            
            var loadStage = SaveSystem.ExistsStageFile();
            if (loadStage)
            {
                SaveSystem.LoadStageInfo();
            } else
            {
                _model.InitSaveStageInfo();
                _model.StartOpeningStage();
            }
            _view.CommandGotoSceneChange(Scene.Tactics);
        }

        private void CommandBattleStart()
        {
            _view.ClearMap();
            var actorInfos = new List<ActorInfo>();
            var b = new ActorInfo(DataSystem.FindActor(1));
            b.SetBattleIndex(1);
            actorInfos.Add(b);
            //var a = new ActorInfo(DataSystem.FindActor(2));
            //a.SetBattleIndex(2);
            //actorInfos.Add(a);
            var enemyInfos = new List<BattlerInfo>();
            var enemyData = DataSystem.Enemies[3];
            enemyInfos.Add( new BattlerInfo(enemyData,1,0,LineType.Front,false));
            enemyInfos.Add( new BattlerInfo(enemyData,1,1,LineType.Front,false));
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorInfos = actorInfos,
                EnemyInfos = enemyInfos
            };
            _view.CommandGotoSceneChange(Scene.Battle,battleSceneInfo);
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("TITLE");
        }

        private void CommandSelectSideMenu()
        {
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGotoSceneChange(Scene.Map);
        }
    }
}