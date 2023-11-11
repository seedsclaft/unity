using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ruling;

public class RulingView : BaseView
{
    [SerializeField] private BaseList commandList = null;
    
    [SerializeField] private RuleList ruleList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    private new System.Action<RulingViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
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
        SetBackCommand(() => 
        {    
            if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
    }
    
    public void SetRulingCommand(List<ListData> ruleList)
    {
        commandList.Initialize(ruleList.Count);
        commandList.SetData(ruleList);
        commandList.SetInputHandler(InputKeyType.Down,() => CallRulingCommand());
        commandList.SetInputHandler(InputKeyType.Up,() => CallRulingCommand());
        commandList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        commandList.SetSelectedHandler(() => CallRulingCommand());
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        commandList.Activate();
    }

    private void CallRulingCommand()
    {
        var eventData = new RulingViewEvent(CommandType.SelectTitle);
        var listData = commandList.ListData;
        if (listData != null)
        {
            var data = (SystemData.CommandData)listData.Data;
            eventData.templete = data.Id;
            _commandData(eventData);
        }
    }

    public void CommandSelectTitle(List<string> helpList )
    {
        ruleList.Refresh(helpList);
    }

    public void CommandRefresh(List<string> helpList)
    {
        commandList.Refresh();
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
