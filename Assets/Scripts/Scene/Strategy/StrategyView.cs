using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Strategy;
using TMPro;
using DG.Tweening;

public class StrategyView : BaseView
{
    [SerializeField] private Image backgroundImage = null; 
    [SerializeField] private StrategyActorList strategyActorList = null; 
    [SerializeField] private GetItemList strategyResultList = null; 
    [SerializeField] private BaseList statusList = null; 
    [SerializeField] private TextMeshProUGUI title = null; 
    [SerializeField] private ActorInfoComponent actorInfoComponent = null;
    [SerializeField] private Button lvUpStatusButton = null;
    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    private BattleStartAnim _battleStartAnim = null;
    private bool _animationBusy = false;

    private new System.Action<StrategyViewEvent> _commandData = null;

    private float _lastActorPosition = 0;
    public override void Initialize() 
    {
        base.Initialize();
        statusList.Initialize();
        statusList.SetInputHandler(InputKeyType.Decide,() => CallLvUpNext());
        SetInputHandler(statusList.GetComponent<IInputHandlerEvent>());
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        _battleStartAnim.gameObject.SetActive(false);
        lvUpStatusButton.onClick.AddListener(() => CallLvUpNext());
        lvUpStatusButton.gameObject.SetActive(false);
        
        var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0,0,0);
        actorInfoComponent.MainThumb.DOFade(0,0);
        new StrategyPresenter(this);
    }

    private void CallLvUpNext()
    {
        lvUpStatusButton.gameObject.SetActive(false);
        actorInfoComponent.gameObject.SetActive(false);
        statusList.gameObject.SetActive(false);
        var eventData = new StrategyViewEvent(CommandType.LvUpNext);
        _commandData(eventData);
    }

    public void StartLvUpAnimation()
    {
        _battleStartAnim.SetText(DataSystem.GetTextData(3080).Text);
        _battleStartAnim.StartAnim();
        _battleStartAnim.gameObject.SetActive(true);
        _animationBusy = true;
    }

    public void ShowLvUpActor(ActorInfo actorInfo,List<ListData> status)
    {
        strategyResultList.TacticsCommandList.Deactivate();
        statusList.gameObject.SetActive(true);
        statusList.Activate();
        lvUpStatusButton.gameObject.SetActive(true);
        actorInfoComponent.gameObject.SetActive(true);
        actorInfoComponent.Clear();
        actorInfoComponent.UpdateInfo(actorInfo,null);
        
        var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0,0,0);
        actorInfoComponent.MainThumb.DOFade(0,0);
        
        BaseAnimation.MoveAndFade(rect,actorInfoComponent.MainThumb,24,1);

        statusList.SetData(status);
        HelpWindow.SetInputInfo("LEVELUP");
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetHelpWindow(){
        HelpWindow.SetInputInfo("");
        HelpWindow.SetHelpText(DataSystem.GetTextData(14010).Text);
    }

    public void SetActors(List<ActorInfo> actorInfos)
    {
        strategyActorList.Initialize();
        strategyActorList.gameObject.SetActive(false);
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

    public void SetEvent(System.Action<StrategyViewEvent> commandData)
    {
        _commandData = commandData;
    }
    

    private void CallStrategyStart(){
        var eventData = new StrategyViewEvent(CommandType.StartStrategy);
        _commandData(eventData);
    }

    public void StartResultAnimation(List<ListData> actorInfos,List<bool> isBonusList = null)
    {
        DeactivateAll();
        strategyActorList.gameObject.SetActive(false);
        strategyActorList.SetData(actorInfos);
        strategyActorList.StartResultAnimation(actorInfos.Count,isBonusList,() => {
            CallEndAnimation();
        });
        strategyActorList.gameObject.SetActive(true);
    }

    private void CallEndAnimation(){
        var eventData = new StrategyViewEvent(CommandType.EndAnimation);
        _commandData(eventData);
    }

    public void ShowResultList(List<ListData> getItemInfos)
    {
        strategyResultList.Deactivate();
        strategyResultList.SetData(getItemInfos);
        strategyResultList.gameObject.SetActive(true);
        strategyResultList.Activate();
        strategyResultList.TacticsCommandList.Activate();
        strategyResultList.TacticsCommandList.UpdateSelectIndex(1);
        SetHelpInputInfo("STRATEGY");
    }

    private void CallResultCommand(ConfirmCommandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.ResultClose);
        eventData.template = commandType;
        _commandData(eventData);
    }

    public void HideResultList()
    {
        strategyResultList.Deactivate();
        strategyResultList.gameObject.SetActive(false);
    }

    

    private void OnChangeSkipToggle(bool needChangeView)
    {
        var eventData = new StrategyViewEvent(CommandType.ChangeSkipToggle);
        eventData.template = needChangeView;
        _commandData(eventData);
    }

    private void CallBattleCommand(ConfirmCommandType commandType)
    {
        var eventData = new StrategyViewEvent(CommandType.BattleClose);
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
            var eventData = new StrategyViewEvent(CommandType.EndLvUpAnimation);
            _commandData(eventData);
        }
    }

    public void EndShinyEffect()
    {
        strategyActorList.SetShinyReflect(false);
    }

    public void FadeOut()
    {
        backgroundImage.DOFade(0,0.4f);
    }

    private void DeactivateAll()
    {
        strategyResultList.Deactivate();
        strategyResultList.TacticsCommandList.Deactivate();
    }

}

namespace Strategy
{
    public enum CommandType
    {
        None = 0,
        StartStrategy = 1,
        EndAnimation = 2,
        PopupSkillInfo = 3,
        CallEnemyInfo = 4,
        ResultClose = 5,
        BattleClose = 6,
        LvUpNext = 7,
        
        EndLvUpAnimation = 9,
        ChangeSkipToggle = 10,
    }
}
public class StrategyViewEvent
{
    public Strategy.CommandType commandType;
    public object template;

    public StrategyViewEvent(Strategy.CommandType type)
    {
        commandType = type;
    }
}
