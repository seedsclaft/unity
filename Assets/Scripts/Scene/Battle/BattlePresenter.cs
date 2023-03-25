using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePresenter : BasePresenter
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
    private Battle.CommandType _backCommandType = Battle.CommandType.None;
    public BattlePresenter(BattleView view)
    {
        _view = view;
        _model = new BattleModel();

        Initialize();
    }

    private async void Initialize()
    {
        _model.CreateBattleData();
        _view.CreateObject();
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetActiveBack(false);
        _view.SetEvent((type) => updateCommand(type));

        //List<ActorInfo> actorInfos = _model.Actors();
        //_view.SetActorInfo(_model.CurrentActor);
        _view.SetActors(_model.BattlerActors());
        _view.SetEnemies(_model.BattlerEnemies());

        _view.SetAttributeTypes(_model.AttributeTypes());
        var bgm = await _model.GetBgmData("BATTLE4");
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _view.StartBattleStartAnim(DataSystem.System.GetTextData(4).Text);
        _busy = false;
    }

    private void updateCommand(BattleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Battle.CommandType.UpdateAp)
        {
            CommandUpdateAp();
        }
        if (viewEvent.commandType == Battle.CommandType.SkillAction)
        {
            CommandSkillAction((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.EnemyLayer)
        {
            CommandSelectIndex((List<int>)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.ActorList)
        {
            CommandSelectIndex((List<int>)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == Battle.CommandType.AttributeType)
        {
            CommandAttributeType((AttributeType)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.DecideActor)
        {
            CommandDecideActor();
        }
        if (viewEvent.commandType == Battle.CommandType.LeftActor)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == Battle.CommandType.RightActor)
        {
            CommandRightActor();
        }
        if (viewEvent.commandType == Battle.CommandType.Condition)
        {
            CommandCondition();
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
    }

    private void CommandBack()
    {
        var eventData = new BattleViewEvent(_backCommandType);
        updateCommand(eventData);
    }

    private void CommandUpdateAp()
    {
        var chainActionResults = _model.UpdateChainState();
        for (int i = 0; i < chainActionResults.Count; i++)
        {    
            PopupActionResult(chainActionResults[i],chainActionResults[i].TargetIndex,false);
        }
        for (int i = 0; i < chainActionResults.Count; i++)
        {    
            _model.ExecActionResultInfo(chainActionResults[i]);
        }
        StartDeathAnimation(chainActionResults);
        var benedictionActionResults = _model.UpdateBenedictionState();
        for (int i = 0; i < benedictionActionResults.Count; i++)
        {
            PopupActionResult(benedictionActionResults[i],benedictionActionResults[i].TargetIndex);
        }
        for (int i = 0; i < benedictionActionResults.Count; i++)
        {
            _model.ExecActionResultInfo(benedictionActionResults[i]);
        }
        StartDeathAnimation(benedictionActionResults);
        if (CheckBattleEnd())
        {
            return;
        }
        _model.UpdateAp();
        _view.UpdateAp();
        if (_model.CurrentBattler != null)
        {
            _view.SetBattleBusy(true);
            if (!_model.EnableCurrentBattler())
            {
                int skillId = 0;
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillId,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
                return;
            }
            List<int> chainTargetIndexs = _model.CheckChainBattler();
            if (chainTargetIndexs.Count > 0)
            {
                // 拘束解除
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,31,false);
                CommandSelectIndex(chainTargetIndexs);
                return;
            } 
            if (_model.CurrentBattler.isActor)
            {
                CommandDecideActor();
            } else
            {
                int autoSkillId = _model.MakeAutoSkillId(_model.CurrentBattler);
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,autoSkillId,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
            }
        }
    }

    private void CommandSkillAction(int skillId)
    {
        _model.ClearActionInfo();
        _model.SetLastSkill(skillId);
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillId,false);
        _view.HideSkillActionList();
        _view.HideSkillAtribute();
        _view.HideBattleThumb();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            _view.ShowEnemyTarget();
            _view.RefreshBattlerEnemyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
            _view.ShowPartyTarget();
            _view.RefreshBattlerPartyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
        } else
        {
            _view.RefreshBattlerEnemyLayerTarget(-1);
            _view.RefreshBattlerPartyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.ShowPartyTarget();
            _view.DeactivateEnemyList();
        }
        _backCommandType = Battle.CommandType.DecideActor;
        _view.SetActiveBack(true);
        //
    }

    private void CommandSelectIndex(List<int> indexList)
    {
        _view.SetHelpText("");
        _view.SetActiveBack(false);
        MakeActionResultInfo(indexList);
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            if (actionInfo.Master.SkillType == SkillType.Demigod)
            {
                StartAnimationDemigod();
                return;
            }
            StartAnimationSkill();
        }
    }

    private void MakeActionResultInfo(List<int> indexList)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _view.RefreshBattlerEnemyLayerTarget(-1);
            _view.RefreshBattlerPartyLayerTarget(-1);
            _model.MakeActionResultInfo(actionInfo,indexList);
            var result = _model.CheckTriggerSkillInfos(TriggerTiming.Interrupt);
            if (result)
            {
                _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
                _model.MakeActionResultInfo(_model.CurrentActionInfo(),_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            }
        }
    }

    private async void StartAnimationDemigod()
    {
        var demigod = await _model.SkillActionAnimation("NA_Effekseer/NA_cut-in_002_" + _model.CurrentBattler.ActorInfo.ActorId.ToString());
        _view.StartAnimationDemigod(demigod);
        _view.SetAnimationEndTiming(90);
        _nextCommandType = Battle.CommandType.EndDemigodAnimation;
    }

    private async void StartAnimationRegene()
    {
        var regeneActionResults = _model.UpdateRegeneState();
        var animation = await _model.SkillActionAnimation("tktk01/Cure1");
        for (int i = 0; i < regeneActionResults.Count; i++)
        {
            _view.StartAnimation(regeneActionResults[i].TargetIndex,animation,0);
            _view.StartHeal(regeneActionResults[i].TargetIndex,DamageType.HpHeal,regeneActionResults[i].HpHeal);
            if (regeneActionResults[i].HpHeal != 0)
            {
                _model.gainHpTargetIndex(regeneActionResults[i].TargetIndex,regeneActionResults[i].HpHeal);
            }
        }
        _nextCommandType = Battle.CommandType.EndRegeneAnimation;
    }

    private async void StartAnimationSlipDamage()
    {
        var slipDamageActionResults = _model.UpdateSlipDamageState();
        var animation = await _model.SkillActionAnimation("NA_Effekseer/NA_Fire_001");
        for (int i = 0; i < slipDamageActionResults.Count; i++)
        {
            _view.StartDamage(slipDamageActionResults[i].TargetIndex,DamageType.HpDamage,slipDamageActionResults[i].HpDamage);
            _view.StartAnimation(slipDamageActionResults[i].TargetIndex,animation,0);
            
            if (slipDamageActionResults[i].HpDamage != 0)
            {            
                _model.gainHpTargetIndex(slipDamageActionResults[i].TargetIndex,slipDamageActionResults[i].HpDamage * -1);
            }
            if (slipDamageActionResults[i].DeadIndexList.Contains(slipDamageActionResults[i].TargetIndex))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(slipDamageActionResults[i].TargetIndex);
            }
        }
        _nextCommandType = Battle.CommandType.EndSlipDamageAnimation;
    }

    private async void StartAnimationSkill()
    {
        //_view.ClearDamagePopup();
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.actionResults.Count == 0)
        {
            _nextCommandType = Battle.CommandType.SkillAction;
            CommandEndAnimation();
            return;
        }
        var animation = await _model.SkillActionAnimation(actionInfo.Master.AnimationName);
        if (actionInfo.Master.AnimationType == AnimationType.All)
        {
            _view.StartAnimationAll(animation);
        }
        for (int i = 0; i < actionInfo.actionResults.Count; i++)
        {
            if (actionInfo.Master.AnimationType != AnimationType.All)
            {
                _view.StartAnimation(actionInfo.actionResults[i].TargetIndex,animation,actionInfo.Master.AnimationPosition);
            }
            _view.StartSkillDamage(actionInfo.actionResults[i].TargetIndex,actionInfo.Master.DamageTiming,(targetIndex) => StartSkillDamage(targetIndex));
        }
        _nextCommandType = Battle.CommandType.EndAnimation;
    }
    

    private void StartSkillDamage(int targetIndex)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                PopupActionResult(actionResultInfos[i],targetIndex);
            }
        }
    }

    private void PopupActionResult(ActionResultInfo actionResultInfo,int targetIndex,bool needDamageBlink = true)
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
                _view.StartDamage(targetIndex,DamageType.HpDamage,actionResultInfo.HpDamage);
                if (needDamageBlink){
                    _view.StartBlink(targetIndex);
                    SoundManager.Instance.PlayStaticSe(SEType.Damage);
                }
            }
        }
        if (actionResultInfo.HpHeal > 0)
        {
            if (actionResultInfo.TargetIndex == targetIndex)
            {
                _view.StartHeal(targetIndex,DamageType.HpHeal,actionResultInfo.HpHeal);
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
                _view.StartStatePopup(targetIndex,DamageType.State,"+行動可能");
            }
        }
        if (actionResultInfo.ReDamage > 0)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Damage);
            _view.StartDamage(actionResultInfo.SubjectIndex,DamageType.HpDamage,actionResultInfo.ReDamage);
            _view.StartBlink(actionResultInfo.SubjectIndex);
        }
        if (actionResultInfo.ReHeal > 0)
        {
            _view.StartHeal(actionResultInfo.SubjectIndex,DamageType.HpHeal,actionResultInfo.ReHeal);
        }
        if (actionResultInfo.AddedStates.Count > 0)
        {
            for (int j = 0;j < actionResultInfo.AddedStates.Count;j++)
            {
                if (actionResultInfo.TargetIndex == targetIndex)
                {
                    _view.StartStatePopup(actionResultInfo.AddedStates[j].TargetIndex,DamageType.State,"+" + actionResultInfo.AddedStates[j].Master.Name);
                }
            }
        }
        if (actionResultInfo.RemovedStates.Count > 0)
        {
            for (int j = 0;j < actionResultInfo.RemovedStates.Count;j++)
            {
                if (actionResultInfo.TargetIndex == targetIndex){
                    _view.StartStatePopup(actionResultInfo.RemovedStates[j].TargetIndex,DamageType.State,"-" + actionResultInfo.RemovedStates[j].Master.Name);
                }
            }
        }
    }

    private void StartDeathAnimation(List<ActionResultInfo> actionResultInfos)
    {
        List<int> deathBattlerIndex = _model.DeathBattlerIndex(actionResultInfos);
        if (deathBattlerIndex.Count > 0)
        {
            for (int i = 0; i < deathBattlerIndex.Count; i++)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(deathBattlerIndex[i]);
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
            _view.CommandSceneChange(Scene.Strategy);
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndDemigodAnimation)
        {
            StartAnimationSkill();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndRegeneAnimation)
        {
            CommandEndRegeneAnimation();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndSlipDamageAnimation)
        {
            CommandEndSlipDamageAnimation();
            return;
        }
        // ダメージなどを適用
        _model.ExecActionResult();
        StartDeathAnimation(_model.CurrentActionInfo().actionResults);
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            if (actionInfo.Master.SkillType != SkillType.Demigod)
            {
                // ステートなどを適用
                var slipDamage = _model.CheckSlipDamage();
                if (slipDamage == true)
                {
                    StartAnimationSlipDamage();
                    return;
                }
                // リジェネ
                var regene = _model.CheckRegene();
                if (regene)
                {
                    StartAnimationRegene();
                    return;
                }
            }
        }
        // ターン終了
        _view.RefreshStatus();
        // PlusSkill
        _model.CheckPlusSkill();
        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After);
        _model.TurnEnd();

        // 勝敗判定
        if (CheckBattleEnd() && result == false)
        {
            return;
        }
        if (result == true)
        {        
            _view.RefreshStatus();
        }

        // 次の行動者がいれば続ける
        if (_model.CurrentActionInfo() != null)
        {
            _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            return;
        }

        _view.SetBattleBusy(false);
    }

    private void CommandEndSlipDamageAnimation()
    {
        var regene = _model.CheckRegene();
        if (regene)
        {
            StartAnimationRegene();
            return;
        }
        // ターン終了
        _view.RefreshStatus();
        // PlusSkill
        _model.CheckPlusSkill();
        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After);
        _model.TurnEnd();

        // 勝敗判定
        if (CheckBattleEnd() && result == false)
        {
            return;
        }
        if (result == true)
        {        
            _view.RefreshStatus();
        }

        // 次の行動者がいれば続ける
        if (_model.CurrentActionInfo() != null)
        {
            _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            return;
        }

        _view.SetBattleBusy(false);
    }

    private void CommandEndRegeneAnimation()
    {
        // ターン終了
        _view.RefreshStatus();
        // PlusSkill
        _model.CheckPlusSkill();
        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After);
        _model.TurnEnd();

        // 勝敗判定
        if (CheckBattleEnd() && result == false)
        {
            return;
        }
        if (result == true)
        {        
            _view.RefreshStatus();
        }

        // 次の行動者がいれば続ける
        if (_model.CurrentActionInfo() != null)
        {
            _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            return;
        }

        _view.SetBattleBusy(false);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    private void CommandDecideActor()
    {
        // 行動選択開始
        _view.SetHelpText(DataSystem.System.GetTextData(15010).Text);
        _view.ShowSkillActionList(_model.CurrentBattler);
        _view.ShowConditionTab();
        CommandAttributeType(_model.CurrentAttributeType);
        _view.RefreshBattlerEnemyLayerTarget(-1);
        _view.RefreshBattlerPartyLayerTarget(-1);
        _view.SetActiveBack(false);
    }
    
    private void CommandLeftActor()
    {
         _model.ChangeActorIndex(-1);
        _view.SetActorInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private void CommandRightActor()
    {
         _model.ChangeActorIndex(1);
        _view.SetActorInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private bool CheckBattleEnd()
    {
        bool isEnd = false;
        if (_model.CheckVictory())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15020).Text);
            _view.SetAnimationEndTiming(180);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
        } else
        if (_model.CheckDefeat())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);
            _view.SetAnimationEndTiming(180);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
        }
        return isEnd;
    }

    public void CommandCondition()
    {
        _view.HideSkillActionList();
        _view.ActivateConditionList();
        _view.SetCondition(_model.CurrentBattler.StateInfos);
        _view.ShowConditionAll();
        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    private void CommandSelectEnemy()
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            //_view.ShowEnemyTarget();
            //_view.RefreshBattlerEnemyLayerTarget(actionInfo);
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
            //_view.ShowPartyTarget();
            //_view.RefreshBattlerPartyLayerTarget(actionInfo);
        } else
        {
            _view.RefreshBattlerEnemyLayerTarget(actionInfo.TargetIndexList.Find(a => a >= 100),actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.RefreshBattlerPartyLayerTarget(-1);
            _view.ShowEnemyTarget();
            _view.DeactivateActorList();
        }
    }

    private void CommandSelectParty()
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            //_view.ShowEnemyTarget();
            //_view.RefreshBattlerEnemyLayerTarget(actionInfo);
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
            //_view.ShowPartyTarget();
            //_view.RefreshBattlerPartyLayerTarget(actionInfo);
        } else
        {
            _view.RefreshBattlerEnemyLayerTarget(-1);
            _view.RefreshBattlerPartyLayerTarget(actionInfo.TargetIndexList.Find(a => a < 100),actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.ShowPartyTarget();
            _view.DeactivateEnemyList();
        }
    }
}