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

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));
        _view.SetOptionCommand(_model.OptionCommand(),() => 
        {
            _view.InitializeVolume(_model.BGMVolume(),_model.BGMMute(),_model.SEVolume(),_model.SEMute());
            _view.InitializeGraphic(_model.GraphicIndex());
            _view.InitializeEventSkip(GameSystem.ConfigData._eventSkipIndex ? 1 : 2);
            _view.InitializeCommandEndCheck(GameSystem.ConfigData._commandEndCheck ? 2 : 1);
            _view.InitializeBattleWait(GameSystem.ConfigData._battleWait ? 2 : 1);
            _view.InitializeBattleAnimation(GameSystem.ConfigData._battleAnimationSkip ? 1 : 2);
            _view.InitializeInputType(GameSystem.ConfigData._inputType ? 1 : 2);
            _view.InitializeBattleAuto(GameSystem.ConfigData._battleAuto ? 1 : 2);
        });
        _view.SetHelpWindow();
        _busy = false;
    }

    
    private void updateCommand(OptionViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.SelectCategory)
        {
           CommandSelectCategory((OptionCategory)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeBGMValue)
        {
           CommandChangeBGMValue((float)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeBGMMute)
        {
           CommandChangeBGMMute((bool)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeSEValue)
        {
           CommandChangeSEValue((float)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeSEMute)
        {
           CommandChangeSEMute((bool)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeGraphicIndex)
        {
           CommandChangeGraphicIndex((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeEventSkipIndex)
        {
           CommandChangeEventSkipIndex((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeCommandEndCheck)
        {
           CommandChangeCommandEndCheck((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeBattleWait)
        {
           CommandChangeBattleWait((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeBattleAnimation)
        {
           CommandChangeBattleAnimation((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeInputType)
        {
           CommandChangeInputType((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ChangeBattleAuto)
        {
           CommandChangeBattleAuto((int)viewEvent.template);
        }
    }

    private void CommandSelectCategory(OptionCategory optionCategory)
    {
        var optionIndex = _model.OptionIndex(optionCategory);
        _view.CommandSelectCategory(optionIndex);
    }

    private void CommandChangeBGMValue(float bgmValue)
    {
        _model.ChangeBGMValue(bgmValue);
    }

    private void CommandChangeBGMMute(bool bgmMute)
    {
        _model.ChangeBGMMute(bgmMute);
    }

    private void CommandChangeSEValue(float seValue)
    {
        _model.ChangeSEValue(seValue);
    }

    private void CommandChangeSEMute(bool seMute)
    {
        _model.ChangeSEMute(seMute);
    }

    private void CommandChangeGraphicIndex(int graphicIndex)
    {
        _model.ChangeGraphicIndex(graphicIndex);
    }

    private void CommandChangeEventSkipIndex(int eventSkipIndex)
    {
        GameSystem.ConfigData._eventSkipIndex = (eventSkipIndex == 1);
        _view.CommandChangeEventSkipIndex(eventSkipIndex == 1);
    }

    private void CommandChangeCommandEndCheck(int commandEndCheckIndex)
    {
        GameSystem.ConfigData._commandEndCheck = (commandEndCheckIndex == 2);
    }

    private void CommandChangeBattleWait(int battleWaitIndex)
    {
        GameSystem.ConfigData._battleWait = (battleWaitIndex == 2);
    }

    private void CommandChangeBattleAnimation(int animationIndex)
    {
        GameSystem.ConfigData._battleAnimationSkip = (animationIndex == 1);
    }

    private void CommandChangeInputType(int inputTypeIndex)
    {
        _view.SetTempInputType(inputTypeIndex);
        //GameSystem.ConfigData._inputType = (inputTypeIndex == 1);
    }

    private void CommandChangeBattleAuto(int autoIndex)
    {
        GameSystem.ConfigData._battleAuto = (autoIndex == 1);
    }
}

public class OptionInfo
{
}