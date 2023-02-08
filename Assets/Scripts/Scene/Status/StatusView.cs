using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Status;

public class StatusView : BaseView
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private StatusCommandList commandList = null;
    private new System.Action<StatusViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private GameObject backPrefab = null;
    [SerializeField] private GameObject leftPrefab = null;
    [SerializeField] private GameObject rightPrefab = null;
    private HelpWindow _helpWindow = null;

    private Button _leftButton = null;
    private Button _rightButton = null;

    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new StatusPresenter(this);
    }
    
    public void SetUIButton(){
        GameObject prefab = Instantiate(backPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        
        GameObject prefab2 = Instantiate(leftPrefab);
        prefab2.transform.SetParent(helpRoot.transform, false);
        _leftButton = prefab2.GetComponent<Button>();
        _leftButton.onClick.AddListener(() => OnClickLeft());
        
        GameObject prefab3 = Instantiate(rightPrefab);
        prefab3.transform.SetParent(helpRoot.transform, false);
        _rightButton = prefab3.GetComponent<Button>();
        _rightButton.onClick.AddListener(() => OnClickRight());
        //_helpWindow = prefab.GetComponent<HelpWindow>();
    }

    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
    }

    public void SetEvent(System.Action<StatusViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActorInfo(ActorInfo actorInfo){
        actorInfoComponent.UpdateInfo(actorInfo);
    }

    
    public void SetTitleCommand(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallTitleCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallTitleCommand(StatusComandType commandType){
        /*
        var eventData = new StatusViewEvent(CommandType.StatusCommand);
        eventData.templete = commandType;
        _commandData(eventData);
        */
    }

    private void OnClickLeft()
    {
        var eventData = new StatusViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        var eventData = new StatusViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }
}

namespace Status
{
    public enum CommandType
    {
        None = 0,
        StatusCommand,
        LeftActor,
        RightActor,
    }
}

public class StatusViewEvent
{
    public Status.CommandType commandType;
    public object templete;

    public StatusViewEvent(Status.CommandType type)
    {
        commandType = type;
    }
}