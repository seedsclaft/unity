using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleView : BaseView
{    
    
    private new System.Action<BattleViewEvent> _commandData = null;
    [SerializeField] private BattleActorList battleActorsList = null;
    [SerializeField] private BattleEnemyList battleEnemyList = null;
    [SerializeField] private BattleOrder battleOrder = null;
    [SerializeField] private BattlePicture battlePicture = null;
    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new BattlePresenter(this);
    }
    
    public void SetEvent(System.Action<BattleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBattleActorData(List<BattlerInfo> battlerInfos)
    {
        battleActorsList.Initialize(battlerInfos,(battler) => CallBattleActorCommand(battler));
        //SetInputHandler(battleActorsList.GetComponent<IInputHandlerEvent>());
    }

    private void CallBattleActorCommand(BattlerInfo battlerInfo)
    {
        var eventData = new BattleViewEvent(Battle.CommandType.SelectActor);
        eventData.templete = battlerInfo;
        _commandData(eventData);
    }

    public void SetBattleEnemyData(List<BattlerInfo> battlerInfos)
    {
        battleEnemyList.Initialize(battlerInfos,(battler) => CallBattleEnemyCommand(battler));
        //SetInputHandler(battleActorsList.GetComponent<IInputHandlerEvent>());
    }

    private void CallBattleEnemyCommand(BattlerInfo battlerInfo)
    {
        var eventData = new BattleViewEvent(Battle.CommandType.SelectActor);
        eventData.templete = battlerInfo;
        _commandData(eventData);
    }

    public void SetBattleOrder(List<BattlerInfo> battleMembers)
    {
        battleOrder.Initialize(battleMembers);
        battleOrder.UpdateBattleMambers(battleMembers);
    }
    
    public void SetNextBattler(BattlerInfo battler)
    {
        UpdateBattlePicture(battler);
    }

    private void UpdateBattlePicture(BattlerInfo battler)
    {
        battlePicture.UpdatePicture(battler);
    }
}

namespace Battle
{
    public enum CommandType
    {
        None = 0,
        Initialize,
        SelectActor,
        SelectEnemy,

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