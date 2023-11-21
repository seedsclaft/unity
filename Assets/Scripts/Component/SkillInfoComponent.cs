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
    [SerializeField] private bool nameAndMpCost;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mpCost;
    [SerializeField] private Image lineImage;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI range;
    [SerializeField] private TextMeshProUGUI learningCost;
    [SerializeField] private GameObject selectable;

    public void UpdateSkillInfo(SkillInfo skillInfo){
        if (skillInfo == null){
            Clear();
            return;
        }
        if (skillInfo.Master.SkillType == SkillType.Reborn)
        {
            UpdateRebornInfo(skillInfo);
        } else
        {
            UpdateSkillData(skillInfo.Id);
        }
        if (selectable != null)
        {
            selectable.SetActive(skillInfo.LearningState == LearningState.SelectLearn);
        }
        if (learningCost != null)
        {
            learningCost.gameObject.SetActive(skillInfo.LearningCost > 0);
            learningCost.text = skillInfo.LearningCost.ToString();
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
            if (nameAndMpCost)
            {
                nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
            }
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
            UpdateLineImage();
        }
        if (type != null)
        {
            type.text = skillData.SkillType.ToString();
        }
        if (description != null)
        {
            description.text = skillData.ConvertHelpText(skillData.Help);
        }
        if (range != null)
        {
            var rangeTextId = skillData.Range == RangeType.S ? 351 : 352;
            range.text = DataSystem.System.GetTextData(rangeTextId).Text;
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

    private void UpdateLineImage()
    {
        lineImage.gameObject.SetActive(true);
        if (nameAndMpCost)
        {
            nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
            lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
        }
    }

    public void UpdateRebornInfo(SkillInfo rebornSkillInfo)
    {
        var skillData = DataSystem.Skills.Find(skill => skill.Id == rebornSkillInfo.Id);
        if (nameText != null)
        {
            nameText.text = skillData.Name.Replace("\\d",rebornSkillInfo.Param1);
            if (nameAndMpCost)
            {
                nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
            }
        }
        if (description != null)
        {
            description.text = skillData.Help.Replace("\\d",rebornSkillInfo.Param1);
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
        if (learningCost != null)
        {
            learningCost.gameObject.SetActive(false);
            learningCost.text = "";
        }
    }
}
