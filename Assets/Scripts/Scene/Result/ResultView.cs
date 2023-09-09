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
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    [SerializeField] private RebornActorList actorInfoList = null;
    public int ActorInfoListIndex => actorInfoList.Index;

    [SerializeField] private RebornSkillList rebornSkillList = null;    
    [SerializeField] private ActorInfoComponent actorInfoComponent = null; 
    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

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
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(16030).Text);
        HelpWindow.SetInputInfo("RESULT");
    }


    public void SetResultList(List<SystemData.CommandData> confirmCommands)
    {
        commandList.Initialize((confirmCommands) => CallResultCommand(confirmCommands));
        commandList.Refresh(confirmCommands);
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        commandList.gameObject.SetActive(false);
    }

    public void UpdateResultCommand(List<SystemData.CommandData> confirmCommands)
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

    public void CommandActorAssign()
    {
        actorInfoList.gameObject.SetActive(true);
        actorInfoComponent.gameObject.SetActive(true);
        rebornSkillList.gameObject.SetActive(true);
        endingTypeObj.SetActive(false);
        evaluateObj.SetActive(false);
        playerNameObj.SetActive(false);
        rankingObj.SetActive(false);
        rankingInfo.gameObject.SetActive(false);
        evaluateNew.gameObject.SetActive(false);
        commandList.gameObject.SetActive(false);
    }
    
    public void SetActorList(List<ActorInfo> actorInfos,List<int> disableIndexs) 
    {
        actorInfoList.Initialize(actorInfos,disableIndexs);
        actorInfoList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor(disableIndexs));
        actorInfoList.SetInputHandler(InputKeyType.Cancel,() => CallCancelActor());
        actorInfoList.SetInputHandler(InputKeyType.Down,() => CallUpdate());
        actorInfoList.SetInputHandler(InputKeyType.Up,() => CallUpdate());
        actorInfoList.Refresh();
        SetInputHandler(actorInfoList.GetComponent<IInputHandlerEvent>());
        actorInfoList.Activate();
    }

    private void CallDecideActor(List<int> disableIndexs)
    {
        var eventData = new ResultViewEvent(CommandType.DecideActor);
        if (actorInfoList.Index > -1 && !disableIndexs.Contains(actorInfoList.Index))
        {
            eventData.templete = actorInfoList.Index;
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
        actorInfoComponent.Clear();
        actorInfoComponent.UpdateInfo(actorInfo,null);
        rebornSkillList.Initialize(actorInfo.RebornSkillInfos);
        rebornSkillList.SetInputHandler(InputKeyType.SideLeft1,() => OnPageUpRebornSkill());
        rebornSkillList.SetInputHandler(InputKeyType.SideRight1,() => OnPageDownRebornSkill());
        SetInputHandler(rebornSkillList.GetComponent<IInputHandlerEvent>());
        rebornSkillList.Refresh();
    }

    private void OnPageUpRebornSkill()
    {
        if (rebornSkillList.Data != null && rebornSkillList.Data.Count < 4) return;
        var margin = 1.0f / (rebornSkillList.Data.Count - 4);

        var value = rebornSkillList.ScrollRect.normalizedPosition.y - margin;
        if ((rebornSkillList.Data.Count - 4) == 0)
        {
            value = 0;
        }
        rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,value);
        if (rebornSkillList.ScrollRect.normalizedPosition.y < 0)
        {
            rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,0);
        }
    }

    private void OnPageDownRebornSkill()
    {
        if (rebornSkillList.Data != null && rebornSkillList.Data.Count < 4) return;
        var margin = 1.0f / (rebornSkillList.Data.Count - 4);

        var value = rebornSkillList.ScrollRect.normalizedPosition.y + margin;
        if ((rebornSkillList.Data.Count - 4) == 0)
        {
            value = 1;
        }
        rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,value);
        if (rebornSkillList.ScrollRect.normalizedPosition.y > 1)
        {
            rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,1);
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
        DecideActor = 11,
        CancelActor = 12,
        UpdateActor = 13,
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
