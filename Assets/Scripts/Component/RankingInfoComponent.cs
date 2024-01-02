using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingInfoComponent : ListItem ,IListViewItem 
{   
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private List<Image> actorImages;
    [SerializeField] private List<TextMeshProUGUI> actorEvaluates;
    [SerializeField] private Button detailButton;
    [SerializeField] private TextMeshProUGUI rankingTypeText = null; 

    private bool _isInit = false;
    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (RankingInfo)ListData.Data;
        playerName.text = data.Name;
        score.text = data.Score.ToString();
        rank.text = data.Rank.ToString() + DataSystem.System.GetTextData(16070).Text;
        rankingTypeText.text = data.RankingTypeText;
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
        if (_isInit == false)
        {
            if (detailButton != null)
            {
                detailButton.onClick.AddListener(() => data.DetailEvent(Index));
            }
        }
        _isInit = true;
    }
    
    private void UpdateAwakenFaceThumb(Image image, int actorId)
    {
        if (image != null) 
        {
            image.sprite = ResourceSystem.LoadActorAwakenFaceSprite(actorId.ToString("D4"));
        }
    }
}
