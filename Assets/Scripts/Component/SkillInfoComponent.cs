using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillInfoComponent : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image iconBack;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mpCost;
    [SerializeField] private Image lineImage;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject selectable;

    public void SetInfoData(SkillInfo skillInfo){
        if (skillInfo == null){
            Clear();
            return;
        }
        UpdateSkillData(skillInfo.Id);
        if (selectable != null)
        {
            selectable.SetActive(skillInfo.LearningState == LearningState.SelectLearn);
        }
    }

    public void UpdateSkillData(int skillId)
    {
        if (skillId == 0)
        {
            Clear();
            return;
        }
        var skillData = DataSystem.Skills.Find(skill => skill.Id == skillId);
        if (icon != null)
        {
            UpdateSkillIcon(skillData.IconIndex);
        }
        if (iconBack != null)
        {
            UpdateSkillIconBack(skillData.Attribute);
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

    private async void UpdateSkillIcon(MagicIconType iconIndex)
    {
        icon.gameObject.SetActive(true);
        var handle = await ResourceSystem.LoadAsset<Sprite>("Spells/" + iconIndex.ToString());
        if (icon != null)
        {
            icon.sprite = handle;
        }
    }

    private async void UpdateSkillIconBack(AttributeType attributeType)
    {
        iconBack.gameObject.SetActive(true);
        var handle = await ResourceSystem.LoadAsset<Sprite>("Spells/" + attributeType.ToString());
        if (iconBack != null)
        {
            iconBack.sprite = handle;
        }
    }

    private void UpdateLineImege()
    {
        lineImage.gameObject.SetActive(true);
        nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
        lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
    }

    private void Clear()
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
