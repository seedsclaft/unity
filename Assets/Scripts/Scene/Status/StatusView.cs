using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Status;

public class StatusView : BaseView ,IInputHandlerEvent
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private StatusActorList actorList = null;
    [SerializeField] private StatusCommandList commandList = null;
    [SerializeField] private SkillList skillList = null;
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
    private string _helpText;
    private bool _isDisplayBack = true;
    public bool IsDisplayBack => _isDisplayBack;
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        skillList.Initialize();
        InitializeSkillActionList();
        skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute),null);
        
        new StatusPresenter(this);
    }

    private void InitializeSkillActionList()
    {
        skillList.InitializeAction((a) => CallSkillAction(a),() => OnClickBack(),(a) => CallSkillLearning(a),null,null);
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideActionList();
        skillList.HideAttributeList();
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
            _helpWindow.SetHelpText(_helpText);
            _helpWindow.SetInputInfo("SELECT_HEROINE");
        }
    }

    public void HideDecideButton()
    {
        decideButton.gameObject.SetActive(false);
    }

    public void SetHelpWindow(string helpText)
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        _helpText = helpText;
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
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }

    public void SetViewInfo(StatusViewInfo statusViewInfo)
    {
        DisplayDecideButton(statusViewInfo.DisplayDecideButton);
        DisplayBackButton(statusViewInfo.DisplayBackButton);
        DisableStrength(statusViewInfo.DisableStrength);
        SetBackEvent(statusViewInfo.BackEvent);
    }

    public void CommandBack()
    {
        if (_backEvent != null)
        {
            _backEvent();
        }
    }

    private void DisplayDecideButton(bool isDisplay)
    {
        _isDisplayDecide = isDisplay;
        decideButton.gameObject.SetActive(isDisplay);
        if (_isDisplayDecide)
        {
            _helpWindow.SetHelpText(_helpText);
            _helpWindow.SetInputInfo("SELECT_HEROINE");
        } else
        {
            _helpWindow.SetHelpText(DataSystem.System.GetTextData(202).Help);
            _helpWindow.SetInputInfo("STATUS");
        }
    }

    private void DisplayBackButton(bool isDisplay)
    {
        _isDisplayBack = isDisplay;
    }

    public new void SetActiveBack(bool IsActive)
    {
        //if (_isDisplayBack == false) IsActive = false;
        base.SetActiveBack(IsActive);
    }

    private void DisableStrength(bool IsDisable)
    {
        //commandList.SetDisable(DataSystem.StatusCommand[0],IsDisable);
    }
    
    public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        actorList.Refresh(actorInfo,actorInfos);
        actorInfoComponent.UpdateInfo(actorInfo,actorInfos);
    }

    public void MoveActorLeft(System.Action endEvent)
    {
        actorList.MoveActorLeft(endEvent);
    }

    public void MoveActorRight(System.Action endEvent)
    {
        actorList.MoveActorRight(endEvent);
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
        if (actorList.AnimationBusy) return;
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
        if (actorList.AnimationBusy) return;
        var eventData = new StatusViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        if (!_rightButton.gameObject.activeSelf) return;
        if (actorList.AnimationBusy) return;
        var eventData = new StatusViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickDecide()
    {
        if (!decideButton.gameObject.activeSelf) return;
        if (actorList.AnimationBusy) return;
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
        skillList.ShowActionList();
        skillList.ShowAttributeList();
        ActivateSkillActionList();
        if (_isDisplayDecide)
        {
            _helpWindow.SetInputInfo("HEROINE_SKILL");
        } else
        {
            _helpWindow.SetInputInfo("STATUS_SKILL");
        }
    }

    public void HideSkillActionList()
    {
        skillList.HideActionList();
        skillList.HideAttributeList();
        DeactivateSkillActionList();
        if (_isDisplayDecide)
        {
            _helpWindow.SetInputInfo("SELECT_HEROINE");
        } else
        {
            _helpWindow.SetInputInfo("STATUS");
        }
    }
    
    public void ActivateSkillActionList()
    {
        skillList.ActivateActionList();
        skillList.ActivateAttributeList();
    }

    public void DeactivateSkillActionList()
    {
        skillList.DeactivateActionList();
        skillList.DeactivateAttributeList();
    }

    public void ShowAttributeList()
    {
        skillList.ShowAttributeList();
    }

    public void HideAttributeList()
    {
        skillList.HideAttributeList();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos,List<AttributeType> attributeTypes,AttributeType currentAttibuteType)
    {
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction();
        skillList.RefreshAttribute(attributeTypes,currentAttibuteType);
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes,AttributeType currentAttibuteType)
    {
        skillList.RefreshAttribute(attributeTypes,currentAttibuteType);
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
        skillList.RefreshAction();
        skillList.RefreshCostInfo();
    }

    public void InputHandler(InputKeyType keyType)
    {

    }

    public new void MouseCancelHandler()
    {
        var eventData = new StatusViewEvent(CommandType.Back);
        _commandData(eventData);
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
    public System.Action BackEvent => _backEvent;
    private bool _displayDecideButton = false;
    public bool DisplayDecideButton => _displayDecideButton;
    private bool _displayBackButton = true;
    public bool DisplayBackButton => _displayBackButton;
    private bool _disableStrength = false;
    public bool DisableStrength => _disableStrength;
    private List<BattlerInfo> _enemyInfos = null;
    public List<BattlerInfo> EnemyInfos => _enemyInfos;
    private bool _isBattle = false;
    public bool IsBattle => _isBattle;
    
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
    
    public void SetEnemyInfos(List<BattlerInfo> enemyInfos,bool isBattle)
    {
        _enemyInfos = enemyInfos;
        _isBattle = isBattle;
    }
}