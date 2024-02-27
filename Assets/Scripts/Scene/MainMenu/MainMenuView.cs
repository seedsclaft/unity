using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;
using TMPro;

public class MainMenuView : BaseView
{
    [SerializeField] private BaseList stageList = null;
    [SerializeField] private StageInfoComponent component;
    [SerializeField] private TextMeshProUGUI numinous = null;
    [SerializeField] private TextMeshProUGUI totalScore = null;

    private new System.Action<MainMenuViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        stageList.Initialize();
        new MainMenuPresenter(this);
    }

    public void SetNuminous(int value)
    {
        numinous.SetText(DataSystem.GetReplaceDecimalText(value));
    }

    public void SetTotalScore(int value)
    {
        totalScore.SetText(DataSystem.GetReplaceDecimalText(value));
    }

    public void SetInitHelpText()
    {
        HelpWindow.SetHelpText(DataSystem.GetTextData(11040).Text);
        HelpWindow.SetInputInfo("MAINMENU");
    }

    public void SetHelpWindow(){
        SetInitHelpText();
    }

    public void SetEvent(System.Action<MainMenuViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetStagesData(List<ListData> stages){
        stageList.SetData(stages);
        stageList.SetInputHandler(InputKeyType.Decide,() => CallMainMenuStage());
        stageList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
        stageList.SetInputHandler(InputKeyType.Option2,() => CallStageRanking());
        stageList.SetSelectedHandler(() => UpdateMainMenuStage());
        for (var i = 0;i < stageList.ItemPrefabList.Count;i++)
        {
            if (i < stages.Count)
            {
                var stageInfo = stageList.ItemPrefabList[i].GetComponent<MainMenuStage>();
                stageInfo.SetRankingDetailHandler((a) => CallStageRanking(a));
            }
        }
        stageList.UpdateSelectIndex(0);
        SetInputHandler(stageList.GetComponent<IInputHandlerEvent>());
    }
    
    private void CallMainMenuStage(){
        var listData = stageList.ListData;
        if (listData != null)
        {
            var eventData = new MainMenuViewEvent(CommandType.StageSelect);
            var data = (StageInfo)listData.Data;
            eventData.template = data.Id;
            _commandData(eventData);
        }
    }    
    
    private void CallStageRanking(int stageId = -1){
        var listData = stageList.ListData;
        if (listData != null)
        {
            var eventData = new MainMenuViewEvent(CommandType.Ranking);
            if (stageId > 0)
            {
                eventData.template = stageId;
            } else
            {
                var data = (StageInfo)listData.Data;
                eventData.template = data.Id;
            }
            _commandData(eventData);
        }
    }

    public void UpdateMainMenuStage()
    {
        var listData = stageList.ListData;
        if (listData != null)
        {
            var data = (StageInfo)listData.Data;
            component.UpdateInfo(data);
        }
    }

    private void OnClickOption(){
        var eventData = new MainMenuViewEvent(CommandType.Option);
        _commandData(eventData);
    }

    public void SetSideMenu(List<ListData> menuCommands){
        /*
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CommandCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
        */
    }
    
    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
    }

    public void DeactivateSideMenu()
    {
        HelpWindow.SetInputInfo("MAINMENU");
    }

    public new void CommandOpenSideMenu()
    {
        base.CommandOpenSideMenu();
        stageList.Deactivate();
    }

    public void CommandCloseSideMenu()
    {
        stageList.Activate();
        HelpWindow.SetInputInfo("MAINMENU");
        stageList.UpdateHelpWindow();
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new MainMenuViewEvent(CommandType.SelectSideMenu);
        eventData.template = sideMenu;
        _commandData(eventData);
    }
}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        StageSelect = 101,
        Option = 103,
        Ranking = 104,
        SelectSideMenu,
    }
}
public class MainMenuViewEvent
{
    public MainMenu.CommandType commandType;
    public object template;

    public MainMenuViewEvent(MainMenu.CommandType type)
    {
        commandType = type;
    }
}
