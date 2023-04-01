using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TacticsCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _menuCommands = new List<SystemData.MenuCommandData>();

    public void Initialize(System.Action<TacticsComandType> callEvent,System.Action alcanaEvent = null)
    {
        InitializeListView(cols);
        for (int i = 0; i < cols;i++)
        {
            var TacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            TacticsCommand.SetCallHandler(callEvent);
            TacticsCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,alcanaEvent));
    }

    public void Refresh(List<SystemData.MenuCommandData> menuCommands)
    {
        _menuCommands = menuCommands;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var TacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            if (i < _menuCommands.Count)
            {
                TacticsCommand.SetData(_menuCommands[i],i);
            }
            ObjectList[i].SetActive(i < _menuCommands.Count);
        }
        UpdateAllItems();
        UpdateSelectIndex(-1);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null && Index >= 0)
        {
            _helpWindow.SetHelpText(_menuCommands[Index].Help);
        }
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        for (int i = 0; i < _menuCommands.Count;i++)
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
            callEvent((TacticsComandType)_menuCommands[Index].Id);
        }
        if (keyType == InputKeyType.Option1)
        {
            alcanaEvent();
        }
    }
}
