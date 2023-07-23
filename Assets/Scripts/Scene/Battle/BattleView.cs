using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using Effekseer;

public class BattleView : BaseView
{
    [SerializeField] private BattleActorList battleActorList = null;
    [SerializeField] private BattleEnemyLayer battleEnemyLayer = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;
    [SerializeField] private SkillList skillList = null;
    public SkillList SkillList => skillList;
    [SerializeField] private StatusConditionList statusConditionList = null;
    [SerializeField] private BattleThumb battleThumb = null;

    private new System.Action<BattleViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;

    
    [SerializeField] private EffekseerEmitter effekseerEmitter = null;

    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    [SerializeField] private Button escapeButton = null;
    [SerializeField] private SkillInfoComponent skillInfoComponent = null;
    [SerializeField] private SideMenuList sideMenuList = null;
    private BattleStartAnim _battleStartAnim = null;

    private HelpWindow _helpWindow = null;

    private bool _battleBusy = false;
    public void SetBattleBusy(bool isBusy)
    {
        _battleBusy = isBusy;
    }
    private bool _animationBusy = false;

    private Dictionary<int,BattlerInfoComponent> _battlerComps = new Dictionary<int, BattlerInfoComponent>();
    private int _animationEndTiming = 0;
    public void SetAnimationEndTiming(int value)
    {
        _animationEndTiming = value;
    }

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        skillList.Initialize();
        InitializeSkillActionList();
        statusConditionList.Initialize(() => OnClickCondition());
        SetInputHandler(statusConditionList.GetComponent<IInputHandlerEvent>());
        DeactivateConditionList();
        HideConditionAll();

        skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute),() => OnClickCondition());
        SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideAttributeList();

        new BattlePresenter(this);
    }

    private void InitializeSkillActionList()
    {
        skillList.InitializeAction(a => CallSkillAction(a),() => OnClickBack(),null,() => OnClickEscape(),() => CallOpenSideMenu());
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        skillList.HideActionList();
        skillList.HideAttributeList();
    }
    
    private void CallSkillAction(SkillInfo skillInfo)
    {
        var eventData = new BattleViewEvent(CommandType.SkillAction);
        eventData.templete = skillInfo;
        _commandData(eventData);
    }

    public void HideSkillAction(ActionInfo actionInfo)
    {
        
    }

    public void ShowEnemyTarget()
    {
        battleEnemyLayer.gameObject.SetActive(true);
        _helpWindow.SetInputInfo("BATTLE_ENEMY");
    }

    public void ShowPartyTarget()
    {
        battleActorList.gameObject.SetActive(true);
        _helpWindow.SetInputInfo("BATTLE_PARTY");
    }

    public void CreateObject()
    {
        battleActorList.Initialize(actorInfo => CallActorList(actorInfo),() => OnClickBack(),() => OnClickSelectEnemy());
        SetInputHandler(battleActorList.GetComponent<IInputHandlerEvent>());
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
    }

    public void StartBattleStartAnim(string text)
    {
        _battleStartAnim.SetText(text);
        _battleStartAnim.StartAnim();
        _animationBusy = true;
    }

    public void SetUIButton()
    {
        
        CreateBackCommand(() => OnClickBack());
        escapeButton.onClick.AddListener(() => OnClickEscape());
        SetEscapeButton(false);
        SetRuleButton(false);
    }

    public void SetEscapeButton(bool isEscape)
    {
        escapeButton.gameObject.SetActive(isEscape);
    }

    public void SetRuleButton(bool isActive)
    {
        sideMenuList.gameObject.SetActive(isActive);
    }


    private void OnClickCondition()
    {
        var eventData = new BattleViewEvent(CommandType.Condition);
        _commandData(eventData);
    }

    private void OnClickBack()
    {
        var eventData = new BattleViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    private void OnClickEscape()
    {
        var eventData = new BattleViewEvent(CommandType.Escape);
        _commandData(eventData);
    }

    private void OnClickRuling()
    {
        var eventData = new BattleViewEvent(CommandType.Rule);
        _commandData(eventData);
    }

    private void OnClickOption()
    {
        var eventData = new BattleViewEvent(CommandType.Option);
        _commandData(eventData);
    }

    public void SetHelpWindow()
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
    }

    public void SetHelpText(string text)
    {
        _helpWindow.SetHelpText(text);
        if (text != "")
        {        
            _helpWindow.SetInputInfo("BATTLE");
        } else
        {
            _helpWindow.SetInputInfo("BATTLE_NO");
        }
    }

    public void SetEvent(System.Action<BattleViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActorInfo(ActorInfo actorInfo)
    {
    }

    
    public void SetBattleCommand(List<SystemData.MenuCommandData> menuCommands)
    {
    }

    public void SetActors(List<BattlerInfo> battlerInfos)
    {
        battleActorList.Refresh(battlerInfos);
        foreach (var item in battlerInfos)
        {
            _battlerComps[item.Index] = battleActorList.GetBattlerInfoComp(item.Index);
        }
        battleGridLayer.SetActorInfo(battlerInfos);
    }
    
    public void SetEnemies(List<BattlerInfo> battlerInfos)
    {
        battleEnemyLayer.Initialize(battlerInfos,(a) => CallEnemyInfo(a),() => OnClickBack(),() => OnClickSelectParty(),(a) => CallEnemyDetailInfo(a));
        SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        foreach (var item in battlerInfos)
        {
            _battlerComps[item.Index] = battleEnemyLayer.GetBattlerInfoComp(item.Index);
        }
        battleGridLayer.SetEnemyInfo(battlerInfos);
    }

    private void CallEnemyInfo(List<int> indexList)
    {
        if (_animationBusy) return;
        var eventData = new BattleViewEvent(CommandType.EnemyLayer);
        eventData.templete = indexList;
        _commandData(eventData);
    }

    private void CallEnemyDetailInfo(int enemyIndex)
    {
        if (_animationBusy) return;
        var eventData = new BattleViewEvent(CommandType.EnemyDetail);
        eventData.templete = enemyIndex;
        _commandData(eventData);
    }

    private void CallActorList(List<int> indexList)
    {
        if (_animationBusy) return;
        var eventData = new BattleViewEvent(CommandType.ActorList);
        eventData.templete = indexList;
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        var eventData = new BattleViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        var eventData = new BattleViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    private void OnClickDecide()
    {
        var eventData = new BattleViewEvent(CommandType.DecideActor);
        _commandData(eventData);
    }

    public void ShowSkillActionList(BattlerInfo battlerInfo,ActorsData.ActorData actorData)
    {
        skillList.ShowActionList();
        skillList.ShowAttributeList();
        skillList.ActivateActionList();
        if (battlerInfo.IsState(StateType.Demigod))
        {
            battleThumb.ShowAwakenThumb(actorData);
        } else{
            battleThumb.ShowMainThumb(actorData);
        }
    }

    public void HideSkillActionList()
    {
        skillList.HideActionList();
        skillList.DeactivateActionList();
    }

    public void HideBattleThumb()
    {
        battleThumb.HideThumb();
    }

    public void HideSkillAtribute()
    {
        skillList.HideAttributeList();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        skillList.ActivateActionList();
        skillList.ShowActionList();
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction();
    }

    public void SetCondition(List<StateInfo> stateInfos)
    {
        statusConditionList.Refresh(stateInfos,() => OnClickBack(),() => {
            skillList.ShowActionList();
            skillList.ActivateActionList();
            HideCondition();
        });
    }

    public void ShowConditionTab()
    {
        ShowConditionAll();
        HideCondition();
    }

    public void ShowConditionAll()
    {
        statusConditionList.gameObject.SetActive(true);
        statusConditionList.ShowMainView();
    }

    public void HideCondition()
    {
        statusConditionList.HideMainView();
    }

    public void HideConditionAll()
    {
        statusConditionList.gameObject.SetActive(false);
    }
    
    public void ActivateConditionList()
    {
        statusConditionList.Activate();
    }

    public void DeactivateConditionList()
    {
        statusConditionList.Deactivate();
    }

    public void ActivateEnemyList()
    {
        battleEnemyLayer.Activate();
    }

    public void DeactivateEnemyList()
    {
        battleEnemyLayer.Deactivate();
    }

    public void ActivateActorList()
    {
        battleActorList.Activate();
    }

    public void DeactivateActorList()
    {
        battleActorList.Deactivate();
    }

    private void OnClickSelectEnemy()
    {
        var eventData = new BattleViewEvent(CommandType.SelectEnemy);
        _commandData(eventData);
    }

    private void OnClickSelectParty()
    {
        var eventData = new BattleViewEvent(CommandType.SelectParty);
        _commandData(eventData);
    }

    public void RefreshBattlerEnemyLayerTarget(int selectIndex,List<int> targetIndexList = null,ScopeType scopeType = ScopeType.None,AttributeType attributeType = AttributeType.None)
    {
        if (selectIndex != -1){
            ActivateEnemyList();
        }
        battleEnemyLayer.RefreshTarget(selectIndex,targetIndexList,scopeType,attributeType);
        if (targetIndexList != null)
        {
            SetBattlerSelectable(false);
            HideEnemyStatus();
            foreach (var idx in targetIndexList)
            {
                _battlerComps[idx].SetSelectable(true);
                _battlerComps[idx].SetActiveStatus(true);
            }
        }
    }

    public void RefreshBattlerPartyLayerTarget(int selectIndex,List<int> targetIndexList = null,ScopeType scopeType = ScopeType.None)
    {
        if (selectIndex != -1){
            ActivateActorList();
        }
        battleActorList.RefreshTarget(selectIndex,targetIndexList,scopeType);
        if (targetIndexList != null)
        {
            SetBattlerSelectable(false);
            foreach (var idx in targetIndexList)
            {
                _battlerComps[idx].SetSelectable(true);
            }
        }
    }

    public void HideEnemyStatus()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.SetActiveStatus(false);
        }
    }

    public void SetCurrentSkillData(SkillsData.SkillData skillData)
    {
        if (skillData.Id >= 100)
        {
            skillInfoComponent.gameObject.SetActive(true);
            skillInfoComponent.UpdateSkillData(skillData.Id);
        }
    }

    public void ClearCurrentSkillData()
    {
        skillInfoComponent.gameObject.SetActive(false);
        skillInfoComponent.Clear();
    }

    public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,int animationPosition)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        _animationBusy = true;
        if (GameSystem.ConfigData._battleAnimationSkip == true) 
        {
            _animationEndTiming = 1;
            return;
        }
        _battlerComps[targetIndex].StartAnimation(effekseerEffectAsset,animationPosition);
    }

    public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset)
    {
        _animationBusy = true;
        if (GameSystem.ConfigData._battleAnimationSkip == true) 
        {
            _animationEndTiming = 1;
            return;
        }
        effekseerEmitter.Stop();
        effekseerEmitter.Play(effekseerEffectAsset);
    }

    public void StartAnimationDemigod(EffekseerEffectAsset effekseerEffectAsset)
    {
        _animationBusy = true;
        effekseerEmitter.Stop();
        effekseerEmitter.Play(effekseerEffectAsset);
    }

    public void StartSkillDamage(int targetIndex,int damageTiming,System.Action<int> callEvent)
    {
        _battlerComps[targetIndex].SetStartSkillDamage(damageTiming,callEvent);
    }

    public void ClearDamagePopup()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.ClearDamagePopup();
        }
    }

    public void StartDamage(int targetIndex,DamageType damageType,int value)
    {
        _battlerComps[targetIndex].StartDamage(damageType,value);
    }

    public void StartBlink(int targetIndex)
    {
        _battlerComps[targetIndex].StartBlink();
    }

    public void StartHeal(int targetIndex,DamageType damageType,int value)
    {
        _battlerComps[targetIndex].StartHeal(damageType,value);
    }

    public void StartStatePopup(int targetIndex,DamageType damageType,string stateName)
    {
        _battlerComps[targetIndex].StartStatePopup(damageType,stateName);
    }

    public void StartDeathAnimation(int targetIndex)
    {
        _battlerComps[targetIndex].StartDeathAnimation();
    }

    public void StartAliveAnimation(int targetIndex)
    {
        _battlerComps[targetIndex].StartAliveAnimation();
    }

    public void RefreshStatus()
    {
        battleGridLayer.RefreshStatus();
        foreach (var item in _battlerComps)
        {
            item.Value.RefreshStatus();
        }
    }

    public void SetBattlerSelectable(bool selectable)
    {
        foreach (var item in _battlerComps)
        {
            item.Value.SetSelectable(selectable);
        }
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes,AttributeType currentAttibuteType)
    {
        skillList.RefreshAttribute(attributeTypes,currentAttibuteType);
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
    }

    private void CallAttributeTypes(AttributeType attributeType)
    {
        var eventData = new BattleViewEvent(CommandType.AttributeType);
        eventData.templete = attributeType;
        _commandData(eventData);
    }

    public void CommandAttributeType(AttributeType attributeType)
    {
        
    }

    private new void Update() 
    {
        if (_animationBusy == true)
        {
            CheckAnimationBusy();
            return;
        }
        base.Update();
        if (_battleBusy == true) return;
        var eventData = new BattleViewEvent(CommandType.UpdateAp);
        _commandData(eventData);
    }

    public void UpdateAp() 
    {
        battleGridLayer.UpdatePosition();
    }

    private void CheckAnimationBusy()
    {
        if (_animationBusy == true)
        {
            bool IsBusy = false;
            foreach (var item in _battlerComps)
            {
                if (item.Value.IsBusy == true)
                {
                    IsBusy = true;
                } else
                {
                    item.Value.DisableEmitter();
                }
            }
            if (_animationEndTiming > 0)
            {
                _animationEndTiming--;
            }
            if (IsBusy == false && _animationEndTiming <= 0 && _battleStartAnim.IsBusy == false)
            {
                _animationBusy = false;
                var eventData = new BattleViewEvent(CommandType.EndAnimation);
                _commandData(eventData);
            }
        }
    }
    public void SetSideMenu(List<SystemData.MenuCommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
    }
    
    public void ActivateSideMenu()
    {
        _helpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        _helpWindow.SetInputInfo("BATTLE");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        _helpWindow.SetInputInfo("SIDEMENU");
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        skillList.DeactivateActionList();
        skillList.DeactivateAttributeList();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        _helpWindow.SetInputInfo("BATTLE");
        skillList.ActivateActionList();
        skillList.ActivateAttributeList();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        skillList.skillActionList.UpdateHelpWindow();
        _helpWindow.SetHelpText(DataSystem.System.GetTextData(15010).Text);
    }

    private void CallOpenSideMenu()
    {
        var eventData = new BattleViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.MenuCommandData sideMenu)
    {
        var eventData = new BattleViewEvent(CommandType.SelectSideMenu);
        eventData.templete = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new BattleViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
    }
}

namespace Battle
{
    public enum CommandType
    {
        None = 0,
        Back,
        Escape,
        Rule,
        Option,
        OpenSideMenu,
        SelectSideMenu,
        CloseSideMenu,
        BattleCommand,
        AttributeType,
        DecideActor,
        LeftActor,
        RightActor,
        ActorList,
        UpdateAp,
        SkillAction,
        EnemyLayer,
        EndAnimation,
        EndDemigodAnimation,
        EndRegeneAnimation,
        EndSlipDamageAnimation,
        StartDamage,
        Condition,
        SelectEnemy,
        SelectParty,
        EnemyDetail,
        EventCheck,
        EndBattle
    }
}

public class BattleViewEvent
{
    public Battle.CommandType commandType;
    public object templete;

    public BattleViewEvent(Battle.CommandType type)
    {
        commandType = type;
    }
}