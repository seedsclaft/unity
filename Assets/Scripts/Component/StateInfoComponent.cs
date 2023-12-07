using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.U2D;

public class StateInfoComponent : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI turns;

    public void UpdateInfo(StateInfo stateInfo)
    {
        if (stateInfo == null)
        {
            return;
        }
        UpdateData(stateInfo.StateType);
        if (description != null)
        {
            string effectText = stateInfo.Master.Help.Replace("\\d",stateInfo.Effect.ToString());
            description.text = effectText;
            var skill = DataSystem.Skills.Find(a => a.Id == stateInfo.SkillId);
            if (skill != null)
            {
                description.text = description.text + "(" + skill.Name + ")";
            }
        }
        if (turns != null)
        {
            turns.text = "";
            var removalTiming = stateInfo.Master.RemovalTiming;
            if (removalTiming == RemovalTiming.UpdateTurn)
            {
                if (stateInfo.Turns > 100)
                {
                    turns.text = DataSystem.System.GetTextData(403).Text;
                } else
                {
                    turns.text = DataSystem.System.GetReplaceText(401,stateInfo.Turns.ToString());
                }
            } else
            if (removalTiming == RemovalTiming.UpdateCount)
            {
                turns.text = DataSystem.System.GetReplaceText(404,stateInfo.Turns.ToString());
            } else
            if (removalTiming == RemovalTiming.UpdateAp)
            {
                if (stateInfo.Turns > 100)
                {
                    turns.text = DataSystem.System.GetTextData(403).Text;
                } else
                {
                    turns.text = DataSystem.System.GetReplaceText(405,stateInfo.Turns.ToString());
                }
            }
        }
    }

    public void UpdateData(StateType stateType)
    {
        if (stateType == 0)
        {
            return;
        }

        var stateData = DataSystem.States.Find(a => a.StateType == stateType);
        if (stateData == null)
        {
            return;
        }

        
        if (icon != null)
        {
            UpdateStateIcon(stateData.IconPath);
        }
        if (nameText != null)
        {
            nameText.text = stateData.Name;
        }
    }

    private void UpdateStateIcon(string iconPath)
    {
        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/Icons");
        if (icon != null)
        {
            icon.sprite = spriteAtlas.GetSprite(iconPath);
        }
    }
}
