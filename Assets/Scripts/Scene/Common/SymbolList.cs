using UnityEngine;
using System.Collections.Generic;

namespace Ryneus
{
    public class SymbolList : BaseList ,IInputHandlerEvent
    {
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private SymbolComponent symbolComponent;

        public SymbolInfo SelectSymbolInfo()
        {
            var symbolInfos = SelectSymbolInfos();
            if (symbolInfos != null)
            {
                return symbolInfos[_selectSeekIndex];
            }
            return null;
        }
        
        private int _selectSeekIndex = 0;
        public void SetSeekIndex(int seekIndex)
        {
            _selectSeekIndex = seekIndex;
            UpdateSelectSeekIndex();
        }

        public new void Initialize()
        {
            base.Initialize();
            symbolComponent.Initialize();
            SetInputHandler(InputKeyType.Right,() => OnSelectSymbolIndex(1));
            SetInputHandler(InputKeyType.Left,() => OnSelectSymbolIndex(-1));
            SetSelectedHandler(UpdateSelectSeekIndex);
        }

        public List<SymbolInfo> SelectSymbolInfos()
        {
            var data = ListItemData<List<SymbolInfo>>();
            if (data != null)
            {
                return data;
            }
            return null;
        }

        private void OnSelectSetSymbolIndex(int selectIndex)
        {
            _selectSeekIndex = selectIndex;
            UpdateSymbolInfo();
            UpdateChild();
        }

        private void OnSelectSymbolIndex(int selectIndex)
        {
            GainSeekIndex(selectIndex);
            //UpdateAllItems();
            UpdateSymbolInfo();
            UpdateChild();
        }

        private void UpdateSelectSeekIndex()
        {
            var symbolInfos = SelectSymbolInfos();
            if (symbolInfos != null && _selectSeekIndex >= symbolInfos?.Count)
            {
                _selectSeekIndex = symbolInfos.Count-1;
            }
            UpdateSymbolInfo();
            UpdateChild();
            var symbolInfos1 = GetComponentsInChildren<SymbolItem>();
            var symbolInfo = SelectSymbolInfo();
            foreach (var symbolItem in symbolInfos1)
            {
                symbolItem.SetSelectedHandler(symbolInfo?.Master.Seek,(a) => OnSelectSetSymbolIndex(a));
            }
        }

        private void GainSeekIndex(int gain)
        {
            var current = _selectSeekIndex;
            if ((current + gain) < 0)
            {
                gain = 0;
            } else
            if ((current + gain) >= SelectSymbolInfos().Count)
            {
                gain = 0;
            }
            _selectSeekIndex = current + gain;
            UpdateChild();
        }

        private void UpdateChild()
        {
            var recordDates = GetComponentsInChildren<SymbolInfos>();
            foreach (var recordDate in recordDates)
            {
                if (recordDate.ListData == null)
                {
                    continue;
                }
                var select = SelectSymbolInfo();
                var symbolInfos = recordDate.ListItemData<List<SymbolInfo>>();
                foreach (var symbolInfo in symbolInfos)
                {
                    symbolInfo.SetLastSelected(select == symbolInfo);
                }
            }
            UpdateAllItems();
        }

        public void UpdateSymbolInfo()
        {
            var symbolInfo = SelectSymbolInfo();
            if (symbolInfo != null)
            {
                symbolComponent.UpdateInfo(symbolInfo);
            }
            symbolComponent.gameObject.SetActive(symbolInfo != null);
        }

        public void UpdatePartyInfo(PartyInfo partyInfo)
        {
            partyInfoComponent.UpdateInfo(partyInfo);
        }
    }
}
