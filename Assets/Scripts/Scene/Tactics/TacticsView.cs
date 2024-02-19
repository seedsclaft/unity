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

    
    [SerializeField] private TacticsSymbolList tacticsSymbolList = null;
    [SerializeField] private StageInfoComponent stageInfoComponent = null;
    [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;

    [SerializeField] private TextMeshProUGUI turnText = null;
    [SerializeField] private TextMeshProUGUI numinousText = null;
    private new System.Action<TacticsViewEvent> _commandData = null;
    [SerializeField] private TacticsAlcana tacticsAlcana = null;

    [SerializeField] private SideMenuList sideMenuList = null;

    [SerializeField] private Button alcanaButton = null;
    
    [SerializeField] private GameObject backGround = null;
    public void SetActiveBackGround(bool isActive)
    {
        backGround.SetActive(isActive);
    }


    private CommandType _lastCallEventType = CommandType.None;

    public SkillInfo SelectMagic => battleSelectCharacter.ActionData;

    public override void Initialize()
    {
        base.Initialize();

        battleSelectCharacter.Initialize();
        SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();

        tacticsAlcana.gameObject.SetActive(false);
        alcanaButton.onClick.AddListener(() => CallAlcanaCheck());
        
        tacticsCommandList.Initialize();
        tacticsSymbolList.Initialize();
        battleSelectCharacter.gameObject.SetActive(false);
        new TacticsPresenter(this);
        selectCharacter.gameObject.SetActive(false);
    }
    
    private void InitializeSelectCharacter()
    {
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAlchemy());
        battleSelectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(battleSelectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        battleSelectCharacter.HideActionList();
    }

    public void SetUIButton()
    {
        SetBackCommand(() => OnClickBack());
        tacticsSymbolList.SetInputHandler(InputKeyType.Decide,() => CallBattleEnemy());
        tacticsSymbolList.SetInputHandler(InputKeyType.Option1,() => OnClickEnemyInfo());
        tacticsSymbolList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(tacticsSymbolList.GetComponent<IInputHandlerEvent>());
        tacticsSymbolList.SetInputCallHandler();
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
        var eventData = new TacticsViewEvent(CommandType.SymbolClose);
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
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var eventData = new TacticsViewEvent(CommandType.TacticsCommand);
            eventData.template = commandData.Id;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    public void SetSelectCharacter(List<ListData> actorInfos,List<ListData> confirmCommands)
    {
        selectCharacter.Initialize();
        selectCharacter.SetCharacterData(actorInfos);
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        selectCharacter.SetTacticsCommand(confirmCommands);
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallFrontBattleIndex());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallBattleBackIndex());
        selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
        selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
        selectCharacter.SetInputHandlerCommand(InputKeyType.Cancel,() => CallTrainCommandCancel());
        SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
        SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
        selectCharacter.CharacterList.SetSelectedHandler(() => {
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                var eventData = new TacticsViewEvent(CommandType.ChangeSelectTacticsActor);
                eventData.template = data.ActorInfo.ActorId;
                _commandData(eventData);
            }
        });
        HideSelectCharacter();

    }

    public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
    {
        tacticsCharaLayer.SetData(actorInfos);
    }

    public void SetSymbols(List<ListData> symbolInfos)
    {
        tacticsSymbolList.SetData(symbolInfos);
        tacticsSymbolList.SetInfoHandler((a) => OnClickEnemyInfo(a));
        HideSymbolList();
    }

    public void CommandRefresh()
    {
        selectCharacter.Refresh();
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
        battleSelectCharacter.gameObject.SetActive(false);
        selectCharacter.gameObject.SetActive(false); 
        SetHelpInputInfo("TACTICS");
    }

    public void ShowSelectCharacterCommand()
    {
        selectCharacter.ShowCharacterList();
    }

    public void HideSelectCharacterCommand()
    {
        selectCharacter.HideCharacterList();
    }

    public void ShowConfirmCommand()
    {
        selectCharacter.ShowCommandList();
    }
    
    public void HideConfirmCommand()
    {
        selectCharacter.HideCommandList();
    }

    public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party)
    {
        battleSelectCharacter.gameObject.SetActive(true);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,true);
        battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Detail);
        battleSelectCharacter.SetActorInfo(actorInfo,party);
        battleSelectCharacter.SetSkillInfos(actorInfo.SkillActionList());
        SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
    }

    public void ShowLeaningList(List<ListData> learnMagicList)
    {
        battleSelectCharacter.gameObject.SetActive(true);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Magic);
        battleSelectCharacter.SetSkillInfos(learnMagicList);
        battleSelectCharacter.ShowActionList();
        battleSelectCharacter.HideStatus();
        battleSelectCharacter.MagicList.Activate();
        SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
    }

    public void HideAttributeList()
    {
        battleSelectCharacter.HideActionList();
        battleSelectCharacter.MagicList.Deactivate();
        
        SetHelpInputInfo("ALCHEMY");
    }

    private void CallFrontBattleIndex()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            if (data.TacticsCommandType != TacticsCommandType.Paradigm)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectFrontBattleIndex);
            eventData.template = data.ActorInfo.ActorId;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallBattleBackIndex()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = selectCharacter.CharacterData;
        if (listData != null)
        {
            var data = (TacticsActorInfo)listData.Data;
            if (data.TacticsCommandType != TacticsCommandType.Paradigm)
            {
                return;
            }
            var eventData = new TacticsViewEvent(CommandType.SelectBackBattleIndex);
            eventData.template = data.ActorInfo.ActorId;
            _commandData(eventData);
            _lastCallEventType = eventData.commandType;
        }
    }

    private void CallBattleEnemy()
    {
        if (_lastCallEventType != CommandType.None) return;
        if (tacticsSymbolList.IsSelectSymbol())
        {
            var listIndex = tacticsSymbolList.Index;
            if (listIndex > -1)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var eventData = new TacticsViewEvent(CommandType.SelectSymbol);
                eventData.template = listIndex;
                _commandData(eventData);
                _lastCallEventType = eventData.commandType;
            }
        } else
        {
            var getItemInfo = tacticsSymbolList.GetItemInfo();
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
        var index = tacticsSymbolList.Index;
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

    public void ShowSymbolList()
    {
        tacticsSymbolList.gameObject.SetActive(true);
        tacticsSymbolList.ResetInputFrame(1);
        ChangeBackCommandActive(true);
        SetHelpInputInfo("ENEMY_SELECT");
    }

    public void HideSymbolList()
    {
        tacticsSymbolList.gameObject.SetActive(false);
        ChangeBackCommandActive(false);
        SetHelpInputInfo("TACTICS");
    }

    public void SetTurns(int turns)
    {
        turnText.text = (turns).ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
    }

    private void CallSkillAlchemy()
    {
        if (_lastCallEventType != CommandType.None) return;
        var listData = battleSelectCharacter.ActionData;
        if (listData != null && listData.Enable)
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

    public void SetSideMenu(List<ListData> menuCommands){
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