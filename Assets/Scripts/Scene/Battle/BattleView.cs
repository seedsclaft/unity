using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using TMPro;

public class BattleView : BaseView
{

    [SerializeField] private BattleActorList battleActorList = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;
    [SerializeField] private SkillAttributeList skillAttributeList = null;

    private new System.Action<BattleViewEvent> _commandData = null;
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
        new BattlePresenter(this);
    }

    public void CreateObject()
    {
        battleActorList.Initialize(actorInfo => CallActorList(actorInfo));
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

    public void SetEvent(System.Action<BattleViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActorInfo(ActorInfo actorInfo)
    {
    }

    
    public void SetBattleCommand(List<SystemData.MenuCommandData> menuCommands)
    {
    }



    public void SetActors(List<BattlerInfo> battlerInfos)
    {
        battleActorList.Refresh(battlerInfos);
        battleGridLayer.SetActorInfo(battlerInfos);
    }



    private void CallActorList(int index)
    {
        var eventData = new BattleViewEvent(CommandType.ActorList);
        eventData.templete = index;
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        var eventData = new BattleViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        var eventData = new BattleViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickDecide()
    {
        var eventData = new BattleViewEvent(CommandType.DecideActor);
        _commandData(eventData);
    }

    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        //skillActionList.Refresh(skillInfos);
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        //skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
    }

    private void CallAttributeTypes(AttributeType attributeType)
    {
        var eventData = new BattleViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
    }

    public void CommandAttributeType(AttributeType attributeType)
    {
        
    }
}

namespace Battle
{
    public enum CommandType
    {
        None = 0,
        BattleCommand,
        AttributeType,
        DecideActor,
        LeftActor,
        RightActor,
        ActorList,
    }
}

public class BattleViewEvent
{
    public Battle.CommandType commandType;
    public object templete;

    public BattleViewEvent(Battle.CommandType type)
    {
        commandType = type;
    }
}