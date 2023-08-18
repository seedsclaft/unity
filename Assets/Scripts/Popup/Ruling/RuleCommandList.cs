using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class RuleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;

    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();
    public SystemData.MenuCommandData Data {
        get 
        {
            if (Index >= 0) 
            {
                return _data[Index];
            }
            return null;
        }
    }

    public void Initialize(List<SystemData.MenuCommandData> command,System.Action callEvent)
    {
        InitializeListView(command.Count);
        // スクロールするものはObjectList.CountでSetSelectHandlerを登録する
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ConfirmCommand skillAction = ObjectList[i].GetComponent<ConfirmCommand>();
            //skillAction.SetCallHandler();
            skillAction.SetSelectHandler((data) => 
                {
                    UpdateSelectIndex(data);
                    if (callEvent != null)
                    {
                        callEvent();
                    }
                });
            //ObjectList[i].SetActive(false);
        }
        SetInputCallHandler((a) => CallInputHandler(a));
        SetMenuCommandDatas(command);
    }

    public void SetMenuCommandDatas(List<SystemData.MenuCommandData> skillInfoData)
    {
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

    private void CallInputHandler(InputKeyType keyType)
    {
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                UpdateScrollRect(keyType,rows,_data.Count);
            }
            if (keyType == InputKeyType.Up)
            {
                UpdateScrollRect(keyType,rows,_data.Count);
            }
        }
    }
}
