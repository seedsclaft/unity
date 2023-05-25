using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

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
            turns.text = stateInfo.Turns.ToString();
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
        var handle = Resources.Load<Sprite>("Texture/Icon/" + iconPath);
        if (icon != null)
        {
            icon.sprite = handle;
        }
    }
}
