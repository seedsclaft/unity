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
    [SerializeField] private SkillActionList skillActionList = null;
    [SerializeField] private SkillAttributeList skillAttributeList = null;
    [SerializeField] private BattleThumb battleThumb = null;

    private new System.Action<BattleViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;

    
    [SerializeField] private EffekseerEmitter effekseerEmitter = null;
    private HelpWindow _helpWindow = null;

    private bool _battleBusy = false;
    public void SetBattleBusy(bool isBusy)
    {
        _battleBusy = isBusy;
    }
    private bool _animationBusy = false;

    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new BattlePresenter(this);
        InitializeSkillActionList();
    }

    private void InitializeSkillActionList()
    {
        skillActionList.Initialize(actorInfo => CallSkillAction(actorInfo),() => OnClickBack());
        SetInputHandler(skillActionList.GetComponent<IInputHandlerEvent>());
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
    }
    
    private void CallSkillAction(int skillId)
    {
        var eventData = new BattleViewEvent(CommandType.SkillAction);
        eventData.templete = skillId;
        _commandData(eventData);
    }

    public void HideSkillAction(ActionInfo actionInfo)
    {
        
    }

    public void ShowEnemyTarget()
    {
        battleEnemyLayer.gameObject.SetActive(true);
    }

    public void ShowPartyTarget()
    {
        battleActorList.gameObject.SetActive(true);
    }

    public void CreateObject()
    {
        battleActorList.Initialize(actorInfo => CallActorList(actorInfo),() => OnClickBack());
        SetInputHandler(battleActorList.GetComponent<IInputHandlerEvent>());
    }

    
    public void SetUIButton()
    {
        
        CreateBackCommand(() => OnClickBack());
        //_helpWindow = prefab.GetComponent<HelpWindow>();

    }


    private void OnClickBack()
    {
        var eventData = new BattleViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    public void SetHelpWindow()
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
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
        battleGridLayer.SetActorInfo(battlerInfos);
    }
    
    public void SetEnemies(List<BattlerInfo> battlerInfos)
    {
        battleEnemyLayer.Initialize(battlerInfos,(batterInfo) => CallEnemyInfo(batterInfo),() => OnClickBack());
        SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        battleGridLayer.SetEnemyInfo(battlerInfos);
    }

    private void CallEnemyInfo(List<int> indexList)
    {
        var eventData = new BattleViewEvent(CommandType.EnemyLayer);
        eventData.templete = indexList;
        _commandData(eventData);
    }

    private void CallActorList(List<int> indexList)
    {
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

    public void ShowSkillActionList(BattlerInfo battlerInfo)
    {
        skillActionList.gameObject.SetActive(true);
        skillAttributeList.gameObject.SetActive(true);
        if (battlerInfo.IsState(StateType.Demigod))
        {
            battleThumb.ShowAwakenThumb(battlerInfo.ActorInfo);
        } else{
            battleThumb.ShowMainThumb(battlerInfo.ActorInfo);
        }
    }

    public void HideSkillActionList()
    {
        skillActionList.gameObject.SetActive(false);
        skillAttributeList.gameObject.SetActive(false);
        battleThumb.HideThumb();
    }
    
    public void RefreshSkillActionList(List<SkillInfo> skillInfos)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        skillActionList.Refresh(skillInfos);
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

    public void RefreshBattlerEnemyLayerTarget(ActionInfo actionInfo)
    {
        if (actionInfo != null){
            ActivateEnemyList();
        }
        battleEnemyLayer.RefreshTarget(actionInfo);
    }

    public void RefreshBattlerPartyLayerTarget(ActionInfo actionInfo)
    {
        if (actionInfo != null){
            ActivateActorList();
        }
        battleActorList.RefreshTarget(actionInfo);
    }

    public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,int animationPosition)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        _animationBusy = true;
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartAnimation(targetIndex - 100,effekseerEffectAsset,animationPosition);
        } else
        {
            battleActorList.StartAnimation(targetIndex,effekseerEffectAsset);
        }
    }

    public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset)
    {
        _animationBusy = true;
        effekseerEmitter.Play(effekseerEffectAsset);
    }

    public void StartAnimationDemigod(EffekseerEffectAsset effekseerEffectAsset)
    {
        _animationBusy = true;
        effekseerEmitter.Play(effekseerEffectAsset);
    }


    public void StartSkillDamage(int targetIndex,int damageTiming,System.Action<int> callEvent)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartSkillDamage(targetIndex - 100,damageTiming,callEvent);
        } else{
            battleActorList.StartSkillDamage(targetIndex,damageTiming,callEvent);
        }
    }

    public void ClearDamagePopup()
    {
        
        battleEnemyLayer.ClearDamagePopup();
        battleActorList.ClearDamagePopup();
    }

    public void StartDamage(int targetIndex,DamageType damageType,int value)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartDamage(targetIndex - 100,damageType,value);
        } else{
            battleActorList.StartDamage(targetIndex,damageType,value);
        }
    }

    public void StartBlink(int targetIndex)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartBlink(targetIndex - 100);
        } else{
            battleActorList.StartBlink(targetIndex);
        }
    }

    public void StartHeal(int targetIndex,DamageType damageType,int value)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartHeal(targetIndex - 100,damageType,value);
        } else{
            battleActorList.StartHeal(targetIndex,damageType,value);
        }
    }

    public void StartStatePopup(int targetIndex,DamageType damageType,string stateName)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartStatePopup(targetIndex - 100,damageType,stateName);
        } else{
            battleActorList.StartStatePopup(targetIndex,damageType,stateName);
        }
    }

    public void StartDeathAnimation(int targetIndex)
    {
        if (targetIndex >= 100)
        {
            battleEnemyLayer.StartDeathAnimation(targetIndex - 100);
        }
    }

    public void RefreshStatus()
    {
        battleGridLayer.RefreshStatus();
        battleActorList.RefreshStatus();
        battleEnemyLayer.RefreshStatus();
    }

    public void SetAttributeTypes(List<AttributeType> attributeTypes)
    {
        skillAttributeList.Initialize(attributeTypes ,(attribute) => CallAttributeTypes(attribute));
        SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
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
            if (battleEnemyLayer.AnimationBusy == false && battleActorList.AnimationBusy == false && !effekseerEmitter.exists)
            {
                _animationBusy = false;
                var eventData = new BattleViewEvent(CommandType.EndAnimation);
                _commandData(eventData);
            }
        }
    }
}

namespace Battle
{
    public enum CommandType
    {
        None = 0,
        Back,
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
        StartDamage
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