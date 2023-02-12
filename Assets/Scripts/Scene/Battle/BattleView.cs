using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using TMPro;

public class BattleView : BaseView
{

    [SerializeField] private BattleActorList battleActorList = null;
    [SerializeField] private BattleEnemyLayer battleEnemyLayer = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;
    [SerializeField] private SkillActionList skillActionList = null;
    [SerializeField] private SkillAttributeList skillAttributeList = null;
    [SerializeField] private BattleThumb battleThumb = null;

    private new System.Action<BattleViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private GameObject backPrefab = null;
    private HelpWindow _helpWindow = null;

    private bool _busy = false;
    public void SetBusy(bool isBusy)
    {
        _busy = isBusy;
    }


    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new BattlePresenter(this);
        InitializeSkillActionList();
    }

    private void InitializeSkillActionList()
    {
        skillActionList.Initialize(actorInfo => CallSkillAction(actorInfo));
        SetInputHandler(skillActionList.GetComponent<IInputHandlerEvent>());
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }
    
    private void CallSkillAction(int skillId)
    {
        var eventData = new BattleViewEvent(CommandType.SkillAction);
        eventData.templete = skillId;
        _commandData(eventData);
    }

    public void HideSkillAction(ActionInfo actionInfo)
    {
        
    }

    public void ShowEnemyTarget(ActionInfo actionInfo)
    {
        battleEnemyLayer.gameObject.SetActive(true);
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
    
    public void SetEnemies(List<BattlerInfo> battlerInfos)
    {
        battleEnemyLayer.Initialize(battlerInfos,(batterInfo) => CallEnemyInfo(batterInfo));
        battleGridLayer.SetEnemyInfo(battlerInfos);
    }

    private void CallEnemyInfo(List<int> indexList)
    {
        var eventData = new BattleViewEvent(CommandType.EnemyLayer);
        eventData.templete = indexList;
        _commandData(eventData);
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

    public void ShowSkillActionList(ActorInfo actorInfo)
    {
        skillActionList.gameObject.SetActive(true);
        skillAttributeList.gameObject.SetActive(true);
        battleThumb.ShowMainThumb(actorInfo);
    }

    public void HideSkillActionList()
    {
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
        battleThumb.HideThumb();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        skillActionList.Refresh(skillInfos);
    }

    public void RefreshBattlerEnemyLayerTarget(ActionInfo actionInfo)
    {
        battleEnemyLayer.RefreshTarget(actionInfo);
    }
    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
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

    private void Update() 
    {
        if (_busy == true) return;
        var eventData = new BattleViewEvent(CommandType.UpdateAp);
        _commandData(eventData);
    }

    public void UpdateAp() 
    {
        battleGridLayer.UpdatePosition();
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
        UpdateAp,
        SkillAction,
        EnemyLayer
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