using UnityEngine;
using System.Collections.Generic;

namespace Ryneus
{
    public class SymbolList : BaseList
    {
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        public List<SymbolInfo> SelectSymbolInfo()
        {
            var data = ListItemData<List<SymbolInfo>>();
            if (data != null)
            {
                return data;
            }
            return null;
        }

        public void UpdateSymbolInfo(SymbolInfo symbolInfo)
        {
            
        }

        public void UpdatePartyInfo(PartyInfo partyInfo)
        {
            partyInfoComponent.UpdateInfo(partyInfo);
        }
    }
}
