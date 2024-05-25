using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class TacticsSymbol : ListItem ,IListViewItem 
    {
        [SerializeField] private SymbolComponent symbolComponent;
        [SerializeField] private BaseList getItemList = null;
        public BaseList GetItemList => getItemList;
        [SerializeField] private Button enemyInfoButton;
        private System.Action<int> _enemyInfoHandler = null;
        private System.Action _getItemInfoHandler = null;
        private System.Action<int> _getItemInfoSelectHandler = null;
        [SerializeField] private GameObject selected = null;

        private int _getItemIndex = -1;
        private bool _selectable = false;
        public bool Selectable => _selectable;

        private bool _getItemInit = false;
        public void SetSelectable(bool selectable)
        {
            _selectable = selectable;
        }
        public int GetItemIndex => _getItemIndex;
        public GetItemInfo GetItemInfo()
        {
            if (ListData == null) return null;
            var data = (SymbolResultInfo)ListData.Data;
            var convert = MakeGetItemListData(data.SymbolInfo);
            var getItemInfo = convert[getItemList.Index];
            return (GetItemInfo)getItemInfo.Data;
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
            var data = (SymbolResultInfo)ListData.Data;
            symbolComponent.UpdateInfo(data.SymbolInfo,data.Selected,data.Seek);
            if (_getItemInit == false)
            {
                getItemList.Initialize();
            }
            getItemList.SetData(MakeGetItemListData(data.SymbolInfo),false);
            if (_getItemInit == false)
            {
                getItemList.SetSelectedHandler(() => 
                {
                    _getItemIndex = getItemList.Index;
                    if (_getItemInfoSelectHandler != null)
                    {
                        if (_getItemIndex != -1)
                        {
                            _getItemInfoSelectHandler(Index);
                        }
                    }
                });
                _getItemInit = true;
            }
            UpdateItemIndex(_getItemIndex);
            UpdateSelected(data.Selected);
            Cursor?.SetActive(ListData.Enable);
            //UpdateCleared(data);
        }

        private List<ListData> MakeGetItemListData(SymbolInfo symbolInfo)
        {
            var list = new List<ListData>();
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {
                var data = new ListData(getItemInfo);
                data.SetEnable(symbolInfo.Cleared != true || getItemInfo.GetItemType != GetItemType.Numinous);
                list.Add(data);
            }
            return list;
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

        private void UpdateSelected(bool select)
        {
            selected.SetActive(select);
        }

        private void UpdateCleared(SymbolInfo symbolInfo)
        {
            if (selected == null) return;
            //cleared.SetActive(symbolInfo.Cleared);
        }

        private void LateUpdate() 
        {
            Cursor.SetActive(_getItemIndex == -1 && _selectable && ListData.Enable);
        }
    }
}