using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Option;

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
                    _model.ChangeBGMValue(Mathf.Min(1, Ryneus.SoundManager.Instance.BGMVolume + 0.05f));
                }
                if (inputKeyType == InputKeyType.Left)
                {
                    _model.ChangeBGMValue(Mathf.Max(0, Ryneus.SoundManager.Instance.BGMVolume - 0.05f));
                }
                if (inputKeyType == InputKeyType.Option1)
                {
                    _model.ChangeBGMMute(!Ryneus.SoundManager.Instance.BGMMute);
                }
                break;
            case "SE_VOLUME":
                if (inputKeyType == InputKeyType.Right)
                {
                    _model.ChangeSEValue(Mathf.Min(1, Ryneus.SoundManager.Instance.SeVolume + 0.05f));
                }
                if (inputKeyType == InputKeyType.Left)
                {
                    _model.ChangeSEValue(Mathf.Max(0, Ryneus.SoundManager.Instance.SeVolume - 0.05f));
                }
                if (inputKeyType == InputKeyType.Option1)
                {
                    _model.ChangeSEMute(!Ryneus.SoundManager.Instance.SeMute);
                }
                break;
            case "GRAPHIC_QUALITY":
                if (inputKeyType == InputKeyType.Right)
                {
                    _model.ChangeGraphicIndex(1);
                }
                if (inputKeyType == InputKeyType.Left)
                {
                    _model.ChangeGraphicIndex(2);
                };
                break;
            case "EVENT_SKIP":
                _model.ChangeEventSkipIndex(inputKeyType == InputKeyType.Right);
                break;
            case "COMMAND_END_CHECK":
                _model.ChangeCommandEndCheck(inputKeyType == InputKeyType.Left);
                break;
            case "BATTLE_WAIT":
                _model.ChangeBattleWait(inputKeyType == InputKeyType.Left);
                break;
            case "BATTLE_ANIMATION":
                _model.ChangeBattleAnimation(inputKeyType == InputKeyType.Right);
                break;
            case "INPUT_TYPE":
                _model.ChangeTempInputType(inputKeyType == InputKeyType.Right);
                break;
            case "BATTLE_AUTO":
                _model.ChangeBattleAuto(inputKeyType == InputKeyType.Right);
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
                _model.ChangeBGMValue(volume);
            } else
            if (data.OptionCommand.Key == "SE_VOLUME")
            {
                _model.ChangeSEValue(volume);
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
                _model.ChangeBGMMute(isMute);
            } else
            if (data.OptionCommand.Key == "SE_VOLUME")
            {
                _model.ChangeSEMute(isMute);
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
                        _model.ChangeGraphicIndex(1);
                    }
                    if (toggleIndex == 0)
                    {
                        _model.ChangeGraphicIndex(2);
                    };
                    break;
                case "EVENT_SKIP":
                    _model.ChangeEventSkipIndex(toggleIndex == 1);
                    break;
                case "COMMAND_END_CHECK":
                    _model.ChangeCommandEndCheck(toggleIndex == 0);
                    break;
                case "BATTLE_WAIT":
                    _model.ChangeBattleWait(toggleIndex == 0);
                    break;
                case "BATTLE_ANIMATION":
                    _model.ChangeBattleAnimation(toggleIndex == 1);
                    break;
                case "INPUT_TYPE":
                    _model.ChangeTempInputType(toggleIndex == 1);
                    break;
                case "BATTLE_AUTO":
                    _model.ChangeBattleAuto(toggleIndex == 1);
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
                switch (data.OptionCommand.Key)
                {
                    case "INITALIZE_GAMEDATA":
                    var popupInfo = new ConfirmInfo(DataSystem.GetTextData(581).Text,(a) => UpdatePopupDeletePlayerData((ConfirmCommandType)a));
                    _view.CommandCallConfirm(popupInfo);
                    return;
                }
            }
        }
    }

    private void UpdatePopupDeletePlayerData(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _view.CommandGameSystem(Base.CommandType.ClosePopup);
            _model.DeletePlayerData();
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(582).Text,(a) => {
                Ryneus.SoundManager.Instance.StopBgm();
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                _view.CommandGotoSceneChange(Scene.Boot);
            });
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        }
    }
}
