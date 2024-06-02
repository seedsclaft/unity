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
        private bool _triggerAfterChecked = false;
        private bool _slipDamageChecked = false;
        private bool _regenerateChecked = false;
        private bool _battleEnded = false;
        private Battle.CommandType _nextCommandType = Battle.CommandType.None;

        public BattleReplayPresenter(BattleView view)
        {
            _view = view;
            SetView(_view);
            _model = new BattleReplayModel();
            SetModel(_model);

            Initialize();
        }

        public async void Initialize()
        {
            var replayData = await SaveSystem.LoadBattleInfo(1);
            await UniTask.WaitUntil(() => replayData != null);
            _view.SetBattleBusy(true);
            _model.CreateBattleData();
            _model.SetSaveBattleInfo(replayData);
            _view.SetHelpText("");
            _view.CreateBattleBackGround(_model.BattleBackGroundObject());
            await _model.LoadBattleResources(_model.Battlers);
            _view.CommandGameSystem(Base.CommandType.CloseLoading);

            _view.ClearCurrentSkillData();
            _view.CreateObject();
            _view.SetUIButton();
            _view.RefreshTurn(_model.TurnCount);
            _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
            _view.ChangeBackCommandActive(false);

            _view.CommandStartTransition(() => 
            {
                BattleInitialize();
            });
        }

        public async void BattleInitialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetActors(_model.BattlerActors());
            _view.SetEnemies(_model.BattlerEnemies());
            _view.BattlerBattleClearSelect();
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.StartUIAnimation();
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(62));
            _view.SetSkillLogButton(DataSystem.GetText(63));
            _view.SetBattleAutoButton(true);
            await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);
            _view.SetBattleSkipActive(true);

            
            _view.SetBattleBusy(false);
            SeekReplayData();
            _busy = false;
        }

        private async void SeekReplayData()
        {
            var actionData = _model.GetSaveActionData();
            if (actionData != null)
            {
                StartSkillAnimation(actionData);
                return;
            }
            var actionResultData = _model.GetSaveResultData();
            if (actionResultData != null)
            {
                await ExecActionResult(actionResultData);
                _model.SeekReplayCounter();
                SeekReplayData();
                return;
            }
            if (IsBattleEnd())
            {
                BattleEnd();
                return;
            }
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
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case Battle.CommandType.UpdateAp:
                    CommandUpdateAp();
                    break;
                case Battle.CommandType.ActorList:
                case Battle.CommandType.EnemyLayer:
                    var targetIndexes = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo(),(int)viewEvent.template);
                    //CommandSelectTargetIndexes(targetIndexes);
                    break;
                case Battle.CommandType.SelectActorList:
                case Battle.CommandType.SelectEnemyList:
                    var targetIndexes2 = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo(),(int)viewEvent.template);
                    _view.UpdateSelectIndexList(targetIndexes2);
                    break;
                case Battle.CommandType.AttributeType:
                    RefreshSkillInfos();
                    break;
                case Battle.CommandType.SelectEnemy:
                    CommandSelectEnemy();
                    break;
                case Battle.CommandType.StartSelect:
                    CommandStartSelect();
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
            _model.CheckTriggerPassiveInfos(BattleUtility.StartTriggerTimings(),null,null);
        }

        private void CommandUpdateAp()
        {
            _model.MakeActionBattler();
            CommandStartSelect();
        }

        private void CommandStartSelect()
        {
            // Ap更新で行動するキャラがいる
            if (_model.CurrentBattler != null)
            {
                _view.UpdateGridLayer();
                _view.SetBattleBusy(true);
                CommandAutoSkillId();
            }
        }

        private void CommandAutoSkillId()
        {
            CommandSelectTargetIndexes();
        }

        // スキル対象を決定
        public void CommandSelectTargetIndexes()
        {
            var actionInfo = _model.GetSaveActionData();
            _view.SetHelpText("");
            _view.ChangeBackCommandActive(false);
            SetActionInfo(actionInfo);
            MakeActionResultInfo();
            // 行動変化対応のため再取得
            actionInfo = _model.GetSaveActionData();
            if (actionInfo != null)
            {
                if (actionInfo.IsUnison())
                {
                    StartWaitCommand();
                    return;
                }
                StartSkillAnimation(actionInfo);
            }
        }

        public void StartSkillAnimation(ActionInfo actionInfo)
        {
            var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor;
            if (actionInfo.Master.SkillType == SkillType.Messiah && isActor || actionInfo.Master.SkillType == SkillType.Awaken && isActor)
            {
                StartAnimationDemigod(actionInfo);
            } else
            {
                StartAnimationSkill(actionInfo);
            }
        }

        private void StartWaitCommand()
        {
            var actionInfo = _model.CurrentActionInfo();
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

        private void MakeActionResultInfo()
        {
            var actionInfo = _model.GetSaveActionData();
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();
            }
        }

        private async void StartAnimationDemigod(ActionInfo actionInfo)
        {
            if (_skipBattle)
            {
                StartAnimationSkill(actionInfo);
                return;
            }
            if (GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Demigod);
                _view.StartAnimationDemigod(_model.GetBattlerInfo(actionInfo.SubjectIndex),_model.CurrentActionInfo().Master,GameSystem.ConfigData.BattleSpeed);
                _view.HideStateOverlay();
                _view.SetAnimationBusy(true);
                await UniTask.DelayFrame((int)(20 / GameSystem.ConfigData.BattleSpeed));
                SoundManager.Instance.PlayStaticSe(SEType.Awaken);
                await UniTask.DelayFrame((int)(90 / GameSystem.ConfigData.BattleSpeed));
            }
            StartAnimationSkill(actionInfo);
        }

        private async void StartAnimationRegenerate(List<ActionResultInfo> regenerateActionResults)
        {
            var animation = ResourceSystem.LoadResourceEffect("tktk01/Cure1");
            await ExecActionResult(regenerateActionResults);
            if (_skipBattle == false)
            {
                foreach (var regenerateActionResult in regenerateActionResults)
                {
                    var targetIndex = regenerateActionResult.TargetIndex;
                    if (regenerateActionResult.HpHeal != 0)
                    {
                        _view.StartAnimation(targetIndex,animation,0);
                    }
                }
            }
            EndTurn();
        }

        private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
        {
            var animation = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_Fire_001");
            await ExecActionResult(slipDamageResults);
            if (_skipBattle == false)
            {
                foreach (var slipDamageResult in slipDamageResults)
                {
                    var targetIndex = slipDamageResult.TargetIndex;
                    if (slipDamageResult.HpDamage != 0)
                    {            
                        _view.StartAnimation(targetIndex,animation,0);
                    }
                    if (slipDamageResult.DeadIndexList.Contains(targetIndex))
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                        _view.StartDeathAnimation(targetIndex);
                    }
                }
            }
            _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),null,slipDamageResults);

            EndTurn();
        }

        private async void StartAnimationSkill(ActionInfo actionInfo)
        {           
            if (_skipBattle)
            {
                CommandEndAnimation();
                return;
            }
            _view.ChangeSideMenuButtonActive(false);
            _view.SetBattlerThumbAlpha(true);
            //_view.ShowEnemyStateOverlay();
            _view.HideStateOverlay();
            _view.SetAnimationBusy(true);
            if (actionInfo.ActionResults.Count == 0)
            {
                _nextCommandType = Battle.CommandType.SelectedSkill;
                CommandEndAnimation();
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Skill);
            var selfAnimation = ResourceSystem.LoadResourceEffect("MAGICALxSPIRAL/WHead1");
            _view.StartAnimation(actionInfo.SubjectIndex,selfAnimation,0,1f,1.0f);
            if (actionInfo.IsBattleDisplay == false)
            {
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
            }
            var animationData = BattleUtility.AnimationData(actionInfo.Master.AnimationId);
            if (actionInfo.IsBattleDisplay == false && animationData != null &&  animationData.AnimationPath != "" && GameSystem.ConfigData.BattleAnimationSkip == false)
            {
                var targetIndexList = new List<int>();
                foreach (var actionResult in actionInfo.ActionResults)
                {
                    targetIndexList.Add(actionResult.TargetIndex);
                }
                PlayAnimation(animationData,actionInfo.Master.AnimationType,targetIndexList,false);
                StartAliveAnimation(_model.CurrentActionInfo().ActionResults);

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
                StartAliveAnimation(_model.CurrentActionInfo().ActionResults);
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

        private async void RepeatAnimationSkill()
        {           
            var actionInfo = _model.CurrentActionInfo();
            if (actionInfo.ActionResults.Count == 0)
            {
                _nextCommandType = Battle.CommandType.SelectedSkill;
                CommandEndAnimation();
                return;
            }
            
            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                if (actionInfo.IsBattleDisplay == false)
                {
                    _view.SetCurrentSkillData(actionInfo.SkillInfo);
                }
            }

            StartAliveAnimation(_model.CurrentActionInfo().ActionResults);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
            }
            await UniTask.DelayFrame(_model.WaitFrameTime(8));
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
            _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),_model.CurrentActionInfo(),_model.CurrentActionInfo().ActionResults);
            
            var actionInfo = _model.CurrentActionInfo();
            if (actionInfo != null)
            {
                StartDeathAnimation(actionInfo.ActionResults);
                StartAliveAnimation(actionInfo.ActionResults);
                // 繰り返しがある場合
                if (actionInfo.RepeatTime > 0)
                {
                    _model.ResetTargetIndexList(actionInfo);
                    MakeActionResultInfo();
                    RepeatAnimationSkill();
                    return;
                }
            }
            _view.ClearCurrentSkillData();
            EndTurn();
        }

        private async UniTask<bool> ExecActionResult(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
        {
            _model.AdjustActionResultInfo(resultInfos);
            if (_skipBattle)
            {
                _model.ExecActionResultInfos(resultInfos);
                return true;
            }
            if (resultInfos.Count > 0)
            {
                var skillData = DataSystem.FindSkill(resultInfos[0].SkillId);
                if (skillData != null && skillData.IsDisplayBattleSkill())
                {
                    //_view.SetCurrentSkillData(resultInfos[0]);
                }
            }
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
            _model.ExecActionResultInfos(resultInfos);
            if (resultInfos.Count > 0)
            {
                _view.RefreshStatus();
            }
            return true;
        }

        private void EndTurn()
        {
            // ターン終了
            _view.RefreshStatus();
            _model.SeekReplayCounter();
            SeekReplayData();
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
            var strategySceneInfo = new StrategySceneInfo();
            strategySceneInfo.ActorInfos = _model.BattleMembers();
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15020));
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                _model.MakeBattleScore(true);
            } else
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15030)); 
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();   
                _model.MakeBattleScore(false);       
            }
            _model.EndBattle();
            _battleEnded = true;
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
            var sideMenuViewInfo = new SideMenuViewInfo();
            sideMenuViewInfo.EndEvent = () => {

            };
            sideMenuViewInfo.CommandLists = _model.SideMenu();
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