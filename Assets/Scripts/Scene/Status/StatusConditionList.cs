using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusConditionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private Button conditionButton = null;
    [SerializeField] private GameObject mainView = null;

    private List<StateInfo> _stateInfos = new List<StateInfo>();

    public void Initialize(System.Action conditionEvent)
    {
        conditionButton.onClick.AddListener(() => conditionEvent());
    }

    public void Refresh(List<StateInfo> stateInfos)
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
        SetInputCallHandler((a) => CallSelectHandler(a));
        UpdateAllItems();
        UpdateSelectIndex(0);
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
