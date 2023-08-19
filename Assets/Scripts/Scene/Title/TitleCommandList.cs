using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();
    public SystemData.MenuCommandData Data {
        get {
            if (Index >= 0)
            {
                return _data[Index];
            }
            return null;
        }
    }
    public void Initialize(List<SystemData.MenuCommandData> menuCommands)
    {
        InitializeListView(rows);
        _data = menuCommands;
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            titleCommand.SetData(menuCommands[i],i);
            titleCommand.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            titleCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void Refresh(int selectIndex = 0)
    {
        UpdateSelectIndex(selectIndex);
        UpdateHelpWindow();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null && Index >= 0)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
            _helpWindow.SetInputInfo("TITLE");
        }
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        for (int i = 0; i < _data.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            titleCommand.SetDisable(menuCommandData,IsDisable);
        }
    }
}
