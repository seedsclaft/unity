using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class RuleHelp : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        commandName.text = (string)ListData.Data;
    }
}
