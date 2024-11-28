using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SymbolRecordData : ListItem ,IListViewItem
    {
        [SerializeField] private List<SymbolComponent> symbolComponents;
        [SerializeField] private TextMeshProUGUI stageDataText;
        private int _seekIndex = -1;
        public void SetSeekIndex(int seekIndex) => _seekIndex = seekIndex;

        private bool _isButtonInit = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var dates = ListItemData<List<SymbolInfo>>();
            foreach (var symbolComponent in symbolComponents)
            {
                symbolComponent.gameObject.SetActive(false);
            }
            var idx = 0;
            foreach (var data in dates)
            {
                if (symbolComponents.Count > 0)
                {
                    var symbolComponent = symbolComponents[data.Master.SeekIndex];
                    symbolComponent.gameObject.SetActive(true);
                    symbolComponent.UpdateInfo(data);
                }
                idx++;
            }
            if (dates.Count > 0)
            {
                if (dates[0].SymbolType == SymbolType.None)
                {
                    stageDataText?.SetText("");
                } else
                {
                    stageDataText?.SetText(dates[0].Master.StageId.ToString() + "-" + dates[0].Master.Seek.ToString());
                }
            }
        }

        public void SetSymbolItemCallHandler(System.Action<int> handler)
        {
            if (_isButtonInit) return;
            _isButtonInit = true;
            foreach (var symbolComponent in symbolComponents)
            {
                var button = symbolComponent.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => 
                {
                    if (symbolComponent?.SymbolInfo?.SymbolType == SymbolType.None)
                    {
                        return;
                    }
                    handler(symbolComponent.Seek);
                });
            }
        }

        void Update() 
        {
            if (Cursor != null)
            {
                var currentSeekIndex = _seekIndex;
                var idx = 0;
                foreach (var symbolComponent in symbolComponents)
                {
                    symbolComponent.UpdateCursor(Cursor.activeSelf && currentSeekIndex == idx);
                    idx++;
                }
            }
        }
    }
}