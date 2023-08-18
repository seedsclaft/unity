using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<TitleComandType> callEvent,System.Action optionEvent)
    {
        InitializeListView(rows);
        _data = menuCommands;
        for (int i = 0; i < menuCommands.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            titleCommand.SetData(menuCommands[i],i);
            titleCommand.SetCallHandler(callEvent);
            titleCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent,optionEvent));
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

    private void CallInputHandler(InputKeyType keyType, System.Action<TitleComandType> callEvent,System.Action optionEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent((TitleComandType)Index);
        }
        if (keyType == InputKeyType.Option1)
        {
            optionEvent();
        }
    }

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var titleCommand = gameObject.GetComponent<TitleCommand>();
        titleCommand.SetData(_data[itemIndex],itemIndex);
        titleCommand.UpdateViewItem();
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
