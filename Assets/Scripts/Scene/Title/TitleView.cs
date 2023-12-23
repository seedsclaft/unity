using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

public class TitleView : BaseView 
{
    [SerializeField] private TextMeshProUGUI versionText = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private SideMenuList sideMenuList = null;
    [SerializeField] private BaseList titleCommandList = null;
    public override void Initialize() 
    {
        base.Initialize();
        new TitlePresenter(this);
    }

    private void CallCredit()
    {
        var eventData = new TitleViewEvent(CommandType.Credit);
        _commandData(eventData);
    }

    private void OnClickOption()
    {
        var eventData = new TitleViewEvent(CommandType.Option);
        _commandData(eventData);
    }

    public void SetHelpWindow(){
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetOpenEvent(() => {
            titleCommandList.Deactivate();
            sideMenuList.Activate();
        });
        sideMenuList.SetCloseEvent(() => {
            titleCommandList.Activate();
            sideMenuList.Deactivate();
            HelpWindow.SetInputInfo("TITLE");
            UpdateHelpWindow();
        });
    }

    public void SetEvent(System.Action<TitleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetVersion(string text)
    {
        versionText.text = text;
    }

    public void SetTitleCommand(List<ListData> titleCommands){
        titleCommandList.SetData(titleCommands);
        titleCommandList.SetInputHandler(InputKeyType.Decide,() => CallTitleCommand());
        titleCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
        titleCommandList.SetSelectedHandler(() => UpdateHelpWindow());
        SetInputHandler(titleCommandList.GetComponent<IInputHandlerEvent>());
        titleCommandList.SetHelpWindow(HelpWindow);
    }

    public void SetSideMenu(List<SystemData.CommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CommandCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }

    public void RefreshCommandIndex(int selectIndex)
    {
        titleCommandList.Refresh(selectIndex);
        titleCommandList.Activate();
    }

    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
        UpdateHelpWindow();
    }

    public void DeactivateSideMenu()
    {
        HelpWindow.SetInputInfo("CREDIT");
        sideMenuList.Deactivate();
    }

    public new void CommandOpenSideMenu()
    {
        base.CommandOpenSideMenu();
        sideMenuList.OpenSideMenu();
        titleCommandList.Deactivate();
    }

    public void CommandCloseSideMenu()
    {
        titleCommandList.Activate();
        sideMenuList.CloseSideMenu();
        HelpWindow.SetInputInfo("TITLE");
        UpdateHelpWindow();
    }

    private void CallTitleCommand(){
        var listData = titleCommandList.ListData;
        if (listData != null && listData.Enable)
        {
            var eventData = new TitleViewEvent(CommandType.TitleCommand);
            var commandData = (SystemData.CommandData)listData.Data;
            eventData.template = commandData.Id;
            _commandData(eventData);
        }
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
        eventData.template = sideMenu;
        _commandData(eventData);
    }
    
    private void UpdateHelpWindow()
    {
        var listData = titleCommandList.ListData;
        if (listData != null)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            HelpWindow.SetHelpText(commandData.Help);
        }
    }
}

namespace Title
{
    public enum CommandType
    {
        None = 0,
        TitleCommand,
        Credit,
        Option,
        SelectSideMenu,
    }
}

public class TitleViewEvent
{
    public Title.CommandType commandType;
    public object template;

    public TitleViewEvent(Title.CommandType type)
    {
        commandType = type;
    }
}