using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePresenter : BasePresenter
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
    private bool _triggerAfterChecked = false;
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
    private Battle.CommandType _backCommandType = Battle.CommandType.None;
    public BattlePresenter(BattleView view)
    {
        _view = view;
        SetView(_view);
        _model = new BattleModel();
        SetModel(_model);

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
        _view.CreateObject();
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetActiveBack(false);

        if (_view.TestMode == true)
        {
            BattleInitialized();
            return;
        }
        _view.CommandStartTransition(() => {
            BattleInitialized();
        });
    }

    private void BattleInitialized()
    {
            _view.SetEvent((type) => updateCommand(type));

            _view.SetActors(_model.BattlerActors());
            _view.SetEnemies(_model.BattlerEnemies());

            _view.SetAttributeTypes(_model.AttributeTypes(),_model.CurrentAttributeType);

            _view.StartBattleStartAnim(DataSystem.System.GetTextData(4).Text);
            _nextCommandType = Battle.CommandType.EventCheck;
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
            CommandSkillAction((SkillInfo)viewEvent.templete);
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
        if (viewEvent.commandType == Battle.CommandType.Escape)
        {
            CommandEscape();
        }
        if (viewEvent.commandType == Battle.CommandType.Rule)
        {
            CommandRule();
        }
        if (viewEvent.commandType == Battle.CommandType.Option)
        {
            CommandOption();
        }
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.EndBattle();
            _model.EscapeBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);
            _view.SetAnimationEndTiming(180);
            _nextCommandType = Battle.CommandType.EndBattle;
        }
    }

    private void CommandBack()
    {
        var eventData = new BattleViewEvent(_backCommandType);
        updateCommand(eventData);
    }

    private void CommandEscape()
    {
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(410).Text,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(popupInfo);
    }

    private void CommandRule()
    {
        _view.SetBattleBusy(true);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.CommandCallRuling(() => {
            _view.SetBattleBusy(false);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
    }

    private void CommandOption()
    {
        _busy = true;
        _view.CommandCallOption(() => {
            _busy = false;
        });
    }
    
    private void CommandUpdateAp()
    {
        var chainActionResults = _model.UpdateChainState();
        _model.PopupActionResultInfo(chainActionResults);
        for (int i = 0; i < chainActionResults.Count; i++)
        {    
            PopupActionResult(chainActionResults[i],chainActionResults[i].TargetIndex,false);
        }
        for (int i = 0; i < chainActionResults.Count; i++)
        {    
            _model.ExecActionResultInfo(chainActionResults[i]);
        }
        StartAliveAnimation(chainActionResults);
        StartDeathAnimation(chainActionResults);
        var benedictionActionResults = _model.UpdateBenedictionState();
        _model.PopupActionResultInfo(benedictionActionResults);
        for (int i = 0; i < benedictionActionResults.Count; i++)
        {
            PopupActionResult(benedictionActionResults[i],benedictionActionResults[i].TargetIndex);
        }
        for (int i = 0; i < benedictionActionResults.Count; i++)
        {
            _model.ExecActionResultInfo(benedictionActionResults[i]);
        }
        StartDeathAnimation(benedictionActionResults);
        StartAliveAnimation(benedictionActionResults);
        _view.RefreshStatus();
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
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillId,false,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
                return;
            }
            List<int> chainTargetIndexs = _model.CheckChainBattler();
            if (chainTargetIndexs.Count > 0)
            {
                // 拘束解除
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,31,false,false);
                CommandSelectIndex(chainTargetIndexs);
                // 成功して入ればカウント
                if (actionInfo.ActionResults.Find(a => !a.Missed) != null)
                {
                    _model.CurrentBattler.GainChainCount(1);
                }
                return;
            } 
            if (_model.CurrentBattler.isActor)
            {
                CommandDecideActor();
            } else
            {
                int autoSkillId = _model.MakeAutoSkillId(_model.CurrentBattler);
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,autoSkillId,false,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
            }
        }
    }

    private void CommandSkillAction(SkillInfo skillInfo)
    {
        _model.ClearActionInfo();
        _model.SetLastSkill(skillInfo.Id);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo.Id,false,false);
        _view.HideSkillActionList();
        _view.SetEscapeButton(false);
        _view.HideSkillAtribute();
        _view.HideConditionAll();
        _view.HideBattleThumb();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            _view.ShowEnemyTarget();
            _view.RefreshBattlerEnemyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
            _view.ShowPartyTarget();
            _view.RefreshBattlerPartyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
        } else
        {
            _view.RefreshBattlerEnemyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType,skillInfo.Attribute);
            _view.RefreshBattlerPartyLayerTarget(actionInfo.LastTargetIndex,actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.ShowPartyTarget();
            _view.ShowEnemyTarget();
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
            
            var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.Use);
            _model.PopupActionResultInfo(PassiveResults);
            foreach (var PassiveResult in PassiveResults)
            {
                PopupActionResult(PassiveResult,PassiveResult.TargetIndex,true);
            }
            for (int i = 0; i < PassiveResults.Count; i++)
            {    
                _model.ExecActionResultInfo(PassiveResults[i]);
            }
        }
    }

    private void StartAnimationDemigod()
    {
        var demigod = _model.SkillActionAnimation("NA_Effekseer/NA_cut-in_002_" + _model.CurrentBattler.CharaId.ToString());
        _view.StartAnimationDemigod(demigod);
        _view.SetAnimationEndTiming(90);
        _nextCommandType = Battle.CommandType.EndDemigodAnimation;
    }

    private void StartAnimationRegene()
    {
        var regeneActionResults = _model.UpdateRegeneState();
        var animation = _model.SkillActionAnimation("tktk01/Cure1");
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

    private void StartAnimationSlipDamage()
    {
        var slipDamageActionResults = _model.UpdateSlipDamageState();
        var animation = _model.SkillActionAnimation("NA_Effekseer/NA_Fire_001");
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
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(slipDamageActionResults[i].TargetIndex);
            }
        }
        _nextCommandType = Battle.CommandType.EndSlipDamageAnimation;
    }

    private void StartAnimationSkill()
    {
        _view.SetRuleButton(false);
        _view.SetBattlerSelectable(true);
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.ActionResults.Count == 0)
        {
            _nextCommandType = Battle.CommandType.SkillAction;
            CommandEndAnimation();
            return;
        }
        var animation = _model.SkillActionAnimation(actionInfo.Master.AnimationName);
        _view.SetCurrentSkillData(actionInfo.Master);
        if (actionInfo.Master.AnimationType == AnimationType.All)
        {
            _view.StartAnimationAll(animation);
        }
        for (int i = 0; i < actionInfo.ActionResults.Count; i++)
        {
            if (actionInfo.Master.AnimationType != AnimationType.All)
            {
                _view.StartAnimation(actionInfo.ActionResults[i].TargetIndex,animation,actionInfo.Master.AnimationPosition);
            }
            _view.StartSkillDamage(actionInfo.ActionResults[i].TargetIndex,actionInfo.Master.DamageTiming,(targetIndex) => StartSkillDamage(targetIndex));
        }
        StartAliveAnimation(_model.CurrentActionInfo().ActionResults);
        _nextCommandType = Battle.CommandType.EndAnimation;
    }
    

    private void StartSkillDamage(int targetIndex)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            List<ActionResultInfo> actionResultInfos = actionInfo.ActionResults;
            _model.PopupActionResultInfo(actionResultInfos);
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
                    Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Damage);
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
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Damage);
            _view.StartDamage(actionResultInfo.SubjectIndex,DamageType.HpDamage,actionResultInfo.ReDamage);
            _view.StartBlink(actionResultInfo.SubjectIndex);
        }
        if (actionResultInfo.ReHeal > 0)
        {
            _view.StartHeal(actionResultInfo.SubjectIndex,DamageType.HpHeal,actionResultInfo.ReHeal);
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
        List<int> deathBattlerIndex = _model.DeathBattlerIndex(actionResultInfos);
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
        List<int> aliveBattlerIndex = _model.AliveBattlerIndex(actionResultInfos);
        if (aliveBattlerIndex.Count > 0)
        {
            for (int i = 0; i < aliveBattlerIndex.Count; i++)
            {
                //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Defeat);
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
        if (_nextCommandType == Battle.CommandType.EventCheck)
        {
            var isAbort = CheckAdvStageEvent(EventTiming.StartBattle,() => {
                _view.HideEnemyStatus();
                _view.SetBattleBusy(false);
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
            EndTurn();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndSlipDamageAnimation)
        {
            CommandEndSlipDamageAnimation();
            return;
        }
        // ダメージなどを適用
        _model.ExecActionResult();
        
        _view.ClearCurrentSkillData();
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            StartDeathAnimation(_model.CurrentActionInfo().ActionResults);
            if (_triggerAfterChecked == false && CheckBattleEnd() == false)
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
        EndTurn();
    }

    private void CommandStartBattleAction()
    {
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.StartBattle);
        _model.PopupActionResultInfo(PassiveResults);
        foreach (var PassiveResult in PassiveResults)
        {
            _model.ExecActionResultInfo(PassiveResult);
            PopupActionResult(PassiveResult,PassiveResult.TargetIndex,true);
        }
    }

    private void CommandEndSlipDamageAnimation()
    {
        var regene = _model.CheckRegene();
        if (regene && _triggerAfterChecked == false)
        {
            StartAnimationRegene();
            return;
        }
        EndTurn();
    }

    private void EndTurn()
    {
        // ターン終了
        _view.RefreshStatus();
        // PlusSkill
        _model.CheckPlusSkill();
        // Passive
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
        _model.PopupActionResultInfo(PassiveResults);
        foreach (var PassiveResult in PassiveResults)
        {
            PopupActionResult(PassiveResult,PassiveResult.TargetIndex,true);
        }
        for (int i = 0; i < PassiveResults.Count; i++)
        {    
            _model.ExecActionResultInfo(PassiveResults[i]);
        }
        var RemovePassiveResults = _model.CheckRemovePassiveInfos();
        _model.PopupActionResultInfo(RemovePassiveResults);
        foreach (var RemovePassiveResult in RemovePassiveResults)
        {
            PopupActionResult(RemovePassiveResult,RemovePassiveResult.TargetIndex,true);
        }
        foreach (var RemovePassiveResult in RemovePassiveResults)
        {
            _model.ExecActionResultInfo(RemovePassiveResult);
        }
        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After);
        
        bool isDemigodActor = false;
        if (_model.CurrentBattler != null)
        {
            isDemigodActor = _model.CurrentBattler.IsState(StateType.Demigod);
        }
        bool isTriggeredSkill = _model.CurrentActionInfo().TriggeredSkill;
        if (result == false && _triggerAfterChecked == false && isTriggeredSkill == false)
        {
            _model.UpdateTurn();
        }
        _model.TurnEnd();
        if (isTriggeredSkill == false)
        {
            _triggerAfterChecked = true;
        }

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
        ActionInfo CurrentActionInfo = _model.CurrentActionInfo();
        if (CurrentActionInfo != null)
        {
            _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(CurrentActionInfo));
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
        _view.HideEnemyStatus();
        _triggerAfterChecked = false;
        _view.SetBattleBusy(false);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
        _view.SetAttributeTypes(_model.AttributeTypes(),_model.CurrentAttributeType);
        _view.HideCondition();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    private void CommandDecideActor()
    {
        // 行動選択開始
        _view.SetHelpText(DataSystem.System.GetTextData(15010).Text);
        _view.ShowSkillActionList(_model.CurrentBattler,_model.CurrentBattlerActorData());
        _view.SetEscapeButton(_model.EnableEspape());
        _view.SetRuleButton(true);
        _view.ShowConditionTab();
        CommandAttributeType(_model.CurrentAttributeType);
        _view.RefreshBattlerEnemyLayerTarget(-1);
        _view.RefreshBattlerPartyLayerTarget(-1);
        _view.SetActiveBack(false);
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
        _view.SetEscapeButton(false);
        _view.ActivateConditionList();
        _view.SetCondition(_model.CurrentBattler.StateInfos);
        _view.ShowConditionAll();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
            _view.RefreshBattlerEnemyLayerTarget(actionInfo.TargetIndexList.Find(a => a >= 100),actionInfo.TargetIndexList,actionInfo.ScopeType,actionInfo.Master.Attribute);
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