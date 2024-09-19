using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class CheckConflictElement : ListItem ,IListViewItem
    {
        [SerializeField] private SymbolComponent mainSymbol;
        [SerializeField] private BaseList mainGetItemInfo;
        [SerializeField] private SymbolComponent brunchSymbol;
        [SerializeField] private BaseList brunchGetItemInfo;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            mainGetItemInfo.Initialize();
            brunchGetItemInfo.Initialize();
            var data = ListItemData<List<SymbolResultInfo>>();
            var mainResultInfo = data[0];
            var brunchResultInfo = data[1];
            mainSymbol.UpdateInfo(mainResultInfo.SymbolInfo,mainResultInfo.Selected,mainResultInfo.Seek);
            brunchSymbol.UpdateInfo(brunchResultInfo.SymbolInfo,brunchResultInfo.Selected,brunchResultInfo.Seek);
        
            mainGetItemInfo.SetData(MakeGetItemListData(mainResultInfo.SymbolInfo));
            brunchGetItemInfo.SetData(MakeGetItemListData(brunchSymbol.SymbolInfo));
        }

        
        private List<ListData> MakeGetItemListData(SymbolInfo symbolInfo)
        {
            var list = new List<ListData>();
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.None)
                {
                    continue;
                }
                var data = new ListData(getItemInfo);
                if (getItemInfo.GetFlag)
                {
                    list.Add(data);
                }
            }
            return list;
        }
    }
}
