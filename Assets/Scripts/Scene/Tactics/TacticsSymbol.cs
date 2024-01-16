using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsSymbol : ListItem ,IListViewItem 
{
    [SerializeField] private SymbolComponent symbolComponent;
    [SerializeField] private BaseList getItemList = null;
    public BaseList GetItemList => getItemList;
    [SerializeField] private Button enemyInfoButton;
    private System.Action<int> _enemyInfoHandler = null;
    private System.Action _getItemInfoHandler = null;
    private System.Action<int> _getItemInfoSelectHandler = null;

    private int _getItemIndex = -1;
    private bool _selectable = false;
    public bool Selectable => _selectable;
    public void SetSelectable(bool selectable)
    {
        _selectable = selectable;
    }
    public int GetItemIndex => _getItemIndex;
    public GetItemInfo GetItemInfo()
    {
        if (ListData == null) return null;
        var data = (SymbolInfo)ListData.Data;
        return data.GetItemInfos[getItemList.Index];
    }

    public void SetGetItemInfoCallHandler(System.Action handler)
    {
        if (_getItemInfoHandler != null)
        {
            return;
        }
        _getItemInfoHandler = handler;
        getItemList.SetInputHandler(InputKeyType.Decide,() => 
        {
            _getItemInfoHandler();
        });
    }

    public void SetSymbolInfoCallHandler(System.Action<int> handler)
    {
        if (_enemyInfoHandler != null)
        {
            return;
        }
        _enemyInfoHandler = handler;
        enemyInfoButton.onClick.AddListener(() => handler(Index));
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
        var data = (SymbolInfo)ListData.Data;
        symbolComponent.UpdateInfo(data);
        if (data.GetItemInfos.Count == 0)
        {
            return;
        }
        getItemList.SetData(ListData.MakeListData(data.GetItemInfos));
        getItemList.SetSelectedHandler(() => {
            _getItemIndex = getItemList.Index;
            if (_getItemInfoSelectHandler != null)
            {
                if (_getItemIndex != -1)
                {
                    _getItemInfoSelectHandler(Index);
                }
            }
        });
        UpdateItemIndex(_getItemIndex);
    }

    public void UpdateItemIndex(int getItemIndex)
    {
        _getItemIndex = getItemIndex;
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
