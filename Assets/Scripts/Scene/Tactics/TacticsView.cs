using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

public class TacticsView : BaseView
{
    [SerializeField] private BaseList tacticsCommandList = null;
    [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;


    [SerializeField] private BattleSelectCharacter battleSelectCharacter = null;
    [SerializeField] private TacticsSelectCharacter selectCharacter = null;

    
    [SerializeField] private TacticsEnemyList tacticsEnemyList = null;
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

        battleSelectCharacter.Initialize();
        SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();

        SetRuleButton(true);
        SetDropoutButton(true);

        new TacticsPresenter(this);
    }
    
    private void InitializeSelectCharacter()
    {
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAlchemy());
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(battleSelectCharacter.DeckMagicList.GetComponent<IInputHandlerEvent>());
        battleSelectCharacter.HideActionList();
    }

    public void SetUIButton()
    {
        CreateBackCommand(() => OnClickBack());
        tacticsEnemyList.Initialize((a) => CallBattleEnemy(a));
        tacticsEnemyList.SetInputHandler(InputKeyType.Decide,() => CallPopupSkillInfo());
        tacticsEnemyList.SetInputHandler(InputKeyType.Option1,() => OnClickEnemyInfo());
        tacticsEnemyList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
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
            SetHelpInputInfo("TACTICS");
            UpdateHelpWindow();
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

    public void SetTacticsCommand(List<ListData> menuCommands)
    {
        tacticsCommandList.Initialize(menuCommands.Count);
        tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallTacticsCommand());
        tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CallOpenSideMenu());
        tacticsCommandList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickDropout());
        tacticsCommandList.SetInputHandler(InputKeyType.Option2,() => CallAlcanaEvent());
        tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
        tacticsCommandList.SetData(menuCommands);
        SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsCommandList.Activate();
        UpdateHelpWindow();
    }


    public void RefreshListData(ListData listData)
    {
        tacticsCommandList.RefreshListData(listData);
        tacticsCommandList.Refresh();
        UpdateHelpWindow();
        //tacticsCommandList.SelectEnableIndex();
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

    private void CallTacticsCommand()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = tacticsCommandList.ListData;
        if (listData != null)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
            eventData.templete = commandData.Id;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallAlcanaEvent()
    {
        var eventData = new TacticsViewEvent(CommandType.OpenAlcana);
        _commandData(eventData);
    }

    public void SetSelectCharacter(List<ActorInfo> actorInfos,List<ListData> confirmCommands,Dictionary<TacticsComandType, int> commandRankInfo)
    {
        selectCharacter.Initialize(actorInfos.Count);
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        selectCharacter.SetTacticsCommand(confirmCommands);
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallRecoveryPlus());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallRecoveryMinus());
        selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
        SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
        SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
        HideSelectCharacter();

        tacticsCharaLayer.Initialize(actorInfos);
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
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
            selectCharacter.Refresh();
            return;
            case TacticsComandType.Alchemy:
            selectCharacter.Refresh();
            return;
            case TacticsComandType.Recovery:
            selectCharacter.Refresh();
            return;
            case TacticsComandType.Battle:
            selectCharacter.Refresh();
            return;
            case TacticsComandType.Resource:
            selectCharacter.Refresh();
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

    private void CallActorTrain()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            var eventData = new TacticsViewEvent(CommandType.SelectTacticsActor);
            eventData.templete = data;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallTrainCommand()
    {
        if (_lastCallEventType != CommandType.None) return;
        var commandData = selectCharacter.CommandData;
        if (commandData != null)
        {
            var data = (SystemData.CommandData)commandData.Data;
            var commandType = ConfirmComandType.No;
            if (data.Key == "Yes")
            {
                commandType = ConfirmComandType.Yes;
            }
            var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
            eventData.templete = commandType;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
    {
        selectCharacter.SetTacticsChracter(tacticsActorInfo);
        selectCharacter.SetTacticsCommandData(tacticsCommandData);
        selectCharacter.gameObject.SetActive(true);
    }

    public void HideSelectCharacter()
    {
        selectCharacter.gameObject.SetActive(false);
        SetHelpInputInfo("TACTICS");
    }

    public void ShowSelectCharacterCommand()
    {
        selectCharacter.ShowCharacterList();
        selectCharacter.ShowCommandList();
    }

    public void HideSelectCharacterCommand()
    {
        selectCharacter.HideCharacterList();
        selectCharacter.HideCommandList();
    }

    public void ShowAttributeList(ActorInfo actorInfo, List<ListData> learnMagicList)
    {
        //battleSelectCharacter.SetActorThumb(actorInfo);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        battleSelectCharacter.SetSkillInfos(learnMagicList);
        battleSelectCharacter.ShowActionList();
        battleSelectCharacter.DeckMagicList.Activate();
        SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
    }

    public void HideAttributeList()
    {
        battleSelectCharacter.HideActionList();
        battleSelectCharacter.DeckMagicList.Deactivate();
        SetHelpInputInfo("ALCHEMY");
    }

    private void CallRecoveryPlus()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            if (data.TacticsComandType != TacticsComandType.Recovery)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectRecoveryPlus);
            eventData.templete = data.ActorInfo.ActorId;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallRecoveryMinus()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            if (data.TacticsComandType != TacticsComandType.Recovery)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectRecoveryMinus);
            eventData.templete = data.ActorInfo.ActorId;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallBattleEnemy(int enemyIndex)
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SelectBattleEnemy);
        eventData.templete = enemyIndex;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void CallPopupSkillInfo()
    {
        var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo);
        var item = tacticsEnemyList.GetItemInfo;
        if (item != null)
        {
            eventData.templete = item;
            _commandData(eventData);
        }
    }

    private void OnClickEnemyInfo()
    {
        var eventData = new TacticsViewEvent(CommandType.CallEnemyInfo);
        var item = tacticsEnemyList.EnemyIndex;
        if (item > -1)
        {
            eventData.templete = item;
            _commandData(eventData);
        }
    }

    public void ShowEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(true);
        tacticsEnemyList.ResetInputFrame(1);
        SetHelpInputInfo("ENEMY_SELECT");
    }

    public void HideEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(false);
        SetHelpInputInfo("TACTICS");
    }

    public void SetTurns(int turns)
    {
        turnText.text = turns.ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.text = numinous.ToString();
    }

    private void CallSkillAlchemy()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
        var listData = battleSelectCharacter.ActionData;
        if (listData != null)
        {
            var data = (SkillInfo)listData;
            eventData.templete = listData;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
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
        if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Activate();
    }

    public void DeactivateTacticsCommand()
    {
        if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Deactivate();
    }

    public void SetSideMenu(List<SystemData.CommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }
    
    public void ActivateSideMenu()
    {
        SetHelpInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        SetHelpInputInfo("TACTICS");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        SetHelpInputInfo("SIDEMENU");
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
        SetHelpInputInfo("TACTICS");
        UpdateHelpWindow();
    }

    private void CallOpenSideMenu()
    {
        var eventData = new TacticsViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
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
    
    private void UpdateHelpWindow()
    {
        var listData = tacticsCommandList.ListData;
        if (listData != null)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            HelpWindow.SetHelpText(commandData.Help);
        }
    }
}

namespace Tactics
{
    public enum CommandType
    {
        None = 0,
        AddAlcana,
        TacticsCommand,
        SelectTacticsActor, // アクターを決定
        TacticsCommandClose,
        SelectActorAlchemy,
        SelectAlchemyClose,
        SkillAlchemy,
        SelectRecoveryPlus,
        SelectRecoveryMinus,
        SelectBattleEnemy,
        PopupSkillInfo,
        EnemyClose,
        SelectActorResource,
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