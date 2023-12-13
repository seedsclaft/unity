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
    [SerializeField] private BaseList commandList = null; 
    [SerializeField] private GameObject resultMain = null;
    [SerializeField] private TextMeshProUGUI endingType = null; 
    [SerializeField] private TextMeshProUGUI evaluateValue = null; 
    [SerializeField] private TextMeshProUGUI evaluateNew = null; 
    [SerializeField] private TextMeshProUGUI playerName = null; 
    [SerializeField] private TextMeshProUGUI rankingInfo = null; 
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    [SerializeField] private BaseList actorInfoList = null;
    public int ActorInfoListIndex => actorInfoList.Index;

    [SerializeField] private BattleSelectCharacter selectCharacter = null; 
    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

    private new System.Action<ResultViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        resultMain.SetActive(false);
        rankingInfo.gameObject.SetActive(false);
        evaluateNew.gameObject.SetActive(false);
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();
        actorInfoList.gameObject.SetActive(false);
        selectCharacter.gameObject.SetActive(false);
        new ResultPresenter(this);
    }

    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallDecideActor());
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => {});
        selectCharacter.SetInputHandlerAction(InputKeyType.Start,() => CallDecideActor());

        SetInputHandler(selectCharacter.DeckMagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
        selectCharacter.HideStatus();
        selectCharacter.gameObject.SetActive(false);
    }

    public void StartAnimation()
    {
        _battleStartAnim.SetText("終焉回帰");
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize(actorInfos.Count);
        strategyActorList.gameObject.SetActive(false);
    }
    
    public void SetEvaluate(int evaluate,bool isNew)
    {
        evaluateValue.text = evaluate.ToString();
        evaluateNew.gameObject.SetActive(isNew);
    }

    public void SetEndingType(string endType)
    {
        endingType.text = endType;
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
        resultMain.SetActive(true);
    }

    public void SetRanking(string ranking)
    {
        rankingInfo.text = ranking;
        rankingInfo.gameObject.SetActive(true);
    }

    public void SetHelpWindow(){
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(16030).Text);
        HelpWindow.SetInputInfo("RESULT");
    }

    public void SetResultList(List<ListData> confirmCommands)
    {
        commandList.SetData(confirmCommands);
        commandList.SetInputHandler(InputKeyType.Decide,() => {
            if (commandList.ListData.Enable == false)
            {
                return;
            }
            var data = (SystemData.CommandData)commandList.ListData.Data;
            if (data.Key == "Yes")
            {
                CallResultCommand(ConfirmCommandType.Yes);
            } else
            if (data.Key == "No")
            {
                CallResultCommand(ConfirmCommandType.No);
            }
        });
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        commandList.gameObject.SetActive(false);
    }

    public void UpdateResultCommand(List<ListData> confirmCommands)
    {
        commandList.SetData(confirmCommands);
    }

    public void SetEvent(System.Action<ResultViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void StartResultAnimation(List<ListData> actorInfos)
    {
        var bonusDummy = new List<bool>();
        foreach (var item in actorInfos)
        {
            bonusDummy.Add(false);
        }
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.StartResultAnimation(actorInfos.Count,bonusDummy,() => {
        });
    }

    private void CallResultCommand(ConfirmCommandType commandType)
    {
        var eventData = new ResultViewEvent(CommandType.ResultClose);
        eventData.template = commandType;
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

    public void CommandActorAssign()
    {
        actorInfoList.gameObject.SetActive(true);
        selectCharacter.gameObject.SetActive(true);
        resultMain.SetActive(false);
        rankingInfo.gameObject.SetActive(false);
        evaluateNew.gameObject.SetActive(false);
        commandList.gameObject.SetActive(false);
    }
    
    public void SetActorList(List<ListData> actorInfos) 
    {
        actorInfoList.Initialize(actorInfos.Count);
        actorInfoList.SetData(actorInfos);
        actorInfoList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor());
        actorInfoList.SetInputHandler(InputKeyType.Cancel,() => CallCancelActor());
        actorInfoList.SetInputHandler(InputKeyType.Down,() => CallUpdate());
        actorInfoList.SetInputHandler(InputKeyType.Up,() => CallUpdate());
        actorInfoList.Refresh();
        SetInputHandler(actorInfoList.GetComponent<IInputHandlerEvent>());
        actorInfoList.Activate();
    }

    private void CallDecideActor()
    {
        var listData = actorInfoList.ListData;
        if (listData != null)
        {
            var data = (ActorInfo)listData.Data;
            var eventData = new ResultViewEvent(CommandType.DecideActor);
            eventData.template = actorInfoList.Index;
            _commandData(eventData);
        }
    }
    
    private void CallCancelActor()
    {
        var eventData = new ResultViewEvent(CommandType.CancelActor);
        _commandData(eventData);
    }

    private void CallUpdate()
    {
        var eventData = new ResultViewEvent(CommandType.UpdateActor);
        _commandData(eventData);
    }

    public void UpdateActor(ActorInfo actorInfo)
    {
    }

    public void CommandRefreshStatus(List<ListData> skillInfos,ActorInfo actorInfo,List<ActorInfo> party,int lastSelectIndex)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
        selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        selectCharacter.ShowActionList();
        selectCharacter.DeckMagicList.Activate();
        selectCharacter.SetActorThumbOnly(actorInfo);
        selectCharacter.SetActorInfo(actorInfo,party);
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction(lastSelectIndex);
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
        
        EndLvUpAnimation = 9,
        DecideActor = 11,
        CancelActor = 12,
        UpdateActor = 13,
    }
}
public class ResultViewEvent
{
    public Result.CommandType commandType;
    public object template;

    public ResultViewEvent(Result.CommandType type)
    {
        commandType = type;
    }
}
