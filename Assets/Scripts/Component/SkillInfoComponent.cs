using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
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
        [SerializeField] private TextMeshProUGUI learningText;
        [SerializeField] private GameObject selectable;
        [SerializeField] private GameObject selectedAlcana;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;

        public void UpdateSkillInfo(SkillInfo skillInfo){
            if (skillInfo == null){
                Clear();
                return;
            }
            if (skillInfo.Master.SkillType == SkillType.Reborn)
            {
                UpdateSkillData(skillInfo.Id);
                UpdateRebornInfo(skillInfo);
            } else
            {
                UpdateSkillData(skillInfo.Id);
            }
            if (skillInfo.Master.SkillType == SkillType.UseAlcana)
            {
                if (skillInfo.Master.FeatureDates.Find(a => a.FeatureType == FeatureType.AddSkillOrCurrency) != null)
                {
                    var convertText = skillInfo.Master.ConvertHelpText(skillInfo.Master.Help);
                    var skillNameData = DataSystem.FindSkill(skillInfo.Param1);
                    if (skillNameData != null)
                    {
                        description.text = convertText.Replace("\\d",skillNameData.Name);
                    }
                }
            }
            if (selectable != null)
            {
                selectable.SetActive(skillInfo.LearningState == LearningState.SelectLearn);
            }
            if (learningCost != null)
            {
                learningCost.gameObject.SetActive(skillInfo.LearningCost > 0);
                learningCost.text = skillInfo.LearningCost.ToString();// + DataSystem.System.GetTextData(1000).Text;
            }
            if (learningText != null)
            {
                learningText.transform.parent.gameObject.SetActive(skillInfo.LearningState == LearningState.NotLearn);
                learningText.SetText(DataSystem.GetReplaceText(380,skillInfo.LearningLv.ToString()));
            }
            if (shinyReflect != null)
            {
                shinyReflect.enabled = skillInfo.SelectedAlcana;
            }
            if (selectedAlcana != null)
            {
                selectedAlcana.SetActive(skillInfo.SelectedAlcana);
            }
        }

        public void UpdateSkillData(int skillId)
        {
            if (skillId == 0)
            {
                Clear();
                return;
            }
            var skillData = DataSystem.FindSkill(skillId);
            if (skillData != null)
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(true);
                    UpdateSkillIcon(skillData.IconIndex);
                }
                if (iconBack != null)
                {
                    iconBack.gameObject.SetActive(true);
                    UpdateSkillIconBack(skillData.Attribute);
                }
            } else
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(false);
                }
                if (iconBack != null)
                {
                    iconBack.gameObject.SetActive(false);
                }
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
                if (skillData.SkillType == SkillType.Active)
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
                var convertText = skillData.ConvertHelpText(skillData.Help);
                if (skillData.FeatureDates.Find(a => a.FeatureType == FeatureType.AddSkillOrCurrency) != null)
                {
                    //description.text = convertText.Replace("\\d",replace);
                } else
                {
                    description.text = convertText;
                }
            }
            if (range != null)
            {
                range.gameObject.SetActive(true);
                var rangeTextId = skillData.Range == RangeType.S ? 351 : 352;
                range.text = DataSystem.GetText(rangeTextId);
            }
        }

        private void UpdateSkillIcon(MagicIconType iconIndex)
        {
            icon.gameObject.SetActive(true);
            var spriteAtlas = ResourceSystem.LoadSpellIcons();
            if (icon != null)
            {
                icon.sprite = spriteAtlas.GetSprite(iconIndex.ToString());
            }
        }

        private void UpdateSkillIconBack(AttributeType attributeType)
        {
            iconBack.gameObject.SetActive(true);

            var spriteAtlas = ResourceSystem.LoadSpellIcons();
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
            var skillData = DataSystem.FindSkill(rebornSkillInfo.Id);
            var rebornAddSkill = skillData.FeatureDates.Find(a => a.FeatureType == FeatureType.RebornAddSkill) != null;
            if (nameText != null)
            {
                if (rebornAddSkill)
                {
                    nameText.text = skillData.Name.Replace("\\d",DataSystem.FindSkill(rebornSkillInfo.Param1).Name);
                } else
                {
                    nameText.text = skillData.Name.Replace("\\d",rebornSkillInfo.Param1.ToString());
                }
                if (nameAndMpCost)
                {
                    nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
                }
            }
            if (description != null)
            {
                description.text = skillData.Help.Replace("\\d",rebornSkillInfo.Param1.ToString());
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
                learningCost?.gameObject.SetActive(false);
                learningCost?.SetText("");
            }
            range?.gameObject.SetActive(false);
            range?.SetText("");
        }
    }
}