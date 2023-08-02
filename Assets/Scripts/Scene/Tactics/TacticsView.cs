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
    [SerializeField] private TacticsAlcana tacticsAlcana = null;

    [SerializeField] private SideMenuList sideMenuList = null;


    private CommandType _lastCallEventType = CommandType.None;


    public override void Initialize()
    {
        base.Initialize();
        InitializeInput();

        skillList.Initialize();
        InitializeSkillActionList();
        //skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute));
        
        tacticsTrainList.Initialize((actorinfo) => CallActorTrain(actorinfo));
        SetInputHandler(tacticsTrainList.GetComponent<IInputHandlerEvent>());
        
        SetRuleButton(true);
        SetDropoutButton(true);

        new TacticsPresenter(this);
    }
    
    private void InitializeSkillActionList()
    {
        skillList.InitializeAction((a) => CallSkillAction(a),() => OnClickBack(),null,null,null);
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        //SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideActionList();
        skillList.DeactivateActionList();
        skillList.HideAttributeList();
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
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetOpenEvent(() => {
            tacticsCommandList.Deactivate();
            sideMenuList.Activate();
        });
        sideMenuList.SetCloseEvent(() => {
            HelpWindow.SetInputInfo("TACTICS");
            tacticsCommandList.UpdateHelpWindow();
            tacticsCommandList.Activate();
            sideMenuList.Deactivate();
        });
    }

    public void SetEvent(System.Action<TacticsViewEvent> commandData)
    {
        _commandData = commandData;
    }

    private void SetRuleButton(bool isActive)
    {
        //ruleButton.gameObject.SetActive(isActive);
    }
    
    private void OnClickRuling()
    {
        var eventData = new TacticsViewEvent(CommandType.Rule);
        _commandData(eventData);
    }

    private void SetDropoutButton(bool isActive)
    {
        //dropoutButton.gameObject.SetActive(isActive);
    }

    private void OnClickDropout()
    {
        var eventData = new TacticsViewEvent(CommandType.Dropout);
        _commandData(eventData);
    }

    private void OnClickOption()
    {
        var eventData = new TacticsViewEvent(CommandType.Option);
        _commandData(eventData);
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
        tacticsCommandList.Initialize((a) => CallTacticsCommand(a),() => CallOpenSideMenu(),() => OnClickDropout(),() => CallAlcanaEvent());
        tacticsCommandList.SetHelpWindow(HelpWindow);
        tacticsCommandList.Refresh(menuCommands);
        SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
    }

    public void SetCommandAble(int commandId)
    {
        tacticsCommandList.SetDisable(DataSystem.TacticsCommand[commandId],false);
    }

    public void SetCommandDisable(int commandId)
    {
        tacticsCommandList.SetDisable(DataSystem.TacticsCommand[commandId],true);
        tacticsCommandList.SelectEnableIndex();
    }

    public void ShowCommandList()
    {
        sideMenuList.gameObject.SetActive(true);
        tacticsCommandList.gameObject.SetActive(true);
    }

    public void HideCommandList()
    {
        sideMenuList.gameObject.SetActive(false);
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

    public void SetActors(List<ActorInfo> actorInfos,List<SystemData.MenuCommandData> confirmCommands,Dictionary<TacticsComandType, int> commandRankInfo)
    {
        tacticsCharaLayer.Initialize(actorInfos);
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
    
        tacticsTrainList.Refresh(actorInfos,commandRankInfo[TacticsComandType.Train]);
        tacticsTrainList.InitializeConfirm(confirmCommands,(confirmCommands) => CallTrainCommand(confirmCommands));
        //HideTrainList();

        tacticsAlchemyList.Initialize(actorInfos,(actorinfo) => CallActorAlchemy(actorinfo),commandRankInfo[TacticsComandType.Alchemy]);
        SetInputHandler(tacticsAlchemyList.GetComponent<IInputHandlerEvent>());
        tacticsAlchemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallAlchemyCommand(confirmCommands));
        //HideAlchemyList();

        tacticsRecoveryList.Initialize(actorInfos,
            (actorinfo) => CallActorRecovery(actorinfo),
            (actorinfo) => CallRecoveryPlus(actorinfo),
            (actorinfo) => CallRecoveryMinus(actorinfo),
            commandRankInfo[TacticsComandType.Recovery]
        );
        SetInputHandler(tacticsRecoveryList.GetComponent<IInputHandlerEvent>());
        tacticsRecoveryList.InitializeConfirm(confirmCommands,(confirmCommands) => CallRecoveryCommand(confirmCommands));
        //HideRecoveryList();

        
        tacticsBattleList.Initialize(actorInfos,(actorinfo) => CallActorBattle(actorinfo),commandRankInfo[TacticsComandType.Battle]);
        SetInputHandler(tacticsBattleList.GetComponent<IInputHandlerEvent>());
        tacticsBattleList.InitializeConfirm(confirmCommands,(confirmCommands) => CallBattleCommand(confirmCommands));
        //HideBattleList();

        tacticsResourceList.Initialize(actorInfos,(actorinfo) => CallActorResource(actorinfo),commandRankInfo[TacticsComandType.Resource]);
        SetInputHandler(tacticsResourceList.GetComponent<IInputHandlerEvent>());
        tacticsResourceList.InitializeConfirm(confirmCommands,(confirmCommands) => CallResourceCommand(confirmCommands));
        //HideResourceList();
    }

    public void SetEnemies(List<TroopInfo> troopInfos)
    {
        tacticsEnemyList.Refresh(troopInfos);
        HideEnemyList();
    }

    public void CommandRefresh(TacticsComandType tacticsComandType)
    {
        switch (tacticsComandType)
        {
            case TacticsComandType.Train:
            tacticsTrainList.Refresh();
            return;
            case TacticsComandType.Alchemy:
            tacticsAlchemyList.Refresh();
            return;
            case TacticsComandType.Recovery:
            tacticsRecoveryList.Refresh();
            return;
            case TacticsComandType.Battle:
            tacticsBattleList.Refresh();
            return;
            case TacticsComandType.Resource:
            tacticsResourceList.Refresh();
            return;
        }
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
        HelpWindow.SetInputInfo("TRAIN");
    }

    public void HideTrainList()
    {
        tacticsTrainList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("TACTICS");
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
        HelpWindow.SetInputInfo("ALCHEMY");
    }

    public void HideAlchemyList()
    {
        tacticsAlchemyList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("TACTICS");
    }

    public void ShowAttributeList()
    {
        tacticsAttributeList.Activate();
        tacticsAttributeList.gameObject.SetActive(true);
        HelpWindow.SetInputInfo("ALCHEMY_ATTRIBUTE");
    }

    public void HideAttributeList()
    {
        tacticsAttributeList.Deactivate();
        tacticsAttributeList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("ALCHEMY");
    }

    public void ShowSkillAlchemyList(List<SkillInfo> skillInfos)
    {
        skillList.ActivateActionList();
        skillList.ShowActionList();
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction();
        HelpWindow.SetInputInfo("ALCHEMY_SKILL");
        //skillList.ShowAttributeList();
    }

    public void HideSkillAlchemyList()
    {
        skillList.DeactivateActionList();
        skillList.HideActionList();
        skillList.HideAttributeList();
        HelpWindow.SetInputInfo("ALCHEMY_ATTRIBUTE");
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
        HelpWindow.SetInputInfo("RECOVERY");
    }

    public void HideRecoveryList()
    {
        tacticsRecoveryList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("TACTICS");
    }


    private void CallBattleEnemy(int enemyIndex)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectBattleEnemy);
        eventData.templete = enemyIndex;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallPopupSkillInfo(GetItemInfo getItemInfo)
    {
        var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo);
        eventData.templete = getItemInfo;
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
        tacticsEnemyList.ResetInputFrame(1);
        HelpWindow.SetInputInfo("ENEMY_SELECT");
    }

    public void HideEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("TACTICS");
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
        tacticsBattleList.Activate();
        HelpWindow.SetInputInfo("ENEMY_BATTLE");
    }

    public void HideBattleList()
    {
        tacticsBattleList.gameObject.SetActive(false);
        tacticsBattleList.Deactivate();
        HelpWindow.SetInputInfo("ENEMY_SELECT");
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
        HelpWindow.SetInputInfo("RESOURCE");
    }

    public void HideResourceList()
    {
        tacticsResourceList.gameObject.SetActive(false);
        HelpWindow.SetInputInfo("TACTICS");
    }

    private void CallResourceCommand(TacticsComandType commandType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.ResourceClose);
        eventData.templete = commandType;
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
        tacticsAttributeList.Deactivate();
    }

    public void SetAttributeValues(List<string> attributeValues,List<int> learningCosts,int currensy)
    {
        tacticsAttributeList.Refresh(attributeValues,learningCosts,currensy);
        tacticsAttributeList.SelectEnableIndex();
    }

    private void CallSkillAlchemy(AttributeType attributeType)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
        eventData.templete = attributeType;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallSkillAction(SkillInfo skillInfo)
    {
        var eventData = new TacticsViewEvent(CommandType.SelectAlchemySkill);
        eventData.templete = skillInfo.Id;
        _commandData(eventData);
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

    public void SetSideMenu(List<SystemData.MenuCommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }
    
    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        HelpWindow.SetInputInfo("TACTICS");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        tacticsCommandList.Deactivate();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        tacticsCommandList.Activate();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        HelpWindow.SetInputInfo("TACTICS");
        tacticsCommandList.UpdateHelpWindow();
    }

    private void CallOpenSideMenu()
    {
        var eventData = new TacticsViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.MenuCommandData sideMenu)
    {
        var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
        eventData.templete = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new TacticsViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
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
        SelectActorTrain,
        TrainClose,
        SelectActorAlchemy,
        AlchemyClose,
        SelectAlchemyClose,
        SkillAlchemy,
        SelectAlchemySkill,
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
        Back,
        Rule,
        Dropout,
        Option,
        OpenSideMenu,
        SelectSideMenu,
        CloseSideMenu
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