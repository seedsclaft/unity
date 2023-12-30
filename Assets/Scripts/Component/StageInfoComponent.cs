using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject achieve;
    [SerializeField] private TextMeshProUGUI achieveText;
    [SerializeField] private TextMeshProUGUI help;
    [SerializeField] private TextMeshProUGUI turns;
    [SerializeField] private TextMeshProUGUI clearCount;
    [SerializeField] private GameObject subordinate;
    [SerializeField] private TextMeshProUGUI subordinateValue;
    [SerializeField] private Image subordinateGauge;
    [SerializeField] private Image subordinateBorder;

    public void UpdateInfo(StageInfo stageInfo)
    {
        if (stageInfo == null){
            return;
        }
        var stageData = stageInfo.Master;
        
        if (nameText != null){
            nameText.text = stageData.Name;
        }
        if (achieve != null)
        {
            achieve.SetActive(stageData.AchieveText != "");
        }
        if (achieveText != null){
            if (stageData.AchieveText != "")
            {
                achieveText.text = DataSystem.System.GetTextData(31).Text + stageData.AchieveText;
            } else
            {
                achieveText.text = DataSystem.System.GetTextData(31).Text + DataSystem.System.GetTextData(10000).Text;
            }
        }
        if (help != null){
            help.text = stageData.Help.Replace("\\p",GameSystem.CurrentData.PlayerInfo.PlayerName);
        }
        if (turns != null){
            turns.text = (stageData.Turns).ToString();
        }
        if (clearCount != null){
            clearCount.text = stageInfo.ClearCount.ToString();
        }
        if (subordinate != null){
            subordinate.gameObject.SetActive(stageInfo.IsSubordinate == true);
        }
        if (subordinateValue != null){
            subordinateValue.text = stageInfo.SubordinateValue.ToString();
        }
        if (subordinateGauge != null){
            subordinateGauge.fillAmount = (float)stageInfo.SubordinateValue / 100f;
        }
        if (subordinateBorder != null){
            var borderRect = subordinateBorder.GetComponent<RectTransform>();
            borderRect.localScale = new Vector3(stageInfo.Master.SubordinateValue * 0.01f,1,1);
        }
    }
}
