using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Credit;

public class CreditView : BaseView
{
    private new System.Action<CreditViewEvent> _commandData = null;
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new CreditPresenter(this);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    public void SetEvent(System.Action<CreditViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        CreateBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (backEvent != null) backEvent();
        });
        SetActiveBack(true);
    }
}

namespace Credit
{
    public enum CommandType
    {
        None = 0,
    }
}

public class CreditViewEvent
{
    public Credit.CommandType commandType;
    public object templete;

    public CreditViewEvent(Credit.CommandType type)
    {
        commandType = type;
    }
}
