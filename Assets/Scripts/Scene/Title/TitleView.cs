using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

public class TitleView : BaseView ,IInputHandlerEvent
{
    [SerializeField] private TextMeshProUGUI versionText = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private Button logoButton = null;
    [SerializeField] private SideMenuList sideMenuList = null;
    [SerializeField] private BaseList titleCommandList = null;

    
    
    public override void Initialize() 
    {
        base.Initialize();
        new TitlePresenter(this);
        logoButton.onClick.AddListener(() => CallLogoClick());
        logoButton.gameObject.SetActive(true);
    }

    private void CallCredit()
    {
        var eventData = new TitleViewEvent(CommandType.Credit);
        _commandData(eventData);
    }

    private void CallLogoClick()
    {
        var eventData = new TitleViewEvent(CommandType.LogoClick);
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
        titleCommandList.SetInputHandler(InputKeyType.Option1,() => CallOpenSideMenu());
        titleCommandList.SetSelectedHandler(() => UpdateHelpWindow());
        SetInputHandler(titleCommandList.GetComponent<IInputHandlerEvent>());
        titleCommandList.SetHelpWindow(HelpWindow);
    }

    public void SetSideMenu(List<SystemData.CommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
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

    public void CommandOpenSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        titleCommandList.Deactivate();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        titleCommandList.Activate();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        HelpWindow.SetInputInfo("TITLE");
        UpdateHelpWindow();
    }

    private void CallTitleCommand(){
        var eventData = new TitleViewEvent(CommandType.TitleCommand);
        var listData = titleCommandList.ListData;
        if (listData != null)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            eventData.template = commandData.Id;
            _commandData(eventData);
        }
    }

    private void CallOpenSideMenu()
    {
        var eventData = new TitleViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
        eventData.template = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new TitleViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
    }

    public void CommandLogoClick()
    {
        titleCommandList.ResetInputFrame(1);
        logoButton.gameObject.SetActive(false);
    }    
    
    public void InputHandler(InputKeyType keyType,bool pressed)
    {
        if (keyType == InputKeyType.Decide || keyType == InputKeyType.Cancel || keyType == InputKeyType.Start)
        {
            CallLogoClick();
        }
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
        LogoClick,
        Option,
        OpenSideMenu,
        SelectSideMenu,
        CloseSideMenu,
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