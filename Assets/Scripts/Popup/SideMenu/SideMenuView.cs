using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SideMenu;

public class SideMenuView : BaseView
{
    [SerializeField] private BaseList sideMenuInfoList = null;
    [SerializeField] private Button closeButton = null;
    private new System.Action<SideMenuViewEvent> _commandData = null;

    public SystemData.CommandData SideMenuCommand 
    { 
        get {
        var listData = sideMenuInfoList.ListData;
        if (listData != null)
        {
            return (SystemData.CommandData)listData.Data;
        }
        return null;
        }
    }

    public override void Initialize() 
    {
        base.Initialize();
        sideMenuInfoList.Initialize();
        new SideMenuPresenter(this);
        closeButton.onClick.AddListener(() => 
        {
            BackEvent();
        });
    }

    private void OnClickSideMenu()
    {
        var eventData = new SideMenuViewEvent(CommandType.SelectSideMenu);
        _commandData(eventData);
    }


    public void SetEvent(System.Action<SideMenuViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetSideMenuViewInfo(SideMenuViewInfo sideMenuViewInfo)
    {
        sideMenuInfoList.SetData(sideMenuViewInfo.CommandLists);
        sideMenuInfoList.SetInputHandler(InputKeyType.Decide,() => OnClickSideMenu());
        sideMenuInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        SetInputHandler(sideMenuInfoList.GetComponent<IInputHandlerEvent>());
    }

    public void SetBackEvent(System.Action backEvent)
    {
        SetBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
    }
}

namespace SideMenu
{
    public enum CommandType
    {
        None = 0,
        SelectSideMenu = 1,
        Detail = 2,
    }
}

public class SideMenuViewEvent
{
    public CommandType commandType;
    public object template;

    public SideMenuViewEvent(CommandType type)
    {
        commandType = type;
    }
}

public class SideMenuViewInfo{
    public List<ListData> CommandLists;
    public System.Action EndEvent;
}