using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class TacticsRecovery : ListItem ,IListViewItem 
{
    [SerializeField] private TacticsComponent tacticsComponent;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private ActorInfo _data;
    public void SetData(ActorInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)_data.ActorId));
        tacticsComponent.SetToggleHandler(handler);
    }

    public void SetPlusHandler(System.Action<int> handler)
    {
        plusButton.onClick.AddListener(() => handler((int)_data.ActorId));
    }

    public void SetMinusHandler(System.Action<int> handler)
    {
        minusButton.onClick.AddListener(() => handler((int)_data.ActorId));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        tacticsComponent.UpdateInfo(_data,TacticsComandType.Recovery);
        Disable.SetActive(!_data.EnableTactics(TacticsComandType.Recovery));
    }
}
