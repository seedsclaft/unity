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
            var data = (KindType)ListData.Data;
            var kindText = "";
            if (data != KindType.None)
            {
                kindText = DataSystem.GetText((int)data + 500);
            }
            kindName?.SetText(kindText);
        }
    }
}