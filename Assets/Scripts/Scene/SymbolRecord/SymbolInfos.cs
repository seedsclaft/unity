using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SymbolInfos : ListItem ,IListViewItem
    {
        [SerializeField] private BaseList symbolList;
        [SerializeField] private TextMeshProUGUI stageDataText;

        private bool _isInitialized = false;
        public void Initialize()
        {
            symbolList.Initialize();
        }
    
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            if (_isInitialized == false)
            {
                Initialize();
                _isInitialized = true;
            }
            var dates = ListItemData<List<SymbolInfo>>();
            symbolList.SetData(ListData.MakeListData(dates));
            if (dates.Count > 0)
            {
                if (dates[0].SymbolType == SymbolType.None)
                {
                    stageDataText?.SetText("");
                } else
                {
                    var stageData = DataSystem.FindStage(dates[0].Master.StageId);
                    stageDataText?.SetText(stageData.StageNo + "-" + dates[0].Master.Seek.ToString());
                }
            }
        }
    }
}