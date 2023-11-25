using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class RankingInfoComponent : ListItem ,IListViewItem 
{   
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private List<Image> actorImages;
    [SerializeField] private List<TextMeshProUGUI> actorEvaluates;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (RankingInfo)ListData.Data;
        playerName.text = data.Name;
        score.text = data.Score.ToString();
        rank.text = data.Rank.ToString() + "位";
        for (int i = 0;i < actorImages.Count;i++)
        {
            if (data.SelectIdx.Count > i)
            {
                actorImages[i].gameObject.SetActive(true);
                actorEvaluates[i].gameObject.SetActive(true);
                actorEvaluates[i].text = data.SelectRank[i].ToString();
                UpdateAwakenFaceThumb(actorImages[i],data.SelectIdx[i]);
            } else
            {
                actorEvaluates[i].gameObject.SetActive(false);
                actorImages[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateAwakenFaceThumb(Image image, int actorId)
    {
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + actorId.ToString("D4") + "/AwakenFace");
        if (image != null) image.sprite = handle;
    }
}
