using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RebornResult;
using Effekseer;
using TMPro;

public class RebornResultView : BaseView
{   
    [SerializeField] private StrategyActorList strategyActorList = null;
    [SerializeField] private GetItemList strategyResultList = null; 
    [SerializeField] private BaseList commandList = null; 
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;


    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

    private new System.Action<RebornResultViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        commandList.Initialize();
        var prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        new RebornResultPresenter(this);
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
    }

    public void StartAnimation()
    {
        _battleStartAnim.SetText(DataSystem.GetTextData(17040).Text);
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }

    public void SetHelpWindow(){
        HelpWindow.SetHelpText(DataSystem.GetTextData(17030).Text);
        HelpWindow.SetInputInfo("RESULT");
    }

    public void SetEvent(System.Action<RebornResultViewEvent> commandData)
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
        var eventData = new RebornResultViewEvent(CommandType.RebornResultClose);
        eventData.template = commandType;
        _commandData(eventData);
    }

    public void StartRebornResultAnimation(List<ListData> actorInfos)
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
            var eventData = new RebornResultViewEvent(CommandType.EndAnimation);
            _commandData(eventData);
            commandList.gameObject.SetActive(true);
        }
    }


}

namespace RebornResult
{
    public enum CommandType
    {
        None = 0,

        EndAnimation = 2,
        RebornResultClose = 5,
        

    }
}
public class RebornResultViewEvent
{
    public RebornResult.CommandType commandType;
    public object template;

    public RebornResultViewEvent(RebornResult.CommandType type)
    {
        commandType = type;
    }
}
