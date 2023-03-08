using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StrategyActor : ListItem ,IListViewItem  
{   
    [SerializeField] private ActorInfoComponent component;
    private ActorInfo _data; 

    private System.Action _callEvent = null;

    public void SetData(ActorInfo data){
        _data = data;
        UpdateViewItem();
    }

    public void SetCallHandler(System.Action<ActorInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        component.UpdateInfo(_data);
    }

    public void StartResultAnimation(int animId)
    {
        //gameObject.transform.DOLocalMoveX(80,0.0f);
        int initPosy = (animId % 2 == 1) ? -80 : 80;
        Sequence sequence = DOTween.Sequence()
            .Append(gameObject.transform.DOLocalMoveY(initPosy,0.0f))
            .Append(gameObject.transform.DOLocalMoveY(0,1.0f))
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                if (_callEvent != null) _callEvent();
            });
    }

    public void SetEndCallEvent(System.Action callEvent)
    {
        _callEvent = callEvent;
    }
}
