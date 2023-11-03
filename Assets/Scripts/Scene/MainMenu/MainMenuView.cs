using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

public class MainMenuView : BaseView
{
    [SerializeField] private BaseList stageList = null;
    [SerializeField] private StageInfoComponent component;
    [SerializeField] private SideMenuList sideMenuList = null;

    private new System.Action<MainMenuViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        new MainMenuPresenter(this);
    }

    public void SetInitHelpText()
    {
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(11040).Text);
        HelpWindow.SetInputInfo("MAINMENU");
    }

    public void SetHelpWindow(){
        SetInitHelpText();
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetOpenEvent(() => {
            stageList.Deactivate();
            sideMenuList.Activate();
        });
        sideMenuList.SetCloseEvent(() => {
        SetInitHelpText();
            stageList.Activate();
            sideMenuList.Deactivate();
            HelpWindow.SetInputInfo("MAINMENU");
            stageList.UpdateHelpWindow();
        });
    }

    public void SetEvent(System.Action<MainMenuViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetStagesData(List<ListData> stages){
        stageList.Initialize(stages.Count);
        stageList.SetData(stages);
        stageList.SetInputHandler(InputKeyType.Decide,() => CallMainMenuStage());
        stageList.SetInputHandler(InputKeyType.Option1,() => CallOpenSideMenu());
        stageList.SetSelectedHandler(() => UpdateMainMenuStage());
        SetInputHandler(stageList.GetComponent<IInputHandlerEvent>());
        stageList.Activate();
    }
    
    private void CallMainMenuStage(){
        var eventData = new MainMenuViewEvent(CommandType.StageSelect);
        var listData = stageList.ListData;
        if (listData != null)
        {
            var data = (StageInfo)listData.Data;
            eventData.templete = data.Id;
            _commandData(eventData);
        }
    }

    private void UpdateMainMenuStage()
    {
        var listData = stageList.ListData;
        if (listData != null)
        {
            var data = (StageInfo)listData.Data;
            component.UpdateInfo(data);
        }
    }

    private void OnClickRuling(){
        var eventData = new MainMenuViewEvent(CommandType.Rule);
        _commandData(eventData);
    }

    private void OnClickOption(){
        var eventData = new MainMenuViewEvent(CommandType.Option);
        _commandData(eventData);
    }

    private void OnClickRanking()
    {
        var eventData = new MainMenuViewEvent(CommandType.Ranking);
        _commandData(eventData);
    }

    public void SetSideMenu(List<SystemData.CommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }
    
    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        HelpWindow.SetInputInfo("MAINMENU");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        stageList.Deactivate();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        stageList.Activate();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        HelpWindow.SetInputInfo("MAINMENU");
        stageList.UpdateHelpWindow();
    }
    private void CallOpenSideMenu()
    {
        var eventData = new MainMenuViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new MainMenuViewEvent(CommandType.SelectSideMenu);
        eventData.templete = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new MainMenuViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
    }
}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        StageSelect = 101,
        Rule = 102,
        Option = 103,
        Ranking = 104,
        OpenSideMenu,
        SelectSideMenu,
        CloseSideMenu
    }
}
public class MainMenuViewEvent
{
    public MainMenu.CommandType commandType;
    public object templete;

    public MainMenuViewEvent(MainMenu.CommandType type)
    {
        commandType = type;
    }
}
