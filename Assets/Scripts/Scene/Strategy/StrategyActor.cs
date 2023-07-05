using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StrategyActor : ListItem ,IListViewItem  
{   
    [SerializeField] private ActorInfoComponent component;
    [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;
    [SerializeField] private Image bonusImage;    
    private ActorInfo _data;    
    private bool _isBonus; 

    private System.Action _callEvent = null;

    public void SetData(ActorInfo data,bool isBonus){
        _data = data;
        _isBonus = isBonus;
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
        component.UpdateInfo(_data,null);
    }

    public void StartResultAnimation(int animId)
    {
        shinyReflect.enabled = false;
        //gameObject.transform.DOLocalMoveX(80,0.0f);
        int initPosy = (animId % 2 == 1) ? -80 : 80;
        Sequence sequence = DOTween.Sequence()
            .Append(gameObject.transform.DOLocalMoveY(initPosy,0.0f))
            .Append(gameObject.transform.DOLocalMoveY(0,1.0f))
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                if (_isBonus) 
                {
                    StartBonusAnimation();
                } else
                {
                }
                if (_callEvent != null) _callEvent();
            });
    }

    private void StartBonusAnimation()
    {
        int rand = Random.Range(1,100);
        bonusImage.transform.DOScaleY(0,0.0f);
        Sequence sequence = DOTween.Sequence()
            .Append(bonusImage.transform.DOScaleY(1.5f,0.4f))
            .Join(bonusImage.DOFade(0.75f,0.1f))
            .Append(bonusImage.DOFade(0.0f,0.3f))
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                Sequence sequence = DOTween.Sequence()
                .SetDelay(rand * 0.01f)
                .OnComplete(() => {
                    shinyReflect.enabled = true;
                });
            });
    }

    public void SetEndCallEvent(System.Action callEvent)
    {
        _callEvent = callEvent;
    }
}
