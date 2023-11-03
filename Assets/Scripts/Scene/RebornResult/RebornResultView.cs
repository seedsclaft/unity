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
    [SerializeField] private TacticsCommandList commandList = null; 
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;


    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

    private new System.Action<RebornResultViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        
        GameObject prefab = Instantiate(animPrefab);
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
        _battleStartAnim.SetText("思念継承");
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }


    public void SetUiView()
    {
    }


    public void SetHelpWindow(){
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(17030).Text);
        HelpWindow.SetInputInfo("RESULT");
    }



    public void SetEvent(System.Action<RebornResultViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetResultList(List<SystemData.CommandData> confirmCommands)
    {
        strategyResultList.Initialize();
        SetInputHandler(strategyResultList.GetComponent<IInputHandlerEvent>());
        strategyResultList.Deactivate();
        strategyResultList.gameObject.SetActive(false);
        strategyResultList.InitializeConfirm(confirmCommands,(a) => CallResultCommand(a));
        SetInputHandler(strategyResultList.TacticsCommandList.GetComponent<IInputHandlerEvent>());
        strategyResultList.TacticsCommandList.Deactivate();
        strategyResultList.TacticsCommandList.SetDisable(confirmCommands[0],true);
    }
    
    private void CallResultCommand(TacticsComandType commandType)
    {
        var eventData = new RebornResultViewEvent(CommandType.RebornResultClose);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void StartRebornResultAnimation(List<ActorInfo> actorInfos)
    {
        strategyActorList.gameObject.SetActive(true);
        strategyActorList.StartResultAnimation(actorInfos,new List<bool>{false},() => {
        });
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
    public object templete;

    public RebornResultViewEvent(RebornResult.CommandType type)
    {
        commandType = type;
    }
}
