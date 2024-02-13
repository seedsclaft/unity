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
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI scoreMax;

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
                achieveText.text = DataSystem.GetTextData(31).Text + stageData.AchieveText;
            } else
            {
                achieveText.text = DataSystem.GetTextData(31).Text + DataSystem.GetTextData(10000).Text;
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

        if (score != null){
            score.SetText(stageInfo.Score.ToString());
        }
        if (scoreMax != null){
            scoreMax.SetText(stageInfo.ScoreMax.ToString());
        }
    }
}
