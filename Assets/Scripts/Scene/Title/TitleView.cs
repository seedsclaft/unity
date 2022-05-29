using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

public class TitleView : BaseView
{
    [SerializeField] private TitleCommandList commandList = null;

    private new System.Action<TitleViewEvent> _commandData = null;

    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new TitlePresenter(this);
    }

    
    public void SetEvent(System.Action<TitleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetTitleCommand(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallTitleCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallTitleCommand(MenuComandType commandType){
        var eventData = new TitleViewEvent(CommandType.TitleCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }
    
}

namespace Title
{
    public enum CommandType
    {
        None = 0,
        TitleCommand,
    }
}

public class TitleViewEvent
{
    public Title.CommandType commandType;
    public object templete;

    public TitleViewEvent(Title.CommandType type)
    {
        commandType = type;
    }
}