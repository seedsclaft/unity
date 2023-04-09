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
    [SerializeField] private EffekseerEmitter effekseerEmitter = null; 
    [SerializeField] private SpriteRenderer backGround = null; 
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private TextMeshProUGUI title = null; 
    private HelpWindow _helpWindow = null;

    private new System.Action<StrategyViewEvent> _commandData = null;

    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new StrategyPresenter(this);
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetUiView()
    {
        tacticsEnemyList.Initialize(null,null,null);
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

    public void ShowEnemyList(TroopInfo troopInfo,List<SystemData.MenuCommandData> confirmCommands)
    {
        List<TroopInfo> troopInfos = new List<TroopInfo>();
        troopInfos.Add(troopInfo);
        tacticsEnemyList.Refresh(troopInfos);
        //SetInputHandler(tacticsEnemyList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.InitializeConfirm(confirmCommands,(confirmCommands) => CallBattleCommand(confirmCommands));
        SetInputHandler(tacticsEnemyList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        tacticsEnemyList.gameObject.SetActive(true);
    }

    private void CallBattleCommand(TacticsComandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.BattleClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }
}

namespace Strategy
{
    public enum CommandType
    {
        None = 0,
        StartStretegy = 1,
        EndAnimation = 2,
        ResultClose = 3,
        BattleClose = 4
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
