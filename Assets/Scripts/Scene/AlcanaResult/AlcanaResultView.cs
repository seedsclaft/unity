using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlcanaResult;

public class AlcanaResultView : BaseView
{
    [SerializeField] private StrategyActorList strategyActorList = null;
    [SerializeField] private GetItemList strategyResultList = null; 
    [SerializeField] private BaseList commandList = null; 
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;
    
    private new System.Action<AlcanaResultViewEvent> _commandData = null;
    public override void Initialize() 
    {
        base.Initialize();
        
        var prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        new AlcanaResultPresenter(this);
    }
    
    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
    }

    public void StartAnimation()
    {
        _battleStartAnim.SetText(DataSystem.GetTextData(19010).Text);
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }

    public void SetHelpWindow(){
        HelpWindow.SetHelpText(DataSystem.GetTextData(19020).Text);
        HelpWindow.SetInputInfo("RESULT");
    }

    public void SetEvent(System.Action<AlcanaResultViewEvent> commandData)
    {
        _commandData = commandData;
    }
    public void SetResultList(List<ListData> confirmCommands)
    {
        SetInputHandler(strategyResultList.GetComponent<IInputHandlerEvent>());
        strategyResultList.Deactivate();
        strategyResultList.gameObject.SetActive(false);
        strategyResultList.InitializeConfirm(confirmCommands,(a) => CallResultCommand(a));
        SetInputHandler(strategyResultList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        strategyResultList.TacticsCommandList.Deactivate();
    }
    
    private void CallResultCommand(ConfirmCommandType commandType)
    {
        var eventData = new AlcanaResultViewEvent(CommandType.ResultClose);
        eventData.template = commandType;
        _commandData(eventData);
    }

    public void StartResultAnimation(List<ListData> actorInfos)
    {
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.SetData(actorInfos);
        strategyActorList.StartResultAnimation(actorInfos.Count,new List<bool>{false},() => {
        });
    }


    public void ShowResultList(List<ListData> getItemInfos)
    {
        strategyResultList.Deactivate();
        strategyResultList.SetData(getItemInfos);
        strategyResultList.gameObject.SetActive(true);
        strategyResultList.Activate();
        strategyResultList.TacticsCommandList.Activate();
        strategyResultList.TacticsCommandList.UpdateSelectIndex(1);
        HelpWindow.SetInputInfo("STRATEGY");
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
            var eventData = new AlcanaResultViewEvent(CommandType.EndAnimation);
            _commandData(eventData);
            commandList.gameObject.SetActive(true);
        }
    }
}

namespace AlcanaResult
{
    public enum CommandType
    {
        None = 0,
        EndAnimation = 2,
        ResultClose = 5,
    }
}

public class AlcanaResultViewEvent
{
    public AlcanaResult.CommandType commandType;
    public object template;

    public AlcanaResultViewEvent(AlcanaResult.CommandType type)
    {
        commandType = type;
    }
}