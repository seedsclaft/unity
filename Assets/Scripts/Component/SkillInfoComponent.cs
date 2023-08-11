using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.U2D;

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
            if (skillData.SkillType == SkillType.Magic)
            {
                mpCost.text = "(" + skillData.MpCost.ToString() + ")";
            } else
            {
                mpCost.text = "";
            }
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

    private void UpdateSkillIcon(MagicIconType iconIndex)
    {
        icon.gameObject.SetActive(true);
        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/SpellIcons");
        if (icon != null)
        {
            icon.sprite = spriteAtlas.GetSprite(iconIndex.ToString());
        }
    }

    private void UpdateSkillIconBack(AttributeType attributeType)
    {
        iconBack.gameObject.SetActive(true);

        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/SpellIcons");
        if (iconBack != null)
        {
            iconBack.sprite = spriteAtlas.GetSprite(attributeType.ToString());
        }
    }

    private void UpdateLineImege()
    {
        lineImage.gameObject.SetActive(true);
        nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
        lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
    }

    public void SetRebornInfoData(RebornSkillInfo rebornSkillInfo)
    {
        var skillData = DataSystem.Skills.Find(skill => skill.Id == rebornSkillInfo.Id);
        if (nameText != null)
        {
            nameText.text = skillData.Name.Replace("\\d",rebornSkillInfo.Param2.ToString());
            nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
        }
        if (description != null)
        {
            description.text = skillData.Help.Replace("\\d",rebornSkillInfo.Param2.ToString());
        }
    }

    public void Clear()
    {
        if (icon != null)
        {
            icon.gameObject.SetActive(false);
        }
        if (iconBack != null)
        {
            iconBack.gameObject.SetActive(false);
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
