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

            _view.SetStagesData(_model.Stages());
            _view.SetBackGround(_model.NextStage().Master.BackGround);
            _view.SetStageData(_model.NextStage());
            //_view.SetNuminous(_model.Currency);
            //_view.SetTotalScore(_model.TotalScore);
            //_view.SetTacticsCommand(_model.TacticsCommand());
            //_model.InitStageData();

            var bgm = await _model.GetBgmData("MAINMENU");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.UpdateMainMenuStage();
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
                case CommandType.StageSelect:
                    CommandStageSelect((int)viewEvent.template);
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
            }
        }
        
        private void CommandStageSelect(int stageId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.NeedSlotData(stageId))
            {
                _view.CommandSceneChange(Scene.Slot);
            } else
            {
                _model.StartSelectStage(stageId);
                _view.CommandGotoSceneChange(Scene.Tactics);
            }
        }

        private void CommandSelectSideMenu()
        {
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {

                },
                CommandLists = _model.SideMenu()
            };
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }    
    }
}