using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class StatusCommandList : ListWindow , IInputHandlerEvent
{
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public SystemData.MenuCommandData Data{
        get {
            if (Index < 0)
            {
                return null;
            }
            if (ObjectList[Index].GetComponent<StatusCommand>().Disable.gameObject.activeSelf)
            {
                return null;
            }
            return _data[Index];
        }
    }

    public void Initialize(List<SystemData.MenuCommandData> menuCommands)
    {
        InitializeListView(menuCommands.Count);
        _data = menuCommands;
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var statusCommand = ObjectList[i].GetComponent<StatusCommand>();
            statusCommand.SetData(menuCommands[i],i);
            statusCommand.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            statusCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        for (int i = 0; i < _data.Count;i++)
        {
            var statusCommand = ObjectList[i].GetComponent<StatusCommand>();
            statusCommand.SetDisable(menuCommandData,IsDisable);
        }
    }
}
