using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

public class TitleView : BaseView ,IInputHandlerEvent
{
    [SerializeField] private TextMeshProUGUI versionText = null;
    [SerializeField] private TitleCommandList commandList = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private Button logoButton = null;
    [SerializeField] private SideMenuList sideMenuList = null;
    
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new TitlePresenter(this);
        logoButton.onClick.AddListener(() => CallLogoClick());
        logoButton.gameObject.SetActive(true);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
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
        commandList.SetHelpWindow(HelpWindow);
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetCloseEvent(() => {
            HelpWindow.SetInputInfo("TITLE");
            commandList.UpdateHelpWindow();
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

    public void SetTitleCommand(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(a) => CallTitleCommand(a),() => CallOpenSideMenu());
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        commandList.Deactivate();
    }

    public void SetSideMenu(List<SystemData.MenuCommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }

    public void RefreshCommandIndex(int selectIndex)
    {
        commandList.Refresh(selectIndex);
        commandList.Activate();
    }

    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
        commandList.UpdateHelpWindow();
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
        commandList.Deactivate();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        commandList.Activate();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        HelpWindow.SetInputInfo("TITLE");
        commandList.UpdateHelpWindow();
    }

    private void CallTitleCommand(TitleComandType commandType){
        var eventData = new TitleViewEvent(CommandType.TitleCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    private void CallOpenSideMenu()
    {
        var eventData = new TitleViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.MenuCommandData sideMenu)
    {
        var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
        eventData.templete = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new TitleViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
    }

    public void SetCommandDisable(int commandId)
    {
        commandList.SetDisable(DataSystem.TacticsCommand[commandId],true);
    }

    public void CommandLogoClick()
    {
        commandList.ResetInputFrame(1);
        logoButton.gameObject.SetActive(false);
    }    
    
    public void InputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Decide || keyType == InputKeyType.Cancel || keyType == InputKeyType.Start)
        {
            CallLogoClick();
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
    public object templete;

    public TitleViewEvent(Title.CommandType type)
    {
        commandType = type;
    }
}