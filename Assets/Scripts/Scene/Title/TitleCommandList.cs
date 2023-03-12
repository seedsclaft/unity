using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TitleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<TitleComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        _data = menuCommands;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            titleCommand.SetData(menuCommands[i],i);
            titleCommand.SetCallHandler(callEvent);
            titleCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null && Index > 0)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<TitleComandType> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent((TitleComandType)Index);
        }
    }
}
