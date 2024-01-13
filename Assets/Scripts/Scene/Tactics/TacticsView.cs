using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tactics;
using TMPro;

public class TacticsView : BaseView
{
    [SerializeField] private Button enemyListBackCommand = null;
    [SerializeField] private BaseList tacticsCommandList = null;
    [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;
    [SerializeField] private GameObject charaLayerBG = null;


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

    [SerializeField] private Button alcanaButton = null;


    private CommandType _lastCallEventType = CommandType.None;

    public override void Initialize()
    {
        base.Initialize();

        battleSelectCharacter.Initialize();
        SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();

        tacticsAlcana.gameObject.SetActive(false);
        alcanaButton.onClick.AddListener(() => CallAlcanaCheck());
        enemyListBackCommand.onClick.AddListener(() => OnClickEnemyListClose());
        
        new TacticsPresenter(this);
    }
    
    private void InitializeSelectCharacter()
    {
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAlchemy());
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(battleSelectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        battleSelectCharacter.HideActionList();
        charaLayerBG.SetActive(false);
    }

    public void SetUIButton()
    {
        SetBackCommand(() => OnClickBack());
        tacticsEnemyList.SetInputHandler(InputKeyType.Decide,() => CallBattleEnemy());
        tacticsEnemyList.SetInputHandler(InputKeyType.Option1,() => OnClickEnemyInfo());
        tacticsEnemyList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(tacticsEnemyList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.SetInputCallHandler();
    }

    private void OnClickBack()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.Back);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    private void OnClickEnemyListClose()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.EnemyClose);
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void SetActiveEnemyListClose(bool isActive)
    {
        enemyListBackCommand.gameObject.SetActive(isActive);
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
        tacticsCommandList.SetData(menuCommands);
        tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallTacticsCommand());
        tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
        tacticsCommandList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickDropout());
        tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
        SetInputHandler(tacticsCommandList.GetComponent<IInputHandlerEvent>());
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
        if (listData != null && listData.Enable)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
            eventData.template = commandData.Id;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    public void SetSelectCharacter(List<ActorInfo> actorInfos,List<ListData> confirmCommands,Dictionary<TacticsCommandType, int> commandRankInfo)
    {
        selectCharacter.Initialize(actorInfos.Count);
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        selectCharacter.SetTacticsCommand(confirmCommands);
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallRecoveryPlus());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallRecoveryMinus());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
        selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
        selectCharacter.SetInputHandlerCommand(InputKeyType.Cancel,() => CallTrainCommandCancel());
        SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
        SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
        HideSelectCharacter();

        tacticsCharaLayer.SetData(actorInfos);
        SetInputHandler(tacticsCharaLayer.GetComponent<IInputHandlerEvent>());
    }

    public void SetEnemies(List<ListData> troopInfos)
    {
        tacticsEnemyList.SetData(troopInfos);
        tacticsEnemyList.SetInfoHandler((a) => OnClickEnemyInfo(a));
        HideEnemyList();
    }

    public void CommandRefresh(TacticsCommandType tacticsCommandType)
    {
        switch (tacticsCommandType)
        {
            case TacticsCommandType.Train:
            case TacticsCommandType.Alchemy:
            case TacticsCommandType.Recovery:
            case TacticsCommandType.Battle:
            case TacticsCommandType.Resource:
            selectCharacter.Refresh();
            return;
        }
    }


    private void CallActorTrain()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            var eventData = new TacticsViewEvent(CommandType.SelectTacticsActor);
            eventData.template = data;
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
            var commandType = ConfirmCommandType.No;
            if (data.Key == "Yes")
            {
                commandType = ConfirmCommandType.Yes;
            }
            var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
            eventData.template = commandType;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallTrainCommandCancel()
    {
        if (_lastCallEventType != CommandType.None) return;
        var eventData = new TacticsViewEvent(CommandType.TacticsCommandClose);
        eventData.template = ConfirmCommandType.No;
        _commandData(eventData);
        _lastCallEventType = eventData.commandType;
    }

    public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
    {
        selectCharacter.SetTacticsCharacter(tacticsActorInfo);
        selectCharacter.SetTacticsCommandData(tacticsCommandData);
        selectCharacter.UpdateSmoothSelect();
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
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        battleSelectCharacter.SetSkillInfos(learnMagicList);
        battleSelectCharacter.ShowActionList();
        charaLayerBG.SetActive(true);
        battleSelectCharacter.HideStatus();
        battleSelectCharacter.MagicList.Activate();
        SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
    }

    public void HideAttributeList()
    {
        battleSelectCharacter.HideActionList();
        charaLayerBG.SetActive(false);
        battleSelectCharacter.MagicList.Deactivate();
        
        SetHelpInputInfo("ALCHEMY");
    }

    private void CallRecoveryPlus()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            if (data.TacticsCommandType != TacticsCommandType.Recovery)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectRecoveryPlus);
            eventData.template = data.ActorInfo.ActorId;
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
            if (data.TacticsCommandType != TacticsCommandType.Recovery)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectRecoveryMinus);
            eventData.template = data.ActorInfo.ActorId;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallBattleEnemy()
    {
        if (_lastCallEventType != CommandType.None) return;
        if (tacticsEnemyList.IsSelectEnemy())
        {
            var listIndex = tacticsEnemyList.Index;
            if (listIndex > -1)
            {
                var eventData = new TacticsViewEvent(CommandType.SelectBattleEnemy);
                eventData.template = listIndex;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        } else
        {
            var getItemInfo = tacticsEnemyList.GetItemInfo();
            if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
            {
                var eventData = new TacticsViewEvent(CommandType.PopupSkillInfo);
                eventData.template = getItemInfo;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        }
    }

    private void OnClickEnemyInfo(int selectIndex = -1)
    {
        var index = tacticsEnemyList.Index;
        if (index > -1)
        {
            if (selectIndex > -1)
            {
                index = selectIndex;
            }
            var eventData = new TacticsViewEvent(CommandType.CallEnemyInfo);
            eventData.template = index;
            _commandData(eventData);
        }
    }

    public void ShowEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(true);
        tacticsEnemyList.ResetInputFrame(1);
        charaLayerBG.SetActive(true);
        SetHelpInputInfo("ENEMY_SELECT");
    }

    public void HideEnemyList()
    {
        tacticsEnemyList.gameObject.SetActive(false);
        charaLayerBG.SetActive(false);
        SetHelpInputInfo("TACTICS");
    }

    public void SetTurns(int turns)
    {
        turnText.text = (turns).ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.text = numinous.ToString();
    }

    private void CallSkillAlchemy()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = battleSelectCharacter.ActionData;
        if (listData != null)
        {
            var eventData = new TacticsViewEvent(CommandType.SkillAlchemy);
            var data = (SkillInfo)listData;
            eventData.template = listData;
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
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CommandCloseSideMenu());
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

    public new void CommandOpenSideMenu()
    {
        base.CommandOpenSideMenu();
        sideMenuList.OpenSideMenu();
        tacticsCommandList.Deactivate();
    }

    public void CommandCloseSideMenu()
    {
        tacticsCommandList.Activate();
        sideMenuList.CloseSideMenu();
        SetHelpInputInfo("TACTICS");
        UpdateHelpWindow();
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new TacticsViewEvent(CommandType.SelectSideMenu);
        eventData.template = sideMenu;
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
    
    public void StartAlcanaAnimation(System.Action endEvent)
    {
        tacticsAlcana.StartAlcanaAnimation(endEvent);
    }

    private void CallAlcanaCheck()
    {
        var eventData = new TacticsViewEvent(CommandType.AlcanaCheck);
        _commandData(eventData);
    }
}