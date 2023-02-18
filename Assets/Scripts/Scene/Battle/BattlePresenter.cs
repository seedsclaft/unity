using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePresenter 
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
    private Battle.CommandType _nextCommandType = Battle.CommandType.None;
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
    }

    private void CommandUpdateAp()
    {
        var chainActionResults = _model.UpdateChainState();
        for (int i = 0; i < chainActionResults.Count; i++)
        {
            _view.StartDamage(chainActionResults[i].TargetIndex,DamageType.HpDamage,chainActionResults[i].HpDamage);
        }
        _model.UpdateAp();
        _view.UpdateAp();
        if (_model.CurrentBattler != null)
        {
            _view.SetBusy(true);
            if (_model.CurrentBattler.isActor)
            {
                List<int> chainTargetIndexs = _model.CheckChainBattler();
                if (chainTargetIndexs.Count > 0)
                {
                    // 拘束解除
                    ActionInfo actionInfo = _model.MakeActionInfo(202);
                    CommandSelectIndex(chainTargetIndexs);
                } 
                else{
                    _view.ShowSkillActionList(_model.CurrentBattler);
                    CommandAttributeType(_model.CurrentAttributeType);
                }
            } else
            {
                ActionInfo actionInfo = _model.MakeActionInfo(1);
                CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
            }
        }
    }

    private void CommandSkillAction(int skillId)
    {
        _model.ClearActionInfo();
        ActionInfo actionInfo = _model.MakeActionInfo(skillId);
        _view.HideSkillActionList();
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            _view.ShowEnemyTarget();
            _view.RefreshBattlerEnemyLayerTarget(actionInfo);
        } else
        if (actionInfo.TargetType == TargetType.Friend)
        {
            _view.ShowPartyTarget();
            _view.RefreshBattlerPartyLayerTarget(actionInfo);
        }
        //
    }

    private void CommandSelectIndex(List<int> indexList)
    {
        MakeActionResultInfo(indexList);
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            //_view.RefreshBattlerEnemyLayerTarget(null);
            //_view.RefreshBattlerPartyLayerTarget(null);
            //_model.MakeActionResultInfo(actionInfo,indexList);
            if (actionInfo.Master.SkillType == SkillType.Demigod)
            {
                StartAnimationDemigod();
                return;
            }
            StartAnimationSkill();
            /*
            var animation = await _model.SkillActionAnimation(actionInfo.Master.AnimationName);

            _nextCommandType = Battle.CommandType.EndAnimation;
            for (int i = 0; i < indexList.Count; i++)
            {
                _view.StartAnimation(indexList[i],animation);
                _view.StartSkillDamage(indexList[i],actionInfo.Master.DamageTiming,(targetIndex) => StartSkillDamage(targetIndex));
            }
            */
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
        }
    }

    private async void StartAnimationDemigod()
    {
        var demigod = await _model.SkillActionAnimation("NA_cut-in_002");
        _view.StartAnimationDemigod(demigod);
        _nextCommandType = Battle.CommandType.EndDemigodAnimation;
    }

    private async void StartAnimationSkill()
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        var animation = await _model.SkillActionAnimation(actionInfo.Master.AnimationName);

        for (int i = 0; i < actionInfo.actionResults.Count; i++)
        {
            _view.StartAnimation(actionInfo.actionResults[i].TargetIndex,animation);
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
                if (actionResultInfos[i].HpDamage > 0)
                {
                    if (actionResultInfos[i].TargetIndex == targetIndex)
                    {
                        _view.StartDamage(targetIndex,DamageType.HpDamage,actionResultInfos[i].HpDamage);
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
        _model.CheckTriggerSkillInfos(TriggerTiming.After);
        _model.TurnEnd();
        // 次の行動者がいれば続ける
        if (_model.CurrentActionInfo() != null)
        {
            _model.SetActionBattler(_model.CurrentActionInfo().SubjectIndex);
            CommandSelectIndex(_model.MakeAutoSelectIndex(_model.CurrentActionInfo()));
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
