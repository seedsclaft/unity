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
    }

    public void SetCallHandler(System.Action<ActorInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        //component.UpdateInfo(_data);
    }

    public void StartResultAnimation(int animId)
    {
        int initPosy = (animId % 2 == 1) ? 270 : -270;
        Sequence sequence = DOTween.Sequence()
            .Append(gameObject.transform.DOLocalMoveY(0.0f, initPosy))
            .Append(gameObject.transform.DOLocalMoveY(2, 0))
            .OnComplete(() => {
                if (_callEvent != null) _callEvent();
            });
    }

    public void SetEndCallEvent(System.Action callEvent)
    {
        _callEvent = callEvent;
    }
}
