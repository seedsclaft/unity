using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        // ActionInfoのアニメーション開始～スリップダメージ～リジェネ回復～Animation終了までを管理
        // この間はActionInfoは変わらない
        
        /// <summary>
        /// ActionInfoのアニメーション開始
        /// </summary>
        /// <param name="actionInfo"></param>
        public void StartActionInfoAnimation(ActionInfo actionInfo)
        {
            if (_skipBattle || actionInfo.Master.IsDisplayStartBattle())
            {
                CommandEndAnimation();
                return;
            }
            StartAnimation(actionInfo);
        }

        private void StartAnimation(ActionInfo actionInfo)
        {
            var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActorView;
            if (actionInfo.Master.SkillType == SkillType.Unique && actionInfo.Master.AnimationId > 0)
            {
                if (isActor)
                {
                    StartAnimationMessiah();
                } else
                {
                    StartAnimationMessiahEnemy();
                }
            } else
            if (actionInfo.Master.SkillType == SkillType.Awaken && actionInfo.Master.AnimationId > 0)
            {
                StartAnimationAwaken();
            } else
            {
                StartAnimationSkill();
            }
        }
        
        /// <summary>
        /// 覚醒アニメーション再生してからアニメーション再生
        /// </summary>
        private async void StartAnimationMessiah()
        {
            var actionInfo = _model.CurrentActionInfo;
            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex);
            var actorId = subject.ActorInfo != null ? subject.ActorInfo.ActorId : subject.EnemyData.Id - 1000;
            var sprite = _model.AwakenSprite(actorId);
            await _view.StartAnimationMessiah(subject,sprite);
            StartAnimationSkill();
        }

        /// <summary>
        /// 覚醒アニメーション再生してからアニメーション再生
        /// </summary>
        private async void StartAnimationMessiahEnemy()
        {
            var actionInfo = _model.CurrentActionInfo;
            var subject = _model.GetBattlerInfo(actionInfo.SubjectIndex);
            var sprite = _model.AwakenEnemySprite(subject.EnemyData.Id);
            await _view.StartAnimationMessiah(subject,sprite);
            StartAnimationSkill();
        }
        
        /// <summary>
        /// カットインアニメーション再生してからアニメーション再生
        /// </summary>
        private async void StartAnimationAwaken()
        {
            var actionInfo = _model.CurrentActionInfo;
            await _view.StartAnimationDemigod(_model.GetBattlerInfo(actionInfo.SubjectIndex),actionInfo.Master);
            StartAnimationSkill();
        }

        private async void StartAnimationSkill()
        {           
            var actionInfo = _model.CurrentActionInfo;
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
            var speed = GameSystem.ConfigData.BattleSpeed;
            await UniTask.DelayFrame((int)(24/speed));
            if (actionInfo.TriggeredSkill && actionInfo.Master.SkillType != SkillType.Unique && actionInfo.Master.SkillType != SkillType.Awaken)
            {
                if (actionInfo.Master.IsDisplayBattleSkill() && _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                {
                    _view.ShowCutinBattleThumb(_model.GetBattlerInfo(actionInfo.SubjectIndex));
                    speed = GameSystem.ConfigData.BattleSpeed;
                    await UniTask.DelayFrame((int)(16/speed));
                }
            }
            if (actionInfo.Master.IsDisplayBattleSkill() || _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor == false)
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo,_model.GetBattlerInfo(actionInfo.SubjectIndex));
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
                if (!actionInfo.LastAttack() && waitFrame > 1)
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
                if (!actionInfo.LastAttack() && waitFrame > 1)
                {
                    waitFrame = 8;
                }
                await UniTask.DelayFrame(waitFrame);
            }
            CommandEndAnimation();
        }
        

        private async void RepeatAnimationSkill(ActionInfo actionInfo)
        {           
            if (actionInfo.ActionResults.Count == 0 || !_model.CurrentActionBattler.IsAlive())
            {
                CommandEndAnimation();
                return;
            }
            if (actionInfo.FirstAttack())
            {
                StartAnimation(actionInfo);
            }
            
            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo,_model.GetBattlerInfo(actionInfo.SubjectIndex));
            }

            StartAliveAnimation(actionInfo.ActionResults);
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                PopupActionResult(actionResultInfo,actionResultInfo.TargetIndex,true,true);
            }
            await UniTask.DelayFrame(_model.WaitFrameTime(8));
            CommandEndAnimation();
        }

        private async void StartAnimationSlipDamage(List<ActionResultInfo> slipDamageResults)
        {
            var actionInfo = _model.CurrentActionInfo;
            await ExecActionResultInfos(slipDamageResults);
            if (_skipBattle == false)
            {
                _view.StartAnimationSlipDamage(ActionResultInfo.ConvertIndexes(slipDamageResults));
            }
            StartDeathAnimation(slipDamageResults);
            //_model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),null,slipDamageResults);

            // regenerate
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            if (_triggerAfterChecked == false && _regenerateChecked == false && isTriggeredSkill == false)
            {
                if (_model.FirstActionBattler != null && actionInfo.SubjectIndex == _model.FirstActionBattler.Index)
                {
                    _regenerateChecked = true;
                    if (_model.FirstActionBattler.IsAlive())
                    {
                        var regenerateResult = _model.CheckRegenerate(actionInfo);
                        if (regenerateResult.Count > 0)
                        {
                            StartAnimationRegenerate(regenerateResult);
                            return;
                        }
                    }
                }
            }
            EndTurn();
        }

        private async void StartAnimationRegenerate(List<ActionResultInfo> regenerateActionResults)
        {
            await ExecActionResultInfos(regenerateActionResults);
            if (_skipBattle == false)
            {
                _view.StartAnimationRegenerate(ActionResultInfo.ConvertIndexes(regenerateActionResults));
            }
            EndTurn();
        }
    }
}
