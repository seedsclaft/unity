using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class StageInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI help;
    [SerializeField] private TextMeshProUGUI turns;
    [SerializeField] private TextMeshProUGUI clearCount;
    public void UpdateInfo(StageInfo stageInfo)
    {
        if (stageInfo == null){
            return;
        }
        var stageData = DataSystem.Stages.Find(stage => stage.Id == stageInfo.Id);
        
        if (name != null){
            name.text = stageData.Name;
        }
        if (help != null){
            help.text = stageData.Help;
        }
        if (turns != null){
            turns.text = stageData.Turns.ToString();
        }
        if (clearCount != null){
            clearCount.text = stageInfo.ClearCount.ToString();
        }
        
    }
}
