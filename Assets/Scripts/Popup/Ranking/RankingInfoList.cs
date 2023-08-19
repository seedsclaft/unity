using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingInfoList : ListWindow , IInputHandlerEvent
{
    private List<RankingInfo> _data = new List<RankingInfo>();
    public List<RankingInfo> Data => _data;

    public void Initialize(List<RankingInfo> rankingInfos)
    {
        InitializeListView(rankingInfos.Count);
        _data.Clear();
        _data = rankingInfos;
        for (int i = 0; i < rankingInfos.Count;i++)
        {
            var rankingInfoComp = ObjectList[i].GetComponent<RankingInfoComponent>();
            rankingInfoComp.SetData(rankingInfos[i],i);
        }
        UpdateAllItems();
        UpdateSelectIndex(-1);
    }
}
