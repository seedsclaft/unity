using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillInfoComponent : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mpCost;
    [SerializeField] private Image lineImage;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI learingcost;

    public void SetInfoData(SkillInfo skillInfo){
        if (skillInfo == null){
            Claer();
            return;
        }
        UpdateSkillData(skillInfo.Id);
        if (learingcost != null)
        {
            learingcost.text = skillInfo.LearingCost.ToString();
        }
    }

    public void UpdateSkillData(int skillId)
    {
        if (skillId == 0)
        {
            Claer();
            return;
        }
        var skillData = DataSystem.Skills.Find(skill => skill.Id == skillId);
        if (icon != null)
        {
            UpdateSkillIcon(skillData.IconIndex);
        }
        if (nameText != null)
        {
            nameText.text = skillData.Name;
            nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
        }
        if (mpCost != null)
        {
            mpCost.text = "(" + skillData.MpCost.ToString() + ")";
        }
        if (lineImage != null)
        {
            UpdateLineImege();
        }
        if (type != null)
        {
            type.text = skillData.SkillType.ToString();
        }
        if (description != null)
        {
            description.text = skillData.Help;
        }
    }

    private void UpdateSkillIcon(int iconIndex)
    {
        icon.gameObject.SetActive(true);
        Addressables.LoadAssetAsync<IList<Sprite>>(
            "Assets/Images/System/IconSet.png"
            //"Assets/Images/System/IconSet_" + iconIndex.ToString()
        ).Completed += op => {
            icon.sprite = op.Result[iconIndex];
        };
    }

    private void UpdateLineImege()
    {
        lineImage.gameObject.SetActive(true);
        nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
        lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
    }

    private void Claer()
    {
        if (icon != null)
        {
            icon.gameObject.SetActive(false);
        }
        if (nameText != null)
        {
            nameText.text = "";
        }
        if (mpCost != null)
        {
            mpCost.text = "";
        }
        if (lineImage != null)
        {
            lineImage.gameObject.SetActive(false);
        }
        if (type != null)
        {
            type.text = "";
        }
        if (description != null)
        {
            description.text = "";
        }
    }
}
