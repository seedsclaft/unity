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

    [SerializeField] private TextMeshProUGUI turnText = null;
    [SerializeField] private TextMeshProUGUI numinousText = null;
    private new System.Action<TacticsViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private GameObject backPrefab = null;
    private HelpWindow _helpWindow = null;


    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new TacticsPresenter(this);
    }

    
    public void SetUIButton()
    {
        GameObject prefab = Instantiate(backPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        
        //_helpWindow = prefab.GetComponent<HelpWindow>();

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

    public void SetActors(List<ActorInfo> actorInfos)
    {
        tacticsCharaLayer.Initialize(actorInfos,(actorinfo) => CallActorLayer(actorinfo));
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
    
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