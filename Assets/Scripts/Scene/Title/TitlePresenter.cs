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
            }
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
                var confirmInfo = new ConfirmInfo("セーブデータを読み込めませんでした。\n誠に申し訳ないですがNewGameから開始をお願いします。",(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
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
            }
            _view.CommandGotoSceneChange(Scene.Tactics);
        }

        private void CommandRefresh()
        {
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(GetListData(_model.SideMenu()),() => {_busy = false;});
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}