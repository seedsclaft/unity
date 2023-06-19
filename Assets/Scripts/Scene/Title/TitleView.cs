using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

public class TitleView : BaseView
{
    [SerializeField] private TitleCommandList commandList = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    private HelpWindow _helpWindow = null;

    public int titleCommandIndex{
        get {return commandList.selectIndex;}
    }
    
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new TitlePresenter(this);
    }

    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        commandList.SetHelpWindow(_helpWindow);
    }

    public void SetEvent(System.Action<TitleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetTitleCommand(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallTitleCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    public void RefreshCommandIndex(int selectIndex)
    {
        commandList.Refresh(selectIndex);
    }

    private void CallTitleCommand(TitleComandType commandType){
        var eventData = new TitleViewEvent(CommandType.TitleCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void SetCommandDisable(int commandId)
    {
        commandList.SetDisable(DataSystem.TacticsCommand[commandId],true);
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