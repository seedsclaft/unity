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
}

public class OptionInfo
{
    private string _title = "";
    public string Title {get {return _title;}}
    private bool _isNoChoise = false;
    public bool IsNoChoise {get {return _isNoChoise;}}
    private List<SkillInfo> _skillInfos = null;
    public List<SkillInfo> SkillInfos {get {return _skillInfos;}}


    public void SetIsNoChoise(bool isNoChoice)
    {
        _isNoChoise = isNoChoice;
    }

    public void SetSkillInfo(List<SkillInfo> skillInfos)
    {
        _skillInfos = skillInfos;
    }
}