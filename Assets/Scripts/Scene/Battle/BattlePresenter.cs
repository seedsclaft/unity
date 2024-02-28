using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ryneus;

public class BattlePresenter : BasePresenter
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
#if UNITY_EDITOR
    private bool _debug = false;
    public void SetDebug(bool busy)
    {
        _debug = busy;
    }
    private bool _testBattle = false;
#endif
    private bool _triggerAfterChecked = false;
    private bool _triggerInterruptChecked = false;
    private bool _slipDamageChecked = false;
    private bool _regenerateChecked = false;
    private bool _battleEnded = false;
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
    private Battle.CommandType _backCommandType = Battle.CommandType.None;
    public BattlePresenter(BattleView view)
    {
        _view = view;
        SetView(_view);
        _model = new BattleModel();
        SetModel(_model);

#if UNITY_EDITOR
        _view.gameObject.AddComponent<DebugBattleData>();
        var debugger = _view.gameObject.GetComponent<DebugBattleData>();
        debugger.SetDebugger(_model,this,_view);
        debugger.consoleInputField = GameSystem.DebugBattleData.consoleInputField;
#endif
        Initialize();
    }

    private async void Initialize()
    {
        _view.SetBattleBusy(true);
        _model.CreateBattleData();
        _view.SetHelpText("");
        _view.CreateBattleBackGround(_model.BattleBackGroundObject());
        await _model.LoadBattleResources(_model.Battlers);
        if (SoundManager.Instance.CrossFadeMode == false)
        {
            var bgm = await _model.GetBattleBgm();
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        }
        _view.CommandGameSystem(Base.CommandType.CloseLoading);

        _view.ClearCurrentSkillData();
        _view.CreateObject(_model.BattlerActors().Count);
        _view.SetUIButton();
        _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
        _view.ChangeBackCommandActive(false);

#if UNITY_EDITOR
        if (_view.TestMode == true && _view.TestBattleMode)
        {
            BattleInitialize();
            _model.MakeTestBattleAction();
            _testBattle = _model.testActionDates.Count > 0;
            return;
        }
#endif
        
        _view.CommandStartTransition(() => {
            BattleInitialize();
        });
    }

    private async void BattleInitialize()
    {
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetActors(_model.BattlerActors());
        _view.SetEnemies(_model.BattlerEnemies());
        _view.BattlerBattleClearSelect();
        _view.StartBattleStartAnim(DataSystem.GetTextData(61).Text);
        _view.StartBattleAnimation();
        await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);

        var isAbort = CheckAdvStageEvent(EventTiming.StartBattle,() => {
            _view.SetBattleBusy(false);
            _view.SetBattleAutoButton(true);
            CommandStartBattleAction();
            _busy = false;
        });
        if (isAbort)
        {
            _busy = true;
            _view.SetBattleBusy(true);
            return;
        }

        _view.SetBattleBusy(false);
        _view.SetBattleAutoButton(true);
        CommandStartBattleAction();
        _busy = false;
    }

    private void UpdateCommand(BattleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Battle.CommandType.UpdateAp)
        {
            CommandUpdateAp();
        }
        if (viewEvent.commandType == Battle.CommandType.SelectedSkill)
        {
            CommandSelectedSkill((SkillInfo)viewEvent.template);
        }
        if (viewEvent.commandType == Battle.CommandType.EnemyLayer)
        {    
            var targetIndexes = _model.CurrentActionTargetIndexes((int)viewEvent.template);
            CommandSelectTargetIndexes(targetIndexes);
        }
        if (viewEvent.commandType == Battle.CommandType.ActorList)
        {
            var targetIndexes = _model.CurrentActionTargetIndexes((int)viewEvent.template);
            CommandSelectTargetIndexes(targetIndexes);
        }
        if (viewEvent.commandType == Battle.CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == Battle.CommandType.AttributeType)
        {
            RefreshSkillInfos();
        }
        if (viewEvent.commandType == Battle.CommandType.DecideActor)
        {
            CommandDecideActor();
        }
        if (viewEvent.commandType == Battle.CommandType.SelectEnemy)
        {
            CommandSelectEnemy();
        }
        if (viewEvent.commandType == Battle.CommandType.StartSelect)
        {
            CommandStartSelect();
        }
        if (viewEvent.commandType == Battle.CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == Battle.CommandType.Escape)
        {
            CommandEscape();
        }
        if (viewEvent.commandType == Battle.CommandType.EnemyDetail)
        {
            CommandEnemyDetail((int)viewEvent.template);
        }
        if (viewEvent.commandType == Battle.CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu();
        }
        if (viewEvent.commandType == Battle.CommandType.ChangeBattleAuto)
        {
            CommandChangeBattleAuto();
        }
    }

    private async void UpdatePopupEscape(ConfirmCommandType confirmCommandType)
    {
        /*
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _view.HideSkillActionList();
            _view.HideBattleThumb();
            _view.SetBattleBusy(true);
            _model.EscapeBattle();
            _view.StartBattleStartAnim(DataSystem.GetTextData(15030).Text);

            _model.EndBattle();
            _battleEnded = true;
            _view.HideStateOverlay();
            await UniTask.DelayFrame(180);
            _view.SetBattleBusy(false);
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            var strategySceneInfo = new StrategySceneInfo{
                ActorInfos = _model.BattleMembers(),
                GetItemInfos = new List<GetItemInfo>()
            };
            _view.CommandGotoSceneChange(Scene.Strategy);
        }
        */
    }

    private void UpdatePopupNoEscape(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
    }

    private void CommandBack()
    {
        if (_backCommandType != Battle.CommandType.None)
        {
            var eventData = new BattleViewEvent(_backCommandType);
            _backCommandType = Battle.CommandType.None;
            UpdateCommand(eventData);
        }
    }

    private void CommandEscape()
    {
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        if (_model.EnableEscape())
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(410).Text,(a) => UpdatePopupEscape((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        } else
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(412).Text,(a) => UpdatePopupNoEscape((ConfirmCommandType)a));
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void CommandEnemyDetail(int enemyIndex)
    {
        //if (_view.SkillList.skillActionList.gameObject.activeSelf) return;
        _busy = true;
        var enemyInfo = _model.GetBattlerInfo(enemyIndex);
        
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandGameSystem(Base.CommandType.CloseStatus);
            _busy = false;
        });
        statusViewInfo.SetEnemyInfos(new List<BattlerInfo>(){enemyInfo},true);
        _view.CommandCallEnemyInfo(statusViewInfo);
        //_view.SetActiveUi(false);
        SoundManager.Instance.PlayStaticSe(SEType.Decide);    
    }
    
    private void PassiveInfoAction(List<TriggerTiming> triggerTimings)
    {
        var PassiveResults = _model.CheckTriggerPassiveInfos(triggerTimings);
        ExecActionResult(PassiveResults);
        foreach (var passiveResult in PassiveResults)
        {
            var skill = DataSystem.FindSkill(passiveResult.SkillId);
            var plusSkill = skill.FeatureDates.Find(a => a.FeatureType == FeatureType.PlusSkill);
            if (plusSkill != null)
            {
                var PlusResults = _model.CheckPlusPassiveInfos(passiveResult,plusSkill.Param1);
                ExecActionResult(PlusResults);
            }
        }
    }

    private void CommandStartBattleAction()
    {
        PassiveInfoAction(BattleUtility.StartTriggerTimings());
        var startTriggerTimings = new List<TriggerTiming>(){
            TriggerTiming.After,
        };
        PassiveInfoAction(startTriggerTimings);
    }

    private void CommandUpdateAp()
    {
#if UNITY_EDITOR
        if (_testBattle == true && _model.TestBattler() != null) {
            _model.SetActionBattler(_model.TestBattler().Index);
            if (_model.CurrentBattler != null)
            {
                CommandStartSelect();
            } else
            {
                _testBattle = false;
            }
            return;
        }
#endif
        _model.MakeActionBattler();
        if (_model.CurrentBattler != null)
        {
            CommandStartSelect();
            return;
        }

        _view.SetHelpInputInfo("BATTLE_AUTO");
        while (_model.CurrentBattler == null)
        {
            BeforeUpdateAp();
            var CurrentActionInfo = _model.CurrentActionInfo();
            if (CurrentActionInfo != null)
            {
                _view.SetBattleBusy(true);
                _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(CurrentActionInfo));
                return;
            }
            if (IsBattleEnd())
            {
                BattleEnd();
                return;
            }
            var removeStateList = _model.UpdateAp();
            if (removeStateList.Count > 0)
            {
                _view.ClearDamagePopup();
                foreach (var removeState in removeStateList)
                {
                    _view.StartStatePopup(removeState.TargetIndex,DamageType.State,"-" + removeState.Master.Name);
                }
                // Passive解除
                var RemovePassiveResults = _model.CheckRemovePassiveInfos();
                ExecActionResult(RemovePassiveResults);
                _view.RefreshStatus();
            }
            _view.UpdateAp();
        }
        CommandStartSelect();
    }

    private void CommandStartSelect()
    {
        // Ap更新で行動するキャラがいる
        if (_model.CurrentBattler != null)
        {
            _model.UpdateApModify(_model.CurrentBattler);
            _view.UpdateGridLayer();
            _view.SetBattleBusy(true);
            if (_model.CurrentTurnBattler == null)
            {
                _model.SetCurrentTurnBattler(_model.CurrentBattler);
                // 解除判定は行動開始の最初のみ
                var removed =_model.UpdateNextSelfTurn();
                foreach (var removedState in removed)
                {
                    _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
                }
                // 開始前トリガー
                PassiveInfoAction(BattleUtility.BeforeTriggerTimings());
            }
            // 行動不可の場合は行動しない
            if (!_model.EnableCurrentBattler())
            {
                var skillInfo = new SkillInfo(0);
                var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
                return;
            }
            // 行動者が拘束を解除する
            var chainTargetIndexes = _model.CheckChainBattler(_model.CurrentBattler);
            if (chainTargetIndexes.Count > 0)
            {
                // 拘束解除
                var skillInfo = new SkillInfo(31);
                var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
                CommandSelectTargetIndexes(chainTargetIndexes);
                // 成功して入れば成功カウントを加算
                if (actionInfo.ActionResults.Find(a => !a.Missed) != null)
                {
                    _model.CurrentBattler.GainChainCount(1);
                }
                return;
            } 
#if UNITY_EDITOR
            if (_testBattle && _model.TestSkillId() != 0)
            {    
                int testSkillId = _model.TestSkillId();
                var skillInfo = new SkillInfo(testSkillId);
                var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
                _model.SeekActionIndex();
                return;
            }
#endif
            if (_model.CurrentBattler.isActor)
            {
                if (GameSystem.ConfigData.BattleAuto == true)
                {
                    // オート戦闘の場合
                    CommandAutoActorSkillId();
                } else
                {
                    CommandDecideActor();
                }
            } else
            {
                #if UNITY_EDITOR
                if (_debug)
                {
                    CommandDecideEnemy();
                    return;
                }
                #endif
                int autoSkillId = _model.MakeAutoSkillId(_model.CurrentBattler);
                var skillInfo = new SkillInfo(autoSkillId);
                var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
            }
        }
    }

    private void CommandAutoActorSkillId()
    {
        var (autoSkillId,targetIndex) = _model.MakeAutoActorSkillId(_model.CurrentBattler);
        var skillInfo = new SkillInfo(autoSkillId);
        var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
        CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo,targetIndex));
    }

    private void BeforeUpdateAp()
    {
        // 拘束
        var chainActionResults = _model.UpdateChainState();
        ExecActionResult(chainActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,null,chainActionResults);
        _model.CheckTriggerSkillInfos(TriggerTiming.AfterAndStartBattle,null,chainActionResults);
        
        StartAliveAnimation(chainActionResults);
        StartDeathAnimation(chainActionResults);

        // 祝福
        var benedictionActionResults = _model.UpdateBenedictionState();
        ExecActionResult(benedictionActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,null,benedictionActionResults);
        _model.CheckTriggerSkillInfos(TriggerTiming.AfterAndStartBattle,null,benedictionActionResults);
       
        StartDeathAnimation(benedictionActionResults);
        StartAliveAnimation(benedictionActionResults);
    }

    // 行動選択開始
    private void CommandDecideActor()
    {
        _view.SetAnimationBusy(false);
        _view.SetHelpText(DataSystem.GetTextData(15010).Text);
        _view.SelectedCharacter(_model.CurrentBattler);
        _view.SetCondition(_model.SelectCharacterConditions());
        _view.ChangeSideMenuButtonActive(true);
        RefreshSkillInfos();
        _view.BattlerBattleClearSelect();
        _view.ChangeBackCommandActive(false);
        _view.SetBattlerThumbAlpha(true);
        var isAbort = CheckAdvStageEvent(EventTiming.TurnedBattle,() => {  
            _view.SetBattleBusy(false);
            _busy = false;
            CommandDecideActor();
        });
        if (isAbort)
        {
            _view.SetBattleBusy(true);
            _busy = true;
            return;
        }
    }

    private void CommandDecideEnemy()
    {
        _view.SetAnimationBusy(false);
        _view.SetHelpText(DataSystem.GetTextData(15010).Text);
        //_view.SelectedCharacter(_model.CurrentBattler);
        _view.SetCondition(_model.SelectCharacterConditions());
        //_view.ChangeSideMenuButtonActive(true);
        RefreshSkillInfos();
        _view.BattlerBattleClearSelect();
        _view.ChangeBackCommandActive(false);
        _view.SetBattlerThumbAlpha(true);
    }

    // スキルを選択
    private void CommandSelectedSkill(SkillInfo skillInfo)
    {
        if (skillInfo.Enable == false)
        {
            return;
        }
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.ClearActionInfo();
        _model.SetLastSkill(skillInfo.Id);
        var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
        _view.HideSkillActionList();
        _view.HideBattleThumb();
        _view.RefreshPartyBattlerList(_model.TargetBattlerPartyInfos(actionInfo));
        _view.RefreshEnemyBattlerList(_model.TargetBattlerEnemyInfos(actionInfo));
        _backCommandType = Battle.CommandType.StartSelect;
        _view.ChangeBackCommandActive(true);
    }

    // スキル対象を決定
    public void CommandSelectTargetIndexes(List<int> indexList)
    {
        _view.SetHelpText("");
        _view.ChangeBackCommandActive(false);
        MakeActionResultInfo(indexList);
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            if (actionInfo.IsUnison())
            {
                _model.WaitUnison();
                _view.StartStatePopup(actionInfo.SubjectIndex,DamageType.State,"+" + DataSystem.States.Find(a => a.StateType == StateType.Wait).Name);

                _view.BattlerBattleClearSelect();
                _view.ShowStateOverlay();
                _view.RefreshStatus();
                _view.SetBattlerThumbAlpha(true);
                _model.SetCurrentTurnBattler(null);
                _view.SetBattleBusy(false);
                return;
            }
            if (actionInfo.Master.SkillType == SkillType.Demigod && actionInfo.SubjectIndex < 100 || actionInfo.Master.SkillType == SkillType.Awaken && actionInfo.SubjectIndex < 100)
            {
                StartAnimationDemigod();
            } else
            {
                StartAnimationSkill();
            }
        }
    }

    private void MakeActionResultInfo(List<int> indexList)
    {
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _view.BattlerBattleClearSelect();
            _model.MakeActionResultInfo(actionInfo,indexList);
            _model.MakeCurseActionResults(actionInfo,indexList);
            // 行動割り込みスキル判定
            if (_triggerInterruptChecked == false)
            {
                var result = _model.CheckTriggerSkillInfos(TriggerTiming.Interrupt,actionInfo,actionInfo.ActionResults);
                if (result.Count > 0)
                {
                    _model.SetActionBattler(result[0].SubjectIndex);
                    _model.MakeActionResultInfo(result[0],_model.MakeAutoSelectIndex(result[0]));
                } else
                {
                    // 攻撃単体で居合
                    if (actionInfo.Master.Scope == ScopeType.One && actionInfo.Master.TargetType == TargetType.Opponent)
                    {
                        var targetIndex = -1;
                        if (actionInfo.ActionResults.Count > 0)
                        {
                            foreach (var resultInfo in actionInfo.ActionResults)
                            {
                                if (resultInfo.TargetIndex >= 0 && (resultInfo.HpDamage > 0 
                                || resultInfo.DisplayStates.Find(a => (StateType)a.Master.StateType == StateType.NoDamage) != null
                                || resultInfo.RemovedStates.Find(a => (StateType)a.Master.StateType == StateType.NoDamage) != null
                                || resultInfo.AddedStates.Find(a => a.Master.Abnormal == true) != null)
                                || resultInfo.AddedStates.Find(a => (StateType)a.Master.StateType == StateType.Chain) != null)
                                {
                                    targetIndex = resultInfo.TargetIndex;
                                }
                            }
                        }
                        var RevengeActBattler = _model.GetBattlerInfo(targetIndex);
                        if (RevengeActBattler != null && !RevengeActBattler.IsState(StateType.Chain) && !RevengeActBattler.IsState(StateType.Stun))
                        {    
                            var RevengeActState = RevengeActBattler.GetStateInfo(StateType.RevengeAct);
                            if (RevengeActState != null)
                            {                        
                                var RevengeActTarget = _model.GetBattlerInfo(actionInfo.SubjectIndex);
                                RevengeActTarget.ResetAp(false);
                                _model.ClearActionInfo();
                                actionInfo = _model.MakeActionInfo(RevengeActBattler,new SkillInfo(33),false,false);
                                _model.SetActionBattler(RevengeActBattler.Index);
                                _model.MakeActionResultInfo(actionInfo,new List<int>(){RevengeActTarget.Index});
                                
                                actionInfo.ActionResults[0].AddRemoveState(RevengeActState);
                                RevengeActBattler.RemoveState(RevengeActState,true);
                            }
                        }
                    }
                }
                _triggerInterruptChecked = true;
            }
            
            var PassiveResults = _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Use},actionInfo);
            actionInfo.ActionResults.AddRange(PassiveResults);
        }
    }

    private async void StartAnimationDemigod()
    {
        SoundManager.Instance.PlayStaticSe(SEType.Demigod);
        _view.StartAnimationDemigod(_model.CurrentBattler,_model.CurrentActionInfo().Master);
        _view.HideStateOverlay();
        _view.SetAnimationBusy(true);
        await UniTask.DelayFrame(20);
        SoundManager.Instance.PlayStaticSe(SEType.Awaken);
        await UniTask.DelayFrame(90);
        StartAnimationSkill();
    }

    private async void StartAnimationRegenerate(List<ActionResultInfo> regenerateActionResults)
    {
        var animation = ResourceSystem.LoadResourceEffect("tktk01/Cure1");
        ExecActionResult(regenerateActionResults);
        foreach (var regenerateActionResult in regenerateActionResults)
        {
            var targetIndex = regenerateActionResult.TargetIndex;
            if (regenerateActionResult.HpHeal != 0)
            {
                _view.StartAnimation(targetIndex,animation,0);
            }
        }
        var waitFrame = GameSystem.ConfigData.BattleAnimationSkip ? 1 : 60;
        await UniTask.DelayFrame(waitFrame);
        EndTurn();
    }

    private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
    {
        var animation = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_Fire_001");
        ExecActionResult(slipDamageResults);
        foreach (var slipDamageResult in slipDamageResults)
        {
            var targetIndex = slipDamageResult.TargetIndex;
            if (slipDamageResult.HpDamage != 0)
            {            
                _view.StartAnimation(targetIndex,animation,0);
            }
            if (slipDamageResult.DeadIndexList.Contains(targetIndex))
            {
                //SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(targetIndex);
            }
        }
        var PassiveResults = _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),null,slipDamageResults);
        ExecActionResult(PassiveResults);
        var waitFrame = GameSystem.ConfigData.BattleAnimationSkip ? 1 : 60;
        await UniTask.DelayFrame(waitFrame);
        EndTurn();
    }

    private async void StartAnimationSkill()
    {           
        _view.ChangeSideMenuButtonActive(false);
        _view.SetBattlerThumbAlpha(true);
        //_view.ShowEnemyStateOverlay();
        _view.HideStateOverlay();
        _view.SetAnimationBusy(true);
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo.ActionResults.Count == 0)
        {
            _nextCommandType = Battle.CommandType.SelectedSkill;
            CommandEndAnimation();
            return;
        }
        SoundManager.Instance.PlayStaticSe(SEType.Skill);
        var selfAnimation = ResourceSystem.LoadResourceEffect("MAGICALxSPIRAL/WHead1");
        _view.StartAnimation(actionInfo.SubjectIndex,selfAnimation,0,1f,1.0f);
        
        var animationData = BattleUtility.AnimationData(actionInfo.Master.AnimationId);
        if (animationData != null && animationData.AnimationPath != "" && GameSystem.ConfigData.BattleAnimationSkip == false)
        {
            var animation = ResourceSystem.LoadResourceEffect(animationData.AnimationPath);
            var soundTimings = _model.SkillActionSoundTimings(animationData.AnimationPath);
            _view.PlayMakerEffectSound(soundTimings);
            _view.SetCurrentSkillData(actionInfo.Master);
            _view.ClearDamagePopup();
            if (actionInfo.Master.AnimationType == AnimationType.All)
            {
                _view.StartAnimationAll(animation);
            } else
            {
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    var oneAnimation = actionResultInfo.CursedDamage ? ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_curse_001") : animation;
                    _view.StartAnimation(actionResultInfo.TargetIndex,oneAnimation,animationData.Position,animationData.Scale,animationData.Speed);
                }
            }
            StartAliveAnimation(_model.CurrentActionInfo().ActionResults);

            await UniTask.DelayFrame(animationData.DamageTiming);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
            }
            await UniTask.DelayFrame(60);
        } else
        {
            _view.SetCurrentSkillData(actionInfo.Master);
            //_view.ClearDamagePopup();
            StartAliveAnimation(_model.CurrentActionInfo().ActionResults);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
            }
            var waitFrame = GameSystem.ConfigData.BattleAnimationSkip ? 1 : 60;
            await UniTask.DelayFrame(waitFrame);
        }
        _nextCommandType = Battle.CommandType.EndAnimation;
        CommandEndAnimation();
    }

    private void PopupActionResult(ActionResultInfo actionResultInfo,int targetIndex,bool needDamageBlink = true,bool needPopupDelay = true)
    {
        if (actionResultInfo.TargetIndex != targetIndex)
        {
            return;
        }
        if (actionResultInfo.Missed)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Miss);
            _view.StartStatePopup(targetIndex,DamageType.State,"Miss!");
        }
        if (actionResultInfo.HpDamage > 0)
        {
            _model.GainAttackCount(actionResultInfo.TargetIndex);
            var damageType = actionResultInfo.Critical ? DamageType.HpCritical : DamageType.HpDamage;
            _view.StartDamage(targetIndex,damageType,actionResultInfo.HpDamage,needPopupDelay);
            if (needDamageBlink){
                _view.StartBlink(targetIndex);
                PlayDamageSound(damageType);
            }
        }
        if (actionResultInfo.HpHeal > 0)
        {
            if (!actionResultInfo.DeadIndexList.Contains(targetIndex))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Heal);
                _view.StartHeal(targetIndex,DamageType.HpHeal,actionResultInfo.HpHeal,needPopupDelay);
            }
        }
        if (actionResultInfo.MpDamage > 0)
        {    
            _view.StartDamage(targetIndex,DamageType.MpDamage,actionResultInfo.MpDamage);
        }
        if (actionResultInfo.MpHeal > 0)
        {
            _view.StartHeal(targetIndex,DamageType.MpHeal,actionResultInfo.MpHeal);
        }
        if (actionResultInfo.ApHeal > 0)
        {    
            _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(432,actionResultInfo.ApDamage.ToString()));
        }
        if (actionResultInfo.ApDamage > 0)
        {    
            _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(433,actionResultInfo.ApDamage.ToString()));
        }
        if (actionResultInfo.ReDamage > 0)
        {
            var damageType = actionResultInfo.Critical ? DamageType.HpCritical : DamageType.HpDamage;
            PlayDamageSound(damageType);
            _view.StartDamage(actionResultInfo.SubjectIndex,damageType,actionResultInfo.ReDamage);
            _view.StartBlink(actionResultInfo.SubjectIndex);
        }
        if (actionResultInfo.ReHeal > 0)
        {    
            SoundManager.Instance.PlayStaticSe(SEType.Heal);
            _view.StartHeal(actionResultInfo.SubjectIndex,DamageType.HpHeal,actionResultInfo.ReHeal);
        }
        foreach (var addedState in actionResultInfo.AddedStates)
        {    
            if (addedState.IsBuff())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Buff);
            } else
            if (addedState.IsDeBuff())
            {
                SoundManager.Instance.PlayStaticSe(SEType.DeBuff);
            }
            _view.StartStatePopup(addedState.TargetIndex,DamageType.State,"+" + addedState.Master.Name);
        }
        foreach (var removedState in actionResultInfo.RemovedStates)
        {    
            _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
        }
        foreach (var displayState in actionResultInfo.DisplayStates)
        {
            _view.StartStatePopup(displayState.TargetIndex,DamageType.State,displayState.Master.Name);
        }
        if (actionResultInfo.StartDash)
        {        
            //先制攻撃
            _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetTextData(431).Text);
        }
    }

    private void PlayDamageSound(DamageType damageType)
    {
        if (damageType == DamageType.HpDamage)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Damage);
        } else
        if (damageType == DamageType.HpCritical)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Critical);
        }
    }

    private void StartDeathAnimation(List<ActionResultInfo> actionResultInfos)
    {
        var deathBattlerIndexes = _model.DeathBattlerIndex(actionResultInfos);
        foreach (var deathBattlerIndex in deathBattlerIndexes)
        {
            //SoundManager.Instance.PlayStaticSe(SEType.Defeat);
            _view.StartDeathAnimation(deathBattlerIndex);
        }
    }

    private void StartAliveAnimation(List<ActionResultInfo> actionResultInfos)
    {
        var aliveBattlerIndexes = _model.AliveBattlerIndex(actionResultInfos);
        foreach (var aliveBattlerIndex in aliveBattlerIndexes)
        {
            _view.StartAliveAnimation(aliveBattlerIndex);
        }
    }

    private void CommandEndAnimation()
    {
        if (_nextCommandType == Battle.CommandType.None)
        {
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndBattle)
        {
            return;
        }
        // ダメージなどを適用
        _model.ExecCurrentActionResult();
        var PassiveResults = _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),_model.CurrentActionInfo(),_model.CurrentActionInfo().ActionResults);
        ExecActionResult(PassiveResults);
        
        _view.ClearCurrentSkillData();
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            StartDeathAnimation(actionInfo.ActionResults);
            StartAliveAnimation(actionInfo.ActionResults);
        }
        EndTurn();
    }

    private void ExecActionResult(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
    {
        _model.AdjustActionResultInfo(resultInfos);
        foreach (var resultInfo in resultInfos)
        {
              // ダメージ表現をしない
            PopupActionResult(resultInfo,resultInfo.TargetIndex,false,needPopupDelay);
        }
        foreach (var resultInfo in resultInfos)
        {
            _model.ExecActionResultInfo(resultInfo);
        }
        if (resultInfos.Count > 0)
        {
            _view.RefreshStatus();
        }
    }

    private void EndTurn()
    {
        // ターン終了
        _view.RefreshStatus();
        // スリップダメージ
        if (_triggerAfterChecked == false && _slipDamageChecked == false)
        {
            _slipDamageChecked = true;
            var slipResult = _model.CheckBurnDamage();
            if (slipResult.Count > 0)
            {
                StartAnimationSlipDamage(slipResult);
                return;
            }
        }
        // regenerate
        if (_triggerAfterChecked == false && _regenerateChecked == false)
        {
            _regenerateChecked = true;
            var regenerateResult = _model.CheckRegenerate();
            if (regenerateResult.Count > 0)
            {
                StartAnimationRegenerate(regenerateResult);
                return;
            }
        }
        // PlusSkill
        _model.CheckPlusSkill();
        // Passive付与
        PassiveInfoAction(BattleUtility.AfterTriggerTimings());
        // Passive解除
        var RemovePassiveResults = _model.CheckRemovePassiveInfos();
        ExecActionResult(RemovePassiveResults);

        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After,_model.CurrentActionInfo(),_model.CurrentActionInfo().ActionResults);
        var result2 = _model.CheckTriggerSkillInfos(TriggerTiming.AfterAndStartBattle,_model.CurrentActionInfo(),_model.CurrentActionInfo().ActionResults);
        result.AddRange(result2);
        bool isDemigodActor = false;
        if (_model.CurrentBattler != null)
        {
            isDemigodActor = _model.CurrentBattler.IsState(StateType.Demigod);
        }
        bool isTriggeredSkill = _model.CurrentActionInfo().TriggeredSkill;
        if (result.Count == 0 && _triggerAfterChecked == false && isTriggeredSkill == false)
        {
            // 行動者のターンを進める
            var removed =_model.UpdateTurn();
            foreach (var removedState in removed)
            {
                _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
            }
            // Passive付与
            PassiveInfoAction(BattleUtility.AfterTriggerTimings());

            // Passive解除
            RemovePassiveResults = _model.CheckRemovePassiveInfos();
            ExecActionResult(RemovePassiveResults);
        }
        _model.TurnEnd();
        if (isTriggeredSkill == false)
        {
            _triggerAfterChecked = true;
        }

        // 勝敗判定
        if (IsBattleEnd() && result.Count == 0)
        {
            BattleEnd();
            return;
        }
        if (result.Count > 0)
        {
            _battleEnded = false;
        }

        // 敵の蘇生を反映
        var aliveEnemies = _model.PreservedAliveEnemies();
        foreach (var aliveEnemy in aliveEnemies)
        {
            _view.StartAliveAnimation(aliveEnemy.Index);                
        }
        // Hp0以上の戦闘不能を回復
        var notDeadMembers = _model.NotDeadMembers();
        foreach (var notDeadMember in notDeadMembers)
        {
            _view.StartAliveAnimation(notDeadMember.Index);                
        }
        // 戦闘不能に聖棺がいたら他の対象に移す
        var changeHolyCoffinStates = _model.EndHolyCoffinState();
        foreach (var addState in changeHolyCoffinStates)
        {
            _view.StartStatePopup(addState.TargetIndex,DamageType.State,"+" + addState.Master.Name);
        }
        // 透明が外れるケースを適用
        var removeShadowStates = _model.EndRemoveShadowState();
        foreach (var removeShadowState in removeShadowStates)
        {
            _view.StartStatePopup(removeShadowState.TargetIndex,DamageType.State,"-" + removeShadowState.Master.Name);
        };
        // 戦闘不能の拘束ステートを解除する
        var removeChainStates = _model.EndRemoveState();
        foreach (var removeChainState in removeChainStates)
        {
            _view.StartStatePopup(removeChainState.TargetIndex,DamageType.State,"-" + removeChainState.Master.Name);
        };

        // 待機できなくなった場合は待機状態をはずす
        _model.RemoveOneMemberWaitBattlers();
        _view.RefreshStatus();

        // 次の行動者がいれば続ける
        var CurrentActionInfo = _model.CurrentActionInfo();
        if (CurrentActionInfo != null)
        {
            _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
            CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(CurrentActionInfo));
            return;
        }

        if (isDemigodActor == true)
        {
            var isAbort = CheckAdvStageEvent(EventTiming.AfterDemigod,() => { 
                _view.SetBattleBusy(false);
                _busy = false;
            });
            if (isAbort)
            {
                _busy = true;
                return;
            }
        }
        // 行動を全て終了する
        _model.SeekTurnCount();
        _view.ShowStateOverlay();
        _triggerInterruptChecked = false;
        _triggerAfterChecked = false;
        _slipDamageChecked = false;
        _regenerateChecked = false;
        // ウェイトがいたら復帰する
        _model.AssignWaitBattler();
        _model.SetCurrentTurnBattler(null);
        _view.SetBattleBusy(false);
    }

    private void RefreshSkillInfos()
    {
        var skillInfos = _model.SkillActionList();
        _view.RefreshMagicList(skillInfos,_model.SelectSkillIndex(skillInfos));
        //SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    private bool IsBattleEnd()
    {
        return _model.CheckVictory() || _model.CheckDefeat();
    }

    private async void BattleEnd()
    {
        if (_battleEnded == true) return;
        if (_model.CheckVictory())
        {
            _view.StartBattleStartAnim(DataSystem.GetTextData(15020).Text);
        } else
        if (_model.CheckDefeat())
        {
            _view.StartBattleStartAnim(DataSystem.GetTextData(15030).Text);            
        }
        _model.EndBattle();
        _model.MakeBattleScore();
        _battleEnded = true;
        _view.HideStateOverlay();
        await UniTask.DelayFrame(180);
        _view.SetBattleBusy(false);
        if (SoundManager.Instance.CrossFadeMode)
        {
            SoundManager.Instance.ChangeCrossFade();
        } else
        {
            PlayTacticsBgm();
        }
        var strategySceneInfo = new StrategySceneInfo{
            ActorInfos = _model.BattleMembers(),
            GetItemInfos = _model.MakeBattlerResult()
        };
        _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
    }

    private void CommandSelectEnemy()
    {
    }

    private void CommandSelectSideMenu()
    {
        if (_busy) return;
        var sideMenuViewInfo = new SideMenuViewInfo();
        sideMenuViewInfo.EndEvent = () => {

        };
        sideMenuViewInfo.CommandLists = _model.SideMenu();
        _view.CommandCallSideMenu(sideMenuViewInfo);
    }    
    
    private void CommandChangeBattleAuto()
    {
        if (_busy) return;
        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _model.ChangeBattleAuto();
        _view.ChangeBattleAuto(GameSystem.ConfigData.BattleAuto == true);
        if (_view.AnimationBusy == false && _view.BattleBusy && _model.CurrentBattler.isActor && GameSystem.ConfigData.BattleAuto == true)
        {
            _model.ClearActionInfo();
            _view.BattlerBattleClearSelect();
            _view.HideSkillActionList();
            _view.HideBattleThumb();
            CommandAutoActorSkillId();
        }
    }
}