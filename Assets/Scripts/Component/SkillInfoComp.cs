using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoComp : MonoBehaviour
{
    [SerializeField] private Text Name;
    [SerializeField] private Text Mp;
    private SkillInfo _skillInfo;
    public void UpdateInfo(SkillInfo skillInfo)
    {
        if (skillInfo == null){
            return;
        }
        _skillInfo = skillInfo;

        var skillData = DataSystem.Skills.Find(skill => skill.Id == _skillInfo.SkillId);
        
        if (Name != null){
            Name.text = skillData.Name;
        }

        if (Mp != null){
            Mp.text = skillData.Mp.ToString();
        }
        
    }


}
