using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillInfoComponent : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI mpCost;
    [SerializeField] private Image lineImage;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private TextMeshProUGUI description;

    public void SetInfoData(SkillInfo skillInfo){
        if (skillInfo == null){
            return;
        }
        var skillData = DataSystem.Skills.Find(skill => skill.Id == skillInfo.Id);
        
        if (icon != null){
            UpdateSkillIcon(skillData.IconIndex);
        }
        if (name != null){
            name.text = skillData.Name;
            name.rectTransform.sizeDelta = new Vector2(name.preferredWidth,name.preferredHeight);
        }
        if (mpCost != null){
            mpCost.text = "(" + skillData.MpCost.ToString() + ")";
        }
        if (lineImage != null){
            UpdateLineImege();
        }
        if (type != null){
            type.text = skillData.EffectType.ToString();
        }
        if (value != null){
            value.text = skillData.EffectValue.ToString();
        }
        if (description != null){
            description.text = skillData.Help;
        }
    }

    private void UpdateSkillIcon(int iconIndex)
    {

        Addressables.LoadAssetAsync<IList<Sprite>>(
            "Assets/Images/System/IconSet.png"
            //"Assets/Images/System/IconSet_" + iconIndex.ToString()
        ).Completed += op => {
            icon.sprite = op.Result[iconIndex];
        };
    }

    private void UpdateLineImege()
    {
        name.rectTransform.sizeDelta = new Vector2(name.preferredWidth,name.preferredHeight);
        lineImage.rectTransform.sizeDelta = new Vector2(name.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
    }
}
