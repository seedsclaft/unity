using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
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
        [SerializeField] private TextMeshProUGUI stageLv;

        public void UpdateInfo(StageInfo stageInfo)
        {
            if (stageInfo == null)
            {
                return;
            }
            var stageData = stageInfo.Master;
            nameText?.SetText(stageData.Name);
            if (achieve != null)
            {
                achieve?.SetActive(stageData.AchieveText != "");
            }
            if (stageData.AchieveText != "")
            {
                achieveText?.SetText(DataSystem.GetText(31) + stageData.AchieveText);
            } else
            {
                achieveText?.SetText(DataSystem.GetText(31) + DataSystem.GetText(10000));
            }
            help?.SetText(stageData.Help.Replace("\\p",GameSystem.CurrentData.PlayerInfo.PlayerName));
            turns?.SetText(stageData.Turns.ToString());
            /*
            if (clearCount != null){
                clearCount.text = stageInfo.ClearCount.ToString();
            }
            */

            score?.SetText(DataSystem.GetReplaceDecimalText(stageInfo.Score));
            scoreMax?.SetText(DataSystem.GetReplaceDecimalText(stageInfo.ScoreMax));
            stageLv?.SetText(stageData.StageLv.ToString());
        }
    }
}