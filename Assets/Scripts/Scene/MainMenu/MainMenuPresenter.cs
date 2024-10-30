using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainMenu;

namespace Ryneus
{
    public class MainMenuPresenter : BasePresenter
    {
        MainMenuModel _model = null;
        MainMenuView _view = null;

        private bool _busy = true;
        private CommandType _backCommand = CommandType.None;
        public MainMenuPresenter(MainMenuView view)
        {
            _view = view;
            SetView(_view);
            _model = new MainMenuModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));
            if (_model.IsEnding())
            {
                // エンディング再生
                var advInfo = new AdvCallInfo();
                advInfo.SetLabel(_model.GetAdvFile(101));
                advInfo.SetCallEvent(() => 
                {
                    _busy = false;
                    _view.ChangeUIActive(true);
                    _view.CommandGotoSceneChange(Scene.Result);
                });
                _view.CommandCallAdv(advInfo);
                _view.ChangeUIActive(false);
            } else
            {
                _view.SetBackGround(_model.NextStage().Master.BackGround);
                _view.SetStageData(_model.NextStage());

                var bgm = await _model.GetBgmData("MAINMENU");
                SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            }
            _busy = false;
        }

        private void UpdateCommand(MainMenuViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.NextStage:
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    _model.StartSelectStage(_model.NextStage().Id);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                    break;
            }
        }
    }
}