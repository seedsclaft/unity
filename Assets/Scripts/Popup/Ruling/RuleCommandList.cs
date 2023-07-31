using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class RuleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private GameObject mouseBlocker = null;

    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public void Initialize(List<SystemData.MenuCommandData> command,System.Action<ConfirmComandType> callEvent,System.Action cancelEvent)
    {
        mouseBlocker.SetActive(GameSystem.ConfigData._inputType);
        InitializeListView(command.Count);
        // スクロールするものはObjectList.CountでSetSelectHandlerを登録する
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ConfirmCommand skillAction = ObjectList[i].GetComponent<ConfirmCommand>();
            skillAction.SetCallHandler(callEvent);
            skillAction.SetSelectHandler((data) => 
                {
                    UpdateSelectIndex(data);
                    if (callEvent != null && Index > 0)
                    {
                        callEvent((ConfirmComandType)_data[Index].Id);
                    }
                });
            //ObjectList[i].SetActive(false);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent));
        SetMenuCommandDatas(command);
    }

    public void SetMenuCommandDatas(List<SystemData.MenuCommandData> skillInfoData)
    {
        _data.Clear();
        _data = skillInfoData;
        SetDataCount(skillInfoData.Count);
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _data.Count) 
            {
                ConfirmCommand skillAction = ObjectList[i].GetComponent<ConfirmCommand>();
                skillAction.SetData(_data[i],i);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    public void RefreshCostInfo()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<ConfirmComandType> callEvent,System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (callEvent != null && Index > 0)
            {
                callEvent((ConfirmComandType)_data[Index].Id);
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.Option1)
        {
        }
        if (keyType == InputKeyType.Option2)
        {
        }
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                callEvent((ConfirmComandType)_data[Index].Id);
                UpdateScrollRect(keyType,rows,_data.Count);
            }
            if (keyType == InputKeyType.Up)
            {
                callEvent((ConfirmComandType)_data[Index].Id);
                UpdateScrollRect(keyType,rows,_data.Count);
            }
        }
    }

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<ConfirmCommand>();
        skillAction.SetData(_data[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
