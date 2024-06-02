using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class RuleHelp : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI commandName;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            commandName.text = (string)ListData.Data;
        }
    }
}