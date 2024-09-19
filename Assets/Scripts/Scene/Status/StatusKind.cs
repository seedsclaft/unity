using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StatusKind : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI kindName;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<KindType>();
            var kindText = "";
            if (data != KindType.None)
            {
                kindText = DataSystem.GetText((int)data + 18800);
            }
            kindName?.SetText(kindText);
        }
    }
}