using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolRecordData : ListItem ,IListViewItem
{
    [SerializeField] private List<SymbolComponent> symbolComponents;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var dates = (List<SymbolInfo>)ListData.Data;
        foreach (var symbolComponent in symbolComponents)
        {
            symbolComponent.gameObject.SetActive(false);
        }
        foreach (var data in dates)
        {
            var symbolComponent = symbolComponents[data.StageSymbolData.SeekIndex];
            symbolComponent.gameObject.SetActive(true);
            symbolComponent.UpdateInfo(data);
        }
    }

    public void SetSymbolItemCallHandler(System.Action<SymbolInfo> handler)
    {
        foreach (var symbolComponent in symbolComponents)
        {
            var button = symbolComponent.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => handler(symbolComponent.SymbolInfo));
        }
    }

}
