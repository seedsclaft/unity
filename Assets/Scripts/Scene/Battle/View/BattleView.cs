using System;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    using Battle;
    using Unity.VisualScripting;

    public partial class BattleView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BattleBattlerList battleActorList = null;
        [SerializeField] private BattleBattlerList battleEnemyLayer = null;
        [SerializeField] private BattleGridLayer battleGridLayer = null;
        [SerializeField] private BattleThumb battleThumb;
        [SerializeField] private TextMeshProUGUI turns;
        

        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private GameObject currentSkillBg = null;
        [SerializeField] private MakerEffekseerEmitter effekseerEmitter;
        [SerializeField] private OnOffButton battleAutoButton = null;
        [SerializeField] private OnOffButton battleSpeedButton = null;
        [SerializeField] private OnOffButton battleSkipButton = null;
        [SerializeField] private OnOffButton skillLogButton = null;
        [SerializeField] private BattleCutinAnimation battleCutinAnimation = null;
        [SerializeField] private GameObject battleBackGroundRoot = null;
        [SerializeField] private EffekseerEmitter demigodCutinAnimation = null;
        [SerializeField] private BattleAwakenAnimation battleAwakenAnimation = null;
        [SerializeField] private MagicList magicList = null;
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData)
        {
            _commandData = commandData;
        }
        public void CallEvent(CommandType battleCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.Battle,battleCommandType);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }
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

        private readonly Dictionary<int,BattlerInfoComponent> _battlerComps = new ();

        private bool _skipBattle = false;
        public override void Initialize() 
        {
            base.Initialize();
            ClearCurrentSkillData();

            InitializeSelectCharacter();
            InitializeActorList();
            InitializeEnemyLayer();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            battleSpeedButton.OnClickAddListener(() => 
            {
                CallChangeBattleSpeed(1);
            });
            battleSkipButton.OnClickAddListener(() => 
            {
                CallBattleSkip();
            });
            skillLogButton.OnClickAddListener(() => 
            {
                if (skillLogButton.gameObject.activeSelf == false) return;
                CallEvent(CommandType.SkillLog);
            });
            SetBattleSkipActive(false);
            battleCutinAnimation.Initialize();
            InitializeMagicList();
            new BattlePresenter(this);
        }

        public void CreateBattleBackGround(GameObject gameObject)
        {
            //var prefab = Instantiate(gameObject);
            //prefab.transform.SetParent(battleBackGroundRoot.transform,false);
            //_backGroundAnimation = prefab.GetComponent<BattleBackGroundAnimation>();
        }
        
        private void InitializeActorList()
        {
            battleActorList.Initialize();
            //battleActorList.SetInputHandler(InputKeyType.Decide,() => CallOnSelectActor());
            //battleActorList.SetInputHandler(InputKeyType.Cancel,OnCancelEnemy);
            //battleActorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            //battleActorList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickSelectEnemy());
            //battleActorList.SetSelectedHandler(() => CallSelectActorList());
            //SetInputHandler(battleActorList.gameObject);
            AddViewActives(battleActorList);
        }

        public void SetActors(List<ListData> battlerInfos)
        {
            battleActorList.SetData(battlerInfos);
            foreach (var battlerInfo in battlerInfos)
            {
                var data = (BattlerInfo)battlerInfo.Data;
                _battlerComps[data.Index] = battleActorList.GetBattlerInfoComp(data.Index);
            }
        }

        private void InitializeEnemyLayer()
        {
            battleEnemyLayer.Initialize();
            //battleEnemyLayer.SetInputHandler(InputKeyType.Decide,OnSelectEnemy);
            //battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,OnCancelEnemy);
            //battleEnemyLayer.SetSelectedHandler(TargetSelectCursor);
            //SetInputHandler(battleEnemyLayer.gameObject);
            AddViewActives(battleEnemyLayer);
        }

        public void SetEnemies(List<ListData> battlerInfos)
        {
            battleEnemyLayer.SetData(battlerInfos);
            //battleEnemyLayer.SetSelectedHandler(() => CallSelectEnemyList());
            foreach (var battlerInfo in battlerInfos)
            {
                var data = (BattlerInfo)battlerInfo.Data;
                _battlerComps[data.Index] = battleEnemyLayer.GetBattlerInfoComp(data.Index);
            }
        }

        public void UpdateSelectCursor(List<int> targetIndexes)
        {
            battleActorList.UpdateSelectIndexList(targetIndexes);
            battleEnemyLayer.UpdateSelectIndexList(targetIndexes);
        }

        public void SetGridMembers(List<BattlerInfo> battlerInfos)
        {
            battleGridLayer.SetGridMembers(battlerInfos);
        }
        
        public void UpdateGridLayer()
        {
            battleGridLayer.UpdatePosition();
        }

        private void InitializeMagicList()
        {
            magicList.Initialize();
            magicList.SetInputHandler(InputKeyType.Decide,OnDecideSkill);
            magicList.SetInputHandler(InputKeyType.Right,() => OnSelectTarget(InputKeyType.Right));
            magicList.SetInputHandler(InputKeyType.Left,() => OnSelectTarget(InputKeyType.Left));
            magicList.gameObject.SetActive(false);
            magicList.SetSelectedHandler(OnSelectMagic);
            SetInputHandler(magicList.gameObject);
            AddViewActives(magicList);
        }

        private void OnDecideSkill()
        {
            var listData = magicList.ListItemData<SkillInfo>();
            if (listData != null && listData.Enable)
            {
                CallEvent(CommandType.OnDecideSkill,listData);
            }
        }

        private void OnSelectMagic()
        {
            var listData = magicList.ListItemData<SkillInfo>();
            if (listData != null && listData.Enable)
            {
                CallEvent(CommandType.OnSelectSkill,listData);
            }
        }

        private void OnSelectTarget(InputKeyType inputKeyType)
        {
            CallEvent(CommandType.OnSelectTarget,inputKeyType);
        }

        public void EndActionSelect()
        {
            SetActivate(null);
            magicList.gameObject.SetActive(false);
            battleEnemyLayer.ClearSelect();
            battleActorList.ClearSelect();
        }

        private void InitializeSelectCharacter()
        {
            /*
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
            */
        }

        public void SetBattleAutoButton(SystemData.CommandData data,bool isAuto)
        {
            /*
            battleAutoButton.gameObject.SetActive(false);
            battleAutoButton.SetText(data.Name);
            battleAutoButton.SetCallHandler(() => 
            {
                if (battleAutoButton.gameObject.activeSelf == false) return;
                var eventData = CallEvent(CommandType.ChangeBattleAuto);
                _commandData(eventData);
            },() => 
            {
                battleAutoButton.Cursor.SetActive(isAuto);
            });
            battleAutoButton.Cursor.SetActive(isAuto);
            */
        }
        
        public void SetBattleAutoButton(bool isActive)
        {
            //battleAutoButton.gameObject.SetActive(isActive);
            battleSpeedButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSkipActive(bool isActive)
        {
            battleSkipButton.gameObject.SetActive(isActive);
            //skillLogButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSpeedButton(string commandName)
        {
            battleSpeedButton.SetText(commandName);
            battleSpeedButton.UpdateViewItem();
        }

        public void SetBattleSkipButton(string commandName)
        {
            battleSkipButton.SetText(commandName);
            battleSkipButton.UpdateViewItem();
        }

        public void SetSkillLogButton(string commandName)
        {
            skillLogButton.SetText(commandName);
            skillLogButton.UpdateViewItem();
        }

        public void UpdateStartActivate()
        {
            //if (GameSystem.ConfigData.InputType)
            {
                //battleEnemyLayer.Activate();
                //battleEnemyLayer.UpdateSelectIndex(0);
                //battleActorList.Deactivate();
            }
        }

        private void CallBattleSkip()
        {
            if (battleSkipButton.gameObject.activeSelf == false) return;
            _skipBattle = true;
            CallEvent(CommandType.SkipBattle);
        }

        private void CallChangeBattleSpeed(int plus)
        {
            if (battleSpeedButton.gameObject.activeSelf == false) return;
            CallEvent(CommandType.ChangeBattleSpeed,plus);
        }

        public void CreateObject()
        {
            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);
        }

        public void StartBattleStartAnim(string text)
        {
            _battleStartAnim.SetText(text);
            _battleStartAnim.StartAnim(true);
            _battleStartAnim.gameObject.SetActive(true);
        }

        public void StartUIAnimation()
        {
            battleActorList.gameObject.SetActive(true);
            //battleEnemyLayer.gameObject.SetActive(true);
            var duration = 0.8f;
            /*
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
                */
                /*
            var borderRect = battleGridLayer.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(borderRect.gameObject,
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y,0),
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y-480,0),
                duration);
                */
        }

        public void ChangeSideMenuButtonActive(bool isActive)
        {
            //SideMenuButton.gameObject.SetActive(isActive);
        }

        private void OnClickBack()
        { 
            CallEvent(CommandType.Back);
            SetInputFrame(1);
        }

        public void ShowMagicList(List<ListData> skillInfos,bool resetScrollRect,int selectIndex)
        {
            SetActivate(magicList);
            battleActorList.gameObject.SetActive(true);
            magicList.gameObject.SetActive(true);
            magicList.SetData(skillInfos,resetScrollRect);
            if (resetScrollRect)
            {
                magicList.Refresh(selectIndex);
            }
            OnSelectMagic();
        }

        public new void SetHelpText(string text)
        {
            HelpWindow.SetHelpText(text);
        }

        private void CallEnemyDetailInfo(List<BattlerInfo> battlerInfos)
        {
            if (_animationBusy) return;
            var selectedIndex = battleEnemyLayer.SelectedIndex;
            var battlerInfo = battlerInfos.Find(a => a.Index == selectedIndex);
            if (battlerInfo != null)
            {
                CallEvent(CommandType.EnemyDetail,selectedIndex);
            }
        }

        public void SelectedCharacter(BattlerInfo battlerInfo)
        {
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
        }

        public void HideBattleThumb()
        {
            battleThumb.HideThumb();
        }
        
        public void RefreshMagicList(List<ListData> skillInfos,int selectIndex)
        {
            //selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        }

        public void SetCondition(List<ListData> stateInfos)
        {
        }


        public void RefreshPartyBattlerList(List<ListData> battlerInfos)
        {
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
            //battleActorList.ClearSelect();
            //battleEnemyLayer.ClearSelect();
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

        public void SetCurrentSkillData(SkillInfo skillInfo,BattlerInfo battlerInfo)
        {
            skillInfoComponent.gameObject.SetActive(true);
            skillInfoComponent.UpdateInfo(skillInfo);
            var convertHelpText = skillInfo.ConvertHelpText(battlerInfo);
            var length = convertHelpText.Split("\n").Length;
            var height = 32 + 28 * length;
            currentSkillBg.GetComponent<RectTransform>().sizeDelta = new Vector2(480,height);
        }

        public void ClearCurrentSkillData()
        {
            skillInfoComponent.gameObject.SetActive(false);
            skillInfoComponent.Clear();
        }

        public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,int animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            magicList.gameObject.SetActive(false);
            if (GameSystem.ConfigData.BattleAnimationSkip == true) 
            {
                return;
            }
            animationSpeed *= GameSystem.ConfigData.BattleSpeed;
            _battlerComps[targetIndex].StartAnimation(effekseerEffectAsset,animationPosition,animationScale,animationSpeed);
        }

        public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset,int animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            magicList.gameObject.SetActive(false);
            if (GameSystem.ConfigData.BattleAnimationSkip == true) 
            {
                return;
            }
            animationSpeed *= GameSystem.ConfigData.BattleSpeed;
        
            effekseerEmitter.transform.localScale = new Vector3(animationScale,animationScale,animationScale);
            if (effekseerEffectAsset == null)
            { 
                effekseerEmitter.Stop();
                return;
            } 
            effekseerEmitter.Stop();
            effekseerEmitter.speed = animationSpeed;
            effekseerEmitter.Play(effekseerEffectAsset);
        }

        public void StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData,float speedRate)
        {
            battleCutinAnimation.StartAnimation(battlerInfo,skillData,speedRate);
            //var handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
        }

        public async UniTask StartAnimationMessiah(BattlerInfo battlerInfo,Sprite actorSprite)
        {
            var speed = GameSystem.ConfigData.BattleSpeed;
            if (GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Demigod);
                battleAwakenAnimation.StartAnimation(battlerInfo,actorSprite,speed);
                HideStateOverlay();
                SetAnimationBusy(true);
                await UniTask.DelayFrame((int)(60 / speed));
            }
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

        public void BattleVictory(int mvpActorId)
        {
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
            CallEvent(CommandType.UpdateAp);
        }


        private void CallSideMenu()
        {
            CallEvent(CommandType.SelectSideMenu);
        }
        
        public void InputHandler(InputKeyType keyType,bool pressed)
        {
        }

        public void ChangeBattleAuto(bool isAuto)
        {
            /*
            battleAutoButton.Cursor.SetActive(isAuto);
            battleAutoButton.SetCallHandler(() => 
            {
            },() => 
            {
                battleAutoButton.Cursor.SetActive(isAuto);
            });
            */
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
            if (!_battlerComps.ContainsKey(subjectIndex))
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Skill);
            StartAnimation(subjectIndex, effekseerEffect,0, 1.5f, 0.75f);
            _battlerComps[subjectIndex].SetActiveBeforeSkillThumb(true);
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

        public void SetTargetEnemy(BattlerInfo battlerInfo)
        {
        }

        public void SetTargetActor(BattlerInfo battlerInfo)
        {
        }
    }
}