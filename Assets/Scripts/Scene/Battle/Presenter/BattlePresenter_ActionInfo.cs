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
            var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor;
            if (actionInfo.Master.SkillType == SkillType.Messiah && isActor)
            {
                StartAnimationDemigod();
            } else
            if (actionInfo.Master.SkillType == SkillType.Awaken && isActor)
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
        private async void StartAnimationDemigod()
        {
            var actionInfo = _model.CurrentActionInfo;
            var sprite = _model.AwakenSprite(_model.GetBattlerInfo(actionInfo.SubjectIndex).ActorInfo.ActorId);
            await _view.StartAnimationAwaken(sprite);
            StartAnimationSkill();
        }

        
        /// <summary>
        /// カットインアニメーション再生してからアニメーション再生
        /// </summary>
        private async void StartAnimationAwaken()
        {
            var actionInfo = _model.CurrentActionInfo;
            //var effect = _model.AwakenEffect(_model.GetBattlerInfo(actionInfo.SubjectIndex).ActorInfo.ActorId);
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
            await UniTask.DelayFrame((int)(16/speed));
            if (actionInfo.TriggeredSkill && actionInfo.Master.SkillType != SkillType.Messiah && actionInfo.Master.SkillType != SkillType.Awaken)
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
        
        /// <summary>
        /// ActionInfoのアニメーション終了処理
        /// </summary>
        /// <param name="actionInfo"></param>
        private void CommandEndAnimation()
        {
            var actionInfo = _model.CurrentActionInfo;
            if (actionInfo != null)
            {
                // ダメージなどを適用
                _model.ExecCurrentAction(actionInfo,true);
                _model.CheckTriggerActiveInfos(TriggerTiming.HpDamaged,actionInfo,actionInfo.ActionResults,true);
                _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
                StartDeathAnimation(actionInfo.ActionResults);
                StartAliveAnimation(actionInfo.ActionResults);
                // 繰り返しがある場合
                if (actionInfo.RepeatTime > 0)
                {
                    _model.ResetTargetIndexList(actionInfo);
                    MakeActionResultInfo(actionInfo.CandidateTargetIndexList);
                    // 再取得
                    actionInfo = _model.CurrentActionInfo;
                    RepeatAnimationSkill(actionInfo);
                    return;
                }
            }
            _view.ClearCurrentSkillData();
            // スリップダメージ
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            if (_triggerAfterChecked == false && _slipDamageChecked == false && isTriggeredSkill == false)
            {
                if (_model.CurrentTurnBattler != null && actionInfo.SubjectIndex == _model.CurrentTurnBattler.Index)
                {
                    _slipDamageChecked = true;
                    var slipResult = _model.CheckSlipDamage();
                    if (slipResult.Count > 0)
                    {
                        StartAnimationSlipDamage(slipResult);
                        return;
                    }
                }
            }
            
            // regenerate
            if (_triggerAfterChecked == false && _regenerateChecked == false && isTriggeredSkill == false)
            {
                if (_model.CurrentTurnBattler != null && actionInfo.SubjectIndex == _model.CurrentTurnBattler.Index)
                {
                    _regenerateChecked = true;
                    if (_model.CurrentTurnBattler.IsAlive())
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

        private async void RepeatAnimationSkill(ActionInfo actionInfo)
        {           
            if (actionInfo.ActionResults.Count == 0)
            {
                CommandEndAnimation();
                return;
            }
            var isActor = _model.GetBattlerInfo(actionInfo.SubjectIndex).IsActor;
            if (actionInfo.FirstAttack() && (actionInfo.Master.SkillType == SkillType.Messiah && isActor || actionInfo.Master.SkillType == SkillType.Awaken) && isActor)
            {
                StartAnimationDemigod();
                return;
            }
            
            if (actionInfo.Master.IsDisplayBattleSkill())
            {
                _view.SetCurrentSkillData(actionInfo.SkillInfo);
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
                if (_model.CurrentTurnBattler != null && actionInfo.SubjectIndex == _model.CurrentTurnBattler.Index)
                {
                    _regenerateChecked = true;
                    if (_model.CurrentTurnBattler.IsAlive())
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
