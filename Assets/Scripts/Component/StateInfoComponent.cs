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
        UpdateData(stateInfo.StateId);
        if (description != null)
        {
            string effectText = stateInfo.Master.Help.Replace("\\d",stateInfo.Effect.ToString());
            description.text = effectText;
        }
        if (turns != null)
        {
            if (stateInfo.Turns > 100)
            {
                turns.text = DataSystem.System.GetTextData(403).Text;
            } else
            {
                turns.text = stateInfo.Turns.ToString();
            }
        }
    }

    public void UpdateData(int stateId)
    {
        if (stateId == 0)
        {
            return;
        }

        StatesData.StateData stateData = DataSystem.States.Find(a => a.Id == stateId);
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
