using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolRecordData : ListItem ,IListViewItem
{
    [SerializeField] private SymbolComponent symbolComponent;
    [SerializeField] private BaseList getItemList = null;

    private bool _getItemInit = false;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (SymbolInfo)ListData.Data;
        symbolComponent.UpdateInfo(data);
        if (data.GetItemInfos.Count == 0)
        {
            return;
        }
        if (_getItemInit == false)
        {
            getItemList.Initialize();
            _getItemInit = true;
        }
        getItemList.SetData(ListData.MakeListData(data.GetItemInfos));
        getItemList.UpdateSelectIndex(-1);
    }


}
