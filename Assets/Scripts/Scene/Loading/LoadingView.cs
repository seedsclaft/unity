using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LoadingView : BaseView
{
    [SerializeField] private Image spriteImage = null;
    [SerializeField] private TMP_Text tipsText = null;
    [SerializeField] private TMP_Text loadingText = null;
    private new System.Action<LoadingViewEvent> _commandData = null;
    public override void Initialize(){
        new LoadingPresenter(this);
        LoadingAnimation();
    }
    
    public void SetEvent(System.Action<LoadingViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetTips(Sprite tipsSprite,string tips)
    {
        spriteImage.sprite = tipsSprite;
        tipsText.text = tips;
    }

    private void LoadingAnimation()
    {
        Sequence main = DOTween.Sequence()
            .Append(loadingText.DOFade(1f,0.4f))
            .Append(loadingText.DOFade(0f,0.4f)).SetLoops(-1, LoopType.Yoyo);
        
    }
}

namespace Loading
{
    public enum CommandType
    {
        None = 0,
        Back,
        LeftActor,
        RightActor,
        AttributeType,
    }
}
public class LoadingViewEvent
{
    public Loading.CommandType commandType;
    public object templete;

    public LoadingViewEvent(Loading.CommandType type)
    {
        commandType = type;
    }
}