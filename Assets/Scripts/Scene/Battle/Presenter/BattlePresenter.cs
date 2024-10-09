using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Battle;

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
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
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
            _view.SetBattleSkipButton(DataSystem.GetText(16010));
            _view.SetSkillLogButton(DataSystem.GetText(16020));
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

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            var TutorialDates = _model.SceneTutorialDates(Scene.Battle);
            if (TutorialDates.Count > 0)
            {
                var checkFlag = true;
                var tutorialData = TutorialDates[0];
                if (!checkFlag)
                {
                    return;
                }
                _busy = true;
                _view.SetBusy(true);
                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.Tutorial,
                    template = tutorialData,
                    EndEvent = () =>
                    {
                        _busy = false;
                        _view.SetBusy(false);
                        _model.ReadTutorialData(tutorialData);
                        CheckTutorialState(commandType);
                    }
                };
                _view.CommandCallPopup(popupInfo);
            }
        }

        private void UpdateCommand(BattleViewEvent viewEvent)
        {
            switch (viewEvent.commandType)
            {
                case Battle.CommandType.ChangeBattleAuto:
                    //CommandChangeBattleAuto();
                    break;
                case Battle.CommandType.ChangeBattleSpeed:
                    CommandChangeBattleSpeed();
                    break;
                case Battle.CommandType.SkipBattle:
                    CommandSkipBattle();
                    break;
                case Battle.CommandType.EnemyLayer:
                    CommandTargetEnemy((BattlerInfo)viewEvent.template);
                    break;
                case Battle.CommandType.ActorList:
                    CommandTargetActor((BattlerInfo)viewEvent.template);
                    break;
                case Battle.CommandType.CancelSelectActor:
                    CancelSelectActor();
                    break;
                case Battle.CommandType.CancelSelectEnemy:
                    CancelSelectEnemy();
                    break;
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
                    //CommandSelectedSkill((SkillInfo)viewEvent.template);
                    break;
                case Battle.CommandType.ActorList:
                    //CommandTargetEnemy((BattlerInfo)viewEvent.template);
                    // var targetIndexes = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo,(int)viewEvent.template);
                    //CommandSelectTargetIndexes(targetIndexes);
                    break;
                case Battle.CommandType.SelectActorList:
                case Battle.CommandType.SelectEnemyList:
                    /*
                    var targetIndexes2 = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo,(int)viewEvent.template);
                    _view.UpdateSelectIndexList(targetIndexes2);
                    */
                    break;
                case Battle.CommandType.AttributeType:
                    //RefreshSkillInfos();
                    break;
                case Battle.CommandType.DecideActor:
                    //CommandDecideActor();
                    break;
                case Battle.CommandType.SelectEnemy:
                    //CommandSelectEnemy();
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
            CheckTutorialState(viewEvent.commandType);
        }

        private void UpdatePopupEscape(ConfirmCommandType confirmCommandType)
        {
        }

        private void UpdatePopupNoEscape(ConfirmCommandType confirmCommandType)
        {
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
        }

        private void CommandEnemyDetail(int enemyIndex)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);  
            _busy = true;
            var enemyInfo = _model.GetBattlerInfo(enemyIndex);
            CommandEnemyInfo(new List<BattlerInfo>(){enemyInfo},true,() => 
            {
                _busy = false;
            });
        }

        private void CommandStartBattleAction()
        {
            _view.UpdateGridLayer();
            _model.CheckTriggerPassiveInfos(BattleUtility.StartTriggerTimings(),null,null);
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
            _view.SetHelpInputInfo("BATTLE_AUTO");
            var currentActionInfo = _model.CurrentActionInfo;
            if (currentActionInfo != null)
            {
                _view.SetBattleBusy(true);
                _model.SetActionBattler(currentActionInfo.SubjectIndex);
                var targetIndexes = _model.MakeAutoSelectIndex(currentActionInfo);
                MakeActionResultInfoTargetIndexes(targetIndexes);
                return;
            }
            var currentBattler = CheckApCurrentBattler();
            if (currentBattler == null)
            {
                UpdateApBattlerInfos();
            } else
            {
                CommandStartSelect();
            }
        }



        // マニュアル行動選択開始
        /*
        private void CommandDecideActor()
        {
            _view.SetAnimationBusy(false);
            _view.SelectedCharacter(_model.CurrentBattler);
            _view.SetCondition(GetListData(_model.SelectCharacterConditions()));
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
        */

        /*
        private void CommandDecideEnemy()
        {
            _view.SetAnimationBusy(false);
            //_view.SelectedCharacter(_model.CurrentBattler);
            _view.SetCondition(GetListData(_model.SelectCharacterConditions()));
            //_view.ChangeSideMenuButtonActive(true);
            RefreshSkillInfos();
            _view.BattlerBattleClearSelect();
            _view.ChangeBackCommandActive(false);
            _view.SetBattlerThumbAlpha(true);
        }
        */

        // スキルを選択
        /*
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
        */


        private void StartWaitCommand(ActionInfo actionInfo)
        {
            _model.WaitCommand(actionInfo);
            CommandEndAnimation();
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
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(16200,actionResultInfo.ApHeal.ToString()));
            }
            if (actionResultInfo.ApDamage > 0)
            {    
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetReplaceText(16210,actionResultInfo.ApDamage.ToString()));
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
                _view.StartStatePopup(targetIndex,DamageType.State,DataSystem.GetText(16220));
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

        /*
        private void RefreshSkillInfos()
        {
            var skillInfos = _model.SkillActionList();
            _view.RefreshMagicList(GetListData(skillInfos),_model.SelectSkillIndex(skillInfos));
            //SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
        */

        private bool IsBattleEnd()
        {
            return _model.CheckVictory() || _model.CheckDefeat();
        }

        private async void BattleEnd()
        {
            if (_battleEnded == true) return;
            var strategySceneInfo = new StrategySceneInfo
            {
                ActorInfos = _model.BattleMembers(),
                InBattle = true
            };
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16100));
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                strategySceneInfo.BattleTurn = _model.TurnCount;
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(true,strategySceneInfo);
                strategySceneInfo.BattleResultVictory = true;
                _model.AddEnemyInfoSkillId();
                        
            } else
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16110)); 
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();  
                strategySceneInfo.BattleTurn = -1; 
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(false,strategySceneInfo);
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

        /*
        private void CommandSelectEnemy()
        {
        }
        */

        private void CommandTargetActor(BattlerInfo battlerInfo)
        {
            if (_model.TargetActor != null && _model.TargetActor.Index == battlerInfo.Index)
            {
                CancelSelectActor();
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.SetTargetActor(battlerInfo);
            _view.SetTargetActor(battlerInfo);
        }

        private void CancelSelectActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.SetTargetActor(null);
            _view.SetTargetActor(null);
        }

        private void CommandTargetEnemy(BattlerInfo battlerInfo)
        {
            if (_model.TargetEnemy != null && _model.TargetEnemy.Index == battlerInfo.Index)
            {
                CancelSelectEnemy();
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.SetTargetEnemy(battlerInfo);
            _view.SetTargetEnemy(battlerInfo);
        }

        private void CancelSelectEnemy()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.SetTargetEnemy(null);
            _view.SetTargetEnemy(null);
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
            _busy = true;
            CommandCallSideMenu(GetListData(_model.SideMenu()),() => 
            {
                _busy = false;
            });
        }    
        /*
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
                MakeActionInfoSkillTrigger();
            }
        }
        */

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