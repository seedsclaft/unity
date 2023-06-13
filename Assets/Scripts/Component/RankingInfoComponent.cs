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
    private int _index; 
    public void SetData(RankingInfo data,int index){
        _data = data;
        _index = index;
        SetIndex(index);
        UpdateViewItem();
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        playerName.text = _data.Name;
        score.text = _data.Score.ToString();
        rank.text = (_index+1).ToString() + "位";
        for (int i = 0;i < actorImages.Count;i++)
        {
            if (_data.SelectIdx.Count > i)
            {
                actorImages[i].gameObject.SetActive(true);
                actorEvaluates[i].gameObject.SetActive(true);
                actorEvaluates[i].text = _data.SelectRank[i].ToString();
                UpdateMainFaceThumb(actorImages[i],_data.SelectIdx[i]);
            } else
            {
                actorEvaluates[i].gameObject.SetActive(false);
                actorImages[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateMainFaceThumb(Image image, int actorId)
    {
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/MainFace.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/000" + actorId.ToString() + "/MainFace");
        if (image != null) image.sprite = handle;
    }

}
