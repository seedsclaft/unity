using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Status;

public class StatusView : BaseView ,IInputHandlerEvent
{
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private StatusActorList actorList = null;
    [SerializeField] private BattleSelectCharacter selectCharacter = null;
    [SerializeField] private Button decideButton = null;
    private new System.Action<StatusViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;

    [SerializeField] private Button leftButton = null;
    [SerializeField] private Button rightButton = null;

    private System.Action _backEvent = null;
    private bool _isDisplayDecide = false;
    private string _helpText;
    private bool _isDisplayBack = true;
    public bool IsDisplayBack => _isDisplayBack;
    public override void Initialize() 
    {
        base.Initialize();
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();
        
        new StatusPresenter(this);
    }

    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => {});
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(selectCharacter.DeckMagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
    }
    
    public void SetUIButton()
    {
        leftButton.onClick.AddListener(() => OnClickLeft());
        
        rightButton.onClick.AddListener(() => OnClickRight());

        decideButton.onClick.AddListener(() => OnClickDecide());

        actorList.Initialize();
        actorList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickLeft());
        actorList.SetInputHandler(InputKeyType.SideRight1,() => OnClickRight());
        actorList.SetInputHandler(InputKeyType.Start,() => OnClickDecide());
        actorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(actorList.GetComponent<IInputHandlerEvent>());
    }

    public void ShowArrows()
    {
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
    }

    public void HideArrows()
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
    }

    public void ShowDecideButton()
    {
        if (_isDisplayDecide)
        {
            decideButton.gameObject.SetActive(true);
            HelpWindow.SetHelpText(_helpText);
            HelpWindow.SetInputInfo("SELECT_HEROINE");
        }
    }

    public void HideDecideButton()
    {
        decideButton.gameObject.SetActive(false);
    }

    public void SetHelpWindow(string helpText)
    {
        _helpText = helpText;
    }

    public void SetEvent(System.Action<StatusViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        _backEvent = backEvent;
        SetBackCommand(() => 
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
            HelpWindow.SetHelpText(_helpText);
            HelpWindow.SetInputInfo("SELECT_HEROINE");
        } else
        {
            HelpWindow.SetHelpText(DataSystem.System.GetTextData(202).Help);
            HelpWindow.SetInputInfo("STATUS");
        }
    }

    private void DisplayBackButton(bool isDisplay)
    {
        _isDisplayBack = isDisplay;
    }

    public new void SetActiveBack(bool IsActive)
    {
        //if (_isDisplayBack == false) IsActive = false;
        base.ChangeBackCommandActive(IsActive);
    }
    
    public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        actorList.Refresh(actorInfo,actorInfos);
        actorInfoComponent.Clear();
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
    

    private void OnClickBack()
    {
        var eventData = new StatusViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        if (!leftButton.gameObject.activeSelf) return;
        if (actorList.AnimationBusy) return;
        var eventData = new StatusViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        if (!rightButton.gameObject.activeSelf) return;
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


    public void ShowSkillActionList()
    {
        selectCharacter.ShowActionList();
        ActivateSkillActionList();
        if (_isDisplayDecide)
        {
            //HelpWindow.SetInputInfo("HEROINE_SKILL");
        } else
        {
            //HelpWindow.SetInputInfo("STATUS_SKILL");
        }
    }

    public void HideSkillActionList()
    {
        selectCharacter.HideActionList();
        DeactivateSkillActionList();
        if (_isDisplayDecide)
        {
            HelpWindow.SetInputInfo("SELECT_HEROINE");
        } else
        {
            HelpWindow.SetInputInfo("STATUS");
        }
    }

    public int SelectedSkillId()
    {
        var listData = selectCharacter.ActionData;
        if (listData != null)
        {
            return ((SkillInfo)listData).Id;
        }
        return -1;
    }
    
    public void ActivateSkillActionList()
    {
        selectCharacter.DeckMagicList.Activate();
    }

    public void DeactivateSkillActionList()
    {
        selectCharacter.DeckMagicList.Deactivate();
    }
    
    public void CommandRefreshStatus(List<ListData> skillInfos,ActorInfo actorInfo,List<ActorInfo> party,int lastSelectIndex)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        ShowSkillActionList();
        selectCharacter.SetActorThumb(actorInfo);
        selectCharacter.SetActorInfo(actorInfo,party);
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction(lastSelectIndex);
    }

    public void CommandRefresh()
    {
        //skillList.RefreshAction();
        selectCharacter.RefreshCostInfo();
    }

    public void InputHandler(InputKeyType keyType,bool pressed)
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
        DecideActor,
        LeftActor,
        RightActor,
        SelectSkillAction,
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