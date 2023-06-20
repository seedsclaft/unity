using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Result;
using Effekseer;
using TMPro;

public class ResultView : BaseView
{   
    [SerializeField] private StrategyActorList strategyActorList = null;
    [SerializeField] private TacticsCommandList commandList = null; 
    [SerializeField] private GameObject endingTypeObj = null;
    [SerializeField] private TextMeshProUGUI endingType = null; 
    [SerializeField] private GameObject evaluateObj = null;
    [SerializeField] private TextMeshProUGUI evaluateValue = null; 
    [SerializeField] private TextMeshProUGUI evaluateNew = null; 
    [SerializeField] private GameObject playerNameObj = null;
    [SerializeField] private TextMeshProUGUI playerName = null; 
    [SerializeField] private GameObject rankingObj = null;
    [SerializeField] private TextMeshProUGUI rankingInfo = null; 
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;
    private HelpWindow _helpWindow = null;

    private new System.Action<ResultViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        endingTypeObj.SetActive(false);
        evaluateObj.SetActive(false);
        playerNameObj.SetActive(false);
        rankingObj.SetActive(false);
        rankingInfo.gameObject.SetActive(false);
        evaluateNew.gameObject.SetActive(false);
        new ResultPresenter(this);
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
    }
    
    public void SetEvaluate(int evalutate,bool isNew)
    {
        evaluateValue.text = evalutate.ToString();
        evaluateObj.SetActive(true);
        evaluateNew.gameObject.SetActive(isNew);
    }

    public void SetEndingType(string endType)
    {
        endingType.text = endType;
        endingTypeObj.SetActive(true);
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
        playerNameObj.SetActive(true);
        rankingObj.SetActive(true);
    }

    public void SetRanking(string ranking)
    {
        rankingInfo.text = ranking;
        rankingInfo.gameObject.SetActive(true);
    }

    public void StartAnimation()
    {
        _battleStartAnim.SetText("終焉回帰");
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }


    public void SetUiView()
    {
    }


    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(16030).Text);
    }


    public void SetResultList(List<SystemData.MenuCommandData> confirmCommands)
    {
        commandList.Initialize((confirmCommands) => CallResultCommand(confirmCommands));
        commandList.Refresh(confirmCommands);
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        commandList.gameObject.SetActive(false);
    }

    public void UpdateResultCommand(List<SystemData.MenuCommandData> confirmCommands)
    {
        commandList.Refresh(confirmCommands);
    }

    public void SetEvent(System.Action<ResultViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void StartResultAnimation(List<ActorInfo> actorInfos)
    {
        var bonusDummy = new List<bool>();
        foreach (var item in actorInfos)
        {
            bonusDummy.Add(false);
        }
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.StartResultAnimation(actorInfos,bonusDummy,() => {
        });
    }




    private void CallResultCommand(TacticsComandType commandType)
    {
        var eventData = new ResultViewEvent(CommandType.ResultClose);
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
            var eventData = new ResultViewEvent(CommandType.EndAnimation);
            _commandData(eventData);
            commandList.gameObject.SetActive(true);
        }
    }
}

namespace Result
{
    public enum CommandType
    {
        None = 0,
        StartResult = 1,
        EndAnimation = 2,
        ResultClose = 5,
        
        EndLvupAnimation = 9,
    }
}
public class ResultViewEvent
{
    public Result.CommandType commandType;
    public object templete;

    public ResultViewEvent(Result.CommandType type)
    {
        commandType = type;
    }
}