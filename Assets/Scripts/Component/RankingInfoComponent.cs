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

    private RankingInfo _data; 
    private int _rankingIndex; 
    public void SetData(RankingInfo data,int index){
        _data = data;
        _rankingIndex = index;
        SetIndex(index);
        UpdateViewItem();
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        playerName.text = _data.Name;
        score.text = _data.Score.ToString();
        rank.text = (_rankingIndex+1).ToString() + "位";
        for (int i = 0;i < actorImages.Count;i++)
        {
            if (_data.SelectIdx.Count > i)
            {
                actorImages[i].gameObject.SetActive(true);
                actorEvaluates[i].gameObject.SetActive(true);
                actorEvaluates[i].text = _data.SelectRank[i].ToString();
                UpdateAwakenFaceThumb(actorImages[i],_data.SelectIdx[i]);
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
