using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class BattleReplayPresenter : BasePresenter
    {
        BattleReplayModel _model = null;
        BattleView _view = null;
        bool _busy = false;
        private bool _skipBattle = false;


        public BattleReplayPresenter(BattleView view)
        {
            _view = view;
            SetView(_view);
            _model = new BattleReplayModel();
            SetModel(_model);
            _view.SetHelpText("");
            _view.CreateBattleBackGround(_model.BattleBackGroundObject());
            Initialize();
        }

        public void Initialize()
        {
            _view.SetBattleBusy(true);
            _model.CreateBattleData(_model.SaveBattleInfo);
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
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(62));
            _view.SetSkillLogButton(DataSystem.GetText(63));
            _view.SetActors(_model.BattlerActors());
            _view.SetEnemies(_model.BattlerEnemies());
            _view.BattlerBattleClearSelect();

        }
        private async void StartBattle()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.StartUIAnimation();
            await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);
            _view.SetBattleSkipActive(true);

            //_view.SetBattleBusy(false);
            SeekReplayData();
            _busy = false;
        }

        private void UpdateCommand(BattleViewEvent viewEvent)
        {
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
                case Battle.CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case Battle.CommandType.SkillLog:
                    CommandSkillLog();
                    break;
            }
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
            var sideMenuViewInfo = new SideMenuViewInfo();
            sideMenuViewInfo.EndEvent = () => {

            };
            sideMenuViewInfo.CommandLists = _model.SideMenu();
            _view.CommandCallSideMenu(sideMenuViewInfo);
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

        private async void SeekReplayData()
        {
            _model.SeekReplayCounter();
            var actionData = _model.GetSaveActionData();
            if (actionData != null)
            {
                StartActionInfoAnimation(actionData);
                return;
            }
            var actionResultData = _model.GetSaveResultData();
            if (actionResultData != null)
            {
                await ExecActionResultInfos(actionResultData);
                SeekReplayData();
                return;
            }
            BattleEnd(true);
        }
    
        /// <summary>
        /// ActionInfoのアニメーション開始
        /// </summary>
        /// <param name="actionInfo"></param>
        public void StartActionInfoAnimation(ActionInfo actionInfo)
        {
            if (_skipBattle)
            {
                CommandEndAnimation();
                return;
            }
            var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor;
            if (actionInfo.Master.SkillType == SkillType.Messiah && isActor || actionInfo.Master.SkillType == SkillType.Awaken && isActor)
            {
                StartAnimationDemigod();
            } else
            {
                StartAnimationSkill();
            }
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
                            await UniTask.DelayFrame((int)(animationData.DamageTiming / GameSystem.ConfigData.BattleSpeed));
                        }
                    }
                    // ダメージ表現をしない
                    PopupActionResult(resultInfo,resultInfo.TargetIndex,true,false);
                    await UniTask.DelayFrame(_model.WaitFrameTime(16));
                }
            }
            _model.ExecActionResultInfos(resultInfos);
            if (resultInfos.Count > 0)
            {
                _view.RefreshStatus();
            }
            return true;
        }

        private async void BattleEnd(bool victory)
        {
            //if (_battleEnded == true) return;
            if (victory)
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15020));
            } else
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15030)); 
            }
            _model.EndBattle();
            _view.HideStateOverlay();
            if (_skipBattle)
            {
                _view.CommandGameSystem(Base.CommandType.CallLoading);
            }
            await UniTask.DelayFrame(180);
            _view.SetBattleBusy(false);
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            var tacticsSceneInfo = new TacticsSceneInfo
            {
                ReturnBeforeBattle = true,
                SeekIndex = _model.CurrentStage.CurrentSeekIndex
            };
            _view.CommandGotoSceneChange(Scene.Tactics,tacticsSceneInfo);
        }

        
        /// <summary>
        /// ActionInfoのアニメーション終了処理
        /// </summary>
        /// <param name="actionInfo"></param>
        private void CommandEndAnimation()
        {
            var actionInfo = _model.CurrentActionInfo();
            if (actionInfo != null)
            {
                // ダメージなどを適用
                _model.ExecCurrentAction(actionInfo,false);
                //_model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
                StartDeathAnimation(actionInfo.ActionResults);
                StartAliveAnimation(actionInfo.ActionResults);
                // 繰り返しがある場合
                /*
                if (actionInfo.RepeatTime > 0)
                {
                    //_model.ResetTargetIndexList(actionInfo);
                    //MakeActionResultInfo(actionInfo.CandidateTargetIndexList);
                    // 再取得
                    actionInfo = _model.CurrentActionInfo();
                    RepeatAnimationSkill(actionInfo);
                    return;
                }
                */
            }
            _view.ClearCurrentSkillData();
            /*
            // スリップダメージ
            if (_triggerAfterChecked == false && _slipDamageChecked == false)
            {
                if (_model.CurrentTurnBattler != null && actionInfo.SubjectIndex == _model.CurrentTurnBattler.Index)
                {
                    _slipDamageChecked = true;
                    var slipResult = _model.CheckBurnDamage();
                    if (slipResult.Count > 0)
                    {
                        //StartAnimationSlipDamage(slipResult);
                        return;
                    }
                }
            }
            */
            //EndTurn();
            SeekReplayData();
        }

        /// <summary>
        /// 覚醒アニメーション再生してからアニメーション再生
        /// </summary>
        private async void StartAnimationDemigod()
        {
            var actionInfo = _model.CurrentActionInfo();
            await _view.StartAnimationDemigod(_model.GetBattlerInfo(actionInfo.SubjectIndex),actionInfo.Master);
            StartAnimationSkill();
        }

        private async void StartAnimationSkill()
        {           
            var actionInfo = _model.CurrentActionInfo();
            _view.ChangeSideMenuButtonActive(false);
            _view.SetBattlerThumbAlpha(true);
            //_view.ShowEnemyStateOverlay();
            _view.HideStateOverlay();
            _view.SetAnimationBusy(true);
            if (actionInfo.ActionResults.Count == 0)
            {
                CommandEndAnimation();
                return;
            }
            
            var selfAnimation = ResourceSystem.LoadResourceEffect("MAGICALxSPIRAL/WHead1");
            _view.StartAnimationBeforeSkill(actionInfo.SubjectIndex,selfAnimation);
            
            if (actionInfo.TriggeredSkill)
            {
                if (actionInfo.Master.IsDisplayBattleSkill() && _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                {
                    _view.ShowCutinBattleThumb(_model.GetBattlerInfo(actionInfo.SubjectIndex));
                }
            }
            if (actionInfo.Master.IsDisplayBattleSkill() || _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor == false)
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo);
            }
            
            var animationData = BattleUtility.AnimationData(actionInfo.Master.AnimationId);
            if (animationData != null && animationData.AnimationPath != "" && GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                var targetIndexList = new List<int>();
                foreach (var actionResult in actionInfo.ActionResults)
                {
                    targetIndexList.Add(actionResult.TargetIndex);
                }
                PlayAnimation(animationData,actionInfo.Master.AnimationType,targetIndexList,false);
                StartAliveAnimation(actionInfo.ActionResults);

                await UniTask.DelayFrame((int)(animationData.DamageTiming / GameSystem.ConfigData.BattleSpeed));
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
                }
                var waitFrame = (int)(48 / GameSystem.ConfigData.BattleSpeed);
                if (actionInfo.RepeatTime != 0 && waitFrame > 1)
                {
                    waitFrame = 8;
                }
                await UniTask.DelayFrame(waitFrame);
            } else
            {
                StartAliveAnimation(actionInfo.ActionResults);
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
                }
                var waitFrame = _model.WaitFrameTime(30);
                if (actionInfo.RepeatTime != 0 && waitFrame > 1)
                {
                    waitFrame = 8;
                }
                await UniTask.DelayFrame(waitFrame);
            }
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
                //_model.GainAttackedCount(actionResultInfo.TargetIndex);
                //_model.GainMaxDamage(actionResultInfo.TargetIndex,actionResultInfo.HpDamage);
                if (actionResultInfo.Critical)
                {
                    //_model.GainBeCriticalCount(actionResultInfo.TargetIndex);
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
            if (actionResultInfo.ReDamage > 0)
            {
                if (_model.GetBattlerInfo(targetIndex).IsAlive())
                {
                    var damageType = actionResultInfo.Critical ? DamageType.HpCritical : DamageType.HpDamage;
                    PlayDamageSound(damageType);
                    _view.StartDamage(actionResultInfo.SubjectIndex,damageType,actionResultInfo.ReDamage);
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


    }
}