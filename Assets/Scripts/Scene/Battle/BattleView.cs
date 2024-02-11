using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using Effekseer;

public class BattleView : BaseView ,IInputHandlerEvent
{
    private new System.Action<BattleViewEvent> _commandData = null;
    [SerializeField] private BattleBattlerList battleActorList = null;
    [SerializeField] private BattleBattlerList battleEnemyLayer = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;
    [SerializeField] private BattleSelectCharacter selectCharacter = null;
    [SerializeField] private BattleThumb battleThumb;
    

    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;
    [SerializeField] private SkillInfoComponent skillInfoComponent = null;
    [SerializeField] private GameObject currentSkillBg = null;
    [SerializeField] private SideMenuList sideMenuList = null;

    [SerializeField] private GameObject centerAnimPosition = null;
    [SerializeField] private SideMenu battleAutoButton = null;
    [SerializeField] private BattleCutinAnimation battleCutinAnimation = null;
    private BattleStartAnim _battleStartAnim = null;
    public bool StartAnimIsBusy => _battleStartAnim.IsBusy;

    private bool _battleBusy = false;
    public bool BattleBusy => _battleBusy;
    public void SetBattleBusy(bool isBusy)
    {
        _battleBusy = isBusy;
    }
    private bool _animationBusy = false;
    public bool AnimationBusy => _animationBusy;
    public void SetAnimationBusy(bool isBusy)
    {
        _animationBusy = isBusy;
    }
    
    private List<MakerEffectData.SoundTimings> _soundTimings = null;

    private Dictionary<int,BattlerInfoComponent> _battlerComps = new ();

    public override void Initialize() 
    {
        base.Initialize();
        ClearCurrentSkillData();
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());

        InitializeSelectCharacter();
        battleGridLayer.Initialize();
        battleActorList.Initialize();
        battleEnemyLayer.Initialize();
        new BattlePresenter(this);
    }

    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAction());
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        selectCharacter.SetInputHandlerAction(InputKeyType.Option1,() => CommandOpenSideMenu());
        selectCharacter.SetInputHandlerAction(InputKeyType.Option2,() => OnClickEscape());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft2,() => {
            selectCharacter.SelectCharacterTabSmooth(-1);
        });
        selectCharacter.SetInputHandlerAction(InputKeyType.SideRight2,() => {
            selectCharacter.SelectCharacterTabSmooth(1);
        });
        SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
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
        SetBattleAutoButton(false);
    }
    
    public void SetBattleAutoButton(bool isActive)
    {
        battleAutoButton.gameObject.SetActive(isActive);
    }

    private void CallSkillAction()
    {
        var data = selectCharacter.ActionData;
        if (data != null)
        {
            var eventData = new BattleViewEvent(CommandType.SelectedSkill);
            eventData.template = data;
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
        battleActorList.SetInputHandler(InputKeyType.Decide,() => CallActorList());
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
    }

    public void StartBattleAnimation()
    {
        var duration = 0.8f;
        var actorListRect = battleActorList.GetComponent<RectTransform>();
        AnimationUtility.LocalMoveToTransform(battleActorList.gameObject,
            new Vector3(actorListRect.localPosition.x - 240,actorListRect.localPosition.y,0),
            new Vector3(actorListRect.localPosition.x,actorListRect.localPosition.y,0),
            duration);
        var enemyListRect = battleEnemyLayer.GetComponent<RectTransform>();
        AnimationUtility.LocalMoveToTransform(enemyListRect.gameObject,
            new Vector3(enemyListRect.localPosition.x + 240,enemyListRect.localPosition.y,0),
            new Vector3(enemyListRect.localPosition.x,enemyListRect.localPosition.y,0),
            duration);
        var borderRect = battleGridLayer.GetComponent<RectTransform>();
        AnimationUtility.LocalMoveToTransform(borderRect.gameObject,
            new Vector3(borderRect.localPosition.x,borderRect.localPosition.y + 400,0),
            new Vector3(borderRect.localPosition.x,borderRect.localPosition.y,0),
            duration);
    }

    public void SetUIButton()
    {
        SetBackCommand(() => OnClickBack());
        ChangeSideMenuButtonActive(false);
    }

    public void ChangeSideMenuButtonActive(bool isActive)
    {
        sideMenuList.gameObject.SetActive(isActive);
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

    public void SetActors(List<BattlerInfo> battlerInfos)
    {
        battleActorList.SetData(ListData.MakeListData(battlerInfos,false));
        //battleActorList.Refresh(battlerInfos);
        foreach (var battlerInfo in battlerInfos)
        {
            _battlerComps[battlerInfo.Index] = battleActorList.GetBattlerInfoComp(battlerInfo.Index);
        }
        battleGridLayer.SetActorInfo(battlerInfos);
        battleActorList.Deactivate();
    }
    
    public void SetEnemies(List<BattlerInfo> battlerInfos)
    {
        battleEnemyLayer.SetData(ListData.MakeListData(battlerInfos,false));
        battleEnemyLayer.SetInputHandler(InputKeyType.Decide,() => CallEnemyInfo());
        battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        battleEnemyLayer.SetInputHandler(InputKeyType.SideRight1,() => OnClickSelectParty());
        battleEnemyLayer.SetInputHandler(InputKeyType.Option1,() => CallEnemyDetailInfo(battlerInfos));
        SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        foreach (var battlerInfo in battlerInfos)
        {
            _battlerComps[battlerInfo.Index] = battleEnemyLayer.GetBattlerInfoComp(battlerInfo.Index);
        }
        battleGridLayer.SetEnemyInfo(battlerInfos);
        DeactivateEnemyList();
    }

    private void CallEnemyInfo()
    {
        if (_animationBusy) return;
        var listData = battleEnemyLayer.ListData;
        if (listData != null)
        {
            var data = (BattlerInfo)listData.Data;
            var eventData = new BattleViewEvent(CommandType.EnemyLayer);
            eventData.template = data.Index;
            _commandData(eventData);
        }
    }

    private void CallEnemyDetailInfo(List<BattlerInfo> battlerInfos)
    {
        if (_animationBusy) return;
        var selectedIndex = battleEnemyLayer.SelectedIndex;
        var battlerInfo = battlerInfos.Find(a => a.Index == selectedIndex);
        if (battlerInfo != null)
        {
            var eventData = new BattleViewEvent(CommandType.EnemyDetail);
            eventData.template = selectedIndex;
            _commandData(eventData);
        }
    }

    private void CallActorList()
    {
        if (_animationBusy) return;
        var listData = battleActorList.ListData;
        if (listData != null)
        {
            var data = (BattlerInfo)listData.Data;
            var eventData = new BattleViewEvent(CommandType.ActorList);
            eventData.template = data.Index;
            _commandData(eventData);
        }
    }


    public void SelectedCharacter(BattlerInfo battlerInfo)
    {
        selectCharacter.ShowActionList();
        sideMenuList.gameObject.SetActive(true);
        selectCharacter.UpdateStatus(battlerInfo);
        battleThumb.ShowBattleThumb(battlerInfo);
        battleThumb.gameObject.SetActive(true);
        // 敵のstateEffectを非表示
        HideEnemyStateOverlay();
        //HideActorStateOverlay();
    }

    public void HideSkillActionList(bool isSideMenuClose = true)
    {
        selectCharacter.HideActionList();
        if (isSideMenuClose)
        {
            sideMenuList.gameObject.SetActive(false);
        }
        // 敵のstateEffectを表示
        ShowStateOverlay();
    }

    public void HideBattleThumb()
    {
        battleThumb.HideThumb();
    }
    
    public void RefreshMagicList(List<ListData> skillInfos,int selectIndex)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        DeactivateActorList();
        DeactivateEnemyList();
        selectCharacter.ShowActionList();
        sideMenuList.gameObject.SetActive(true);
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction(selectIndex);
    }

    public void SetCondition(List<ListData> stateInfos)
    {
        selectCharacter.SetConditionList(stateInfos);
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

    public void RefreshPartyBattlerList(List<ListData> battlerInfos)
    {
        if (battlerInfos.Count > 0)
        {
            ActivateActorList();
        }
        battleActorList.SetTargetListData(battlerInfos);
        foreach (var item in _battlerComps)
        {
            var battlerInfo = battlerInfos.Find(a => item.Key == ((BattlerInfo)a.Data).Index);
            if (battlerInfo != null)
            {
                var selectable = battlerInfo.Enable;
                item.Value.SetThumbAlpha(selectable);
            }
           
        }
    }

    public void RefreshEnemyBattlerList(List<ListData> battlerInfos)
    {
        if (battlerInfos.Count > 0)
        {
            ActivateEnemyList();
        }
        battleEnemyLayer.SetTargetListData(battlerInfos);
        foreach (var item in _battlerComps)
        {
            var battlerInfo = battlerInfos.Find(a => item.Key == ((BattlerInfo)a.Data).Index);
            if (battlerInfo != null)
            {
                var selectable = battlerInfo.Enable;
                item.Value.SetThumbAlpha(selectable);
            }
           
        }
    }

    public void BattlerBattleClearSelect()
    {
        battleActorList.ClearSelect();
        battleEnemyLayer.ClearSelect();
    }

    public void HideEnemyStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.HideEnemyStateOverlay();
        }
    }

    public void ShowStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.ShowStateOverlay();
        }
    }

    public void HideStateOverlay()
    {
        foreach (var item in _battlerComps)
        {
            item.Value.HideStateOverlay();
        }
    }

    public void SetCurrentSkillData(SkillData skillData)
    {
        // 居合・拘束解除も表示
        if (skillData.Id >= 100 || skillData.Id == 31 || skillData.Id == 33)
        {
            skillInfoComponent.gameObject.SetActive(true);
            skillInfoComponent.UpdateSkillData(skillData.Id);
            currentSkillBg.GetComponent<RectTransform>().sizeDelta = new Vector2(480,56 + ((skillData.Help.Split("\n").Length-1) * 24));
        }

    }

    public void ClearCurrentSkillData()
    {
        skillInfoComponent.gameObject.SetActive(false);
        skillInfoComponent.Clear();
    }

    public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,int animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        if (GameSystem.ConfigData.BattleAnimationSkip == true) 
        {
            return;
        }
        _battlerComps[targetIndex].StartAnimation(effekseerEffectAsset,animationPosition,animationScale,animationSpeed);
    }

    public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        if (GameSystem.ConfigData.BattleAnimationSkip == true) 
        {
            return;
        }
        // transformの位置でエフェクトを再生する
        EffekseerHandle handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
    }

    public void PlayMakerEffectSound(List<MakerEffectData.SoundTimings> soundTimings)
    {
        if (GameSystem.ConfigData.BattleAnimationSkip == true) 
        {
            return;
        }
        _soundTimings = soundTimings;
        if (_soundTimings != null)
        {
            foreach (var soundTimingsData in _soundTimings)
            {
                var clip = Resources.Load<AudioClip>("Animations/Sound/" + soundTimingsData.se.name);
                var volume = soundTimingsData.se.volume * 0.01f;
                var pitch = soundTimingsData.se.pitch * 0.01f;
                var frame = soundTimingsData.frame;
                Ryneus.SoundManager.Instance.PlaySe(clip,volume,pitch,frame);
            }
        }
    }

    public void StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData)
    {
        DeactivateActorList();
        DeactivateEnemyList();
        battleCutinAnimation.StartAnimation(battlerInfo,skillData);
        //var handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
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

    public void SetBattlerThumbAlpha(bool selectable)
    {
        foreach (var item in _battlerComps)
        {
            item.Value.SetThumbAlpha(selectable);
        }
    }

    private new void Update() 
    {     
        base.Update();
        if (_battleBusy == true) return;
        var eventData = new BattleViewEvent(CommandType.UpdateAp);
        _commandData(eventData);
    }

    public void UpdateAp() 
    {
    }

    public void UpdateGridLayer()
    {
        battleGridLayer.UpdatePosition();
    }

    public void SetSideMenu(List<ListData> menuCommands){
        sideMenuList.Initialize(menuCommands,(a) => CallSideMenu(a),() => OnClickOption(),() => CommandCloseSideMenu());
        SetInputHandler(sideMenuList.GetComponent<IInputHandlerEvent>());
        sideMenuList.Deactivate();
        sideMenuList.SetHelpWindow(HelpWindow);
        sideMenuList.SetOpenEvent(() => {
            selectCharacter.MagicList.Deactivate();
            sideMenuList.Activate();
        });
        sideMenuList.SetCloseEvent(() => {
            HelpWindow.SetInputInfo("BATTLE");
            selectCharacter.MagicList.Activate();
            sideMenuList.Deactivate();
            selectCharacter.MagicList.UpdateHelpWindow();
            HelpWindow.SetHelpText(DataSystem.GetTextData(15010).Text);
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

    public new void CommandOpenSideMenu()
    {
        base.CommandOpenSideMenu();
        selectCharacter.MagicList.Deactivate();
        sideMenuList.OpenSideMenu();
    }

    public void CommandCloseSideMenu()
    {
        HelpWindow.SetInputInfo("BATTLE");
        selectCharacter.MagicList.Activate();
        sideMenuList.CloseSideMenu();
        selectCharacter.MagicList.UpdateHelpWindow();
        HelpWindow.SetHelpText(DataSystem.GetTextData(15010).Text);
    }

    private void CallSideMenu(SystemData.CommandData sideMenu)
    {
        var eventData = new BattleViewEvent(CommandType.SelectSideMenu);
        eventData.template = sideMenu;
        _commandData(eventData);
    }
    
    public void InputHandler(InputKeyType keyType,bool pressed)
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
