using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

public class TacticsView : BaseView
{
    [SerializeField] private TacticsCommandList tacticsCommandList = null;
    [SerializeField] private SkillList skillList = null;
    [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;

    [SerializeField] private TacticsTrainList tacticsTrainList = null;

    [SerializeField] private TacticsAlchemyList tacticsAlchemyList = null;

    [SerializeField] private TacticsAttributeList tacticsAttributeList = null;
    
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
        /*
        skillList.Initialize();
        skillList.InitializeAction(actorInfo => CallSkillAlchemy(actorInfo),() => OnClickBack(),null,null);
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        HideSkillAlchemyList();

        skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute));
        SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideAttributeList();
        */

        tacticsTrainList.Initialize((actorinfo) => CallActorTrain(actorinfo));
        SetInputHandler(tacticsTrainList.GetComponent<IInputHandlerEvent>());
        
        new TacticsPresenter(this);
    }
    
    public void SetUIButton()
    {
        CreateBackCommand(() => OnClickBack());
        tacticsEnemyList.Initialize((a) => CallBattleEnemy(a),() => OnClickBack(),(a) => CallPopupSkillInfo(a),(a) => OnClickEnemyInfo(a));
        SetInputHandler(tacticsEnemyList.GetComponent<IInputHandlerEvent>());
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
        tacticsCommandList.Initialize((menuCommandInfo) => CallTacticsCommand(menuCommandInfo),() => CallAlcanaEvent());
        tacticsCommandList.Refresh(menuCommands);
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
    
        tacticsTrainList.Refresh(actorInfos);
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
        tacticsEnemyList.Refresh(troopInfos);
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

    public void ShowAttributeList()
    {
        tacticsAttributeList.gameObject.SetActive(true);
    }

    public void HideAttributeList()
    {
        tacticsAttributeList.gameObject.SetActive(false);
    }

    public void ShowSkillAlchemyList(List<SkillInfo> skillInfos)
    {
        /*
        skillList.ShowActionList();
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction();
        skillList.ShowAttributeList();
        */
    }

    public void HideSkillAlchemyList()
    {
        /*
        skillList.HideActionList();
        skillList.HideAttributeList();
        */
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

    private void CallPopupSkillInfo(int skillId)
    {
        var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo);
        eventData.templete = skillId;
        _commandData(eventData);
    }

    private void OnClickEnemyInfo(int enemyIndex)
    {
        var eventData = new TacticsViewEvent(CommandType.CallEnemyInfo);
        eventData.templete = enemyIndex;
        _commandData(eventData);
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
        //skillList.RefreshAttribute(attributeTypes);
        tacticsAttributeList.Initialize(attributeTypes,(a) => CallSkillAlchemy(a),() => OnClickBack());
        SetInputHandler(tacticsAttributeList.GetComponent<IInputHandlerEvent>());
    }

    public void SetAttributeValues(List<string> attributeValues,List<int> learningCosts,int currensy)
    {
        tacticsAttributeList.Refresh(attributeValues,learningCosts,currensy);
    }

    private void CallSkillAlchemy(AttributeType attributeType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
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
        DecideActor,
        LeftActor,
        RightActor,
        ActorLayer,
        SelectActorTrain,
        TrainClose,
        SelectActorAlchemy,
        AlchemyClose,
        SelectAlchemyClose,
        SkillAlchemy,
        SelectActorRecovery,
        SelectRecoveryPlus,
        SelectRecoveryMinus,
        RecoveryClose,
        SelectBattleEnemy,
        PopupSkillInfo,
        BattleClose,
        SelectActorBattle,
        EnemyClose,
        SelectActorResource,
        ResourceClose,
        ShowUi,
        HideUi,
        OpenAlcana,
        CallEnemyInfo,
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