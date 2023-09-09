using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BaseCommandList : ListWindow , IInputHandlerEvent
{
    private List<SystemData.CommandData> _data = new List<SystemData.CommandData>();

    public SystemData.CommandData Data 
    { 
        get {
            if (Index > -1)
            {
                return _data[Index];
            }
            return null;
        }
    }

    public void Initialize(List<SystemData.CommandData> baseCommands)
    {
        InitializeListView(baseCommands.Count);
        _data = baseCommands;
        /*
        if (menuCommands.Count == 1)
        {
            HorizontalLayoutGroup horizontalLayoutGroup = gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 0;
            horizontalLayoutGroup.padding.left = 0;
        }
        */
        for (int i = 0; i < baseCommands.Count;i++)
        {
            var baseCommand = ObjectList[i].GetComponent<BaseCommand>();
            baseCommand.SetData(baseCommands[i],i);
            baseCommand.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            baseCommand.SetSelectHandler((data) => 
            {
                UpdateSelectIndex(data);
                //CallListInputHandler(InputKeyType.Down);
            });
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
    
    public void Refresh(int selectIndex = 0)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var confirmCommand = ObjectList[i].GetComponent<BaseCommand>();
            confirmCommand.SetData(_data[i],i);
        }
        //ResetScrollPosition();
        UpdateSelectIndex(selectIndex);
        UpdateAllItems();
    }
    
    public void SetDisable(SystemData.CommandData commandData,bool IsDisable)
    {
        for (int i = 0; i < _data.Count;i++)
        {
            var baseCommand = ObjectList[i].GetComponent<BaseCommand>();
            baseCommand.SetDisable(commandData,IsDisable);
        }
    }
}
