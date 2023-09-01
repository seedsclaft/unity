using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Strategy;
using Effekseer;
using TMPro;
using DG.Tweening;

public class StrategyView : BaseView
{
    [SerializeField] private Image backgroundImage = null; 
    [SerializeField] private StrategyActorList strategyActorList = null; 
    [SerializeField] private GetItemList strategyResultList = null; 
    [SerializeField] private TacticsEnemyList tacticsEnemyList = null; 
    [SerializeField] private StrategyStrengthList strategyStrengthList = null; 
    [SerializeField] private TextMeshProUGUI title = null; 
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private Button lvUpStatusButton = null;
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;
    [SerializeField] private Toggle battleSkipToggle = null;

    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

    private new System.Action<StrategyViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        strategyStrengthList.Initialize(() => CallLvUpNext());
        SetInputHandler(strategyStrengthList.GetComponent<IInputHandlerEvent>());
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        lvUpStatusButton.onClick.AddListener(() => CallLvUpNext());
        lvUpStatusButton.gameObject.SetActive(false);
        battleSkipToggle.onValueChanged.AddListener((a) => OnChangeSkipToggle());
        battleSkipToggle.gameObject.SetActive(false);
        new StrategyPresenter(this);
    }

    private void CallLvUpNext()
    {
        lvUpStatusButton.gameObject.SetActive(false);
        actorInfoComponent.gameObject.SetActive(false);
        strategyStrengthList.gameObject.SetActive(false);
        var eventData = new StrategyViewEvent(CommandType.LvUpNext);
        _commandData(eventData);
    }

    public void StartLvUpAnimation()
    {
        _battleStartAnim.SetText("LevelUp!");
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }

    public void ShowLvUpActor(ActorInfo actorInfo)
    {
        strategyResultList.TacticsCommandList.Deactivate();
        strategyStrengthList.gameObject.SetActive(true);
        lvUpStatusButton.gameObject.SetActive(true);
        actorInfoComponent.gameObject.SetActive(true);
        actorInfoComponent.Clear();
        actorInfoComponent.UpdateInfo(actorInfo,null);
        strategyStrengthList.Refresh(actorInfo);
        HelpWindow.SetInputInfo("LEVELUP");
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetUiView()
    {
    }

    public void SetEnemyList(List<SystemData.MenuCommandData> confirmCommands)
    {
        tacticsEnemyList.Initialize(null);
        //tacticsEnemyList.SetInputHandler(InputKeyType.Decide,() => CallPopupSkillInfo());
        tacticsEnemyList.SetInputHandler(InputKeyType.Option1,() => OnClickEnemyInfo());
        tacticsEnemyList.SetInputHandler(InputKeyType.Option2,() => OnChangeSkipToggle());
        SetInputHandler(tacticsEnemyList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.InitializeConfirm(confirmCommands,(a) => CallBattleCommand(a));
        SetInputHandler(tacticsEnemyList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.gameObject.SetActive(false);
        tacticsEnemyList.Deactivate();
        tacticsEnemyList.TacticsCommandList.Deactivate();
    }

    public void SetHelpWindow(){
        HelpWindow.SetInputInfo("");
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(14010).Text);
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
    }

    public void SetResultList(List<SystemData.MenuCommandData> confirmCommands)
    {
        strategyResultList.Initialize();
        SetInputHandler(strategyResultList.GetComponent<IInputHandlerEvent>());
        strategyResultList.Deactivate();
        strategyResultList.gameObject.SetActive(false);
        strategyResultList.InitializeConfirm(confirmCommands,(a) => CallResultCommand(a));
        SetInputHandler(strategyResultList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        strategyResultList.TacticsCommandList.Deactivate();
    }

    public void SetEvent(System.Action<StrategyViewEvent> commandData)
    {
        _commandData = commandData;
    }
    

    public void SetCommandAble(SystemData.MenuCommandData commandData)
    {
        strategyResultList.TacticsCommandList.SetDisable(commandData,false);
        tacticsEnemyList.TacticsCommandList.SetDisable(commandData,false);
    }

    public void SetCommandDisable(SystemData.MenuCommandData commandData)
    {
        strategyResultList.TacticsCommandList.SetDisable(commandData,true);
        tacticsEnemyList.TacticsCommandList.SetDisable(commandData,true);
    }

    private void CallStrategyStart(){
        var eventData = new StrategyViewEvent(CommandType.StartStretegy);
        _commandData(eventData);
    }

    public void StartResultAnimation(List<ActorInfo> actorInfos,List<bool> isBonusList = null)
    {
        DeactivateAll();
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.StartResultAnimation(actorInfos,isBonusList,() => {
            CallEndAnimation();
        });
    }

    private void CallEndAnimation(){
        var eventData = new StrategyViewEvent(CommandType.EndAnimation);
        _commandData(eventData);
    }

    public void ShowResultList(List<GetItemInfo> getItemInfos)
    {
        strategyResultList.Deactivate();
        strategyResultList.Refresh(getItemInfos);
        strategyResultList.gameObject.SetActive(true);
        strategyResultList.Activate();
        strategyResultList.TacticsCommandList.Activate();
        strategyResultList.TacticsCommandList.UpdateSelectIndex(1);
        HelpWindow.SetInputInfo("STRATEGY");
    }

    private void CallResultCommand(TacticsComandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.ResultClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void HideResultList()
    {
        strategyResultList.Deactivate();
        strategyResultList.gameObject.SetActive(false);
    }

    public void ShowEnemyList(TroopInfo troopInfo,bool enableBattleSkip)
    {
        List<TroopInfo> troopInfos = new List<TroopInfo>();
        troopInfos.Add(troopInfo);
        tacticsEnemyList.Refresh(troopInfos);
        tacticsEnemyList.gameObject.SetActive(true);
        tacticsEnemyList.Activate();
        tacticsEnemyList.TacticsCommandList.Activate();
        tacticsEnemyList.TacticsCommandList.UpdateSelectIndex(1);
        tacticsEnemyList.UpdateSelectIndex(-1);
        tacticsEnemyList.SetDisableLeftRight();
        battleSkipToggle.gameObject.SetActive(enableBattleSkip);
        HelpWindow.SetInputInfo("STRATEGY_BATTLE");
    }

    private void CallPopupSkillInfo()
    {
        var eventData = new StrategyViewEvent(CommandType.PopupSkillInfo);
        var item = tacticsEnemyList.GetItemInfo;
        if (item != null)
        {
            eventData.templete = item;
            _commandData(eventData);
        }
    }

    private void OnClickEnemyInfo()
    {
        var eventData = new StrategyViewEvent(CommandType.CallEnemyInfo);
        _commandData(eventData);
    }

    private void OnChangeSkipToggle()
    {
        var eventData = new StrategyViewEvent(CommandType.ChangeSkipToggle);
        _commandData(eventData);
    }

    private void CallBattleCommand(TacticsComandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.BattleClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    private new void Update() {
        if (_animationBusy == true)
        {
            CheckAnimationBusy();
            return;
        }
        base.Update();
    }

    private void CheckAnimationBusy()
    {
        if (_battleStartAnim.IsBusy == false)
        {
            _animationBusy = false;
            var eventData = new StrategyViewEvent(CommandType.EndLvupAnimation);
            _commandData(eventData);
        }
    }

    public void EndShinyEffect()
    {
        strategyActorList.SetShinyRefrect(false);
    }

    public void FadeOut()
    {
        backgroundImage.DOFade(0,0.4f);
    }

    private void DeactivateAll()
    {
        
        tacticsEnemyList.Deactivate();
        tacticsEnemyList.TacticsCommandList.Deactivate();
        strategyResultList.Deactivate();
        strategyResultList.TacticsCommandList.Deactivate();
    }

    public void CommandChangeSkipToggle(bool isCheck)
    {
        if (battleSkipToggle.isOn != isCheck)
        {
            battleSkipToggle.isOn = isCheck;
        }
    }
}

namespace Strategy
{
    public enum CommandType
    {
        None = 0,
        StartStretegy = 1,
        EndAnimation = 2,
        PopupSkillInfo = 3,
        CallEnemyInfo = 4,
        ResultClose = 5,
        BattleClose = 6,
        LvUpNext = 7,
        LvUpActor = 8,
        
        EndLvupAnimation = 9,
        ChangeSkipToggle = 10,
    }
}
public class StrategyViewEvent
{
    public Strategy.CommandType commandType;
    public object templete;

    public StrategyViewEvent(Strategy.CommandType type)
    {
        commandType = type;
    }
}
