using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

public class TacticsView : BaseView
{
    [SerializeField] private TacticsCommandList tacticsCommandList = null;
    [SerializeField] private SkillAttributeList skillAttributeList = null;
    [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;

    [SerializeField] private TacticsTrainList tacticsTrainList = null;
    [SerializeField] private TacticsAlchemyList tacticsAlchemyList = null;
    [SerializeField] private SkillActionList skillAlchemyList = null;

    [SerializeField] private TextMeshProUGUI turnText = null;
    [SerializeField] private TextMeshProUGUI numinousText = null;
    private new System.Action<TacticsViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;

    private HelpWindow _helpWindow = null;


    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new TacticsPresenter(this);
        skillAlchemyList.Initialize(actorInfo => CallSkillAlchemy(actorInfo));
        SetInputHandler(skillAlchemyList.GetComponent<IInputHandlerEvent>());
        HideSkillAlchemyList();
    }

    private void CallSkillAlchemy(int skillId)
    {
        var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
        eventData.templete = skillId;
        _commandData(eventData);
    }
    
    public void SetUIButton()
    {
        CreateBackCommand(() => OnClickBack());
    }

    private void OnClickBack()
    {
        var eventData = new TacticsViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    public void SetHelpWindow()
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
    }

    public void SetEvent(System.Action<TacticsViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActorInfo(ActorInfo actorInfo)
    {
    }

    
    public void SetTacticsCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        tacticsCommandList.Initialize(menuCommands,(menuCommandInfo) => CallTacticsCommand(menuCommandInfo));
        SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsCommandList.SetHelpWindow(_helpWindow);
    }

    private void CallTacticsCommand(TacticsComandType commandType)
    {
        var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void SetActors(List<ActorInfo> actorInfos,List<SystemData.MenuCommandData> confirmCommands)
    {
        tacticsCharaLayer.Initialize(actorInfos,(actorinfo) => CallActorLayer(actorinfo));
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
    
        tacticsTrainList.Initialize(actorInfos,(actorinfo) => CallActorTrain(actorinfo));
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
        tacticsTrainList.InitializeConfirm(confirmCommands,(confirmCommands) => CallTrainCommand(confirmCommands));
        HideTrainList();

        tacticsAlchemyList.Initialize(actorInfos,(actorinfo) => CallActorAlchemy(actorinfo));
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
        tacticsAlchemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallAlchemyCommand(confirmCommands));
        HideAlchemyList();
    }

    public void CommandRefresh()
    {
        tacticsTrainList.Refresh();
        tacticsAlchemyList.Refresh();
    }

    private void CallActorTrain(int actorId)
    {
        var eventData = new TacticsViewEvent(CommandType.SelectActorTrain);
        eventData.templete = actorId;
        _commandData(eventData);
    }

    private void CallTrainCommand(TacticsComandType commandType)
    {
        var eventData = new TacticsViewEvent(CommandType.TrainClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void ShowTrainList()
    {
        tacticsTrainList.gameObject.SetActive(true);
    }

    public void HideTrainList()
    {
        tacticsTrainList.gameObject.SetActive(false);
    }

    private void CallActorAlchemy(int actorId)
    {
        var eventData = new TacticsViewEvent(CommandType.SelectActorAlchemy);
        eventData.templete = actorId;
        _commandData(eventData);
    }

    private void CallAlchemyCommand(TacticsComandType commandType)
    {
        var eventData = new TacticsViewEvent(CommandType.AlchemyClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void ShowAlchemyList()
    {
        tacticsAlchemyList.gameObject.SetActive(true);
    }

    public void HideAlchemyList()
    {
        tacticsAlchemyList.gameObject.SetActive(false);
    }

    public void ShowSkillAlchemyList(List<SkillInfo> skillInfos)
    {
        skillAlchemyList.gameObject.SetActive(true);
        skillAlchemyList.Refresh(skillInfos);
    }

    public void HideSkillAlchemyList()
    {
        skillAlchemyList.gameObject.SetActive(false);
    }

    public void SetAlchemyCost(int trainCost)
    {
        //tacticsTrainList.Refresh(trainCost);
    }

    private void CallActorLayer(ActorInfo actorInfo)
    {
        var eventData = new TacticsViewEvent(CommandType.ActorLayer);
        eventData.templete = actorInfo;
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        var eventData = new TacticsViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        var eventData = new TacticsViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickDecide()
    {
        var eventData = new TacticsViewEvent(CommandType.DecideActor);
        _commandData(eventData);
    }

    public void SetTurns(int turns)
    {
        turnText.text = turns.ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.text = numinous.ToString();
    }
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        //skillActionList.Refresh(skillInfos);
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
    }

    private void CallAttributeTypes(AttributeType attributeType)
    {
        var eventData = new TacticsViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
    }

    public void CommandAttributeType(AttributeType attributeType)
    {
        
    }
}

namespace Tactics
{
    public enum CommandType
    {
        None = 0,
        TacticsCommand,
        AttributeType,
        DecideActor,
        LeftActor,
        RightActor,
        ActorLayer,
        SelectActorTrain,
        TrainClose,
        SelectActorAlchemy,
        AlchemyClose,
        SkillAlchemy,
        Back
    }
}

public class TacticsViewEvent
{
    public Tactics.CommandType commandType;
    public object templete;

    public TacticsViewEvent(Tactics.CommandType type)
    {
        commandType = type;
    }
}