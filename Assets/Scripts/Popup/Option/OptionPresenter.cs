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
        _view.SetOptionCommand(_model.OptionCommand());
        _view.InitializeVolume(_model.BGMVolume(),_model.BGMMute(),_model.SEVolume(),_model.SEMute());
        _view.InitializeGraphic(_model.GraphicIndex());
        _view.InitializeEventSkip(GameSystem.ConfigData._eventSkipIndex ? 1 : 2);
        _view.InitializeCommandEndCheck(GameSystem.ConfigData._commandEndCheck ? 2 : 1);
        _view.InitializeBattleWait(GameSystem.ConfigData._battleWait ? 2 : 1);
        _busy = false;
    }

    
    private void updateCommand(OptionViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.SelectCategory)
        {
           CommandSelectCategory((OptionCategory)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeBGMValue)
        {
           CommandChangeBGMValue((float)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeBGMMute)
        {
           CommandChangeBGMMute((bool)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeSEValue)
        {
           CommandChangeSEValue((float)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeSEMute)
        {
           CommandChangeSEMute((bool)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeGraphicIndex)
        {
           CommandChangeGraphicIndex((int)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeEventSkipIndex)
        {
           CommandChangeEventSkipIndex((int)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeCommandEndCheck)
        {
           CommandChangeCommandEndCheck((int)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ChangeBattleWait)
        {
           CommandChangeBattleWait((int)viewEvent.templete);
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
}

public class OptionInfo
{
}