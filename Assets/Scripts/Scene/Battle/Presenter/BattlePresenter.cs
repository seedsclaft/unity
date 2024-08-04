using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        BattleModel _model = null;
        BattleView _view = null;

        private bool _busy = true;
        private bool _skipBattle = false;
    #if UNITY_EDITOR
        private bool _debug = false;
        public void SetDebug(bool busy)
        {
            _debug = busy;
        }
        private bool _testBattle = false;
    #endif
        private bool _triggerAfterChecked = false;
        /*
        private bool _triggerInterruptChecked = false;
        private bool _triggerUseBeforeChecked = false;
        private bool _triggerOpponentBeforeChecked = false;
        */
        private bool _slipDamageChecked = false;
        private bool _regenerateChecked = false;
        private bool _battleEnded = false;
        private Battle.CommandType _backCommandType = Battle.CommandType.None;
        public BattlePresenter(BattleView view)
        {
            if (view == null) return;
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
            _view.SetHelpText("");
            _view.CreateBattleBackGround(_model.BattleBackGroundObject());
            Initialize();
        }

        private async void Initialize()
        {
            _view.SetBattleBusy(true);
            _model.CreateBattleData();
            await _model.LoadBattleResources(_model.Battlers);
            /*
            if (SoundManager.Instance.CrossFadeMode == false)
            {
                var bgm = await _model.GetBattleBgm();
                SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            }
            */
            _view.CommandGameSystem(Base.CommandType.CloseLoading);

            ViewInitialize();
            
            _view.CommandStartTransition(() => 
            {
                StartBattle();
            });
        }

        public void ViewInitialize()
        {
            _view.SetUIButton();

            _view.ClearCurrentSkillData();
            _view.CreateObject();
            _view.RefreshTurn(_model.TurnCount);
            _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
            _view.ChangeBackCommandActive(false);
            _view.SetBattleAutoButton(false);
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(62));
            _view.SetSkillLogButton(DataSystem.GetText(63));
            _view.SetActors(_model.BattlerActors());
            _view.SetEnemies(_model.BattlerEnemies());
            _view.BattlerBattleClearSelect();

    #if UNITY_EDITOR
            if (_view.TestMode == true && _view.TestBattleMode)
            {
                StartBattle();
                _model.MakeTestBattleAction();
                _testBattle = _model.testActionDates.Count > 0;
                return;
            }
    #endif
        }

        private async void StartBattle()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.StartUIAnimation();
            _view.SetBattleAutoButton(true);
            await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);
            _view.SetBattleSkipActive(true);

            var isAbort = CheckAdvStageEvent(EventTiming.StartBattle,() => 
            {
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
            CommandStartBattleAction();
            _busy = false;
        }

        private void UpdateCommand(BattleViewEvent viewEvent)
        {
            if (viewEvent.commandType == Battle.CommandType.ChangeBattleAuto)
            {
                CommandChangeBattleAuto();
            }
            if (viewEvent.commandType == Battle.CommandType.ChangeBattleSpeed)
            {
                CommandChangeBattleSpeed();
            }
            if (viewEvent.commandType == Battle.CommandType.SkipBattle)
            {
                CommandSkipBattle();
            }
            if (_busy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case Battle.CommandType.UpdateAp:
                    CommandUpdateAp();
                    break;
                case Battle.CommandType.SelectedSkill:
                    CommandSelectedSkill((SkillInfo)viewEvent.template);
                    break;
                case Battle.CommandType.ActorList:
                case Battle.CommandType.EnemyLayer:
                    var targetIndexes = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo,(int)viewEvent.template);
                    CommandSelectTargetIndexes(targetIndexes);
                    break;
                case Battle.CommandType.SelectActorList:
                case Battle.CommandType.SelectEnemyList:
                    var targetIndexes2 = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo,(int)viewEvent.template);
                    _view.UpdateSelectIndexList(targetIndexes2);
                    break;
                case Battle.CommandType.AttributeType:
                    RefreshSkillInfos();
                    break;
                case Battle.CommandType.DecideActor:
                    CommandDecideActor();
                    break;
                case Battle.CommandType.SelectEnemy:
                    CommandSelectEnemy();
                    break;
                case Battle.CommandType.StartSelect:
                    CommandStartSelect();
                    break;
                case Battle.CommandType.Back:
                    CommandBack();
                    break;
                case Battle.CommandType.Escape:
                    CommandEscape();
                    break;
                case Battle.CommandType.EnemyDetail:
                    CommandEnemyDetail((int)viewEvent.template);
                    break;
                case Battle.CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case Battle.CommandType.SkillLog:
                    CommandSkillLog();
                    break;
            }
        }

        private void UpdatePopupEscape(ConfirmCommandType confirmCommandType)
        {
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
                var popupInfo = new ConfirmInfo(DataSystem.GetText(410),(a) => UpdatePopupEscape((ConfirmCommandType)a));
                _view.CommandCallConfirm(popupInfo);
            } else
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetText(412),(a) => UpdatePopupNoEscape((ConfirmCommandType)a));
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

        private void CommandStartBattleAction()
        {
            _view.UpdateGridLayer();
            _model.CheckTriggerPassiveInfos(BattleUtility.StartTriggerTimings(),null,null);
        }

        private async void CommandUpdateAp()
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
                var CurrentActionInfo = _model.CurrentActionInfo;
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
                    await RemovePassiveInfos();
                }
            }
            CommandStartSelect();
        }

        private void CommandStartSelect()
        {
            // Ap更新で行動するキャラがいる
            if (_model.CurrentBattler != null)
            {
                var currentBattler = _model.CurrentBattler;
                _model.UpdateApModify(currentBattler);
                _view.UpdateGridLayer();
                _view.SetBattleBusy(true);
                if (_model.CurrentTurnBattler == null)
                {
                    _model.SetCurrentTurnBattler(currentBattler);
                    // 解除判定は行動開始の最初のみ
                    var removed =_model.UpdateNextSelfTurn();
                    foreach (var removedState in removed)
                    {
                        _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
                    }
                    // 行動開始前トリガー
                    _model.CheckTriggerPassiveInfos(BattleUtility.BeforeTriggerTimings(),null,null);
                }
                // 行動不可の場合は行動しない
                if (!_model.EnableCurrentBattler())
                {
                    MakeActionInfo(_model.CurrentBattler,0);
                    return;
                }
#if UNITY_EDITOR
                if (_testBattle && _model.TestSkillId() != 0)
                {    
                    int testSkillId = _model.TestSkillId();
                    MakeActionInfo(_model.CurrentBattler,testSkillId);
                    _model.SeekActionIndex();
                    return;
                }
#endif
                CommandAutoSkillId();
                /*
                if (currentBattler.IsActor)
                {
                    if (GameSystem.ConfigData.BattleAuto == true)
                    {
                        // オート戦闘の場合
                        CommandAutoSkillId();
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

                    CommandAutoSkillId();
                }
                */
            }
        }

        private void MakeActionInfo(BattlerInfo battlerInfo,int skillId,int oneTargetIndex = -1)
        {
            var skillInfo = battlerInfo.Skills.Find(a => a.Id == skillId);
            if (skillInfo == null)
            {
                skillInfo = new SkillInfo(skillId);
            }
            var actionInfo = _model.MakeActionInfo(battlerInfo,skillInfo,false,false);
            _model.AddActionInfo(actionInfo,false);
            // 対象を自動決定
            CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo,oneTargetIndex));
        }

        private void CommandAutoSkillId()
        {
            var currentBattler = _model.CurrentBattler;
            int autoSkillId;
            int targetIndex;
            (autoSkillId,targetIndex) = _model.MakeAutoSkillTriggerSkillId(currentBattler);
            if (autoSkillId == -1)
            {
                autoSkillId = 20010;
            }
            MakeActionInfo(currentBattler,autoSkillId,targetIndex);
        }

        // マニュアル行動選択開始
        private void CommandDecideActor()
        {
            _view.SetAnimationBusy(false);
            _view.SetHelpText(DataSystem.GetText(15010));
            _view.SelectedCharacter(_model.CurrentBattler);
            _view.SetCondition(_model.SelectCharacterConditions());
            _view.ChangeSideMenuButtonActive(true);
            RefreshSkillInfos();
            _view.BattlerBattleClearSelect();
            _view.ChangeBackCommandActive(false);
            _view.SetBattlerThumbAlpha(true);
            var isAbort = CheckAdvStageEvent(EventTiming.TurnedBattle,() => 
            {  
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
            _view.SetHelpText(DataSystem.GetText(15010));
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
            _model.AddActionInfo(actionInfo,false);

            // 対象選択開始
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
            var actionInfo = _model.CurrentActionInfo;
            _view.SetHelpText("");
            _view.ChangeBackCommandActive(false);
            SetActionInfo(actionInfo);
            MakeActionResultInfo(indexList);
            // 行動変化対応のため再取得
            actionInfo = _model.CurrentActionInfo;
            if (actionInfo != null)
            {
                if (actionInfo.IsWait())
                {
                    StartWaitCommand(actionInfo);
                } else
                {
                    StartActionInfoAnimation(actionInfo);
                }
            }
        }


        private void StartWaitCommand(ActionInfo actionInfo)
        {
            _model.WaitUnison();
            _view.StartStatePopup(actionInfo.SubjectIndex,DamageType.State,"+" + DataSystem.States.Find(a => a.StateType == StateType.Wait).Name);
            _view.BattlerBattleClearSelect();
            _view.ShowStateOverlay();
            _view.RefreshStatus();
            _view.SetBattlerThumbAlpha(true);
            _model.SetCurrentTurnBattler(null);
            _view.SetBattleBusy(false);
        }

        private void SetActionInfo(ActionInfo actionInfo)
        {
            if (actionInfo != null)
            {
                _model.SetActionInfo(actionInfo);
            }
        }

        private void MakeActionResultInfo(List<int> indexList)
        {
            var actionInfo = _model.CurrentActionInfo;
            if (actionInfo != null)
            {
                // 自分,味方,相手の行動前パッシブ
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeActionInfoTriggerTimings(),actionInfo,actionInfo.ActionResults);
                
                _view.BattlerBattleClearSelect();
                // 行動前の行動のため再取得
                actionInfo = _model.CurrentActionInfo;
                _model.MakeActionResultInfo(actionInfo,indexList);
                actionInfo.SetCandidateTargetIndexList(indexList);
                _model.MakeCurseActionResults(actionInfo,indexList);
                // 行動割り込みスキル判定
                _model.CheckTriggerSkillInfos(TriggerTiming.Interrupt,actionInfo,actionInfo.ActionResults,true);
                _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Interrupt},actionInfo,actionInfo.ActionResults);
                _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Use},actionInfo,actionInfo.ActionResults);
            }
        }

        private void PlayAnimation(AnimationData animationData,AnimationType animationType,List<int> targetIndexList,bool isCurse = false)
        {            
            var animation = ResourceSystem.LoadResourceEffect(animationData.AnimationPath);
            var soundTimings = _model.SkillActionSoundTimings(animationData.AnimationPath);
            _view.PlayMakerEffectSound(soundTimings);
            _view.ClearDamagePopup();
            if (animationType == AnimationType.All)
            {
                _view.StartAnimationAll(animation);
            } else
            {
                foreach (var targetIndex in targetIndexList)
                {
                    var oneAnimation = isCurse ? ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_curse_001") : animation;
                    _view.StartAnimation(targetIndex,oneAnimation,animationData.Position,animationData.Scale,animationData.Speed);
                }
            }
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
                _model.GainAttackedCount(actionResultInfo.TargetIndex);
                _model.GainMaxDamage(actionResultInfo.TargetIndex,actionResultInfo.HpDamage);
                if (actionResultInfo.Critical)
                {
                    _model.GainBeCriticalCount(actionResultInfo.TargetIndex);
                }
                var damageType = actionResultInfo.Critical ? DamageType.HpCritical : DamageType.HpDamage;
                _view.StartDamage(targetIndex,damageType,actionResultInfo.HpDamage,needPopupDelay);
                if (needDamageBlink)
                {
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
            if (actionResultInfo.CtDamage > 0)
            {    
                _view.StartDamage(targetIndex,DamageType.MpDamage,actionResultInfo.CtDamage);
            }
            if (actionResultInfo.CtHeal > 0)
            {
                _view.StartHeal(targetIndex,DamageType.MpHeal,actionResultInfo.CtHeal);
            }
            if (actionResultInfo.ApHeal > 0)
            {    
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(432,actionResultInfo.ApHeal.ToString()));
            }
            if (actionResultInfo.ApDamage > 0)
            {    
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(433,actionResultInfo.ApDamage.ToString()));
            }
            if (actionResultInfo.ReDamage > 0 || actionResultInfo.CurseDamage > 0)
            {
                var reDamage = 0;
                if (!actionResultInfo.DeadIndexList.Contains(targetIndex) && _model.GetBattlerInfo(targetIndex).IsAlive())
                {
                    reDamage += actionResultInfo.ReDamage;
                }
                reDamage += actionResultInfo.CurseDamage;
                if (reDamage > 0)
                {
                    var damageType = actionResultInfo.Critical ? DamageType.HpCritical : DamageType.HpDamage;
                    PlayDamageSound(damageType);
                    _view.StartDamage(actionResultInfo.SubjectIndex,damageType,reDamage);
                    _view.StartBlink(actionResultInfo.SubjectIndex);
                }
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
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetText(431));
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
                SoundManager.Instance.PlayStaticSe(SEType.Defeat);
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

        private async UniTask RemovePassiveInfos()
        {
            var RemovePassiveResults = _model.CheckRemovePassiveInfos();
            await ExecActionResultInfos(RemovePassiveResults);
        }

        public async UniTask<bool> ExecActionResultInfos(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
        {
            _model.AdjustActionResultInfo(resultInfos);
            if (_skipBattle == false)
            {
                foreach (var resultInfo in resultInfos)
                {
                    var skillData = DataSystem.FindSkill(resultInfo.SkillId);
                    if (skillData != null)
                    {
                        var animationData = BattleUtility.AnimationData(skillData.AnimationId);
                        if (animationData != null && animationData.AnimationPath != "" && GameSystem.ConfigData.BattleAnimationSkip == false)
                        {
                            PlayAnimation(animationData,skillData.AnimationType,new List<int>(){resultInfo.TargetIndex});
                            await UniTask.DelayFrame(_model.WaitFrameTime(animationData.DamageTiming));
                        }
                    }
                    // ダメージ表現をしない
                    PopupActionResult(resultInfo,resultInfo.TargetIndex,true,false);
                    await UniTask.DelayFrame(_model.WaitFrameTime(16));
                }
            }
            _model.ExecActionResultInfos(resultInfos,true);
            if (resultInfos.Count > 0)
            {
                _view.RefreshStatus();
            }
            return true;
        }

        private async void EndTurn()
        {
            var actionInfo = _model.CurrentActionInfo;
            // ターン終了
            _view.RefreshStatus();
            // PlusSkill
            _model.CheckPlusSkill(actionInfo);
            // Passive付与
            _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
            //PassiveInfoAction(BattleUtility.AfterTriggerTimings());
            // Passive解除
            await RemovePassiveInfos();

            bool isDemigodActor = false;
            if (_model.CurrentBattler != null)
            {
                isDemigodActor = _model.CurrentBattler.IsState(StateType.Demigod);
            }
            // 行動者のActionInfoか
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            // TriggerAfterがある
            var result = _model.CheckTriggerSkillInfos(TriggerTiming.After,actionInfo,actionInfo.ActionResults,true);
            var result2 = _model.CheckTriggerSkillInfos(TriggerTiming.AfterAndStartBattle,actionInfo,actionInfo.ActionResults,true);
            result.AddRange(result2);

            if (result.Count == 0 && _triggerAfterChecked == false && isTriggeredSkill == false)
            {
                // 行動者のターンを進める
                var removed =_model.UpdateTurn();
                foreach (var removedState in removed)
                {
                    _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
                }
                // Passive付与
                _model.CheckTriggerPassiveInfos(BattleUtility.AfterTriggerTimings(),null,null);
                    
                // Passive解除
                await RemovePassiveInfos();

                // 10010行動後にAP+
                var gainAp = _model.CheckActionAfterGainAp(actionInfo);
                if (gainAp > 0)
                {
                    if (_skipBattle == false)
                    {
                        _view.StartHeal(_model.CurrentTurnBattler.Index,DamageType.MpHeal,gainAp); 
                        await UniTask.DelayFrame(_model.WaitFrameTime(16));           
                    }
                    _model.ActionAfterGainAp(gainAp);
                    _view.RefreshStatus();
                }

            }
            var reaction = _model.CheckReaction(actionInfo);
            _model.TurnEnd(actionInfo);
            if (reaction)
            {
                _view.SetBattleBusy(false);
                return;
            }
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

            // 次の行動があれば続ける
            var CurrentActionInfo = _model.CurrentActionInfo;
            if (CurrentActionInfo != null)
            {
                _battleEnded = false;
                CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(CurrentActionInfo));
                return;
            }

            if (isDemigodActor == true)
            {
                var isAbort = CheckAdvStageEvent(EventTiming.AfterDemigod,() => 
                { 
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
            _view.RefreshTurn(_model.TurnCount);
            _view.ShowStateOverlay();
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
            var strategySceneInfo = new StrategySceneInfo
            {
                ActorInfos = _model.BattleMembers()
            };
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15020));
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                strategySceneInfo.BattleTurn = _model.TurnCount;
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(true);
                strategySceneInfo.BattleResultVictory = true;
            } else
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15030)); 
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();  
                strategySceneInfo.BattleTurn = -1; 
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(false);
                strategySceneInfo.BattleResultVictory = false;
            }
            _model.EndBattle();
            _battleEnded = true;
            _view.HideStateOverlay();
            if (_skipBattle)
            {
                _view.CommandGameSystem(Base.CommandType.CallLoading);
            }
            await UniTask.DelayFrame(150);
            _view.SetBattleBusy(false);
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
        }

        private void CommandSelectEnemy()
        {
        }

        private void CommandSkillLog()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var SkillLogViewInfo = new SkillLogViewInfo(_model.SkillLogs,() => 
            {
                _busy = false;
            });

            _view.CommandCallSkillLog(SkillLogViewInfo);
        }

        private void CommandSelectSideMenu()
        {
            if (_busy) return;
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {

                },
                CommandLists = _model.SideMenu()
            };
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }    
        
        private void CommandChangeBattleAuto()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _model.ChangeBattleAuto();
            _view.ChangeBattleAuto(GameSystem.ConfigData.BattleAuto == true);
            if (_view.AnimationBusy == false && _view.BattleBusy && GameSystem.ConfigData.BattleAuto == true)
            {
                _model.ClearActionInfo();
                _view.BattlerBattleClearSelect();
                _view.HideSkillActionList();
                _view.HideBattleThumb();
                CommandAutoSkillId();
            }
        }

        private void CommandChangeBattleSpeed()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            ConfigUtility.ChangeBattleSpeed(1);
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
        }

        private void CommandSkipBattle()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _skipBattle = true;
            _view.CommandGameSystem(Base.CommandType.CallLoading);
        }
    }
}