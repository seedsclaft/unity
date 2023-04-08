using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Status;

public class StatusView : BaseView
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private StatusActorList actorList = null;
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
    private bool _isDisplayBack = true;
    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        InitializeSkillActionList();
        skillAttributeList.Initialize((attribute) => CallAttributeTypes(attribute));
        
        new StatusPresenter(this);
    }

    private void InitializeSkillActionList()
    {
        skillActionList.Initialize((a) => CallSkillAction(a),() => OnClickBack(),null,(a) => CallSkillLearning(a));
        SetInputHandler(skillActionList.GetComponent<IInputHandlerEvent>());
        skillActionList.gameObject.SetActive(false);
        skillActionList.Deactivate();
        skillAttributeList.gameObject.SetActive(false);
        SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillAttributeList.Deactivate();
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

        actorList.Initialize(
            () => OnClickLeft(),
            () => OnClickRight(),
            () => OnClickDecide(),
            () => OnClickBack());
        SetInputHandler(actorList.GetComponent<IInputHandlerEvent>());
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
        SetActiveBack(_isDisplayBack);
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

    public void DisplayBackButton(bool isDisplay)
    {
        _isDisplayBack = isDisplay;
    }

    public new void SetActiveBack(bool IsActive)
    {
        if (_isDisplayBack == false) IsActive = false;
        base.SetActiveBack(IsActive);
    }

    public void DisableStrength(bool IsDisable)
    {
        commandList.SetDisable(DataSystem.StatusCommand[0],IsDisable);
    }
    
    public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        actorList.Refresh(actorInfo,actorInfos);
        actorInfoComponent.UpdateInfo(actorInfo,actorInfos);
    }

    public void ActivateActorList()
    {
        actorList.Activate();
    }

    public void DeactivateActorList()
    {
        actorList.Deactivate();
    }

    public void SetStrengthInfo(List<SystemData.MenuCommandData> confirmCommands)
    {
        statusStrengthList.Initialize(
            (a) => CallStrengthPlus(a),
            (a) => CallStrengthMinus(a),
            () => CallStatusReset()
        );
        SetInputHandler(statusStrengthList.GetComponent<IInputHandlerEvent>());
        statusStrengthList.InitializeConfirm(confirmCommands,(confirmCommands) => CallStrengthCommand(confirmCommands));
        HideStrength();
    }

    public void RefreshActor(ActorInfo actorInfo)
    {
        statusStrengthList.Refresh(actorInfo);
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

    public void ActivateCommandList()
    {
        commandList.Activate();
    }

    public void DeactivateCommandList()
    {
        commandList.Deactivate();
    }

    public void ActivateStrengthList()
    {
        statusStrengthList.Activate();
    }

    public void DeactivateStrengthList()
    {
        statusStrengthList.Deactivate();
    }

    private void OnClickBack()
    {
        var eventData = new StatusViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        if (!_leftButton.gameObject.activeSelf) return;
        var eventData = new StatusViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        if (!_rightButton.gameObject.activeSelf) return;
        var eventData = new StatusViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickDecide()
    {
        if (!decideButton.gameObject.activeSelf) return;
        var eventData = new StatusViewEvent(CommandType.DecideActor);
        _commandData(eventData);
    }

    private void CallSkillAction(SkillInfo skillInfo)
    {
        var eventData = new StatusViewEvent(CommandType.SelectSkillAction);
        eventData.templete = skillInfo;
        _commandData(eventData);
    }

    private void CallSkillLearning(SkillInfo skillInfo)
    {
        var eventData = new StatusViewEvent(CommandType.SelectSkillLearning);
        eventData.templete = skillInfo;
        _commandData(eventData);
    }

    public void ShowSkillActionList()
    {
        skillActionList.gameObject.SetActive(true);
        skillAttributeList.gameObject.SetActive(true);
        ActivateSkillActionList();
    }

    public void HideSkillActionList()
    {
        DeactivateSkillActionList();
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }
    
    public void ActivateSkillActionList()
    {
        skillActionList.Activate();
        skillAttributeList.Activate();
    }

    public void DeactivateSkillActionList()
    {
        skillActionList.Deactivate();
        skillAttributeList.Deactivate();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        skillActionList.SetSkillInfos(skillInfos);
        skillActionList.Refresh();
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Refresh(attributeTypes);
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

    private void CallStatusReset()
    {

        var eventData = new StatusViewEvent(CommandType.SelectStrengthReset);
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

    public void CommandRefresh(int remainSp,int remainNuminous)
    {
        statusStrengthList.RefreshCostInfo(remainSp,remainNuminous);
        skillActionList.Refresh();
        skillActionList.RefreshCostInfo(remainNuminous);
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
        SelectSkillAction,
        SelectSkillLearning,
        SelectStrengthPlus,
        SelectStrengthMinus,
        SelectStrengthReset,
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
    private bool _displayBackButton = true;
    public bool DisplayBackButton {get {return _displayBackButton;}}
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
    
    public void SetDisplayBackButton(bool isDisplay)
    {
        _displayBackButton = isDisplay;
    }

    public void SetDisableStrength(bool IsDisable)
    {
        _disableStrength = IsDisable;
    }
}