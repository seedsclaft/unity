using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleCommandList : ListWindow , IInputHandlerEvent
{
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();
    public SystemData.MenuCommandData Data {
        get 
        {
            if (Index >= 0) 
            {
                return _data[Index];
            }
            return null;
        }
    }

    public void Initialize(List<SystemData.MenuCommandData> command)
    {
        _data = command;
        InitializeListView(command.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var confirmCommand = ObjectList[i].GetComponent<ConfirmCommand>();
            confirmCommand.SetSelectHandler((data) => 
            {
                UpdateSelectIndex(data);
                CallListInputHandler(InputKeyType.Down);
            });
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var confirmCommand = ObjectList[i].GetComponent<ConfirmCommand>();
            confirmCommand.SetData(_data[i],i);
        }
        //ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }
}
