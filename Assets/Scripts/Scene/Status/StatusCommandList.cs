using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class StatusCommandList : ListWindow , IInputHandlerEvent
{
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<StatusComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        _data = menuCommands;
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var statusCommand = ObjectList[i].GetComponent<StatusCommand>();
            statusCommand.SetData(menuCommands[i],i);
            statusCommand.SetCallHandler(callEvent);
            statusCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllItems();
        //UpdateSelectIndex(0);
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

    private void CallInputHandler(InputKeyType keyType, System.Action<StatusComandType> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (ObjectList[Index].GetComponent<StatusCommand>().Disable.gameObject.activeSelf) return;
            callEvent((StatusComandType)Index);
        }
    }
}
