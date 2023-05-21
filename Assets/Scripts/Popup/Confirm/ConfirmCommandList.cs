using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ConfirmCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<ConfirmComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        _data.Clear();
        _data = menuCommands;
        if (menuCommands.Count == 1)
        {
            HorizontalLayoutGroup horizontalLayoutGroup = gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 0;
            horizontalLayoutGroup.padding.left = 0;
        }
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var statusCommand = ObjectList[i].GetComponent<ConfirmCommand>();
            statusCommand.SetData(menuCommands[i],i);
            statusCommand.SetCallHandler(callEvent);
            statusCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            _helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<ConfirmComandType> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent((ConfirmComandType)Index);
        }
        if (keyType == InputKeyType.Cancel)
        {
            callEvent(ConfirmComandType.No);
        }
    }

    public void SetIsNoChoice(bool isNoChoice)
    {
    }
}
