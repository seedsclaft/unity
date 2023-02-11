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
        for (var i = 0; i < menuCommands.Count;i++){
            _data.Add(menuCommands[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var TacticsCommand = ObjectList[i].GetComponent<TacticsCommand>();
            TacticsCommand.SetData(menuCommands[i],i);
            TacticsCommand.SetCallHandler(callEvent);
            TacticsCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }
}
