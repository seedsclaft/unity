using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuStage : ListItem ,IListViewItem  
{   
    [SerializeField] private StageInfoComponent component;
    
    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (StageInfo)ListData.Data;
        component.UpdateInfo(data);
    }
}
