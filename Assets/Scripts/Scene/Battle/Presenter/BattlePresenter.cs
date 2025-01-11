using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    using Battle;
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
        private CommandType _backCommandType = CommandType.None;
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
            _view.SetHelpText("");
            Initialize();
        }

        private async void Initialize()
        {
            _view.SetBattleBusy(true);
            _model.CreateBattleData();
            //await _model.LoadBattleResources(_model.Battlers);
            //var bgm = await _model.GetBattleBgm();
            //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
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
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);

            _view.ClearCurrentSkillData();
            _view.CreateObject();
            _view.RefreshTurn(_model.TurnCount);
            _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
            _view.ChangeBackCommandActive(false);
            _view.SetBattleAutoButton(false);
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(16010));
            _view.SetSkillLogButton(DataSystem.GetText(16020));
            _view.SetActors(MakeListData(_model.BattlerActors()));
            _view.SetEnemies(MakeListData(_model.BattlerEnemies()));
            _view.SetGridMembers(_model.Battlers);
            _view.BattlerBattleClearSelect();

            _view.RefreshStatus();
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
            _view.SetHelpInputInfo("BATTLE");
            _view.SetEvent((type) => UpdateCommand(type));
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.StartUIAnimation();
            _view.SetBattleAutoButton(true);
            //_view.StartBattle(_model.BattlerEnemies().Count);
            await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);
            _view.SetBattleSkipActive(true);
            _view.UpdateStartActivate();
            /*
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
            */

            _view.SetBattleBusy(false);
            CommandStartBattleAction();
            _busy = false;
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            /*
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 600)
                {
                    // 初めてボスバトル
                    checkFlag = _model.CurrentSelectRecord().SymbolType == SymbolType.Boss;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param3 == 610)
                {
                    checkFlag = false;
                }
                if (tutorialData.Param3 == 620)
                {
                    checkFlag = _model.TargetEnemy != null && _model.TargetEnemy.Index > 100;
                    if (checkFlag)
                    {
                        _busy = false;
                    }
                }
                return checkFlag;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)Scene.Battle,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                CheckTrueAction = () => 
                {
                    _busy = true;
                },
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
            */
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            LogOutput.Log(viewEvent.commandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.ChangeBattleAuto:
                    //CommandChangeBattleAuto();
                    break;
                case CommandType.ChangeBattleSpeed:
                    CommandChangeBattleSpeed((int)viewEvent.template);
                    break;
                case CommandType.SkipBattle:
                    CommandSkipBattle();
                    break;
            }
            if (_busy)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.UpdateAp:
                    CommandUpdateAp();
                    break;
                case CommandType.OnDecideSkill:
                    CommandDecideSkill();
                    break;
                case CommandType.OnSelectSkill:
                    CommandOnSelectSkill((SkillInfo)viewEvent.template);
                    break;
                case CommandType.OnSelectTarget:
                    CommandOnSelectTarget((InputKeyType)viewEvent.template);
                    break;
                case CommandType.AttributeType:
                    //RefreshSkillInfos();
                    break;
                case CommandType.StartSelect:
                    CommandStartSelect();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.Escape:
                    CommandEscape();
                    break;
                case CommandType.EnemyDetail:
                    CommandEnemyDetail((int)viewEvent.template);
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case CommandType.SkillLog:
                    CommandSkillLog();
                    break;
            }
            //CheckTutorialState(viewEvent.commandType);
        }

        private void CommandBack()
        {
            if (_backCommandType != CommandType.None)
            {
                //UpdateCommand(_backCommandType);
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

        /// <summary>
        /// バトル開始時のパッシブを付与
        /// </summary>
        private void CommandStartBattleAction()
        {
            _view.UpdateGridLayer();
            _model.CheckTriggerPassiveInfos(BattleUtility.StartTriggerTimings(),null,null);
        }

        private async void CommandUpdateAp()
        {
            /*
            var currentActionInfo = _model.CurrentActionInfo;
            if (currentActionInfo != null)
            {
                _view.SetBattleBusy(true);
                _model.SetActionBattler(currentActionInfo.SubjectIndex);
                var targetIndexes = _model.MakeAutoSelectIndex(currentActionInfo);
                MakeActionResultInfoTargetIndexes(targetIndexes);
                return;
            }
            */
            var currentBattler = CheckApCurrentBattler();
            if (currentBattler == null)
            {
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
                _view.UpdateGridLayer();
            } else
            {
                CommandStartSelect();
            }
        }

        private void StartWaitCommand(ActionInfo actionInfo)
        {
            _model.WaitCommand(actionInfo);
            CommandEndAnimation();
        }

        private void PlayAnimation(AnimationData animationData,AnimationType animationType,List<int> targetIndexList,bool isCurse = false)
        {            
            var animation = ResourceSystem.LoadResourceEffect(animationData.AnimationPath);
            _view.ClearDamagePopup();
            if (animationType == AnimationType.All)
            {
                _view.StartAnimationAll(animation,animationData.Position,animationData.Scale,animationData.Speed);
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
                var damageType = actionResultInfo.Critical || actionResultInfo.WeakPoint ? DamageType.HpCritical : DamageType.HpDamage;
                _view.StartDamage(targetIndex,damageType,actionResultInfo.HpDamage,needPopupDelay);
                if (needDamageBlink)
                {
                    _view.StartBlink(targetIndex);
                    PlayDamageSound(damageType);
                }
            }
            if (actionResultInfo.WeakPoint)
            {
                _model.HitWeakPoint(actionResultInfo.TargetIndex,actionResultInfo.SkillId);
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
                //if (!actionResultInfo.DeadIndexList.Contains(targetIndex) && _model.GetBattlerInfo(targetIndex).IsAlive())
                //{
                    reDamage += actionResultInfo.ReDamage;
                //}
                reDamage += actionResultInfo.CurseDamage;
                if (reDamage > 0)
                {
                    var damageType = actionResultInfo.Critical || actionResultInfo.WeakPoint ? DamageType.HpCritical : DamageType.HpDamage;
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
            foreach (var displayUpperState in actionResultInfo.DisplayUpperStates)
            {
                _view.StartStatePopup(displayUpperState.TargetIndex,DamageType.State,displayUpperState.Master.Name + DataSystem.GetText(16230));
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
            await ExecActionResultInfos(RemovePassiveResults,true);
        }

        public async UniTask<bool> ExecActionResultInfos(List<ActionResultInfo> resultInfos,bool removePassive = false)
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
                        // パッシブが消えるアニメーションは固定
                        if (removePassive)
                        {
                            animationData = BattleUtility.AnimationData(61);
                        }
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
                ActorInfos = _model.SceneParam.ActorInfos,
                InBattle = true
            };
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16110)); 
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();  
                strategySceneInfo.BattleTurn = -1; 
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(false,strategySceneInfo);
                strategySceneInfo.BattleResultVictory = false;
                _model.CurrentStage.GainLoseCount();
            } else
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(16100));
                _view.BattleVictory(_model.BattlerActors()[0].Index);
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                strategySceneInfo.BattleTurn = _model.TurnCount;
                strategySceneInfo.BattleResultScore = _model.MakeBattleScore(true,strategySceneInfo);
                strategySceneInfo.BattleResultVictory = true;
                _model.AddEnemyInfoSkillId();
            }
            
            _model.EndBattle();
            _battleEnded = true;
            _view.HideStateOverlay();
            if (_skipBattle)
            {
                _view.CommandGameSystem(Base.CommandType.CallLoading);
            }
            await UniTask.DelayFrame((int)(150f / GameSystem.ConfigData.BattleSpeed));
            //_view.SetBattleBusy(false);
            /*
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            */
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            //_view.CommandChangeViewToTransition(null);
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
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
            CommandCallSideMenu(MakeListData(_model.SideMenu()),() => 
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

        private void CommandChangeBattleSpeed(int plus)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            ConfigUtility.ChangeBattleSpeed(plus);
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