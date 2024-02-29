using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class SymbolRecordData : ListItem ,IListViewItem
    {
        [SerializeField] private List<SymbolComponent> symbolComponents;

        private bool _isButtonInit = false;
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
                if (symbolComponents.Count > 0)
                {
                    var symbolComponent = symbolComponents[data.StageSymbolData.SeekIndex];
                    symbolComponent.gameObject.SetActive(true);
                    symbolComponent.UpdateInfo(data);
                }
            }
        }

        public void SetSymbolItemCallHandler(System.Action<SymbolInfo> handler)
        {
            if (_isButtonInit) return;
            _isButtonInit = true;
            foreach (var symbolComponent in symbolComponents)
            {
                var button = symbolComponent.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => handler(symbolComponent.SymbolInfo));
            }
        }

    }
}