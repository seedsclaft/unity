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

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<TacticsComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        _data = menuCommands;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var TacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            TacticsCommand.SetData(menuCommands[i],i);
            TacticsCommand.SetCallHandler(callEvent);
            TacticsCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllItems();
        UpdateSelectIndex(-1);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            tacticsCommand.SetDisable(menuCommandData,IsDisable);
        }
    }
    private void CallInputHandler(InputKeyType keyType, System.Action<TacticsComandType> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            TacticsCommand tacticsCommand = ObjectList[Index].GetComponent<TacticsCommand>();
            if (tacticsCommand.Disable.gameObject.activeSelf) return;
            callEvent((TacticsComandType)_data[Index].Id);
        }
    }
}
