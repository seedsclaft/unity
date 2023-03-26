using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Effekseer;
using DG.Tweening;

public class BattleActor : ListItem ,IListViewItem  
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    public BattlerInfoComponent BattlerInfoComponent{get { return battlerInfoComponent;}}
    private BattlerInfo _data; 

    public void SetData(BattlerInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetDamageRoot(GameObject damageRoot)
    {
        battlerInfoComponent.SetDamageRoot(damageRoot);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)_data.Index));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        battlerInfoComponent.UpdateInfo(_data);
        battlerInfoComponent.RefreshStatus();
    }
}
