using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusConditionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private Button conditionButton = null;
    [SerializeField] private GameObject mainView = null;

    private List<StateInfo> _stateInfos = new List<StateInfo>();

    public void Initialize(System.Action conditionEvent)
    {
        conditionButton.onClick.AddListener(() => conditionEvent());
    }

    public void Refresh(List<StateInfo> stateInfos ,System.Action cancelEvent,System.Action optionEvent,System.Action skillEvent)
    {
        InitializeListView(stateInfos.Count);
        _stateInfos = stateInfos;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _stateInfos.Count) 
            {
                var statusCondition = ObjectList[i].GetComponent<StatusCondition>();
                statusCondition.SetData(_stateInfos[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        SetInputCallHandler((a) => CallInputHandler(a,cancelEvent,optionEvent,skillEvent));
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void Refresh()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    private void CallInputHandler(InputKeyType keyType,System.Action cancelEVent,System.Action optionEvent,System.Action skillEvent)
    {
        if (keyType == InputKeyType.Cancel)
        {
            cancelEVent();
        }
        if (keyType == InputKeyType.Option1)
        {
            if (optionEvent != null)
            {
                optionEvent();
            } else
            if (skillEvent != null)
            {
                skillEvent();
            }
        }
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                UpdateScrollRect(keyType,4,_stateInfos.Count);
            }
            if (keyType == InputKeyType.Up)
            {
                UpdateScrollRect(keyType,4,_stateInfos.Count);
            }
        }
    }

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<StatusCondition>();
        skillAction.SetData(_stateInfos[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }

    public void ShowMainView()
    {
        mainView.SetActive(true);
    }

    public void HideMainView()
    {
        mainView.SetActive(false);
    }
}
