using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class StageInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI help;
    [SerializeField] private TextMeshProUGUI turns;
    [SerializeField] private TextMeshProUGUI clearCount;
    [SerializeField] private GameObject subordinate;
    [SerializeField] private TextMeshProUGUI subordinateValue;
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
        if (subordinateBorder != null){
            var borderRect = subordinateBorder.GetComponent<RectTransform>();
            borderRect.localScale = new Vector3(stageInfo.SubordinateValue * 0.01f,1,1);
        }
    }
}
