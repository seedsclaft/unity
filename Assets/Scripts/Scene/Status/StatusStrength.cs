using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusStrength : ListItem ,IListViewItem 
{
    
    [SerializeField] private StrengthComponent strengthComponent;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private ActorInfo _data;
    public void SetData(ActorInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public int listIndex(){
        return Index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(Index));
    }

    public void SetPlusHandler(System.Action<int> handler)
    {
        plusButton.onClick.AddListener(() => handler(Index));
    }

    public void SetMinusHandler(System.Action<int> handler)
    {
        minusButton.onClick.AddListener(() => handler(Index));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        strengthComponent.UpdateInfo(_data,Index);
    }
}
