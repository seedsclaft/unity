using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsTrain : ListItem ,IListViewItem 
{
    [SerializeField] private TacticsComponent tacticsComponent;

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

    public void UpdateViewItem()
    {
        if (_data == null) return;
        tacticsComponent.UpdateInfo(_data,TacticsComandType.Train);
        Disable.SetActive(!_data.EnableTactics(TacticsComandType.Train));
    }
}
