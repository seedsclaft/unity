using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Strategy;
using Effekseer;
using TMPro;

public class StrategyView : BaseView
{
    [SerializeField] private StrategyActorList strategyActorList = null; 
    [SerializeField] private GetItemList strategyResultList = null; 
    [SerializeField] private TacticsEnemyList tacticsEnemyList = null; 
    [SerializeField] private StrategyStrengthList strategyStrengthList = null; 
    [SerializeField] private EffekseerEmitter effekseerEmitter = null; 
    [SerializeField] private SpriteRenderer backGround = null; 
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private TextMeshProUGUI title = null; 
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private Button lvUpStatusButton = null;
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;
    private HelpWindow _helpWindow = null;

    private new System.Action<StrategyViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        strategyStrengthList.Initialize(() => CallLvUpNext());
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        lvUpStatusButton.onClick.AddListener(() => CallLvUpNext());
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
        _animationBusy = true;
    }

    public void ShowLvUpActor(ActorInfo actorInfo)
    {
        strategyStrengthList.gameObject.SetActive(true);
        lvUpStatusButton.gameObject.SetActive(true);
        actorInfoComponent.gameObject.SetActive(true);
        actorInfoComponent.UpdateInfo(actorInfo,null);
        strategyStrengthList.Refresh(actorInfo);
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
        tacticsEnemyList.Initialize(null,null,(a) => CallPopupSkillInfo(a),(a) => OnClickEnemyInfo(a));
        tacticsEnemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallBattleCommand(confirmCommands));
        SetInputHandler(tacticsEnemyList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.gameObject.SetActive(false);
    }

    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(14010).Text);
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
    }

    public void SetResultList(List<SystemData.MenuCommandData> confirmCommands)
    {
        strategyResultList.Initialize();
        strategyResultList.gameObject.SetActive(false);
        strategyResultList.InitializeConfirm(confirmCommands,(confirmCommands) => CallResultCommand(confirmCommands));
        SetInputHandler(strategyResultList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
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

    public void StartResultAnimation(List<ActorInfo> actorInfos)
    {
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.StartResultAnimation(actorInfos,() => {
            CallEndAnimation();
        });
    }

    private void CallEndAnimation(){
        var eventData = new StrategyViewEvent(CommandType.EndAnimation);
        _commandData(eventData);
    }

    public void ShowResultList(List<GetItemInfo> getItemInfos)
    {
        //SetInputHandler(strategyResultList.GetComponent<IInputHandlerEvent>());
        strategyResultList.Refresh(getItemInfos);
        strategyResultList.gameObject.SetActive(true);
    }

    private void CallResultCommand(TacticsComandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.ResultClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void HideResultList()
    {
        strategyResultList.gameObject.SetActive(false);
    }

    public void ShowEnemyList(TroopInfo troopInfo)
    {
        List<TroopInfo> troopInfos = new List<TroopInfo>();
        troopInfos.Add(troopInfo);
        tacticsEnemyList.Refresh(troopInfos);
        tacticsEnemyList.gameObject.SetActive(true);
    }

    private void CallPopupSkillInfo(int skillId)
    {
        var eventData = new StrategyViewEvent(CommandType.PopupSkillInfo);
        eventData.templete = skillId;
        _commandData(eventData);
    }

    private void OnClickEnemyInfo(int enemyIndex)
    {
        var eventData = new StrategyViewEvent(CommandType.CallEnemyInfo);
        eventData.templete = enemyIndex;
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
