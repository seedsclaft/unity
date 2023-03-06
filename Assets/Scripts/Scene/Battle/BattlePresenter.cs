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
        var bgm = await _model.BgmData();
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

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
            _view.StartDamage(chainActionResults[i].TargetIndex,DamageType.HpDamage,chainActionResults[i].HpDamage);
        }
        var benedictionActionResults = _model.UpdateBenedictionState();
        for (int i = 0; i < benedictionActionResults.Count; i++)
        {
            _view.StartDamage(benedictionActionResults[i].TargetIndex,DamageType.HpHeal,benedictionActionResults[i].HpHeal);
        }
        _model.UpdateAp();
        _view.UpdateAp();
        if (_model.CurrentBattler != null)
        {
            _view.SetBusy(true);
            if (!_model.EnableCurrentBattler())
            {
                int skillId = 0;
                ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillId,false);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
                return;
            }
            if (_model.CurrentBattler.isActor)
            {
                List<int> chainTargetIndexs = _model.CheckChainBattler();
                if (chainTargetIndexs.Count > 0)
                {
                    // 拘束解除
                    ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,31,false);
                    CommandSelectIndex(chainTargetIndexs);
                } 
                else{
                    CommandDecideActor();
                }
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
        ActionInfo actionInfo = _model.MakeActionInfo(_model.CurrentBattler,skillId,false);
        _view.HideSkillActionList();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            _view.ShowEnemyTarget();
            _view.RefreshBattlerEnemyLayerTarget(actionInfo);
        } else
        if (actionInfo.TargetType == TargetType.Friend || actionInfo.TargetType == TargetType.Self)
        {
            _view.ShowPartyTarget();
            _view.RefreshBattlerPartyLayerTarget(actionInfo);
        } else
        {
            _view.ShowEnemyTarget();
            _view.ShowPartyTarget();
            _view.RefreshBattlerPartyLayerTarget(actionInfo);
            _view.RefreshBattlerEnemyLayerTarget(actionInfo);
        }
        _backCommandType = Battle.CommandType.DecideActor;
        _view.SetActiveBack(true);
        //
    }

    private void CommandSelectIndex(List<int> indexList)
    {
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
            _view.RefreshBattlerEnemyLayerTarget(null);
            _view.RefreshBattlerPartyLayerTarget(null);
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
        var demigod = await _model.SkillActionAnimation("NA_cut-in_002");
        _view.StartAnimationDemigod(demigod);
        _nextCommandType = Battle.CommandType.EndDemigodAnimation;
    }

    private async void StartAnimationRegene()
    {
        var regeneActionResults = _model.UpdateRegeneState();
        var animation = await _model.SkillActionAnimation("tktk01/Cure1");
        for (int i = 0; i < regeneActionResults.Count; i++)
        {
            _view.StartAnimation(regeneActionResults[i].TargetIndex,animation);
            _view.StartHeal(regeneActionResults[i].TargetIndex,DamageType.HpHeal,regeneActionResults[i].HpHeal);
        }
        _nextCommandType = Battle.CommandType.EndRegeneAnimation;
    }

    private async void StartAnimationSkill()
    {
        //_view.ClearDamagePopup();
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo.actionResults.Count == 0)
        {
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
                _view.StartAnimation(actionInfo.actionResults[i].TargetIndex,animation);
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
                if (actionResultInfos[i].Missed)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartStatePopup(targetIndex,DamageType.State,"Miss!");    
                    }
                }
                if (actionResultInfos[i].HpDamage > 0)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartDamage(targetIndex,DamageType.HpDamage,actionResultInfos[i].HpDamage);
                    }
                }
                if (actionResultInfos[i].HpHeal > 0)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartHeal(targetIndex,DamageType.HpHeal,actionResultInfos[i].HpHeal);
                    }
                }
                if (actionResultInfos[i].MpHeal > 0)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartHeal(targetIndex,DamageType.MpHeal,actionResultInfos[i].MpHeal);
                    }
                }
                if (actionResultInfos[i].ApHeal > 0)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartStatePopup(targetIndex,DamageType.State,"+行動可能");
                    }
                }
                if (actionResultInfos[i].ReDamage > 0)
                {
                    _view.StartDamage(actionResultInfos[i].SubjectIndex,DamageType.HpDamage,actionResultInfos[i].ReDamage);
                }
                if (actionResultInfos[i].ReHeal > 0)
                {
                    _view.StartHeal(actionResultInfos[i].SubjectIndex,DamageType.HpHeal,actionResultInfos[i].ReHeal);
                }
                if (actionResultInfos[i].AddedStates.Count > 0)
                {
                    for (int j = 0;j < actionResultInfos[i].AddedStates.Count;j++)
                    {
                        _view.StartStatePopup(actionResultInfos[i].AddedStates[j].TargetIndex,DamageType.State,"+" + actionResultInfos[i].AddedStates[j].Master.Name);
                    }
                }
                if (actionResultInfos[i].RemovedStates.Count > 0)
                {
                    for (int j = 0;j < actionResultInfos[i].RemovedStates.Count;j++)
                    {
                        _view.StartStatePopup(actionResultInfos[i].RemovedStates[j].TargetIndex,DamageType.State,"-" + actionResultInfos[i].RemovedStates[j].Master.Name);
                    }
                }
            }
        }
    }

    private void CommandEndAnimation()
    {
        if (_nextCommandType == Battle.CommandType.EndDemigodAnimation)
        {
            StartAnimationSkill();
            return;
        }
        if (_nextCommandType == Battle.CommandType.EndRegeneAnimation)
        {
            _view.SetBusy(false);
            return;
        }
        // ダメージなどを適用
        _model.ExecActionResult();
        List<int> deathBattlerIndex = _model.DeathBattlerIndex();
        if (deathBattlerIndex.Count > 0)
        {
            //var animation = await _model.SkillActionAnimation("");
            for (int i = 0; i < deathBattlerIndex.Count; i++)
            {
                _view.StartDeathAnimation(deathBattlerIndex[i]);
            }
        }
        // ステートなどを適用
        // ターン終了
        _view.RefreshStatus();
        // PlusSkill
        _model.CheckPlusSkill();
        // TriggerAfter
        var result = _model.CheckTriggerSkillInfos(TriggerTiming.After);
        _model.TurnEnd();
        // 次の行動者がいれば続ける
        if (_model.CurrentActionInfo() != null)
        {
            _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
            return;
        }
        // リジェネ
        var regene = _model.CheckRegeneBattlers();
        if (regene.Count > 0)
        {
            StartAnimationRegene();
            return;
        }

        _view.SetBusy(false);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
    }

    private void CommandDecideActor()
    {
        _view.ShowSkillActionList(_model.CurrentBattler);
        CommandAttributeType(_model.CurrentAttributeType);
        _view.RefreshBattlerEnemyLayerTarget(null);
        _view.RefreshBattlerPartyLayerTarget(null);
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

}