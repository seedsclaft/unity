using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Confirm;

public class ConfirmPresenter 
{
    ConfirmView _view = null;

    ConfirmModel _model = null;
    private bool _busy = true;
    public ConfirmPresenter(ConfirmView view)
    {
        _view = view;
        _model = new ConfirmModel();

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));
        _view.SetConfirmCommand(_model.ConfirmCommand());
        _busy = false;
    }

    
    private void updateCommand(ConfirmViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.IsNoChoise)
        {
           CommandIsNoChoise();
        }
    }

    private void CommandIsNoChoise()
    {
        _view.SetConfirmCommand(_model.NoChoiceConfirmCommand());
    }
}

public class ConfirmInfo
{
    private string _title = "";
    public string Title {get {return _title;}}
    private System.Action<ConfirmComandType> _callEvent = null;
    public System.Action<ConfirmComandType> CallEvent {get {return _callEvent;}}
    private bool _isNoChoise = false;
    public bool IsNoChoise {get {return _isNoChoise;}}
    private List<SkillInfo> _skillInfos = null;
    public List<SkillInfo> SkillInfos {get {return _skillInfos;}}

    public ConfirmInfo(string title,System.Action<ConfirmComandType> callEvent)
    {
        _title = title;
        _callEvent = callEvent;
    }

    public void SetIsNoChoise(bool isNoChoice)
    {
        _isNoChoise = isNoChoice;
    }

    public void SetSkillInfo(List<SkillInfo> skillInfos)
    {
        _skillInfos = skillInfos;
    }
}