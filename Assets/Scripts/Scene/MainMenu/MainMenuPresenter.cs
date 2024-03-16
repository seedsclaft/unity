using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainMenu;
using Ryneus;

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
            _view.SetNuminous(_model.Currency);
            _view.SetTotalScore(_model.TotalScore);
            _view.SetTacticsCommand(_model.TacticsCommand());
            //_model.InitStageData();

            var bgm = await _model.GetBgmData("MAINMENU");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.UpdateMainMenuStage();
            _busy = false;
        }

        private void UpdateCommand(MainMenuViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.TacticsCommand:
                    CommandTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
            }
            if (viewEvent.commandType == CommandType.StageSelect)
            {
                CommandStageSelect((int)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.SelectSideMenu)
            {
                CommandSelectSideMenu();
            }
        }

        private void CommandTacticsCommand(TacticsCommandType tacticsCommandType)
        {
            _model.SetTacticsCommandType(tacticsCommandType);
            //_view.HideCommandList();
            _view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
            switch (tacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                    return;
                case TacticsCommandType.Train:
                    _view.CallTrainCommand(tacticsCommandType);
                    _view.ChangeBackCommandActive(true);
                    _backCommand = CommandType.TacticsCommandClose;
                    break;
                case TacticsCommandType.Alchemy:
                    _view.CallTrainCommand(tacticsCommandType);
                    _view.ShowLeaningList(_model.SelectActorLearningMagicList());
                    _view.ChangeBackCommandActive(true);
                    //_view.ShowSelectCharacterCommand();
                    _backCommand = CommandType.TacticsCommandClose;
                    break;
                case TacticsCommandType.Status:
                    _view.CallTrainCommand(tacticsCommandType);
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
                if (_model.ClearedStage(stageId))
                {
                    _model.StartClearedStage(stageId);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _model.StartSelectStage(stageId);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                }
            }
        }


        
        private void CommandSlotPopup()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandSceneChange(Scene.Slot);
        }

        private void CommandSelectSideMenu()
        {
        }    
        
    }
}