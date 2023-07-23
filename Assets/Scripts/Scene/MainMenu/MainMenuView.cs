using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

public class MainMenuView : BaseView
{
    [SerializeField] private MainMenuStageList stageList = null;
    [SerializeField] private Button rankingButton = null;
    [SerializeField] private Button ruleButton = null;
    [SerializeField] private Button optionButton = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    private HelpWindow _helpWindow = null;

    private new System.Action<MainMenuViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        rankingButton.onClick.AddListener(() => OnClickRanking());
        ruleButton.onClick.AddListener(() => OnClickRuling());
        optionButton.onClick.AddListener(() => OnClickOption());
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
        stageList.Initialize(stages,(stageInfo) => CallMainMenuStage(stageInfo));
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
