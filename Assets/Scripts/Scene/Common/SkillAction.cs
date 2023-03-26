using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillAction : ListItem ,IListViewItem  
{
    [SerializeField] private SkillInfoComponent skillInfoComponent;
    private SkillInfo _data; 
    public void SetData(SkillInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() =>
            {
                if (Disable.gameObject.activeSelf) return;
                handler((int)_data.Id);
            }
        );
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        skillInfoComponent.SetInfoData(_data);
        Disable.SetActive(_data.Enabel == false);
    }
}
