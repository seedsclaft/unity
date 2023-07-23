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
        InitializeListView(rows);
        conditionButton.onClick.AddListener(() => conditionEvent());
    }

    public void Refresh(List<StateInfo> stateInfos ,System.Action cancelEvent,System.Action skillEvent)
    {
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
        SetInputHandler((a) => CallInputHandler(a,cancelEvent,skillEvent));
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
    
    private void CallInputHandler(InputKeyType keyType,System.Action cancelEVent,System.Action skillEvent)
    {
        if (keyType == InputKeyType.Cancel)
        {
            cancelEVent();
        }
        if (keyType == InputKeyType.Option1)
        {
            if (skillEvent != null)
            {
                skillEvent();
            }
        }
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
