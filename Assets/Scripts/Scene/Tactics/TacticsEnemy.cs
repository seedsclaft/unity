using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsEnemy : ListItem ,IListViewItem 
{
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    [SerializeField] private GetItemList getItemList = null;
    [SerializeField] private Button enemyInfoButton;
    private BattlerInfo _enemyInfo;
    private bool _cursorDeactive = false;
    private System.Action _getItemHandler = null;
    private System.Action<TacticsEnemy,int> _itemSelectHandler = null;

    public void SetData(BattlerInfo data,int index){
        _enemyInfo = data;
        SetIndex(index);
    }

    public void SetGetItemList(List<GetItemInfo> getItemInfos)
    {
        getItemList.Refresh(getItemInfos);
        if (_getItemHandler != null)
        {
            foreach (var gameObjectList in getItemList.ObjectList)
            {
                GetItem getItem = gameObjectList.GetComponent<GetItem>();
                getItem.SetCallHandler((a) => {
                    _getItemHandler();
                });
            }
        }
        if (_itemSelectHandler != null)
        {
            foreach (var gameObjectList in getItemList.ObjectList)
            {
                GetItem getItem = gameObjectList.GetComponent<GetItem>();
                getItem.SetSelectHandler((a) => {
                    _itemSelectHandler(this,a);
                });
            }
        }
        getItemList.gameObject.SetActive(true);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)Index));
    }

    public void SetGetItemCallHandler(System.Action handler)
    {
        _getItemHandler = handler;
        /*
        foreach (var gameObjectList in getItemList.ObjectList)
        {
            GetItem getItem = gameObjectList.GetComponent<GetItem>();
            getItem.SetCallHandler((a) => {
                if (a.IsSkill() || a.IsAttributeSkill()){
                    handler(a);
                }
            });
        }
        */
    }

    public void SetGetItemSelectHandler(System.Action<TacticsEnemy,int> handler)
    {
        _itemSelectHandler = handler;
        /*
        foreach (var gameObjectList in getItemList.ObjectList)
        {
            GetItem getItem = gameObjectList.GetComponent<GetItem>();
            getItem.SetSelectHandler((a) => {
                handler(this,a);
            });
        }
        */
    }

    public void SetEnemyInfoCallHandler(System.Action handler)
    {
        enemyInfoButton.onClick.AddListener(() => handler());
    }

    public void UpdateViewItem()
    {
        if (_enemyInfo == null) return;
        enemyInfoComponent.Clear();
        enemyInfoComponent.UpdateInfo(_enemyInfo);
    }

    public void SetSelectGetItem(int getItemIndex)
    {
        getItemList.UpdateSelectIndex(getItemIndex);
        if (getItemIndex >= 0)
        {
            _cursorDeactive = true;
        }
    }

    private void LateUpdate() {
        if (_cursorDeactive == true)
        {
            _cursorDeactive = false;
            Cursor.SetActive(false);
        }
    }
}
