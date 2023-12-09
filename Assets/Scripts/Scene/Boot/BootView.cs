using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Boot;

public class BootView : BaseView
{
    [SerializeField] private Button logoButton = null;
    [SerializeField] private BaseList baseList = null;
    private new System.Action<BootViewEvent> _commandData = null;
    public override void Initialize() 
    {
        base.Initialize();
        new BootPresenter(this);
        if (TestMode == false)
        {
            logoButton.onClick.AddListener(() => CallLogoClick());
        }
        logoButton.gameObject.SetActive(TestMode == false);
        baseList.SetInputHandler(InputKeyType.Decide,() => CallLogoClick());
        SetInputHandler(baseList.GetComponent<IInputHandlerEvent>());
    }

    public void SetEvent(System.Action<BootViewEvent> commandData)
    {
        _commandData = commandData;
    }

    private void CallLogoClick()
    {
        var eventData = new BootViewEvent(CommandType.LogoClick);
        _commandData(eventData);
    }
    
}


namespace Boot
{
    public enum CommandType
    {
        None = 0,
        LogoClick,
    }
}

public class BootViewEvent
{
    public Boot.CommandType commandType;
    public object template;

    public BootViewEvent(Boot.CommandType type)
    {
        commandType = type;
    }
}