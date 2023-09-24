using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastBattlePresenter : BasePresenter
{
    FastBattleModel _model = null;
    FastBattleView _view = null;

    private bool _busy = true;
    private bool _triggerAfterChecked = false;
	private bool _triggerInterruptChecked = false;
    private bool _slipDamageChecked = false;
    private bool _regeneChecked = false;
    private bool _battleEnded = false;
    private List<ActionResultInfo> _slipDamageResults = new List<ActionResultInfo>();
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
    private Battle.CommandType _backCommandType = Battle.CommandType.None;
    public FastBattlePresenter(FastBattleView view)
    {
        _view = view;
        SetView(_view);
        _model = new FastBattleModel();
        SetModel(_model);

        Initialize();
    }

    private void Initialize()
    {
        Time.timeScale = 4;
        _view.SetBattleBusy(true);
        _model.CreateBattleData();

        _view.CreateObject(_model.BattlerActors().Count);
        _view.SetUIButton();
        _view.SetActiveBack(false);

        _view.CommandStartTransition(() => {
            BattleInitialized();
        });
    }

    private void BattleInitialized()
    {
        _view.SetEvent((type) => updateCommand(type));

        _view.SetActors(_model.BattlerActors());
        _view.SetEnemies(_model.BattlerEnemies());


        _view.SetBattleBusy(false);
        CommandStartBattleAction();
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
    }
    
    private void CommandUpdateAp()
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
                var (autoSkillId,targetIndex) = _model.MakeAutoActorSkillId(_model.CurrentBattler);
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,autoSkillId,false,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo,targetIndex));
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
        
        var benedictionActionResults = _model.UpdateBenedictionState();
        ExecActionResult(benedictionActionResults,false);
        _model.CheckTriggerSkillInfos(TriggerTiming.After,benedictionActionResults,false);
        
    }

    private void CommandSkillAction(SkillInfo skillInfo)
    {
        if (skillInfo.Enable == false)
        {
            return;
        }
        _model.ClearActionInfo();
        _model.SetLastSkill(skillInfo.Id);
        ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillInfo.Id,false,false);
        
        _backCommandType = Battle.CommandType.DecideActor;
        _view.SetActiveBack(true);
    }

    public void CommandSelectIndex(List<int> indexList)
    {
        MakeActionResultInfo(indexList);
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            StartAnimationSkill();
        }
    }

    private void MakeActionResultInfo(List<int> indexList)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _model.MakeActionResultInfo(actionInfo,indexList);
            _model.MakeCurseActionResults(actionInfo,indexList);
            _model.PopupActionResultInfo(actionInfo.ActionResults);
            if (_triggerInterruptChecked == false)
            {
                var result = _model.CheckTriggerSkillInfos(TriggerTiming.Interrupt,actionInfo.ActionResults);
                if (result)
                {
                    _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
                    _model.MakeActionResultInfo(_model.CurrentActionInfo(),_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
                }
                _triggerInterruptChecked = true;
            }
            
            var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.Use);
            ExecActionResult(PassiveResults);
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
                _view.StartAnimation(_slipDamageResults[i].TargetIndex,animation,0);
                _model.GainHpTargetIndex(_slipDamageResults[i].TargetIndex,_slipDamageResults[i].HpDamage * -1);
            }
        }
        _nextCommandType = Battle.CommandType.EndSlipDamageAnimation;
    }

    private void StartAnimationSkill()
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.ActionResults.Count == 0)
        {
            _nextCommandType = Battle.CommandType.SkillAction;
            CommandEndAnimation();
            return;
        }
        var animation = _model.SkillActionAnimation(actionInfo.Master.AnimationName);
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
            StartSkillDamage(actionInfo.ActionResults[i].TargetIndex);
        }
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
            _view.SetBattleBusy(false);
            Time.timeScale = 1;
            _view.CommandLoadingClose();
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
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndRegeneAnimation)
        {
            EndTurn();
            return;
        }
        // ダメージなどを適用
        _model.ExecCurrentActionResult();
        
        EndTurn();
    }

    private void ExecActionResult(List<ActionResultInfo> resultInfos,bool needPopupDelay = true)
    {
        _model.PopupActionResultInfo(resultInfos);
        for (int i = 0; i < resultInfos.Count; i++)
        {    
            _model.ExecActionResultInfo(resultInfos[i]);
        }
        if (resultInfos.Count > 0)
        {
            _view.RefreshStatus();
        }
    }

    private void CommandStartBattleAction()
    {
        var PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.StartBattle);
        ExecActionResult(PassiveResults);
        var AfterPassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
        ExecActionResult(AfterPassiveResults);
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
            // Passive付与
            PassiveResults = _model.CheckTriggerPassiveInfos(TriggerTiming.After);
            ExecActionResult(PassiveResults);
            // Passive解除
            RemovePassiveResults = _model.CheckRemovePassiveInfos();
            ExecActionResult(RemovePassiveResults);
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
        // Hp0以上の戦闘不能を回復
        var notDeadMembers = _model.NotDeadMembers();
        // 戦闘不能の拘束ステートを解除する
        var removeChainStates = _model.EndRemoveState();
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
            _busy = false;
        }
        _triggerInterruptChecked = false;
        _triggerAfterChecked = false;
        _slipDamageChecked = false;
        _regeneChecked = false;
        _view.SetBattleBusy(false);
    }

    private bool CheckBattleEnd()
    {
        if (_battleEnded == true) return false;
        bool isEnd = false;
        if (_model.CheckVictory())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15020).Text);
            _view.SetAnimationEndTiming(4);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
            _battleEnded = true;
        } else
        if (_model.CheckDefeat())
        {
            _model.EndBattle();
            _view.StartBattleStartAnim(DataSystem.System.GetTextData(15030).Text);
            _view.SetAnimationEndTiming(4);
            _nextCommandType = Battle.CommandType.EndBattle;
            isEnd = true;
            _battleEnded = true;
        }
        return isEnd;
    }
}