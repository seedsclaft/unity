using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsEnemy : ListItem ,IListViewItem 
{
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    [SerializeField] private BaseList getItemList = null;
    [SerializeField] private Button enemyInfoButton;
    private System.Action _enemyInfoHandler = null;
    private System.Action _getItemInfoHandler = null;
    private System.Action<int> _getItemInfoSelectHandler = null;

    private int _getItemIndex = -1;
    private bool _selectable = false;
    public void SetSelectable(bool selectable)
    {
        _selectable = selectable;
    }
    public int GetItemIndex => _getItemIndex;
    public GetItemInfo GetItemInfo()
    {
        if (ListData == null) return null;
        var data = (TroopInfo)ListData.Data;
        return data.GetItemInfos[_getItemIndex];
    }

    public void SetGetItemInfoCallHandler(System.Action handler)
    {
        if (_getItemInfoHandler != null)
        {
            return;
        }
        _getItemInfoHandler = handler;
        getItemList.SetInputHandler(InputKeyType.Decide,() => _getItemInfoHandler());
    }

    public void SetEnemyInfoCallHandler(System.Action handler)
    {
        if (_enemyInfoHandler != null)
        {
            return;
        }
        _enemyInfoHandler = handler;
        enemyInfoButton.onClick.AddListener(() => handler());
    }

    public void SetGetItemInfoSelectHandler(System.Action<int> handler)
    {
        if (_getItemInfoSelectHandler != null)
        {
            return;
        }
        _getItemInfoSelectHandler = handler;
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (TroopInfo)ListData.Data;
        enemyInfoComponent.Clear();
        enemyInfoComponent.UpdateInfo(data.BossEnemy);
        var list = new List<ListData>();
        foreach (var itemInfo in data.GetItemInfos)
        {
            var itemData = new ListData(itemInfo);
            list.Add(itemData);
        }
        getItemList.SetData(list);
        getItemList.SetSelectedHandler(() => {
            _getItemIndex = getItemList.Index;
            if (_getItemInfoSelectHandler != null)
            {
                _getItemInfoSelectHandler(Index);
            }
        });
        SetItemIndex(_getItemIndex);
    }

    public void SetItemIndex(int itemIndex)
    {
        _getItemIndex = itemIndex;
        UpdateItemIndex(0);
    }

    public void UpdateItemIndex(int plusIndex)
    {
        _getItemIndex += plusIndex;
        if (_getItemIndex < -1)
        {
            _getItemIndex = -1;
        }
        if (_getItemIndex >= getItemList.DataCount)
        {
            _getItemIndex = getItemList.DataCount - 1;
        }
        getItemList.Refresh(_getItemIndex);
    }

    private void LateUpdate() {
        Cursor.SetActive(_getItemIndex == -1 && _selectable);
    }
}
