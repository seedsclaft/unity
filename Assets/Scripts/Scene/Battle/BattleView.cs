using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using Effekseer;

public class BattleView : BaseView ,IInputHandlerEvent
{
    [SerializeField] private BattleActorList battleActorList = null;
    [SerializeField] private BattleEnemyLayer battleEnemyLayer = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;
    [SerializeField] private SkillList skillList = null;
    public SkillList SkillList => skillList;
    [SerializeField] private StatusConditionList statusConditionList = null;
    [SerializeField] private BattleThumb battleThumb = null;

    private new System.Action<BattleViewEvent> _commandData = null;

    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    [SerializeField] private Button escapeButton = null;
    [SerializeField] private SkillInfoComponent skillInfoComponent = null;
    [SerializeField] private SideMenuList sideMenuList = null;

    
    [SerializeField] private GameObject centerAnimPosition = null;
    [SerializeField] private SideMenu battleAutoButton = null;
    private BattleStartAnim _battleStartAnim = null;



    private bool _battleBusy = false;
    public bool BattleBusy => _battleBusy;
    public void SetBattleBusy(bool isBusy)
    {
        _battleBusy = isBusy;
    }
    private bool _animationBusy = false;
    public bool AnimationBusy => _animationBusy;

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
        statusConditionList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        statusConditionList.SetInputHandler(InputKeyType.Option1,() => CallOpenSideMenu());
        SetInputHandler(statusConditionList.GetComponent<IInputHandlerEvent>());
        DeactivateConditionList();
        HideConditionAll();

        skillList.InitializeAttribute((attribute) => CallAttributeTypes(attribute),() => OnClickCondition());
        
        SetInputHandler(skillList.skillAttributeList.GetComponent<IInputHandlerEvent>());
        skillList.HideAttributeList();

        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
        new BattlePresenter(this);
    }

    private void InitializeSkillActionList()
    {
        skillList.InitializeAction();
        skillList.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAction());
        skillList.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        skillList.SetInputHandlerAction(InputKeyType.Option1,() => CallOpenSideMenu());
        skillList.SetInputHandlerAction(InputKeyType.Option2,() => OnClickEscape());
        SetInputHandler(skillList.skillActionList.GetComponent<IInputHandlerEvent>());
        skillList.HideActionList();
        skillList.HideAttributeList();
        sideMenuList.gameObject.SetActive(false);
    }

    public void SetBattleAutoButton(SystemData.CommandData data,bool isAuto)
    {
        battleAutoButton.SetData(data,0);
        battleAutoButton.UpdateViewItem();
        battleAutoButton.SetCallHandler((a) => {
            if (battleAutoButton.gameObject.activeSelf == false) return;
            var eventData = new BattleViewEvent(CommandType.ChangeBattleAuto);
            _commandData(eventData);
        });
        battleAutoButton.Cursor.SetActive(isAuto);
        SetBattleAutoButtonActive(false);
    }
    
    public void SetBattleAutoButtonActive(bool isActive)
    {
        battleAutoButton.gameObject.SetActive(isActive);
    }

    private void CallSkillAction()
    {
        var eventData = new BattleViewEvent(CommandType.SkillAction);
        var item = skillList.ActionData;
        if (item != null)
        {
            eventData.templete = item;
            _commandData(eventData);
        }
    }

    public void HideSkillAction(ActionInfo actionInfo)
    {
        
    }

    public void ShowEnemyTarget()
    {
        battleEnemyLayer.gameObject.SetActive(true);
        HelpWindow.SetInputInfo("BATTLE_ENEMY");
    }

    public void ShowPartyTarget()
    {
        battleActorList.gameObject.SetActive(true);
        HelpWindow.SetInputInfo("BATTLE_PARTY");
    }

    public void CreateObject(int battleActorsCount)
    {
        battleActorList.Initialize(battleActorsCount,a => CallActorList(a));
        battleActorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        battleActorList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickSelectEnemy());
        SetInputHandler(battleActorList.GetComponent<IInputHandlerEvent>());
        battleActorList.Deactivate();
        
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
        SetInputFrame(1);
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

    public new void SetHelpText(string text)
    {
        HelpWindow.SetHelpText(text);
        if (text != "")
        {        
            HelpWindow.SetInputInfo("BATTLE");
        } else
        {
            HelpWindow.SetInputInfo("BATTLE_AUTO");
        }
    }

    public void SetEvent(System.Action<BattleViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActorInfo(ActorInfo actorInfo)
    {
    }

    
    public void SetBattleCommand(List<SystemData.CommandData> menuCommands)
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
        battleEnemyLayer.Initialize(battlerInfos,(a) => CallEnemyInfo(a));
        battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        battleEnemyLayer.SetInputHandler(InputKeyType.SideRight1,() => OnClickSelectParty());
        battleEnemyLayer.SetInputHandler(InputKeyType.Option1,() => CallEnemyDetailInfo(battlerInfos));
        SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        foreach (var item in battlerInfos)
        {
            _battlerComps[item.Index] = battleEnemyLayer.GetBattlerInfoComp(item.Index);
        }
        battleGridLayer.SetEnemyInfo(battlerInfos);
        DeactivateEnemyList();
    }

    private void CallEnemyInfo(List<int> indexList)
    {
        if (_animationBusy) return;
        var eventData = new BattleViewEvent(CommandType.EnemyLayer);
        eventData.templete = indexList;
        _commandData(eventData);
    }

    private void CallEnemyDetailInfo(List<BattlerInfo> battlerInfos)
    {
        if (_animationBusy) return;
        var eventData = new BattleViewEvent(CommandType.EnemyDetail);
        var selectedIndex = battleEnemyLayer.SelectedIndex;
        BattlerInfo battlerInfo = battlerInfos.Find(a => a.Index == selectedIndex);
        if (battlerInfo != null)
        {
            eventData.templete = selectedIndex;
            _commandData(eventData);
        }
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
        sideMenuList.gameObject.SetActive(true);
        skillList.ShowAttributeList();
        skillList.ActivateActionList();
        if (battlerInfo.IsState(StateType.Demigod))
        {
            battleThumb.ShowAwakenThumb(actorData);
        } else{
            battleThumb.ShowMainThumb(actorData);
        }
        // 敵のstateEffectを非表示
        HideEnemyStateOverlay();
        HideActorStateOverlay();
    }

    public void HideSkillActionList(bool isSideBenuClose = true)
    {
        skillList.HideActionList();
        skillList.DeactivateActionList();
        if (isSideBenuClose)
        {
            sideMenuList.gameObject.SetActive(false);
        }
        // 敵のstateEffectを表示
        ShowEnemyStateOverlay();
        ShowActorStateOverlay();
    }

    public void HideBattleThumb()
    {
        battleThumb.HideThumb();
    }

    public void HideSkillAtribute()
    {
        skillList.HideAttributeList();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos,int selectIndex)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        skillList.ActivateActionList();
        skillList.ShowActionList();
        sideMenuList.gameObject.SetActive(true);
        skillList.SetSkillInfos(skillInfos);
        skillList.RefreshAction(selectIndex);
    }

    public void SetCondition(List<StateInfo> stateInfos)
    {
        statusConditionList.Refresh(stateInfos);
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
        statusConditionList.Activate();
    }

    public void HideCondition()
    {
        statusConditionList.HideMainView();
        statusConditionList.Deactivate();
    }

    public void HideConditionAll()
    {
        statusConditionList.gameObject.SetActive(false);
        statusConditionList.Deactivate();
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
        } else
        {
            if (targetIndexList == null)
            {
                HideEnemyStatus();
            }
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

    public void ShowEnemyStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.ShowEnemyStateOverlay();
        }
    }

    public void HideEnemyStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.HideEnemyStateOverlay();
        }
    }

    public void ShowActorStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.ShowActorStateOverlay();
        }
    }

    public void HideActorStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.HideActorStateOverlay();
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
        DeactivateActorList();
        DeactivateEnemyList();
        _animationBusy = true;
        if (GameSystem.ConfigData._battleAnimationSkip == true) 
        {
            _animationEndTiming = 1;
            return;
        }
        // transformの位置でエフェクトを再生する
        EffekseerHandle handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
    }

    public void StartAnimationDemigod(EffekseerEffectAsset effekseerEffectAsset)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        _animationBusy = true;
        EffekseerHandle handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
    }

    public void StartSkillDamage(int targetIndex,int damageTiming,System.Action<int> callEvent)
    {
        if (GameSystem.ConfigData._battleAnimationSkip == true) 
        {            
            _animationEndTiming = 1;
            damageTiming = 10;
        } else{
            _animationEndTiming = damageTiming + 60;
        }
        _battlerComps[targetIndex].SetStartSkillDamage(damageTiming,callEvent);
    }

    public void ClearDamagePopup()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.ClearDamagePopup();
        }
    }

    public void StartDamage(int targetIndex,DamageType damageType,int value,bool needPopupDelay = true)
    {
        _battlerComps[targetIndex].StartDamage(damageType,value,needPopupDelay);
    }

    public void StartBlink(int targetIndex)
    {
        _battlerComps[targetIndex].StartBlink();
    }

    public void StartHeal(int targetIndex,DamageType damageType,int value,bool needPopupDelay = true)
    {
        _battlerComps[targetIndex].StartHeal(damageType,value,needPopupDelay);
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
        base.Update();
        if (_animationBusy == true)
        {
            CheckAnimationBusy();
            return;
        }
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
            if (_animationEndTiming > 0)
            {
                _animationEndTiming--;
                return;
            }
            foreach (var item in _battlerComps)
            {
                if (item.Value.IsBusy == true)
                {
                    IsBusy = true;
                    break;
                }
            }
            if (IsBusy == false && _animationEndTiming <= 0 && _battleStartAnim.IsBusy == false)
            {
                _animationBusy = false;
                foreach (var item in _battlerComps)
                {
                    if (item.Value.EffekseerEmitter.enabled)
                    {
                        item.Value.DisableEmitter();
                    }
                }
                var eventData = new BattleViewEvent(CommandType.EndAnimation);
                _commandData(eventData);
            }
        }
    }

    public void SetSideMenu(List<SystemData.CommandData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CallCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetOpenEvent(() => {
            skillList.DeactivateActionList();
            skillList.DeactivateAttributeList();
            sideMenuList.Activate();
        });
        sideMenuList.SetCloseEvent(() => {
            HelpWindow.SetInputInfo("BATTLE");
            skillList.ActivateActionList();
            skillList.ActivateAttributeList();
            sideMenuList.Deactivate();
            skillList.skillActionList.UpdateHelpWindow();
            HelpWindow.SetHelpText(DataSystem.System.GetTextData(15010).Text);
        });
    }
    
    public void ActivateSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        sideMenuList.Activate();
    }

    public void DeactivateSideMenu()
    {
        HelpWindow.SetInputInfo("BATTLE");
        sideMenuList.Deactivate();
    }

    public void CommandOpenSideMenu()
    {
        HelpWindow.SetInputInfo("SIDEMENU");
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        skillList.DeactivateActionList();
        skillList.DeactivateAttributeList();
        sideMenuList.Activate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        HelpWindow.SetInputInfo("BATTLE");
        skillList.ActivateActionList();
        skillList.ActivateAttributeList();
        sideMenuList.Deactivate();
        sideMenuList.CloseSideMenu();
        skillList.skillActionList.UpdateHelpWindow();
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(15010).Text);
    }

    private void CallOpenSideMenu()
    {
        var eventData = new BattleViewEvent(CommandType.OpenSideMenu);
        _commandData(eventData);
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new BattleViewEvent(CommandType.SelectSideMenu);
        eventData.templete = sideMenu;
        _commandData(eventData);
    }

    private void CallCloseSideMenu()
    {
        var eventData = new BattleViewEvent(CommandType.CloseSideMenu);
        _commandData(eventData);
        SetInputFrame(1);
    }    
    
    public void InputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Cancel)
        {
            if (battleAutoButton.gameObject.activeSelf == false) return;
            var eventData = new BattleViewEvent(CommandType.ChangeBattleAuto);
            _commandData(eventData);
        }
    }

    public void ChangeBattleAuto(bool isAuto)
    {
        battleAutoButton.Cursor.SetActive(isAuto);
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
        ChangeBattleAuto,
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