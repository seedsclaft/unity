using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

public class MainMenuView : BaseView
{
    [SerializeField] private MainMenuStageList stageList = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private SideMenuList sideMenuList = null;
    private HelpWindow _helpWindow = null;

    private new System.Action<MainMenuViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new MainMenuPresenter(this);
    }

    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(11040).Text);
        _helpWindow.SetInputInfo("MAINMENU");
    }

    public void SetEvent(System.Action<MainMenuViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetStagesData(List<StageInfo> stages){
        stageList.Initialize(stages,(stageInfo) => CallMainMenuStage(stageInfo),() => CallOpenSideMenu());
        SetInputHandler(stageList.GetComponent<IInputHandlerEvent>());
    }
    
    private void CallMainMenuStage(StageInfo stage){
        var eventData = new MainMenuViewEvent(CommandType.StageSelect);
        eventData.templete = stage.Id;
        _commandData(eventData);
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

    public void SetSideMenu(List<SystemData.MenuCommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }
    
    public void ActivateSideMenu()
    {
        _helpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        _helpWindow.SetInputInfo("MAINMENU");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        _helpWindow.SetInputInfo("SIDEMENU");
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        stageList.Deactivate();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        _helpWindow.SetInputInfo("MAINMENU");
        stageList.Activate();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        stageList.UpdateHelpWindow();
    }
    private void CallOpenSideMenu()
    {
        var eventData = new MainMenuViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.MenuCommandData sideMenu)
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
    
    public void SetHelpInputInfo(string key)
    {
        _helpWindow.SetInputInfo(key);
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
