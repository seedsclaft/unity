using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TacticsCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<TacticsComandType> callEvent,System.Action alcanaEvent = null)
    {
        InitializeListView(menuCommands.Count);
        _data = menuCommands;
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var TacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            TacticsCommand.SetData(menuCommands[i],i);
            TacticsCommand.SetCallHandler(callEvent);
            TacticsCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,alcanaEvent));
        UpdateAllItems();
        UpdateSelectIndex(-1);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null && Index >= 0)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        for (int i = 0; i < _data.Count;i++)
        {
            var tacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            tacticsCommand.SetDisable(menuCommandData,IsDisable);
        }
    }
    private void CallInputHandler(InputKeyType keyType, System.Action<TacticsComandType> callEvent,System.Action alcanaEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            TacticsCommand tacticsCommand = ObjectList[Index].GetComponent<TacticsCommand>();
            if (tacticsCommand.Disable.gameObject.activeSelf) return;
            callEvent((TacticsComandType)_data[Index].Id);
        }
        if (keyType == InputKeyType.Option1)
        {
            alcanaEvent();
        }
    }
}
