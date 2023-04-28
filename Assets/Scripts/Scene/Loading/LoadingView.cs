using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingView : BaseView
{
    [SerializeField] private Image spriteImage = null;
    [SerializeField] private TMP_Text tipsText = null;
    private new System.Action<LoadingViewEvent> _commandData = null;
    public override void Initialize(){
        new LoadingPresenter(this);
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