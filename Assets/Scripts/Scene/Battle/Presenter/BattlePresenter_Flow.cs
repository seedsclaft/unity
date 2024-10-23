using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattlePresenter : BasePresenter
    {
        /// <summary>
        /// Apが0以下の行動者を決める
        /// </summary>
        /// <returns></returns>
        public BattlerInfo CheckApCurrentBattler()
        {
            return _model.CheckApCurrentBattler();
        }

        /// <summary>
        /// Apを更新する
        /// </summary>
        /// <returns></returns>
        public async void UpdateApBattlerInfos()
        {
            while (_model.CurrentBattler == null)
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
            }
        }

        private void CommandStartSelect()
        {
            // Ap更新で行動するキャラがいる
            var currentBattler = _model.CurrentBattler;
            if (currentBattler != null)
            {
                _model.UpdateApModify(currentBattler);
                _view.UpdateGridLayer();
                _view.SetBattleBusy(true);
                CheckFirstActionBattler();
                // 行動不可の場合は行動しない
                if (!_model.EnableCurrentBattler())
                {
                    MakeActionInfoTargetIndexes(_model.CurrentBattler,0);
                    return;
                }
#if UNITY_EDITOR
                if (_testBattle && _model.TestSkillId() != 0)
                {    
                    int testSkillId = _model.TestSkillId();
                    MakeActionInfoTargetIndexes(_model.CurrentBattler,testSkillId);
                    _model.SeekActionIndex();
                    return;
                }
#endif
                MakeActionInfoSkillTrigger();
            }
        }

        public async void CheckFirstActionBattler()
        {
            if (_model.FirstActionBattler == null)
            {
                var currentBattler = _model.CurrentBattler;
                _model.SetFirstActionBattler(currentBattler);
                // 解除判定は行動開始の最初のみ
                var removed =_model.UpdateNextSelfTurn();
                foreach (var removedState in removed)
                {
                    _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
                }
                // Passive解除
                await RemovePassiveInfos();
                // 行動開始前トリガー
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeTriggerTimings(),null,null);
            }
        }

        /// <summary>
        /// 作戦に基づいてActionInfoを決定する
        /// </summary>
        private void MakeActionInfoSkillTrigger()
        {
            var currentBattler = _model.CurrentBattler;
            if (currentBattler != null)
            {
                int autoSkillId;
                int targetIndex;
                (autoSkillId,targetIndex) = _model.MakeAutoSkillTriggerSkillId(currentBattler);
                if (autoSkillId == -1)
                {
                    // 何もしない
                    autoSkillId = 20010;
                }
                //LogOutput.Log(autoSkillId);
                MakeActionInfoTargetIndexes(currentBattler,autoSkillId,targetIndex);
            }
        }

        private void MakeActionInfoTargetIndexes(BattlerInfo battlerInfo,int skillId,int oneTargetIndex = -1)
        {
            // 対象を自動決定
            var targetIndexes = _model.MakeActionInfoTargetIndexes(battlerInfo,skillId,oneTargetIndex);
            MakeActionResultInfoTargetIndexes(targetIndexes);
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        public void MakeActionResultInfoTargetIndexes(List<int> indexList)
        {
            _view.SetHelpText("");
            _view.ChangeBackCommandActive(false);

            var actionInfo = _model.CurrentActionInfo;
            _model.SetActionInfoParameter(actionInfo);
            MakeActionResultInfo(indexList);
            StartActionInfo();
        }

        /// <summary>
        /// 行動結果を生成する
        /// </summary>
        /// <param name="indexList"></param>
        private void MakeActionResultInfo(List<int> indexList)
        {
            var actionInfo = _model.CurrentActionInfo;
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブ
                CheckBeforeActionInfo(actionInfo);

                // 開始行動のアクションの結果を生成
                _model.MakeActionResultInfo(actionInfo,indexList);

                // 行動決定後の割り込みスキル判定
                CheckInterruptActionInfoTriggerTimings();
            }
        }

        /// <summary>
        /// 行動前パッシブを確認
        /// </summary>
        /// <param name="indexList"></param>
        private void CheckBeforeActionInfo(ActionInfo actionInfo)
        {
            if (actionInfo != null)
            {
                _view.BattlerBattleClearSelect();

                // 自分,味方,相手の行動前パッシブを確認
                _model.CheckTriggerPassiveInfos(BattleUtility.BeforeActionInfoTriggerTimings(),actionInfo,actionInfo.ActionResults);
            }
        }
        
        /// <summary>
        /// 行動割り込みトリガー確認
        /// </summary>
        private void CheckInterruptActionInfoTriggerTimings()
        {
            var actionInfo = _model.CurrentActionInfo;
            _model.CheckTriggerActiveInfos(TriggerTiming.Interrupt,actionInfo,actionInfo.ActionResults,true);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Interrupt},actionInfo,actionInfo.ActionResults);
            _model.CheckTriggerPassiveInfos(new List<TriggerTiming>(){TriggerTiming.Use},actionInfo,actionInfo.ActionResults);
        }

        private void StartActionInfo()
        {
            // 行動変化対応のため再取得
            var actionInfo = _model.CurrentActionInfo;
            //LogOutput.Log(actionInfo.Master.Id + "行動");
            if (actionInfo != null)
            {
                // 待機か戦闘不能なら何もしない
                if (actionInfo.IsWait() || !_model.CurrentActionBattler.IsAlive())
                {
                    StartWaitCommand(actionInfo);
                } else
                {
                    StartActionInfoAnimation(actionInfo);
                }
            }
        }

        /// <summary>
        /// ActionInfoのアニメーションが終了した後処理
        /// </summary>
        /// <param name="actionInfo"></param>
        private void CommandEndAnimation()
        {
            var actionInfo = _model.CurrentActionInfo;
            if (actionInfo != null)
            {
                // ダメージなどを適用
                _model.ExecCurrentAction(actionInfo,true);

                // Hp変化での行動・パッシブを確認
                _model.CheckTriggerActiveInfos(TriggerTiming.HpDamaged,actionInfo,actionInfo.ActionResults,true);
                _model.CheckTriggerPassiveInfos(BattleUtility.HpDamagedTriggerTimings(),actionInfo,actionInfo.ActionResults);
            
                StartDeathAnimation(actionInfo.ActionResults);
                StartAliveAnimation(actionInfo.ActionResults);
                // 繰り返しがある場合
                if (actionInfo.RepeatTime > 0)
                {
                    RepeatActionInfo(actionInfo);
                    return;
                }
            }
            _view.ClearCurrentSkillData();

            // スリップダメージ
            var slipDamageActionResult = SlipDamageActionResult(actionInfo);
            if (slipDamageActionResult != null && slipDamageActionResult.Count > 0)
            {
                StartAnimationSlipDamage(slipDamageActionResult);
                return;
            }

            // リジェネ回復
            var regenerationActionResult = RegenerationActionResult(actionInfo);
            if (regenerationActionResult != null && regenerationActionResult.Count > 0)
            {
                StartAnimationRegenerate(regenerationActionResult);
                return;
            }

            EndTurn();
        }

        /// <summary>
        /// 連続行動するActionInfo
        /// </summary>
        /// <param name="actionInfo"></param>
        private void RepeatActionInfo(ActionInfo actionInfo)
        {
            _model.ResetTargetIndexList(actionInfo);
            MakeActionResultInfo(actionInfo.CandidateTargetIndexList);
            // 再取得
            if (actionInfo == _model.CurrentActionInfo)
            {
                actionInfo = _model.CurrentActionInfo;
                //LogOutput.Log(actionInfo.Master.Id + "再行動");
                RepeatAnimationSkill(actionInfo);
            } else
            {
                // 割り込みでアクションが変わった場合
                StartActionInfo();
            }
        }

        private List<ActionResultInfo> SlipDamageActionResult(ActionInfo actionInfo)
        {
            // スリップダメージ
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            if (_triggerAfterChecked == false && _slipDamageChecked == false && isTriggeredSkill == false)
            {
                if (_model.FirstActionBattler != null && actionInfo.SubjectIndex == _model.FirstActionBattler.Index)
                {
                    _slipDamageChecked = true;
                    var slipResult = _model.CheckSlipDamage();
                    if (slipResult.Count > 0)
                    {
                        return slipResult;
                    }
                }
            }
            return null;
        }

        private List<ActionResultInfo> RegenerationActionResult(ActionInfo actionInfo)
        {
            bool isTriggeredSkill = actionInfo.TriggeredSkill;
            // regenerate
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
                            return regenerateResult;
                        }
                    }
                }
            }
            return null;
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
            var result = _model.CheckTriggerActiveInfos(TriggerTiming.After,actionInfo,actionInfo.ActionResults,true);
            var result2 = _model.CheckTriggerActiveInfos(TriggerTiming.AfterAndStartBattle,actionInfo,actionInfo.ActionResults,true);
            result.AddRange(result2);

            var checkNoResetAp = _model.CheckNoResetAp(actionInfo);
            if (checkNoResetAp == false && result.Count == 0 && _triggerAfterChecked == false && isTriggeredSkill == false)
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
                        _view.StartHeal(_model.FirstActionBattler.Index,DamageType.MpHeal,gainAp); 
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
            _view.UpdateGridLayer();
            _view.RefreshStatus();

            // 次の行動があれば続ける
            var currentActionInfo = _model.CurrentActionInfo;
            if (currentActionInfo != null)
            {
                _battleEnded = false;
                var targetIndexes = _model.MakeAutoSelectIndex(currentActionInfo);
                MakeActionResultInfoTargetIndexes(targetIndexes);
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
            _model.SetFirstActionBattler(null);
            _view.SetBattleBusy(false);
        }
    }
}
