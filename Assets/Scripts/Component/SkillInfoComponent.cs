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
        [SerializeField] private TextMeshProUGUI countTurn;
        [SerializeField] private TextMeshProUGUI learningText;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private GameObject selectable;
        [SerializeField] private GameObject selectedAlcana;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;

        public void UpdateInfo(SkillInfo skillInfo){
            if (skillInfo == null)
            {
                Clear();
                return;
            }
            UpdateData(skillInfo.Id);
            description?.SetText(skillInfo.ConvertHelpText());
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
                if (skillInfo.LearningState == LearningState.NotLearnedByAlchemy)
                {
                    learningText.transform.parent.gameObject.SetActive(skillInfo.LearningState == LearningState.NotLearnedByAlchemy);
                    learningText.SetText(DataSystem.GetText(381));
                } else
                if (skillInfo.LearningState == LearningState.NotLearn)
                {
                    learningText.transform.parent.gameObject.SetActive(skillInfo.LearningState == LearningState.NotLearn);
                    learningText.SetText(DataSystem.GetReplaceText(380,skillInfo.LearningLv.ToString()));
                } else
                {
                    learningText.transform.parent.gameObject.SetActive(false);
                }
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

        public void UpdateData(int skillId)
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
                if (nameText != null)
                {
                    nameText.text = skillData.Name;
                    if (nameAndMpCost)
                    {
                        nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
                    }
                }
                //var mpCostText = skillData.SkillType == SkillType.Active ? "(" + skillData.CountTurn.ToString() + ")" : "";
                //mpCost?.SetText(mpCostText);
                type?.SetText(skillData.SkillType.ToString());
                countTurn?.gameObject?.SetActive(skillData.SkillType == SkillType.Active || (skillData.SkillType == SkillType.Passive && skillData.CountTurn > 0));
                countTurn?.SetText(skillData.CountTurn.ToString());
                rank?.gameObject?.SetActive(true);
                UpdateSkillRank(skillData.Rank);
            } else
            {
                Clear();
            }
            if (lineImage != null)
            {
                UpdateLineImage();
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

        private void UpdateSkillRank(int skillRank)
        {
            var rankText = "N";
            if (skillRank >= 210)
            {
                rankText = "SSR";
            } else
            if (skillRank >= 200)
            {
                rankText = "SR";
            } else
            if (skillRank >= 20)
            {
                rankText = "R";
            }
            rank?.SetText(rankText.ToString());
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
            nameText?.SetText("");
            mpCost?.SetText("");
            type?.SetText("");
            description?.SetText("");
            if (lineImage != null)
            {
                lineImage.gameObject.SetActive(false);
            }
            if (learningCost != null)
            {
                learningCost?.gameObject.SetActive(false);
                learningCost?.SetText("");
            }
            range?.gameObject.SetActive(false);
            range?.SetText("");
            countTurn?.gameObject?.SetActive(false);
            rank?.gameObject?.SetActive(false);
        }
    }
}