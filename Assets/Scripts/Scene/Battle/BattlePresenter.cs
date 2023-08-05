using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePresenter : BasePresenter
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
#if UNITY_EDITOR
    private bool _debug = false;
    public void SetDebug(bool busy)
    {
        _debug = busy;
    }
#endif
    private bool _triggerAfterChecked = false;
    private bool _slipDamageChecked = false;
    private bool _regeneChecked = false;
    private bool _battleEnded = false;
    private List<ActionResultInfo> _slipDamageResults = new List<ActionResultInfo>();
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
    private Battle.CommandType _backCommandType = Battle.CommandType.None;
    public BattlePresenter(BattleView view)
    {
        _view = view;
        SetView(_view);
        _model = new BattleModel();
        SetModel(_model);

#if UNITY_EDITOR
        _view.gameObject.AddComponent<DebugBattleData>();
        var debgugger = _view.gameObject.GetComponent<DebugBattleData>();
        debgugger.SetDebugger(_model,this,_view);
        debgugger.consoleInputField = GameSystem.DebugBattleData.consoleInputField;
#endif
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
        _view.CreateObject(_model.BattlerActors().Count);
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
        _view.SetSideMenu(_model.SideMenu());

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
        if (viewEvent.commandType == Battle.CommandType.EnemyDetail)
        {
            CommandEnemyDetail((int)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.OpenSideMenu)
        {
            CommandOpenSideMenu();
        }
        if (viewEvent.commandType == Battle.CommandType.CloseSideMenu)
        {
            CommandCloseSideMenu();
        }
        if (viewEvent.commandType == Battle.CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.MenuCommandData)viewEvent.templete);
        }
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _view.HideSkillActionList();
            _view.HideSkillAtribute();
            _view.HideConditionAll();
            _view.HideBattleThumb();
            _view.SetEscapeButton(false);
            _view.SetBattleBusy(true);
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
        if (_model.EnableEspape())
        {
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(410).Text,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void CommandRule()
    {
        _busy = true;
        _view.SetHelpInputInfo("RULING");
        //_view.SetBattleBusy(true);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.CommandCallRuling(() => {
            //_view.SetBattleBusy(false);
            _view.SetHelpInputInfo("OPTION");
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _busy = false;
        });
    }

    private void CommandEnemyDetail(int enemyIndex)
    {
        if (_model.CurrentActor == null) return;
        if (_view.SkillList.skillActionList.gameObject.activeSelf) return;
        _busy = true;
        BattlerInfo enemyInfo = _model.GetBattlerInfo(enemyIndex);
        
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandEnemyInfoClose();
            _busy = false;
        });
        statusViewInfo.SetEnemyInfos(new List<BattlerInfo>(){enemyInfo},true);
        _view.CommandCallEnemyInfo(statusViewInfo);
        //_view.SetActiveUi(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);    
    }
    
    private void CommandUpdateAp()
    {
#if UNITY_EDITOR
        if (_debug == true) return;
#endif
        if (GameSystem.ConfigData._battleWait == false)
        {
            while (_model.CurrentBattler == null)
            {
                BeforeUpdateAp();
                ActionInfo CurrentActionInfo = _model.CurrentActionInfo();
                if (CurrentActionInfo != null)
                {
                    _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
                    CommandSelectIndex(_model.MakeAutoSelectIndex(CurrentActionInfo));
                    return;
                }
                if (CheckBattleEnd())
                {
                    return;
                }
                _model.UpdateAp();
                _view.UpdateAp();
            }
        } else
        {
            BeforeUpdateAp();
            ActionInfo CurrentActionInfo = _model.CurrentActionInfo();
            if (CurrentActionInfo != null)
            {
                _model.SetActionBattler(CurrentActionInfo.SubjectIndex);
                CommandSelectIndex(_model.MakeAutoSelectIndex(CurrentActionInfo));
                return;
            }
            if (CheckBattleEnd())
            {
                return;
            }
            _model.UpdateAp();
            _view.UpdateAp();
        }
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

    private void BeforeUpdateAp()
    {
        var chainActionResults = _model.UpdateChainState();
        ExecActionResult(chainActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,chainActionResults,false);
        
        StartAliveAnimation(chainActionResults);
        StartDeathAnimation(chainActionResults);

        var benedictionActionResults = _model.UpdateBenedictionState();
        ExecActionResult(benedictionActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,benedictionActionResults,false);
        
        StartDeathAnimation(benedictionActionResults);
        StartAliveAnimation(benedictionActionResults);
        if (chainActionResults.Count > 0 || benedictionActionResults.Count > 0)
        {
            _view.RefreshStatus();
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
    }

    public void CommandSelectIndex(List<int> indexList)
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
            _model.PopupActionResultInfo(actionInfo.ActionResults);
            var result = _model.CheckTriggerSkillInfos(TriggerTiming.Interrupt,actionInfo.ActionResults);
            if (result)
            {
                _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
                _model.MakeActionResultInfo(_model.CurrentActionInfo(),_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            }
            
            var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.Use);
            ExecActionResult(PassiveResults);
        }
    }

    private void StartAnimationDemigod()
    {
        if (_model.CurrentBattler.isActor == false)
        {
            StartAnimationSkill();
        } else
        {
            var demigod = _model.SkillActionAnimation("NA_Effekseer/NA_cut-in_002_" + _model.CurrentBattler.CharaId.ToString());
            _view.StartAnimationDemigod(demigod);
            _view.SetAnimationEndTiming(90);
            _nextCommandType = Battle.CommandType.EndDemigodAnimation;
        }
    }

    private void StartAnimationRegene(List<ActionResultInfo> regeneActionResults)
    {
        var animation = _model.SkillActionAnimation("tktk01/Cure1");
        for (int i = 0; i < regeneActionResults.Count; i++)
        {
            if (regeneActionResults[i].HpHeal != 0)
            {
                _view.StartAnimation(regeneActionResults[i].TargetIndex,animation,0);
                _view.StartHeal(regeneActionResults[i].TargetIndex,DamageType.HpHeal,regeneActionResults[i].HpHeal);
                _model.GainHpTargetIndex(regeneActionResults[i].TargetIndex,regeneActionResults[i].HpHeal);
            }
        }
        _nextCommandType = Battle.CommandType.EndRegeneAnimation;
    }

    private void StartAnimationSlipDamage(List<ActionResultInfo> _slipDamageResults)
    {
        var animation = _model.SkillActionAnimation("NA_Effekseer/NA_Fire_001");
        for (int i = 0; i < _slipDamageResults.Count; i++)
        {
            if (_slipDamageResults[i].HpDamage != 0)
            {            
                _view.StartDamage(_slipDamageResults[i].TargetIndex,DamageType.HpDamage,_slipDamageResults[i].HpDamage);
                _view.StartAnimation(_slipDamageResults[i].TargetIndex,animation,0);
                _model.GainHpTargetIndex(_slipDamageResults[i].TargetIndex,_slipDamageResults[i].HpDamage * -1);
            }
            if (_slipDamageResults[i].DeadIndexList.Contains(_slipDamageResults[i].TargetIndex))
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Defeat);
                _view.StartDeathAnimation(_slipDamageResults[i].TargetIndex);
            }
        }
        _nextCommandType = Battle.CommandType.EndSlipDamageAnimation;
    }

    private void StartAnimationSkill()
    {
        _view.SetRuleButton(false);
        _view.SetEscapeButton(false);
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
                bool lastTarget = actionResultInfos[actionResultInfos.Count-1].TargetIndex == targetIndex;
                PopupActionResult(actionResultInfos[i],targetIndex,true,true,lastTarget);
            }
        }
    }

    private void PopupActionResult(ActionResultInfo actionResultInfo,int targetIndex,bool needDamageBlink = true,bool needPopupDelay = true,bool lastTarget = true)
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
                _view.StartDamage(targetIndex,DamageType.HpDamage,actionResultInfo.HpDamage,needPopupDelay);
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
                _view.StartHeal(targetIndex,DamageType.HpHeal,actionResultInfo.HpHeal,needPopupDelay);
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
                _view.StartStatePopup(targetIndex,DamageType.State,"+行動短縮");
            }
        }
        if (actionResultInfo.ReDamage > 0)
        {
            if (lastTarget == true)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Damage);
                _view.StartDamage(actionResultInfo.SubjectIndex,DamageType.HpDamage,actionResultInfo.ReDamage);
                _view.StartBlink(actionResultInfo.SubjectIndex);
            }
        }
        if (actionResultInfo.ReHeal > 0)
        {
            if (lastTarget == true)
            {
                _view.StartHeal(actionResultInfo.SubjectIndex,DamageType.HpHeal,actionResultInfo.ReHeal);
            }
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
            _view.SetBattleBusy(false);
            _view.CommandSceneChange(Scene.Strategy);
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndDemigodAnimation)
        {
            StartAnimationSkill();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndSlipDamageAnimation)
        {
            EndTurn();
            //CommandEndSlipDamageAnimation();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndRegeneAnimation)
        {
            EndTurn();
            return;
        }
        // ダメージなどを適用
        _model.ExecCurrentActionResult();
        
        _view.ClearCurrentSkillData();
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            StartDeathAnimation(_model.CurrentActionInfo().ActionResults);
            /*
            if (_triggerAfterChecked == false)
            {
                if (_slipDamageChecked == false)
                {
                    var slipDamage = _model.CheckSlipDamage();
                    if (slipDamage == true)
                    {
                        StartAnimationSlipDamage();
                        return;
                    }
                }
                // リジェネ
                if (_regeneChecked == false)
                {
                    var regene = _model.CheckRegene();
                    if (regene)
                    {
                        StartAnimationRegene();
                        return;
                    }
                }
            }
            */
        }
        EndTurn();
    }

    private void ExecActionResult(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
    {
        _model.PopupActionResultInfo(resultInfos);
        for (int i = 0; i < resultInfos.Count; i++)
        {    
            // ダメージ表現をしない
            PopupActionResult(resultInfos[i],resultInfos[i].TargetIndex,false,needPopupDelay);
        }
        for (int i = 0; i < resultInfos.Count; i++)
        {    
            _model.ExecActionResultInfo(resultInfos[i]);
        }
    }

    private void CommandStartBattleAction()
    {
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.StartBattle);
        ExecActionResult(PassiveResults);
    }

    private void CommandEndSlipDamageAnimation()
    {    
        /*
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After,_slipDamageResults);
        if (result)
        {    
            if (_model.CurrentActionInfo() != null)
            {
                _model.RemoveActionInfo(_model.CurrentActionInfo());
            }
            ActionInfo CurrentActionInfo = _model.CurrentActionInfo();
            if (CurrentActionInfo != null)
            {
                _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
                CommandSelectIndex(_model.MakeAutoSelectIndex(CurrentActionInfo));
            }
            return;
        }
        */
        /*
        if (CheckBattleEnd() == false && result == false)
        {
            var regene = _model.CheckRegene();
            if (regene && _triggerAfterChecked == false)
            {
                StartAnimationRegene();
                return;
            }
        }
        */
        EndTurn();
    }

    private void EndTurn()
    {
        // ターン終了
        _view.RefreshStatus();
        // スリップダメージ
        if (_triggerAfterChecked == false && _slipDamageChecked == false)
        {
            _slipDamageChecked = true;
            var slipResult = _model.CheckSlipDamage();
            if (slipResult.Count > 0)
            {
                StartAnimationSlipDamage(slipResult);
                return;
            }
        }
        // regene
        if (_triggerAfterChecked == false && _regeneChecked == false)
        {
            _regeneChecked = true;
            var regeneResult = _model.CheckRegene();
            if (regeneResult.Count > 0)
            {
                StartAnimationRegene(regeneResult);
                return;
            }
        }
        // PlusSkill
        _model.CheckPlusSkill();
        // Passive付与
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
        ExecActionResult(PassiveResults);
        // Passive解除
        var RemovePassiveResults = _model.CheckRemovePassiveInfos();
        ExecActionResult(RemovePassiveResults);

        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After,_model.CurrentActionInfo().ActionResults);
        
        bool isDemigodActor = false;
        if (_model.CurrentBattler != null)
        {
            isDemigodActor = _model.CurrentBattler.IsState(StateType.Demigod);
        }
        bool isTriggeredSkill = _model.CurrentActionInfo().TriggeredSkill;
        if (result == false && _triggerAfterChecked == false && isTriggeredSkill == false)
        {
            var removed =_model.UpdateTurn();
            foreach (var removedState in removed)
            {
                _view.StartStatePopup(removedState.TargetIndex,DamageType.State,"-" + removedState.Master.Name);
            }
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
        // 戦闘不能の拘束ステートを解除する
        var removeChainStates = _model.EndRemoveState();
        foreach (var removeChainState in removeChainStates)
        {
            _view.StartStatePopup(removeChainState.TargetIndex,DamageType.State,"-" + removeChainState.Master.Name);
        }
        _view.RefreshStatus();

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
        _slipDamageChecked = false;
        _regeneChecked = false;
        _view.SetBattleBusy(false);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos,_model.SelectSkillIndex(skillInfos));
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
        if (_battleEnded == true) return false;
        bool isEnd = false;
        if (_model.CheckVictory())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15020).Text);
            _view.SetAnimationEndTiming(180);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
            _battleEnded = true;
        } else
        if (_model.CheckDefeat())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);
            _view.SetAnimationEndTiming(180);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
            _battleEnded = true;
        }
        return isEnd;
    }

    public void CommandCondition()
    {
        _view.HideSkillActionList(false);
        _view.SetEscapeButton(_model.EnableEspape());
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
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
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
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
        } else
        {
            _view.RefreshBattlerEnemyLayerTarget(-1);
            _view.RefreshBattlerPartyLayerTarget(actionInfo.TargetIndexList.Find(a => a < 100),actionInfo.TargetIndexList,actionInfo.ScopeType);
            _view.ShowPartyTarget();
            _view.DeactivateEnemyList();
        }
    }

    private void CommandOpenSideMenu()
    {
        _view.CommandOpenSideMenu();
    }

    private void CommandCloseSideMenu()
    {
        _view.CommandCloseSideMenu();
    }

    private void CommandSelectSideMenu(SystemData.MenuCommandData sideMenu)
    {
        if (sideMenu.Key == "Help")
        {
            CommandRule();
        }
    }    
    
    public void CommandOption()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallOption(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }
}