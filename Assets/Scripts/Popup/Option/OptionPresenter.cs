using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Option;

namespace Ryneus
{
    public class OptionPresenter 
    {
        OptionView _view = null;

        OptionModel _model = null;
        private bool _busy = true;
        public OptionPresenter(OptionView view)
        {
            _view = view;
            _model = new OptionModel();
            _model.ChangeTempInputType(GameSystem.ConfigData.InputType);

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetOptionCategoryList(_model.OptionCategoryList());
            _view.SetHelpWindow();
            CommandSelectCategory();
            _busy = false;
        }

        
        private void UpdateCommand(OptionViewEvent viewEvent)
        {
            if (_busy){
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
        }

        private void CommandOptionValue(OptionInfo data)
        {
            var inputKeyType = data.keyType;
            switch (data.OptionCommand.Key)
            {
                case "BGM_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Min(1, SoundManager.Instance.BGMVolume + 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Max(0, SoundManager.Instance.BGMVolume - 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeBGMMute(!Ryneus.SoundManager.Instance.BGMMute);
                    }
                    break;
                case "SE_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Min(1, Ryneus.SoundManager.Instance.SeVolume + 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Max(0, Ryneus.SoundManager.Instance.SeVolume - 0.05f));
                    }
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeSEMute(!Ryneus.SoundManager.Instance.SeMute);
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
            }
            CommandRefresh();
        }

        private void CommandVolumeSlider(float volume)
        {
            var ListData = _view.OptionCommand;
            if (ListData != null)
            {
                var data = (OptionInfo)ListData.Data;
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
            var ListData = _view.OptionCommand;
            if (ListData != null)
            {
                var data = (OptionInfo)ListData.Data;
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
            var ListData = _view.OptionCommand;
            if (ListData != null)
            {
                var data = (OptionInfo)ListData.Data;
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
                _view.SetOptionList(_model.OptionCommandData(
                    categoryIndex,
                    (a) => CommandVolumeSlider(a),
                    (a) => CommandVolumeMute(a),
                    (a) => CommandChangeToggle(a)
                ));
                _view.CommandRefresh();
            }
        }

        private void CommandSelectOptionList()
        {
            var ListData = _view.OptionCommand;
            if (ListData != null)
            {
                var data = (OptionInfo)ListData.Data;
                if (data.OptionCommand.ButtonType == OptionButtonType.Button)
                {
                    Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);

                }
            }
        }

    }
}