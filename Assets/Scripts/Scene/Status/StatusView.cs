using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Status;

public class StatusView : BaseView
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private StatusCommandList commandList = null;
    [SerializeField] private SkillActionList skillActionList = null;
    [SerializeField] private SkillAttributeList skillAttributeList = null;
    [SerializeField] private StatusStrengthList statusStrengthList = null;
    [SerializeField] private Button decideButton = null;
    private new System.Action<StatusViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private GameObject leftPrefab = null;
    [SerializeField] private GameObject rightPrefab = null;
    private HelpWindow _helpWindow = null;

    private Button _leftButton = null;
    private Button _rightButton = null;

    private System.Action _backEvent = null;
    private bool _isDisplayDecide = false;
    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new StatusPresenter(this);
        InitializeSkillActionList();
    }

    private void InitializeSkillActionList()
    {
        skillActionList.Initialize(null);
        SetInputHandler(skillActionList.GetComponent<IInputHandlerEvent>());
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }
    
    public void SetUIButton()
    {
        GameObject prefab2 = Instantiate(leftPrefab);
        prefab2.transform.SetParent(helpRoot.transform, false);
        _leftButton = prefab2.GetComponent<Button>();
        _leftButton.onClick.AddListener(() => OnClickLeft());
        
        GameObject prefab3 = Instantiate(rightPrefab);
        prefab3.transform.SetParent(helpRoot.transform, false);
        _rightButton = prefab3.GetComponent<Button>();
        _rightButton.onClick.AddListener(() => OnClickRight());
        //_helpWindow = prefab.GetComponent<HelpWindow>();

        decideButton.onClick.AddListener(() => OnClickDecide());
    }

    public void ShowArrows()
    {
        _leftButton.gameObject.SetActive(true);
        _rightButton.gameObject.SetActive(true);
    }

    public void HideArrows()
    {
        _leftButton.gameObject.SetActive(false);
        _rightButton.gameObject.SetActive(false);
    }

    public void ShowDecideButton()
    {
        if (_isDisplayDecide)
        {
            decideButton.gameObject.SetActive(true);
        }
    }

    public void HideDecideButton()
    {
        decideButton.gameObject.SetActive(false);
    }

    public void SetHelpWindow()
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
    }

    public void SetEvent(System.Action<StatusViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        _backEvent = backEvent;
        CreateBackCommand(() => 
        {    
            var eventData = new StatusViewEvent(CommandType.Back);
            _commandData(eventData);
        });
    }

    public void CommandBack()
    {
        if (_backEvent != null)
        {
            _backEvent();
        }
    }

    public void DisplayDecideButton(bool isDisplay)
    {
        _isDisplayDecide = isDisplay;
        decideButton.gameObject.SetActive(isDisplay);
    }

    public void DisableStrength(bool IsDisable)
    {
        commandList.SetDisable(DataSystem.StatusCommand[0],IsDisable);
    }
    
    public void SetActorInfo(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo);
    }

    public void SetStrengthInfo(ActorInfo actorInfo,List<SystemData.MenuCommandData> confirmCommands)
    {
        statusStrengthList.Initialize(actorInfo,
            (actorinfo) => CallStrengthPlus(actorinfo),
            (actorinfo) => CallStrengthMinus(actorinfo)
        );
        SetInputHandler(statusStrengthList.GetComponent<IInputHandlerEvent>());
        statusStrengthList.InitializeConfirm(confirmCommands,(confirmCommands) => CallStrengthCommand(confirmCommands));
        HideStrength();
    }
    
    public void SetStatusCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallStatusCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallStatusCommand(StatusComandType commandType)
    {
        var eventData = new StatusViewEvent(CommandType.StatusCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void ShowCommandList()
    {
        commandList.gameObject.SetActive(true);
    }

    public void HideCommandList()
    {
        commandList.gameObject.SetActive(false);
    }

    private void OnClickBack()
    {
        var eventData = new StatusViewEvent(CommandType.Back);
        _commandData(eventData);
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

    private void OnClickDecide()
    {
        var eventData = new StatusViewEvent(CommandType.DecideActor);
        _commandData(eventData);
    }

    public void ShowSkillActionList()
    {
        skillActionList.gameObject.SetActive(true);
        skillAttributeList.gameObject.SetActive(true);
    }


    public void HideSkillActionList()
    {
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        skillActionList.Refresh(skillInfos);
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
    }

    private void CallAttributeTypes(AttributeType attributeType)
    {
        var eventData = new StatusViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
    }

    public void CommandAttributeType(AttributeType attributeType)
    {
        
    }

    private void CallStrengthCommand(TacticsComandType commandType)
    {
        var eventData = new StatusViewEvent(CommandType.StrengthClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    private void CallStrengthPlus(int actorId)
    {
        var eventData = new StatusViewEvent(CommandType.SelectStrengthPlus);
        eventData.templete = actorId;
        _commandData(eventData);
    }

    private void CallStrengthMinus(int actorId)
    {
        var eventData = new StatusViewEvent(CommandType.SelectStrengthMinus);
        eventData.templete = actorId;
        _commandData(eventData);
    }

    public void ShowStrength()
    {
        statusStrengthList.gameObject.SetActive(true);
    }

    public void HideStrength()
    {
        statusStrengthList.gameObject.SetActive(false);
    }

    public void CommandRefresh(int remainSp)
    {
        statusStrengthList.Refresh(remainSp);
    }
}

namespace Status
{
    public enum CommandType
    {
        None = 0,
        StatusCommand,
        AttributeType,
        DecideActor,
        LeftActor,
        RightActor,
        SelectStrengthPlus,
        SelectStrengthMinus,
        StrengthClose,
        DecideStage,
        Back
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

public class StatusViewInfo{
    private System.Action _backEvent = null;
    public System.Action BackEvent {get {return _backEvent;}}
    private bool _displayDecideButton = false;
    public bool DisplayDecideButton {get {return _displayDecideButton;}}
    private bool _disableStrength = false;
    public bool DisableStrength {get {return _disableStrength;}}
    
    public StatusViewInfo(System.Action backEvent)
    {
        _backEvent = backEvent;
    }

    public void SetDisplayDecideButton(bool isDisplay)
    {
        _displayDecideButton = isDisplay;
    }
    
    public void SetDisableStrength(bool IsDisable)
    {
        _disableStrength = IsDisable;
    }
}