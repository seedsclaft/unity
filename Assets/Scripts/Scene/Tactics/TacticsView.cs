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
    [SerializeField] private TacticsRecoveryList tacticsRecoveryList = null;
    [SerializeField] private TacticsEnemyList tacticsEnemyList = null;
    [SerializeField] private TacticsBattleList tacticsBattleList = null;
    [SerializeField] private TacticsResourceList tacticsResourceList = null;
    [SerializeField] private StageInfoComponent stageInfoComponent = null;
    [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;

    [SerializeField] private TextMeshProUGUI turnText = null;
    [SerializeField] private TextMeshProUGUI numinousText = null;
    private new System.Action<TacticsViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private TacticsAlcana tacticsAlcana = null;

    private HelpWindow _helpWindow = null;

    private CommandType _lastCallEventType = CommandType.None;

    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new TacticsPresenter(this);
        skillAlchemyList.Initialize(actorInfo => CallSkillAlchemy(actorInfo),() => OnClickBack(),null);
        SetInputHandler(skillAlchemyList.GetComponent<IInputHandlerEvent>());
        HideSkillAlchemyList();
    }

    private void CallSkillAlchemy(int skillId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
        eventData.templete = skillId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }
    
    public void SetUIButton()
    {
        CreateBackCommand(() => OnClickBack());
        tacticsEnemyList.Initialize();
    }

    private void OnClickBack()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.Back);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
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
    
    public void SetStageInfo(StageInfo stageInfo)
    {
        stageInfoComponent.UpdateInfo(stageInfo);
    }

    public void SetAlcanaInfo(AlcanaInfo alcanaInfo)
    {
        alcanaInfoComponent.UpdateInfo(alcanaInfo);
    }

    
    public void SetTacticsCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        tacticsCommandList.Initialize(menuCommands,(menuCommandInfo) => CallTacticsCommand(menuCommandInfo),() => CallAlcanaEvent());
        SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsCommandList.SetHelpWindow(_helpWindow);
    }

    public void SetCommandAble(int commandId)
    {
        tacticsCommandList.SetDisable(DataSystem.TacticsCommand[commandId],false);
    }

    public void SetCommandDisable(int commandId)
    {
        tacticsCommandList.SetDisable(DataSystem.TacticsCommand[commandId],true);
    }

    public void ShowCommandList()
    {
        tacticsCommandList.gameObject.SetActive(true);
    }

    public void HideCommandList()
    {
        tacticsCommandList.gameObject.SetActive(false);
    }

    private void CallTacticsCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallAlcanaEvent()
    {
        var eventData = new TacticsViewEvent(CommandType.OpenAlcana);
        _commandData(eventData);
    }

    public void SetActors(List<ActorInfo> actorInfos,List<SystemData.MenuCommandData> confirmCommands)
    {
        tacticsCharaLayer.Initialize(actorInfos,(actorinfo) => CallActorLayer(actorinfo));
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
    
        tacticsTrainList.Initialize(actorInfos,(actorinfo) => CallActorTrain(actorinfo));
        SetInputHandler(tacticsTrainList.GetComponent<IInputHandlerEvent>());
        tacticsTrainList.InitializeConfirm(confirmCommands,(confirmCommands) => CallTrainCommand(confirmCommands));
        HideTrainList();

        tacticsAlchemyList.Initialize(actorInfos,(actorinfo) => CallActorAlchemy(actorinfo));
        SetInputHandler(tacticsAlchemyList.GetComponent<IInputHandlerEvent>());
        tacticsAlchemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallAlchemyCommand(confirmCommands));
        HideAlchemyList();

        tacticsRecoveryList.Initialize(actorInfos,
            (actorinfo) => CallActorRecovery(actorinfo),
            (actorinfo) => CallRecoveryPlus(actorinfo),
            (actorinfo) => CallRecoveryMinus(actorinfo)
        );
        SetInputHandler(tacticsRecoveryList.GetComponent<IInputHandlerEvent>());
        tacticsRecoveryList.InitializeConfirm(confirmCommands,(confirmCommands) => CallRecoveryCommand(confirmCommands));
        HideRecoveryList();

        
        tacticsBattleList.Initialize(actorInfos,(actorinfo) => CallActorBattle(actorinfo));
        SetInputHandler(tacticsBattleList.GetComponent<IInputHandlerEvent>());
        tacticsBattleList.InitializeConfirm(confirmCommands,(confirmCommands) => CallBattleCommand(confirmCommands));
        HideBattleList();

        tacticsResourceList.Initialize(actorInfos,(actorinfo) => CallActorResource(actorinfo));
        SetInputHandler(tacticsResourceList.GetComponent<IInputHandlerEvent>());
        tacticsResourceList.InitializeConfirm(confirmCommands,(confirmCommands) => CallResourceCommand(confirmCommands));
        HideResourceList();
    }

    public void SetEnemies(List<TroopInfo> troopInfos)
    {
        tacticsEnemyList.Refresh(troopInfos,(enemyInfo) => CallBattleEnemy(enemyInfo),() => OnClickBack());
        SetInputHandler(tacticsEnemyList.GetComponent<IInputHandlerEvent>());
        //tacticsEnemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallBattleCommand(confirmCommands));
        HideEnemyList();
    }

    public void CommandRefresh()
    {
        tacticsTrainList.Refresh();
        tacticsAlchemyList.Refresh();
        tacticsRecoveryList.Refresh();
        tacticsBattleList.Refresh();
        tacticsResourceList.Refresh();
    }

    public void AddAlcana()
    {
        tacticsAlcana.StartAnim();
    }

    public void UseAlcana()
    {
        tacticsAlcana.UseAnim();
    }

    private void CallActorTrain(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectActorTrain);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallTrainCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.TrainClose);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
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
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectActorAlchemy);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallAlchemyCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.AlchemyClose);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
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
        skillAttributeList.gameObject.SetActive(true);
    }

    public void HideSkillAlchemyList()
    {
        skillAlchemyList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }

    private void CallRecoveryCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.RecoveryClose);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallActorRecovery(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectActorRecovery);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallRecoveryPlus(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectRecoveryPlus);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallRecoveryMinus(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectRecoveryMinus);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void ShowRecoveryList()
    {
        tacticsRecoveryList.gameObject.SetActive(true);
    }

    public void HideRecoveryList()
    {
        tacticsRecoveryList.gameObject.SetActive(false);
    }


    private void CallBattleEnemy(int enemyIndex)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectBattleEnemy);
        eventData.templete = enemyIndex;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void ShowEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(true);
    }

    public void HideEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(false);
    }

    private void CallBattleCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.BattleClose);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void ShowBattleList()
    {
        tacticsBattleList.gameObject.SetActive(true);
    }

    public void HideBattleList()
    {
        tacticsBattleList.gameObject.SetActive(false);
    }

    private void CallActorBattle(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectActorBattle);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallActorResource(int actorId)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectActorResource);
        eventData.templete = actorId;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void ShowResourceList()
    {
        tacticsResourceList.gameObject.SetActive(true);
    }

    public void HideResourceList()
    {
        tacticsResourceList.gameObject.SetActive(false);
    }

    private void CallResourceCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.ResourceClose);
        eventData.templete = commandType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }
    private void CallActorLayer(ActorInfo actorInfo)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.ActorLayer);
        eventData.templete = actorInfo;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void OnClickLeft()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.LeftActor);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void OnClickRight()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.RightActor);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void OnClickDecide()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.DecideActor);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void SetTurns(int turns)
    {
        turnText.text = turns.ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.text = numinous.ToString();
    }
    

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
        SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillAttributeList.gameObject.SetActive(false);
    }

    private void CallAttributeTypes(AttributeType attributeType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void CommandAttributeType(AttributeType attributeType)
    {
        
    }

    public void ActivateCommandList()
    {
        tacticsCommandList.Activate();
    }

    public void DeactivateCommandList()
    {
        tacticsCommandList.Deactivate();
    }

    void LateUpdate() {
        if (_lastCallEventType != CommandType.None){
            _lastCallEventType = CommandType.None;
        }
    }

    public void ActivateTacticsCommand()
    {
        if (tacticsTrainList.gameObject.activeSelf) tacticsTrainList.Activate();
        if (tacticsAlchemyList.gameObject.activeSelf) tacticsAlchemyList.Activate();
        if (tacticsRecoveryList.gameObject.activeSelf) tacticsRecoveryList.Activate();
        if (tacticsResourceList.gameObject.activeSelf) tacticsResourceList.Activate();
        if (tacticsBattleList.gameObject.activeSelf) tacticsBattleList.Activate();
    }

    public void DeactivateTacticsCommand()
    {
        if (tacticsTrainList.gameObject.activeSelf) tacticsTrainList.Deactivate();
        if (tacticsAlchemyList.gameObject.activeSelf) tacticsAlchemyList.Deactivate();
        if (tacticsRecoveryList.gameObject.activeSelf) tacticsRecoveryList.Deactivate();
        if (tacticsResourceList.gameObject.activeSelf) tacticsResourceList.Deactivate();
        if (tacticsBattleList.gameObject.activeSelf) tacticsBattleList.Deactivate();
    }
}

namespace Tactics
{
    public enum CommandType
    {
        None = 0,
        AddAlcana,
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
        SelectActorRecovery,
        SelectRecoveryPlus,
        SelectRecoveryMinus,
        RecoveryClose,
        SelectBattleEnemy,
        BattleClose,
        SelectActorBattle,
        EnemyClose,
        SelectActorResource,
        ResourceClose,
        ShowUi,
        HideUi,
        OpenAlcana,
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