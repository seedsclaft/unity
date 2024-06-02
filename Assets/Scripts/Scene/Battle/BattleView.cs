using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using Effekseer;
using System.Linq;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class BattleView : BaseView ,IInputHandlerEvent
    {
        private new System.Action<BattleViewEvent> _commandData = null;
        [SerializeField] private BattleBattlerList battleActorList = null;
        [SerializeField] private BattleBattlerList battleEnemyLayer = null;
        [SerializeField] private BattleGridLayer battleGridLayer = null;
        [SerializeField] private BattleSelectCharacter selectCharacter = null;
        [SerializeField] private BattleThumb battleThumb;
        [SerializeField] private TextMeshProUGUI turns;
        

        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private GameObject currentSkillBg = null;

        [SerializeField] private GameObject centerAnimPosition = null;
        [SerializeField] private SideMenuButton battleAutoButton = null;
        [SerializeField] private SideMenuButton battleSpeedButton = null;
        [SerializeField] private SideMenuButton battleSkipButton = null;
        [SerializeField] private SideMenuButton skillLogButton = null;
        [SerializeField] private BattleCutinAnimation battleCutinAnimation = null;
        [SerializeField] private GameObject battleBackGroundRoot = null;
        private BattleBackGroundAnimation _backGroundAnimation = null;
        
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

        private bool _skipBattle = false;
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
            SideMenuButton.onClick.AddListener(() => 
            {
                CallSideMenu();
            });
            battleSpeedButton.SetCallHandler((a) => 
            {
                if (battleSpeedButton.gameObject.activeSelf == false) return;
                var eventData = new BattleViewEvent(CommandType.ChangeBattleSpeed);
                _commandData(eventData);
            });
            battleSkipButton.SetCallHandler((a) => 
            {
                if (battleSkipButton.gameObject.activeSelf == false) return;
                var eventData = new BattleViewEvent(CommandType.SkipBattle);
                _skipBattle = true;
                _commandData(eventData);
            });
            skillLogButton.SetCallHandler((a) => 
            {
                if (skillLogButton.gameObject.activeSelf == false) return;
                var eventData = new BattleViewEvent(CommandType.SkillLog);
                _commandData(eventData);
            });
            SetBattleSkipActive(false);
            if (false)
            {
                new BattleReplayPresenter(this);
            } else
            {
                new BattlePresenter(this);
            }
        }

        public void CreateBattleBackGround(GameObject gameObject)
        {
            var prefab = Instantiate(gameObject);
            prefab.transform.SetParent(battleBackGroundRoot.transform,false);
            _backGroundAnimation = prefab.GetComponent<BattleBackGroundAnimation>();
        }

        private void InitializeSelectCharacter()
        {
            selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAction());
            selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            selectCharacter.SetInputHandlerAction(InputKeyType.Option1,() => CommandOpenSideMenu());
            selectCharacter.SetInputHandlerAction(InputKeyType.Option2,() => OnClickEscape());
            selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft2,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(-1);
            });
            selectCharacter.SetInputHandlerAction(InputKeyType.SideRight2,() => 
            {
                selectCharacter.SelectCharacterTabSmooth(1);
            });
            SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
            selectCharacter.HideActionList();
        }

        public void SetBattleAutoButton(SystemData.CommandData data,bool isAuto)
        {
            battleAutoButton.gameObject.SetActive(false);
            battleAutoButton.SetData(data,0);
            battleAutoButton.UpdateViewItem();
            battleAutoButton.SetCallHandler((a) => {
                if (battleAutoButton.gameObject.activeSelf == false) return;
                var eventData = new BattleViewEvent(CommandType.ChangeBattleAuto);
                _commandData(eventData);
            });
            battleAutoButton.Cursor.SetActive(isAuto);
        }
        
        public void SetBattleAutoButton(bool isActive)
        {
            battleAutoButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSkipActive(bool isActive)
        {
            battleSpeedButton.gameObject.SetActive(isActive);
            battleSkipButton.gameObject.SetActive(isActive);
            skillLogButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSpeedButton(string commandName)
        {
            SystemData.CommandData system = new SystemData.CommandData();
            system.Name = commandName;
            battleSpeedButton.SetData(system,-1);
            battleSpeedButton.UpdateViewItem();
        }

        public void SetBattleSkipButton(string commandName)
        {
            SystemData.CommandData system = new SystemData.CommandData();
            system.Name = commandName;
            battleSkipButton.SetData(system,-1);
            battleSkipButton.UpdateViewItem();
        }

        public void SetSkillLogButton(string commandName)
        {
            SystemData.CommandData system = new SystemData.CommandData();
            system.Name = commandName;
            skillLogButton.SetData(system,-1);
            skillLogButton.UpdateViewItem();
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

        public void CreateObject()
        {
            battleActorList.SetInputHandler(InputKeyType.Decide,() => CallActorList());
            battleActorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            battleActorList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickSelectEnemy());
            battleActorList.SetSelectedHandler(() => CallSelectActorList());
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

        public void StartUIAnimation()
        {
            battleActorList.gameObject.SetActive(true);
            battleEnemyLayer.gameObject.SetActive(true);
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
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y,0),
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y-360,0),
                duration);
        }

        public void SetUIButton()
        {
            SetBackCommand(() => OnClickBack());
            ChangeSideMenuButtonActive(false);
        }

        public void ChangeSideMenuButtonActive(bool isActive)
        {
            SideMenuButton.gameObject.SetActive(isActive);
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
            foreach (var battlerInfo in battlerInfos)
            {
                _battlerComps[battlerInfo.Index] = battleActorList.GetBattlerInfoComp(battlerInfo.Index);
            }
            battleGridLayer.SetActorInfo(battlerInfos);
            battleActorList.Deactivate();
            battleActorList.gameObject.SetActive(false);
        }
        
        public void SetEnemies(List<BattlerInfo> battlerInfos)
        {
            battleEnemyLayer.SetData(ListData.MakeListData(battlerInfos,false));
            battleEnemyLayer.SetInputHandler(InputKeyType.Decide,() => CallEnemyInfo());
            battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            battleEnemyLayer.SetInputHandler(InputKeyType.SideRight1,() => OnClickSelectParty());
            battleEnemyLayer.SetInputHandler(InputKeyType.Option1,() => CallEnemyDetailInfo(battlerInfos));
            battleEnemyLayer.SetSelectedHandler(() => CallSelectEnemyList());
            SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
            foreach (var battlerInfo in battlerInfos)
            {
                _battlerComps[battlerInfo.Index] = battleEnemyLayer.GetBattlerInfoComp(battlerInfo.Index);
            }
            battleGridLayer.SetEnemyInfo(battlerInfos);
            DeactivateEnemyList();
            battleEnemyLayer.gameObject.SetActive(false);
        }

        private void CallEnemyInfo()
        {
            if (_animationBusy) return;
            var listData = battleEnemyLayer.ListData;
            if (listData != null && listData.Enable)
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
            if (listData != null && listData.Enable)
            {
                var data = (BattlerInfo)listData.Data;
                var eventData = new BattleViewEvent(CommandType.ActorList);
                eventData.template = data.Index;
                _commandData(eventData);
            }
        }

        private void CallSelectActorList()
        {
            if (_animationBusy) return;
            var listData = battleActorList.ListData;
            if (listData != null && listData.Enable)
            {
                var data = (BattlerInfo)listData.Data;
                var eventData = new BattleViewEvent(CommandType.SelectActorList);
                eventData.template = data.Index;
                _commandData(eventData);
            }
        }

        private void CallSelectEnemyList()
        {
            if (_animationBusy) return;
            var listData = battleEnemyLayer.ListData;
            if (listData != null && listData.Enable)
            {
                var data = (BattlerInfo)listData.Data;
                var eventData = new BattleViewEvent(CommandType.SelectEnemyList);
                eventData.template = data.Index;
                _commandData(eventData);
            }
        }

        public void UpdateSelectIndexList(List<int> targetIndexes)
        {
            var map1 = targetIndexes.Select(a => a - 1);
            battleActorList.UpdateSelectIndexList(map1.ToList());
            var map = targetIndexes.Select(a => a - 100);
            battleEnemyLayer.UpdateSelectIndexList(map.ToList());
        }

        public void SelectedCharacter(BattlerInfo battlerInfo)
        {
            selectCharacter.ShowActionList();
            selectCharacter.UpdateStatus(battlerInfo);
            battleThumb.ShowBattleThumb(battlerInfo);
            battleThumb.gameObject.SetActive(true);
            // 敵のstateEffectを非表示
            HideEnemyStateOverlay();
            //HideActorStateOverlay();
        }

        public void ShowCutinBattleThumb(BattlerInfo battlerInfo)
        {
            battleThumb.ShowCutinBattleThumb(battlerInfo);
            battleThumb.gameObject.SetActive(true);
        }

        public void HideSkillActionList(bool isSideMenuClose = true)
        {
            selectCharacter.HideActionList();
            if (isSideMenuClose)
            {
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
            //selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            DeactivateActorList();
            DeactivateEnemyList();
            selectCharacter.ShowActionList();
            selectCharacter.SetSkillInfos(skillInfos);
            selectCharacter.RefreshAction(selectIndex);
            selectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Magic);
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

        public void SetCurrentSkillData(SkillInfo skillInfo)
        {
            skillInfoComponent.gameObject.SetActive(true);
            skillInfoComponent.UpdateSkillInfo(skillInfo);
            currentSkillBg.GetComponent<RectTransform>().sizeDelta = new Vector2(480,60 + ((skillInfo.Master.Help.Split("\n").Length-1) * 28));
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
            animationSpeed *= GameSystem.ConfigData.BattleSpeed;
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
            EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
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

        public void StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData,float speedRate)
        {
            DeactivateActorList();
            DeactivateEnemyList();
            battleCutinAnimation.StartAnimation(battlerInfo,skillData,speedRate);
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
            _backGroundAnimation?.SeekAnimation();
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

        public void RefreshTurn(int turn)
        {
            turns?.SetText(turn.ToString());
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

        public void UpdateGridLayer()
        {
            battleGridLayer.UpdatePosition();
        }

        private void CallSideMenu()
        {
            var eventData = new BattleViewEvent(CommandType.SelectSideMenu);
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

        public async UniTask StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData)
        {
            var speed = GameSystem.ConfigData.BattleSpeed;
            if (GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Demigod);
                StartAnimationDemigod(battlerInfo,skillData,speed);
                HideStateOverlay();
                SetAnimationBusy(true);
                await UniTask.DelayFrame((int)(20 / speed));
                SoundManager.Instance.PlayStaticSe(SEType.Awaken);
                await UniTask.DelayFrame((int)(90 / speed));
            }
        }

        public void StartAnimationBeforeSkill(int subjectIndex,EffekseerEffectAsset effekseerEffect)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Skill);
            StartAnimation(subjectIndex,effekseerEffect,0,1f,1.0f);
        }

        public void StartAnimationSlipDamage(List<int> targetIndexes)
        {
            var animation = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_Fire_001");
            foreach (var targetIndex in targetIndexes)
            {
                StartAnimation(targetIndex,animation,0);
            }
        }

        public void StartAnimationRegenerate(List<int> targetIndexes)
        {
            var animation = ResourceSystem.LoadResourceEffect("tktk01/Cure1");
            foreach (var targetIndex in targetIndexes)
            {
                StartAnimation(targetIndex,animation,0);
            }
        }
    }
}