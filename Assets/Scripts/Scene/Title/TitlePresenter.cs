using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

namespace Ryneus
{
    public class TitlePresenter : BasePresenter
    {
        TitleModel _model = null;
        TitleView _view = null;
        private bool _busy = true;
        public TitlePresenter(TitleView view)
        {
            _view = view;
            SetView(_view);
            _model = new TitleModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;
            ConfigUtility.ApplyConfigData();

            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetVersion(_model.VersionText());
            CommandRefresh();
            var bgm = await _model.GetBgmData("TITLE");
            SoundManager.Instance.PlayBgm(bgm,1.0f,false);
            _busy = false;
            var existPlayerData = SaveSystem.ExistsLoadPlayerFile();
            if (existPlayerData)
            {
                var loadSuccess = SaveSystem.LoadPlayerInfo();
                if (loadSuccess == false)
                {
                    var confirmInfo = new ConfirmInfo(DataSystem.GetText(13330),(a) => UpdatePopup(a));
                    //SaveSystem.DeletePlayerData();
                    confirmInfo.SetIsNoChoice(true);
                    _view.CommandCallConfirm(confirmInfo);
                    return;
                }
                _view.SetPlayerData(_model.PlayerName(),_model.PlayerId());
            }
        }

        private void UpdateCommand(TitleViewEvent viewEvent)
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
                case CommandType.SelectTitle:
                    CommandSelectTitle();
                    break;
                case CommandType.Ranking:
                    CommandRanking();
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

        private void CommandSelectTitle()
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

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("TITLE");
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(GetListData(_model.SideMenu()),() => 
            {            
                CommandRefresh();
                _busy = false;
            });
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}