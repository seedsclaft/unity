using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Option;
using System;

namespace Ryneus
{
    public class OptionPresenter : BasePresenter
    {
        OptionView _view = null;

        OptionModel _model = null;
        private bool _busy = true;
        public OptionPresenter(OptionView view)
        {
            _view = view;
            _model = new OptionModel();
            _model.ChangeTempInputType(GameSystem.ConfigData.InputType);

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetOptionCategoryList(GetListData(_model.OptionCategoryList()));
            _view.SetHelpWindow();
            CommandSelectCategory();
            _view.OpenAnimation();
            _busy = false;
        }
        
        private void UpdateCommand(OptionViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.commandType == CommandType.ChangeOptionValue)
            {
                CommandOptionValue((OptionInfo)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.SelectCategory)
            {
                CommandSelectCategory();
            }
            if (viewEvent.commandType == CommandType.SelectOptionList)
            {
                CommandSelectOptionList();
            }
            if (viewEvent.commandType == CommandType.CancelOptionList)
            {
                CommandCancelOptionList();
            }
            if (viewEvent.commandType == CommandType.DecideCategory)
            {
                CommandDecideCategory();
            }
        }

        private void CommandOptionValue(OptionInfo data)
        {
            var inputKeyType = data.keyType;
            switch (data.OptionCommand.Key)
            {
                case "BGM_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Min(1, SoundManager.Instance.BgmVolume + 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Max(0, SoundManager.Instance.BgmVolume - 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeBGMMute(!SoundManager.Instance.BGMMute);
                    }
                    break;
                case "SE_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Min(1, SoundManager.Instance.SeVolume + 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Max(0, SoundManager.Instance.SeVolume - 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeSEMute(!SoundManager.Instance.SeMute);
                    }
                    break;
                case "GRAPHIC_QUALITY":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeGraphicIndex(1);
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeGraphicIndex(2);
                    };
                    break;
                case "EVENT_SKIP":
                    ConfigUtility.ChangeEventSkipIndex(inputKeyType == InputKeyType.Right);
                    break;
                case "COMMAND_END_CHECK":
                    ConfigUtility.ChangeCommandEndCheck(inputKeyType == InputKeyType.Left);
                    break;
                case "BATTLE_WAIT":
                    ConfigUtility.ChangeBattleWait(inputKeyType == InputKeyType.Left);
                    break;
                case "BATTLE_ANIMATION":
                    ConfigUtility.ChangeBattleAnimation(inputKeyType == InputKeyType.Right);
                    break;
                case "INPUT_TYPE":
                    _model.ChangeTempInputType(inputKeyType == InputKeyType.Right);
                    break;
                case "BATTLE_AUTO":
                    ConfigUtility.ChangeBattleAuto(inputKeyType == InputKeyType.Right);
                    break;
                case "BATTLE_SPEED":
                    if (inputKeyType == InputKeyType.Right)
                    {                    
                        ConfigUtility.ChangeBattleSpeed(1);
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeBattleSpeed(-1);
                    }
                    break;
                case "TUTORIAL_CHECK":
                    ConfigUtility.ChangeTutorialCheck(inputKeyType == InputKeyType.Left);
                    break;
            }
            CommandRefresh();
        }

        private void CommandVolumeSlider(float volume)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.Key == "BGM_VOLUME")
                {
                    ConfigUtility.ChangeBGMValue(volume);
                } else
                if (data.OptionCommand.Key == "SE_VOLUME")
                {
                    ConfigUtility.ChangeSEValue(volume);
                }
                CommandRefresh();
            }
        }

        private void CommandVolumeMute(bool isMute)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.Key == "BGM_VOLUME")
                {
                    ConfigUtility.ChangeBGMMute(isMute);
                } else
                if (data.OptionCommand.Key == "SE_VOLUME")
                {
                    ConfigUtility.ChangeSEMute(isMute);
                }
                CommandRefresh();
            }
        }

        private void CommandChangeToggle(int toggleIndex)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                switch (data.OptionCommand.Key)
                {
                    case "GRAPHIC_QUALITY":
                        if (toggleIndex == 1)
                        {
                            ConfigUtility.ChangeGraphicIndex(1);
                        }
                        if (toggleIndex == 0)
                        {
                            ConfigUtility.ChangeGraphicIndex(2);
                        };
                        break;
                    case "EVENT_SKIP":
                        ConfigUtility.ChangeEventSkipIndex(toggleIndex == 1);
                        break;
                    case "COMMAND_END_CHECK":
                        ConfigUtility.ChangeCommandEndCheck(toggleIndex == 0);
                        break;
                    case "BATTLE_WAIT":
                        ConfigUtility.ChangeBattleWait(toggleIndex == 0);
                        break;
                    case "BATTLE_ANIMATION":
                        ConfigUtility.ChangeBattleAnimation(toggleIndex == 1);
                        break;
                    case "INPUT_TYPE":
                        _model.ChangeTempInputType(toggleIndex == 1);
                        break;
                    case "BATTLE_AUTO":
                        ConfigUtility.ChangeBattleAuto(toggleIndex == 1);
                        break;
                    case "BATTLE_SPEED":
                        ConfigUtility.SetBattleSpeed(ConfigUtility.SpeedList[toggleIndex+1]);
                        break;
                    case "TUTORIAL_CHECK":
                        ConfigUtility.ChangeTutorialCheck(toggleIndex == 0);
                        break;
                }
                CommandRefresh();
            }
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh();
        }

        private void CommandSelectCategory()
        {
            var categoryIndex = _view.OptionCategoryIndex + 1;
            if (categoryIndex >= 1 && categoryIndex < 3)
            {
                _view.SetOptionList(GetListData(_model.OptionCommandData(
                    categoryIndex,
                    (a) => CommandVolumeSlider(a),
                    (a) => CommandVolumeMute(a),
                    (a) => CommandChangeToggle(a)
                )));
                _view.CommandRefresh();
            }
        }

        private void CommandSelectOptionList()
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.ButtonType == OptionButtonType.Button)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                }
            }
        }

        private void CommandDecideCategory()
        {
            _view.DecideCategory();
            CommandRefresh();
        }

        private void CommandCancelOptionList()
        {
            _view.CancelOptionList();
            CommandRefresh();
        }
    }
}