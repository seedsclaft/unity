using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Effekseer;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattleView : BaseView ,IInputHandlerEvent
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
        [SerializeField] private OnOffButton battleAutoButton = null;
        [SerializeField] private OnOffButton battleSpeedButton = null;
        [SerializeField] private OnOffButton battleSkipButton = null;
        [SerializeField] private OnOffButton skillLogButton = null;
        [SerializeField] private BattleCutinAnimation battleCutinAnimation = null;
        [SerializeField] private GameObject battleBackGroundRoot = null;
        [SerializeField] private EffekseerEmitter demigodCutinAnimation = null;
        [SerializeField] private BattleAwakenAnimation battleAwakenAnimation = null;
        [SerializeField] private GameObject battle3DViewPrefab = null;
        [SerializeField] private Battle3DDamageView battle3DDamageView = null;
        [SerializeField] private Battle3DStatusView battle3DStatusView = null;
        
        [SerializeField] private BattleMagicSelectView magicSelectView = null;
        public SkillInfo SelectSkill => magicSelectView.SelectSkill;

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
            selectCharacter.Initialize();
            SetInputHandler(selectCharacter.gameObject);

            InitializeSelectCharacter();
            battleGridLayer.Initialize();
            battleActorList.Initialize();
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
                var eventData = new BattleViewEvent(CommandType.SkillLog);
                _commandData(eventData);
            });
            SetBattleSkipActive(false);
            battleCutinAnimation.Initialize();
            InitializeMagicList();
            Create3DView();
            if (GameSystem.TempData.InReplay)
            {
                new BattleReplayPresenter(this);
            } else
            {
                new BattlePresenter(this);
            }
        }

        public void CreateBattleBackGround(GameObject gameObject)
        {
            //var prefab = Instantiate(gameObject);
            //prefab.transform.SetParent(battleBackGroundRoot.transform,false);
            //_backGroundAnimation = prefab.GetComponent<BattleBackGroundAnimation>();
        }
        
        private void InitializeEnemyLayer()
        {
            battleEnemyLayer.Initialize();
            battleEnemyLayer.SetInputHandler(InputKeyType.Decide,OnSelectEnemy);
            battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,OnCancelEnemy);
            battleEnemyLayer.SetSelectedHandler(TargetSelectCursor);
            battleEnemyLayer.gameObject.SetActive(false);
            SetInputHandler(battleEnemyLayer.gameObject);
            AddViewActives(battleEnemyLayer);
        }

        private void InitializeMagicList()
        {
            magicSelectView.Initialize();
            magicSelectView.SetInputHandler(InputKeyType.Option1,() => OnSelectMagic(InputKeyType.Option1));
            magicSelectView.SetInputHandler(InputKeyType.Option2,() => OnSelectMagic(InputKeyType.Option2));
            magicSelectView.SetInputHandler(InputKeyType.SideLeft1,() => OnSelectMagic(InputKeyType.SideLeft1));
            magicSelectView.SetInputHandler(InputKeyType.SideRight1,() => OnSelectMagic(InputKeyType.SideRight1));
            magicSelectView.SetInputHandler(InputKeyType.Decide,() => OnSelectMagic(InputKeyType.Decide));
            magicSelectView.gameObject.SetActive(false);
            magicSelectView.ClearSelect();
            SetInputHandler(magicSelectView.gameObject);
            AddViewActives(magicSelectView);
        }

        private void OnSelectMagic(InputKeyType inputKeyType)
        {
            if (inputKeyType == InputKeyType.Decide)
            {
                var listData = magicSelectView.SelectSkill;
                if (listData != null)
                {
                    var eventData = new BattleViewEvent(CommandType.OnSelectSkill)
                    {
                        template = listData
                    };
                    _commandData(eventData);
                }
                return;
            }
            if (magicSelectView.SelectInputKeyType != inputKeyType)
            {
                magicSelectView.SetSelectSkill(inputKeyType);
            }
        }

        private void OnSelectEnemy()
        {
            var listData = battleEnemyLayer.ListItemData<BattlerInfo>();
            if (listData != null)
            {
                var eventData = new BattleViewEvent(CommandType.OnSelectEnemy)
                {
                    template = listData
                };
                _commandData(eventData);
            }
        }

        private void OnCancelEnemy()
        {
            var eventData = new BattleViewEvent(CommandType.OnCancelEnemy);
            _commandData(eventData);
        }

        private void TargetSelectCursor()
        {
            var listData = battleEnemyLayer.ListItemData<BattlerInfo>();
            if (listData != null)
            {
                var eventData = new BattleViewEvent(CommandType.TargetSelectCursor)
                {
                    template = listData
                };
                _commandData(eventData);
            }
        }

        public void UpdateSelectCursor(List<int> targetIndexes)
        {
            _battle3dView.UpdateSelectCursor(targetIndexes);
            battleActorList.UpdateSelectIndexList(targetIndexes);
        }

        public void EndActionSelect()
        {
            SetActivate(null);
            magicSelectView.gameObject.SetActive(false);
            battleEnemyLayer.gameObject.SetActive(false);
            battleActorList.UpdateSelectIndexList(new List<int>());
            _battle3dView.HideSelectCursor();
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
            selectCharacter.HideActionList();
        }

        public void SetBattleAutoButton(SystemData.CommandData data,bool isAuto)
        {
            /*
            battleAutoButton.gameObject.SetActive(false);
            battleAutoButton.SetText(data.Name);
            battleAutoButton.SetCallHandler(() => 
            {
                if (battleAutoButton.gameObject.activeSelf == false) return;
                var eventData = new BattleViewEvent(CommandType.ChangeBattleAuto);
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
            if (GameSystem.ConfigData.InputType)
            {
                //battleEnemyLayer.Activate();
                //battleEnemyLayer.UpdateSelectIndex(0);
                //battleActorList.Deactivate();
            }
        }

        private void CallBattleSkip()
        {
            if (battleSkipButton.gameObject.activeSelf == false) return;
            var eventData = new BattleViewEvent(CommandType.SkipBattle);
            _skipBattle = true;
            _commandData(eventData);
        }

        private void CallChangeBattleSpeed(int plus)
        {
            if (battleSpeedButton.gameObject.activeSelf == false) return;
            var eventData = new BattleViewEvent(CommandType.ChangeBattleSpeed)
            {
                template = plus
            };
            _commandData(eventData);
        }

        public void CreateObject()
        {
            battleActorList.SetInputHandler(InputKeyType.Decide,() => CallActorList());
            //battleActorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
            //battleActorList.SetInputHandler(InputKeyType.SideLeft1,() => OnClickSelectEnemy());
            battleActorList.SetSelectedHandler(() => CallSelectActorList());
            SetInputHandler(battleActorList.gameObject);
            AddViewActives(battleActorList);
            
            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
        }

        public void StartBattleStartAnim(string text)
        {
            _battleStartAnim.SetText(text);
            _battleStartAnim.StartAnim(true);
            foreach (var item in _battlerComps)
            {
                _battle3dView.BattleReady(item.Key);
            }
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

        public void SetUIButton()
        {
            SetBackCommand(() => OnClickBack());
            //ChangeSideMenuButtonActive(false);
        }

        public void ChangeSideMenuButtonActive(bool isActive)
        {
            //SideMenuButton.gameObject.SetActive(isActive);
        }

        private void OnClickBack()
        { 
            var eventData = new BattleViewEvent(CommandType.Back);
            _commandData(eventData);
            SetInputFrame(1);
        }

        public void ShowMagicList(Dictionary<InputKeyType,SkillInfo> skillInfos,bool resetScrollRect)
        {
            battleActorList.gameObject.SetActive(true);
            magicSelectView.gameObject.SetActive(true);
            magicSelectView.SetMagicCommands(skillInfos);
            SetActivate(magicSelectView);
        }

        public void SelectEnemy(List<ListData> battlerInfos)
        {
            battleEnemyLayer.gameObject.SetActive(true);
            battleEnemyLayer.SetData(battlerInfos,true,() => 
            {
                battleEnemyLayer.Refresh(0);
            });
            SetActivate(battleEnemyLayer);
        }

        public void SelectActor(List<ListData> battlerInfos)
        {
            battleEnemyLayer.gameObject.SetActive(true);
            battleEnemyLayer.SetData(battlerInfos,true,() => 
            {
                battleEnemyLayer.Refresh(0);
            });
            SetActivate(battleEnemyLayer);
            _battle3dView.SelectActor(battlerInfos);
        }

        public new void SetHelpText(string text)
        {
            HelpWindow.SetHelpText(text);
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
            //battleActorList.gameObject.SetActive(false);
            _battle3dView.Initialize(battlerInfos);
            foreach (var virtualModelControlDict in _battle3dView.VirtualModelControls)
            {
                battle3DDamageView.Set3DGameObjects(virtualModelControlDict.Key,virtualModelControlDict.Value);
            }
            
            //CommandCreateMapObject(battle3DView.gameObject);
        }
        
        public void SetEnemies(List<BattlerInfo> battlerInfos)
        {
            battleEnemyLayer.SetData(ListData.MakeListData(battlerInfos,false));
            //battleEnemyLayer.SetSelectedHandler(() => CallSelectEnemyList());
            foreach (var battlerInfo in battlerInfos)
            {
                _battlerComps[battlerInfo.Index] = battleEnemyLayer.GetBattlerInfoComp(battlerInfo.Index);
            }
            battleGridLayer.SetEnemyInfo(battlerInfos);
            battleEnemyLayer.gameObject.SetActive(false);
            _battle3dView.Initialize(battlerInfos);
            foreach (var virtualModelControlDict in _battle3dView.VirtualModelControls)
            {
                battle3DDamageView.Set3DGameObjects(virtualModelControlDict.Key,virtualModelControlDict.Value);
                if (!virtualModelControlDict.Value.IsActor)
                {
                    battle3DStatusView.Set3DGameObjects(virtualModelControlDict.Key,virtualModelControlDict.Value.gameObject);
                }
            }
            foreach (var battlerInfo in battlerInfos)
            {
                battle3DStatusView.UpdateBattlerInfo(battlerInfo);
            }
            
        }

        private void CallEnemyDetailInfo(List<BattlerInfo> battlerInfos)
        {
            if (_animationBusy) return;
            var selectedIndex = battleEnemyLayer.SelectedIndex;
            var battlerInfo = battlerInfos.Find(a => a.Index == selectedIndex);
            if (battlerInfo != null)
            {
                var eventData = new BattleViewEvent(CommandType.EnemyDetail)
                {
                    template = selectedIndex
                };
                _commandData(eventData);
            }
        }

        private void CallActorList()
        {
            var listData = battleActorList.ListData;
            if (listData != null)
            {
                var data = (BattlerInfo)listData.Data;
                var eventData = new BattleViewEvent(CommandType.ActorList)
                {
                    template = data
                };
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
                var eventData = new BattleViewEvent(CommandType.SelectActorList)
                {
                    template = data.Index
                };
                _commandData(eventData);
            }
        }

        private void CallSelectEnemyList()
        {
            if (_animationBusy) return;
            var listData = battleEnemyLayer.ListData;
            if (listData != null)
            {
                var data = (BattlerInfo)listData.Data;
                var eventData = new BattleViewEvent(CommandType.SelectEnemyList)
                {
                    template = data.Index
                };
                _commandData(eventData);
            }
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
            selectCharacter.ShowActionList();
            selectCharacter.SetSkillInfos(skillInfos);
            selectCharacter.RefreshAction(selectIndex);
            selectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Magic);
        }

        public void SetCondition(List<ListData> stateInfos)
        {
            selectCharacter.SetConditionList(stateInfos);
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
            if (GameSystem.ConfigData.BattleAnimationSkip == true) 
            {
                return;
            }
            animationSpeed *= GameSystem.ConfigData.BattleSpeed;
            _battle3dView.PlayEffect(targetIndex,effekseerEffectAsset,animationPosition,animationScale,animationSpeed);
            battleActorList.gameObject.SetActive(false);
            battleEnemyLayer.gameObject.SetActive(false);
            magicSelectView.gameObject.SetActive(false);
        }

        public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset)
        {
            if (GameSystem.ConfigData.BattleAnimationSkip == true) 
            {
                return;
            }
            // transformの位置でエフェクトを再生する
            EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
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
                //_battle3dView.BattleReady(item.Key);
            }
        }

        public void SetBattleReady()
        {
            foreach (var item in _battlerComps)
            {
                _battle3dView.BattleReady(item.Key);
            }
        }

        public void SetActorBattleReady(int index,int actorNum,int enemyNum)
        {
            _battle3dView.UpdateParentPosition(actorNum,enemyNum);
            _battle3dView.ActorBattleReady(index);
        }

        public void StartDamage(int targetIndex,DamageType damageType,int value,bool needPopupDelay = true)
        {
            _battlerComps[targetIndex].StartDamage(damageType,value,needPopupDelay);
            battle3DDamageView.StartDamage(targetIndex,damageType,value,needPopupDelay);
            //_backGroundAnimation?.SeekAnimation();

            _battle3dView?.StartDamage(targetIndex,damageType,value);
            battle3DStatusView?.StartDamage(targetIndex,damageType,value);
        }

        public void StartBlink(int targetIndex)
        {
            _battlerComps[targetIndex].StartBlink();
        }

        public void StartHeal(int targetIndex,DamageType damageType,int value,bool needPopupDelay = true)
        {
            _battlerComps[targetIndex].StartHeal(damageType,value,needPopupDelay);
            battle3DDamageView.StartHeal(targetIndex,damageType,value,needPopupDelay);
            _battle3dView?.StartHeal(targetIndex,damageType,value);
            battle3DStatusView?.StartHeal(targetIndex,damageType,value);
        }

        public void StartStatePopup(int targetIndex,DamageType damageType,string stateName)
        {
            //_battlerComps[targetIndex].StartStatePopup(damageType,stateName);
            battle3DDamageView.StartStatePopup(targetIndex,damageType,stateName);
        }

        public void StartDeathAnimation(int targetIndex)
        {
            _battlerComps[targetIndex].StartDeathAnimation();
            _battle3dView.Death(targetIndex);
            battle3DStatusView?.Death(targetIndex,false);
        }

        public void StartAliveAnimation(int targetIndex)
        {
            _battlerComps[targetIndex].StartAliveAnimation();
            battle3DStatusView?.Death(targetIndex,true);
        }

        public void BattleVictory(int mvpActorId)
        {
            _battle3dView.BattleVictory(mvpActorId);
        }

        public void RefreshStatus()
        {
            battleGridLayer.RefreshStatus();
            foreach (var item in _battlerComps)
            {
                item.Value.RefreshStatus();
            }
            battle3DStatusView.RefreshStatus();
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
            if (GameSystem.ConfigData.InputType == false)
            {
                return;
            }
            var selectIndex = 0;
            switch (keyType)
            {
                case InputKeyType.Start:
                    CallBattleSkip();
                    break;
                case InputKeyType.Decide:
                    if (battleActorList.Active)
                    {
                        selectIndex = battleActorList.Index;
                        if (selectIndex >= 0)
                        {
                            CallSelectActorList();
                        }
                    } else
                    {
                        selectIndex = battleEnemyLayer.Index;
                        if (selectIndex >= 0)
                        {
                            CallSelectEnemyList();
                        }
                    }
                    break;
                case InputKeyType.Left:
                    battleActorList.Activate();
                    battleActorList.UpdateSelectIndex(0);
                    break;
                case InputKeyType.Right:
                    battleActorList.Deactivate();
                    battleActorList.UpdateSelectIndex(-1);
                    break;
                case InputKeyType.SideLeft1:
                    CallChangeBattleSpeed(-1);
                    break;
                case InputKeyType.SideRight1:
                    CallChangeBattleSpeed(1);
                    break;
            }
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
            /*
            SoundManager.Instance.PlayStaticSe(SEType.Skill);
            StartAnimation(subjectIndex,effekseerEffect,0,1f,1.0f);
            if (_battlerComps.ContainsKey(subjectIndex))
            {
                _battlerComps[subjectIndex].SetActiveBeforeSkillThumb(true);
            }
            */
            _battle3dView.Attack(subjectIndex);
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