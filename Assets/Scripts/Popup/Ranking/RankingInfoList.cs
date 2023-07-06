using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class RankingInfoList : ListWindow , IInputHandlerEvent
{
    private List<RankingInfo> _data = new List<RankingInfo>();

    public void Initialize(List<RankingInfo> rankingInfos ,System.Action cancelEvent)
    {
        InitializeListView(rankingInfos.Count);
        _data.Clear();
        _data = rankingInfos;
        for (int i = 0; i < rankingInfos.Count;i++)
        {
            var rankingInfoComp = ObjectList[i].GetComponent<RankingInfoComponent>();
            rankingInfoComp.SetData(rankingInfos[i],i);
        }
        SetInputHandler((a) => CallInputHandler(a,cancelEvent));
        UpdateAllItems();
        UpdateSelectIndex(-1);
    }

    private void CallInputHandler(InputKeyType keyType, System.Action callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent();
        }
        if (keyType == InputKeyType.Cancel)
        {
            callEvent();
        }
    }

    public void SetIsNoChoice(bool isNoChoice)
    {
    }

    public void Refresh(List<RankingInfo> menuCommands)
    {
        
    }
}
