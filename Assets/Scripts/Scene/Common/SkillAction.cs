using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillAction : ListItem ,IListViewItem  
{
    [SerializeField] private SkillInfoComponent skillInfoComponent;
    [SerializeField] private GameObject DisableSkill;
    private SkillInfo _skillInfo; 
    public void SetData(SkillInfo data,int index){
        _skillInfo = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() =>
            {
                if (Disable.gameObject.activeSelf) return;
                handler((int)_skillInfo.Id);
            }
        );
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (SkillInfo)ListData.Data;
        skillInfoComponent.SetInfoData(data);
        if (DisableSkill != null) DisableSkill.SetActive(data.Enable == false);
    }
}
