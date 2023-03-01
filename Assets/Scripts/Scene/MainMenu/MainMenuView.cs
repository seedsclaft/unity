using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

public class MainMenuView : BaseView
{
    [SerializeField] private SpriteRenderer backGround = null; 
    [SerializeField] private MainMenuStageList stageList = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    private HelpWindow _helpWindow = null;

    private new System.Action<MainMenuViewEvent> _commandData = null;

    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new MainMenuPresenter(this);
    }
    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        _helpWindow.SetHelpText(DataSystem.System.SystemTextData.Find(a => a.Id == 11040).Text);
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
        eventData.templete = stage;
        _commandData(eventData);
    }

}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        StageSelect = 101,
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
