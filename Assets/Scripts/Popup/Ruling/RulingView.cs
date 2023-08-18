using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ruling;

public class RulingView : BaseView
{
    [SerializeField] private RuleCommandList confirmCommandList = null;
    
    [SerializeField] private RuleList ruleList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    private new System.Action<RulingViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        ruleList.Initialize();
        new RulingPresenter(this);
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetEvent(System.Action<RulingViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        CreateBackCommand(() => 
        {    
            if (backEvent != null) backEvent();
        });
        SetActiveBack(true);
    }
    
    public void SetRulingCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        confirmCommandList.Initialize(menuCommands,() => CallRulingCommand());
        confirmCommandList.SetInputHandler(InputKeyType.Down,() => CallRulingCommand());
        confirmCommandList.SetInputHandler(InputKeyType.Up,() => CallRulingCommand());
        confirmCommandList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        SetInputHandler(confirmCommandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallRulingCommand()
    {
        var eventData = new RulingViewEvent(CommandType.SelectTitle);
        var item = confirmCommandList.Data;
        if (item != null)
        {
            eventData.templete = item.Id;
            _commandData(eventData);
        }
    }

    public void CommandSelectTitle(List<string> helpList )
    {
        ruleList.Refresh(helpList);
    }

    public void CommandRefresh(List<SystemData.MenuCommandData> menuCommands,List<string> helpList)
    {
        confirmCommandList.Refresh();
        ruleList.Refresh(helpList);
    }
}

namespace Ruling
{
    public enum CommandType
    {
        None = 0,
        SelectTitle = 1,
        SelectCategory = 2,
        Back
    }
}
public class RulingViewEvent
{
    public Ruling.CommandType commandType;
    public object templete;

    public RulingViewEvent(Ruling.CommandType type)
    {
        commandType = type;
    }
}
