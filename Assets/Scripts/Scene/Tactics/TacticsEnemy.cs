using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsEnemy : ListItem ,IListViewItem 
{
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    [SerializeField] private GetItemList getItemList = null;
    private BattlerInfo _enemyInfo; 
    private new void Awake() {
        getItemList.Initialize();
    }

    public void SetData(BattlerInfo data,int index){
        _enemyInfo = data;
        SetIndex(index);
    }

    public void SetGetItemList(List<GetItemInfo> getItemInfos)
    {
        getItemList.Refresh(getItemInfos);
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
        getItemList.gameObject.SetActive(true);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)Index));
    }

    public void UpdateViewItem()
    {
        if (_enemyInfo == null) return;
        enemyInfoComponent.UpdateInfo(_enemyInfo);
    }
}
