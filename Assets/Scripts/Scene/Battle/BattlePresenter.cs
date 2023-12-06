using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
        await _model.LoadBattleResources(_model.Battlers);
        var bgm = await _model.GetBattleBgm();
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.CommandLoadingClose();

        _view.ClearCurrentSkillData();
        _view.CreateObject(_model.BattlerActors().Count);
        _view.SetUIButton();
        _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
        _view.ChangeBackCommandActive(false);

        #if UNITY_EDITOR
        if (_view.TestMode == true)
        {
            BattleInitialize();
            _debug = true;
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
        _view.SetEnemies(_model.BattlerEnemies(),_model.BattleCursorEffects());
        _view.SetSideMenu(_model.SideMenu());
        _view.StartBattleStartAnim(DataSystem.System.GetTextData(4).Text);
        await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);

        var isAbort = CheckAdvStageEvent(EventTiming.StartBattle,() => {
            _view.HideEnemyStatus();
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

        _view.HideEnemyStatus();
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
            CommandSelectTargetIndexes((List<int>)viewEvent.template);
        }
        if (viewEvent.commandType == Battle.CommandType.ActorList)
        {
            CommandSelectTargetIndexes((List<int>)viewEvent.template);
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
        if (viewEvent.commandType == Battle.CommandType.SelectParty)
        {
            CommandSelectParty();
        }
        if (viewEvent.commandType == Battle.CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == Battle.CommandType.Escape)
        {
            CommandEscape();
        }
        if (viewEvent.commandType == Battle.CommandType.Option)
        {
            CommandOption();
        }
        if (viewEvent.commandType == Battle.CommandType.EnemyDetail)
        {
            CommandEnemyDetail((int)viewEvent.template);
        }
        if (viewEvent.commandType == Battle.CommandType.OpenSideMenu)
        {
            CommandOpenSideMenu();
        }
        if (viewEvent.commandType == Battle.CommandType.CloseSideMenu)
        {
            CommandCloseSideMenu();
        }
        if (viewEvent.commandType == Battle.CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.CommandData)viewEvent.template);
        }
        if (viewEvent.commandType == Battle.CommandType.ChangeBattleAuto)
        {
            CommandChangeBattleAuto();
        }
    }

    private async void UpdatePopup(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            _view.HideSkillActionList();
            _view.HideBattleThumb();
            _view.SetEscapeButton(false);
            _view.SetBattleBusy(true);
            _model.EscapeBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);

            _model.EndBattle();
            _battleEnded = true;
            _view.HideActorStateOverlay();
            _view.HideEnemyStateOverlay();
            await UniTask.DelayFrame(180);
            _view.SetBattleBusy(false);
            _view.CommandSceneChange(Scene.Strategy);
        }
    }

    private void CommandBack()
    {
        var eventData = new BattleViewEvent(_backCommandType);
        UpdateCommand(eventData);
    }

    private void CommandEscape()
    {
        if (_model.EnableEscape())
        {
            var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(410).Text,(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void CommandRule()
    {
        _busy = true;
        _view.SetHelpInputInfo("RULING");
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.CommandCallRuling(() => {
            _view.SetHelpInputInfo("OPTION");
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _busy = false;
        });
    }

    private void CommandAutoBattle()
    {
        CommandCloseSideMenu();
    }

    private void CommandEnemyDetail(int enemyIndex)
    {
        //if (_view.SkillList.skillActionList.gameObject.activeSelf) return;
        _busy = true;
        var enemyInfo = _model.GetBattlerInfo(enemyIndex);
        
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _busy = false;
        });
        statusViewInfo.SetEnemyInfos(new List<BattlerInfo>(){enemyInfo},true);
        _view.CommandCallEnemyInfo(statusViewInfo);
        //_view.SetActiveUi(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);    
    }
    
    private void CommandStartBattleAction()
    {
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.StartBattle);
        ExecActionResult(PassiveResults);
        var AfterPassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
        ExecActionResult(AfterPassiveResults);
    }
    private void CommandUpdateAp()
    {
#if UNITY_EDITOR
        //if (_debug == true) return;
#endif
        _view.SetHelpInputInfo("BATTLE_AUTO");
        if (GameSystem.ConfigData.BattleWait == false)
        {
            while (_model.CurrentBattler == null)
            {
                BeforeUpdateAp();
                var CurrentActionInfo = _model.CurrentActionInfo();
                if (CurrentActionInfo != null)
                {
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
                    _view.RefreshStatus();
                }
                _view.UpdateAp();
            }
        } else
        {
            BeforeUpdateAp();
            // 拘束・祝福によって発動するトリガースキルを確認
            var CurrentActionInfo = _model.CurrentActionInfo();
            if (CurrentActionInfo != null)
            {
                _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(CurrentActionInfo));
                return;
            }
            if (IsBattleEnd())
            {
                BattleEnd();
                return;
            }
            // Ap更新によるステート削除
            var removeStateList = _model.UpdateAp();
            if (removeStateList.Count > 0)
            {
                _view.ClearDamagePopup();
                foreach (var removeState in removeStateList)
                {
                    _view.StartStatePopup(removeState.TargetIndex,DamageType.State,"-" + removeState.Master.Name);
                }
                _view.RefreshStatus();
            }
            _view.UpdateAp();
        }
        // Ap更新で行動するキャラがいる
        if (_model.CurrentBattler != null)
        {
            _view.SetBattleBusy(true);
            // 行動不可の場合は行動しない
            if (!_model.EnableCurrentBattler())
            {
                var skillInfo = new SkillInfo(0);
                var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
                return;
            }
            // 行動者が拘束を解除する
            var chainTargetIndexes = _model.CheckChainBattler();
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
        
        StartAliveAnimation(chainActionResults);
        StartDeathAnimation(chainActionResults);

        // 祝福
        var benedictionActionResults = _model.UpdateBenedictionState();
        ExecActionResult(benedictionActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,null,benedictionActionResults);
        
        StartDeathAnimation(benedictionActionResults);
        StartAliveAnimation(benedictionActionResults);
    }

    // 行動選択開始
    private void CommandDecideActor()
    {
        _view.SetAnimationBusy(false);
        _view.SetHelpText(DataSystem.System.GetTextData(15010).Text);
        _view.SelectedCharacter(_model.CurrentBattler);
        _view.SetCondition(_model.SelectCharacterConditions());
        _view.SetEscapeButton(_model.EnableEscape());
        _view.ChangeSideMenuButtonActive(true);
        RefreshSkillInfos();
        _view.HideBattlerEnemyTarget();
        _view.HideBattlerPartyTarget();
        _view.ChangeBackCommandActive(false);
        _view.SetBattlerSelectable(true);
        var isAbort = CheckAdvStageEvent(EventTiming.TurnedBattle,() => {  
            _view.HideEnemyStatus(); 
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
        _view.SetHelpText(DataSystem.System.GetTextData(15010).Text);
        //_view.SelectedCharacter(_model.CurrentBattler);
        _view.SetCondition(_model.SelectCharacterConditions());
        //_view.SetEscapeButton(_model.EnableEscape());
        //_view.ChangeSideMenuButtonActive(true);
        RefreshSkillInfos();
        _view.HideBattlerEnemyTarget();
        _view.HideBattlerPartyTarget();
        _view.ChangeBackCommandActive(false);
        _view.SetBattlerSelectable(true);
    }

    // スキルを選択
    private void CommandSelectedSkill(SkillInfo skillInfo)
    {
        if (skillInfo.Enable == false)
        {
            return;
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _model.ClearActionInfo();
        _model.SetLastSkill(skillInfo.Id);
        var actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo,false,false);
        _view.HideSkillActionList();
        _view.SetEscapeButton(false);
        _view.HideBattleThumb();
        if (_model.CurrentBattler.isActor)
        {
            if (actionInfo.TargetType == TargetType.Opponent)
            {
                _view.ShowEnemyTarget();
                _view.RefreshBattlerEnemyTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
            } else
            if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
            {
                _view.ShowPartyTarget();
                _view.RefreshBattlerPartyTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
            } else
            {
                _view.RefreshBattlerEnemyTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
                _view.RefreshBattlerPartyTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
                _view.ShowPartyTarget();
                _view.ShowEnemyTarget();
            }
        } else
        {
            if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
            {
                _view.ShowEnemyTarget();
                _view.RefreshBattlerEnemyTarget(actionInfo.TargetIndexList[0],actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
            } else
            if (actionInfo.TargetType == TargetType.Opponent)
            {
                _view.ShowPartyTarget();
                _view.RefreshBattlerPartyTarget(actionInfo.TargetIndexList[0],actionInfo.TargetIndexList,actionInfo.ScopeType);
            } else
            {
                _view.RefreshBattlerEnemyTarget(actionInfo.TargetIndexList[0],actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
                _view.RefreshBattlerPartyTarget(actionInfo.TargetIndexList[0],actionInfo.TargetIndexList,actionInfo.ScopeType);
                _view.ShowPartyTarget();
                _view.ShowEnemyTarget();
            }
        }
        _backCommandType = Battle.CommandType.DecideActor;
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
            if (actionInfo.Master.SkillType == SkillType.Demigod && actionInfo.SubjectIndex < 100)
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
            _view.HideBattlerEnemyTarget();
            _view.HideBattlerPartyTarget();
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
                                || resultInfo.DisplayStates.Find(a => (StateType)a.Master.Id == StateType.NoDamage) != null
                                || resultInfo.RemovedStates.Find(a => (StateType)a.Master.Id == StateType.NoDamage) != null
                                || resultInfo.AddedStates.Find(a => a.Master.Abnormal == true) != null)
                                || resultInfo.AddedStates.Find(a => (StateType)a.Master.Id == StateType.Chain) != null)
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
            
            var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.Use);
            ExecActionResult(PassiveResults);
        }
    }

    private async void StartAnimationDemigod()
    {
        var demigod = _model.SkillActionAnimation("NA_Effekseer/NA_cut-in_002_" + _model.CurrentBattler.CharaId.ToString());
        _view.StartAnimationDemigod(demigod);
        _view.HideEnemyStateOverlay();
        _view.HideActorStateOverlay();
        _view.SetAnimationBusy(true);
        await UniTask.DelayFrame(90);
        StartAnimationSkill();
    }

    private async void StartAnimationRegenerate(List<ActionResultInfo> regenerateActionResults)
    {
        var animation = _model.SkillActionAnimation("tktk01/Cure1");
        ExecActionResult(regenerateActionResults);
        foreach (var regenerateActionResult in regenerateActionResults)
        {
            if (regenerateActionResult.HpHeal != 0)
            {
                _view.StartAnimation(regenerateActionResult.TargetIndex,animation,0);
                //_view.StartHeal(regenerateActionResult.TargetIndex,DamageType.HpHeal,regenerateActionResult.HpHeal);
                //_model.GainHpTargetIndex(regenerateActionResult.TargetIndex,regenerateActionResult.HpHeal);
            }
        }
        await UniTask.DelayFrame(60);
        EndTurn();
    }

    private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
    {
        var animation = _model.SkillActionAnimation("NA_Effekseer/NA_Fire_001");
        ExecActionResult(slipDamageResults);
        foreach (var slipDamageResult in slipDamageResults)
        {
            var targetIndex = slipDamageResult.TargetIndex;
            if (slipDamageResult.HpDamage != 0)
            {            
                //_view.StartDamage(targetIndex,DamageType.HpDamage,slipDamageResult.HpDamage);
                _view.StartAnimation(targetIndex,animation,0);
                //_model.GainHpTargetIndex(targetIndex,slipDamageResult.HpDamage * -1);
            }
            if (slipDamageResult.DeadIndexList.Contains(targetIndex))
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(targetIndex);
            }
        }
        await UniTask.DelayFrame(60);
        EndTurn();
    }

    private async void StartAnimationSkill()
    {
        _view.ChangeSideMenuButtonActive(false);
        _view.SetEscapeButton(false);
        _view.SetBattlerSelectable(true);
        //_view.ShowEnemyStateOverlay();
        _view.HideEnemyStateOverlay();
        _view.HideActorStateOverlay();
        _view.SetAnimationBusy(true);
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo.ActionResults.Count == 0)
        {
            _nextCommandType = Battle.CommandType.SelectedSkill;
            CommandEndAnimation();
            return;
        }
        var animation = _model.SkillActionAnimation(actionInfo.Master.AnimationName);
        var soundTimings = _model.SkillActionSoundTimings(actionInfo.Master.AnimationName);
        _view.PlayMakerEffectSound(soundTimings);
        _view.SetCurrentSkillData(actionInfo.Master);
        _view.ClearDamagePopup();
        if (actionInfo.Master.AnimationType == AnimationType.All)
        {
            _view.StartAnimationAll(animation);
        } else
        {
            for (int i = 0; i < actionInfo.ActionResults.Count; i++)
            {
                var oneAnimation = actionInfo.ActionResults[i].CursedDamage ? _model.SkillActionAnimation("NA_Effekseer/NA_curse_001") : animation;
                _view.StartAnimation(actionInfo.ActionResults[i].TargetIndex,oneAnimation,actionInfo.Master.AnimationPosition);
            }
        }
        StartAliveAnimation(_model.CurrentActionInfo().ActionResults);

        await UniTask.DelayFrame(actionInfo.Master.DamageTiming);
        for (int i = 0; i < actionInfo.ActionResults.Count; i++)
        {
            bool lastTarget = actionInfo.ActionResults[actionInfo.ActionResults.Count-1].TargetIndex == actionInfo.ActionResults[i].TargetIndex;
            PopupActionResult(actionInfo.ActionResults[i],actionInfo.ActionResults[i].TargetIndex,true,true,lastTarget);
        }
        await UniTask.DelayFrame(60);
        _nextCommandType = Battle.CommandType.EndAnimation;
        CommandEndAnimation();
    }

    private void PopupActionResult(ActionResultInfo actionResultInfo,int targetIndex,bool needDamageBlink = true,bool needPopupDelay = true,bool lastTarget = true)
    {
        if (actionResultInfo.Missed)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartStatePopup(targetIndex,DamageType.State,"Miss!");    
            }
        }
        if (actionResultInfo.HpDamage > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _model.GainAttackCount(actionResultInfo.TargetIndex);
                _view.StartDamage(targetIndex,DamageType.HpDamage,actionResultInfo.HpDamage,needPopupDelay);
                if (needDamageBlink){
                    _view.StartBlink(targetIndex);
                    Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Damage);
                }
            }
        }
        if (actionResultInfo.HpHeal > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartHeal(targetIndex,DamageType.HpHeal,actionResultInfo.HpHeal,needPopupDelay);
            }
        }
        if (actionResultInfo.MpDamage > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartDamage(targetIndex,DamageType.MpDamage,actionResultInfo.MpDamage);
            }
        }
        if (actionResultInfo.MpHeal > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartHeal(targetIndex,DamageType.MpHeal,actionResultInfo.MpHeal);
            }
        }
        if (actionResultInfo.ApHeal > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartStatePopup(targetIndex,DamageType.State,"+行動短縮");
            }
        }
        if (actionResultInfo.ReDamage > 0)
        {
            if (lastTarget == true)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Damage);
                _view.StartDamage(actionResultInfo.SubjectIndex,DamageType.HpDamage,actionResultInfo.ReDamage);
                _view.StartBlink(actionResultInfo.SubjectIndex);
            }
        }
        if (actionResultInfo.ReHeal > 0)
        {
            if (lastTarget == true)
            {
                _view.StartHeal(actionResultInfo.SubjectIndex,DamageType.HpHeal,actionResultInfo.ReHeal);
            }
        }
        foreach (var addedState in actionResultInfo.AddedStates)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartStatePopup(addedState.TargetIndex,DamageType.State,"+" + addedState.Master.Name);
            }
        }
        foreach (var removedState in actionResultInfo.RemovedStates)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
            }
        }
        foreach (var displayState in actionResultInfo.DisplayStates)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartStatePopup(displayState.TargetIndex,DamageType.State,displayState.Master.Name);
            }
        }
    }

    private void StartDeathAnimation(List<ActionResultInfo> actionResultInfos)
    {
        var deathBattlerIndex = _model.DeathBattlerIndex(actionResultInfos);
        if (deathBattlerIndex.Count > 0)
        {
            for (int i = 0; i < deathBattlerIndex.Count; i++)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(deathBattlerIndex[i]);
            }
        }
    }

    private void StartAliveAnimation(List<ActionResultInfo> actionResultInfos)
    {
        var aliveBattlerIndex = _model.AliveBattlerIndex(actionResultInfos);
        if (aliveBattlerIndex.Count > 0)
        {
            for (int i = 0; i < aliveBattlerIndex.Count; i++)
            {
                _view.StartAliveAnimation(aliveBattlerIndex[i]);
            }
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
        
        _view.ClearCurrentSkillData();
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            StartDeathAnimation(actionInfo.ActionResults);
        }
        EndTurn();
    }

    private void ExecActionResult(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
    {
        _model.AdjustActionResultInfo(resultInfos);
        for (int i = 0; i < resultInfos.Count; i++)
        {    
            // ダメージ表現をしない
            PopupActionResult(resultInfos[i],resultInfos[i].TargetIndex,false,needPopupDelay);
        }
        for (int i = 0; i < resultInfos.Count; i++)
        {    
            _model.ExecActionResultInfo(resultInfos[i]);
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
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
        ExecActionResult(PassiveResults);
        // Passive解除
        var RemovePassiveResults = _model.CheckRemovePassiveInfos();
        ExecActionResult(RemovePassiveResults);

        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After,_model.CurrentActionInfo(),_model.CurrentActionInfo().ActionResults);
        
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
            PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
            ExecActionResult(PassiveResults);
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
        // 戦闘不能の拘束ステートを解除する
        var removeChainStates = _model.EndRemoveState();
        foreach (var removeChainState in removeChainStates)
        {
            _view.StartStatePopup(removeChainState.TargetIndex,DamageType.State,"-" + removeChainState.Master.Name);
        }
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
                _view.HideEnemyStatus();  
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
        _view.HideEnemyStatus();
        _view.ShowActorStateOverlay();
        _view.ShowEnemyStateOverlay();
        _triggerInterruptChecked = false;
        _triggerAfterChecked = false;
        _slipDamageChecked = false;
        _regenerateChecked = false;
        _view.SetBattleBusy(false);
    }

    private void RefreshSkillInfos()
    {
        var skillInfos = _model.SkillActionList();
        _view.RefreshMagicList(skillInfos,_model.SelectSkillIndex(skillInfos));
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15020).Text);
        } else
        if (_model.CheckDefeat())
        {
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);            
        }
        _model.EndBattle();
        _battleEnded = true;
        _view.HideActorStateOverlay();
        _view.HideEnemyStateOverlay();
        await UniTask.DelayFrame(180);
        _view.SetBattleBusy(false);
        _view.CommandSceneChange(Scene.Strategy);
    }

    private void CommandSelectEnemy()
    {
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
        } else
        {
            _view.RefreshBattlerEnemyTarget(actionInfo.TargetIndexList.Find(a => a >= 100),actionInfo.TargetIndexList,actionInfo.ScopeType,actionInfo.Master.Attribute);
            _view.RefreshBattlerPartyTarget(-1);
            _view.ShowEnemyTarget();
            _view.DeactivateActorList();
        }
    }

    private void CommandSelectParty()
    {
        var actionInfo = _model.CurrentActionInfo();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
        } else
        {
            _view.RefreshBattlerEnemyTarget(-1);
            _view.RefreshBattlerPartyTarget(actionInfo.TargetIndexList.Find(a => a < 100),actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.ShowPartyTarget();
            _view.DeactivateEnemyList();
        }
    }

    private void CommandOpenSideMenu()
    {
        _view.CommandOpenSideMenu();
    }

    private void CommandCloseSideMenu()
    {
        _view.CommandCloseSideMenu();
    }

    private void CommandSelectSideMenu(SystemData.CommandData sideMenu)
    {
        if (sideMenu.Key == "Help")
        {
            CommandRule();
        }
    }    
    
    private void CommandChangeBattleAuto()
    {
        if (_busy) return;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _model.ChangeBattleAuto();
        _view.ChangeBattleAuto(GameSystem.ConfigData.BattleAuto == true);
        if (_view.AnimationBusy == false && _view.BattleBusy && _model.CurrentBattler.isActor && GameSystem.ConfigData.BattleAuto == true)
        {
            _model.ClearActionInfo();
            _view.HideBattlerEnemyTarget();
            _view.HideBattlerPartyTarget();
            _view.HideSkillActionList();
            _view.SetEscapeButton(false);
            _view.HideBattleThumb();
            CommandAutoActorSkillId();
        }
    }

    public void CommandOption()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallOption(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }
}