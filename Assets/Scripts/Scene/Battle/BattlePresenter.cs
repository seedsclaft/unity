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
            CommandEnemyLayer((List<int>)viewEvent.templete);
        }
        if (viewEvent.commandType == Battle.CommandType.ActorList)
        {
            CommandActorList((List<int>)viewEvent.templete);
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
        _model.UpdateAp();
        _view.UpdateAp();
        if (_model.CurrentBattler != null)
        {
            _view.SetBusy(true);
            if (_model.CurrentBattler.isActor)
            {
                _view.ShowSkillActionList(_model.CurrentBattler.ActorInfo);
                CommandAttributeType(_model.CurrentAttributeType);
            } else
            {
                ActionInfo actionInfo = _model.MakeActionInfo(1);
                var indexList = new List<int>();
                indexList.Add(0);  
                _model.MakeActionResultInfo(actionInfo,indexList);
                if (actionInfo.actionResults[0].Target.isActor){
                    CommandActorList(indexList);
                } 
                else
                {
                    CommandEnemyLayer(indexList);
                }
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

    private async void CommandEnemyLayer(List<int> enemyIndexList)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _view.RefreshBattlerEnemyLayerTarget(null);
            _model.MakeActionResultInfo(actionInfo,enemyIndexList);
            var animation = await _model.SkillActionAnimation(actionInfo.Master.AnimationName);

            _nextCommandType = Battle.CommandType.EndAnimation;
            for (int i = 0; i < enemyIndexList.Count; i++)
            {
                _view.StartAnimationEnemy(enemyIndexList[i],animation);
                _view.StartSkillDamageEnemy(enemyIndexList[i],actionInfo.Master.DamageTiming,(targetIndex) => StartSkillDamage(targetIndex));
            }
        }
    }

    private async void CommandActorList(List<int> partyIndexList)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _view.RefreshBattlerPartyLayerTarget(null);
            _model.MakeActionResultInfo(actionInfo,partyIndexList);
            var animation = await _model.SkillActionAnimation(actionInfo.Master.AnimationName);
            _nextCommandType = Battle.CommandType.EndAnimation;
            for (int i = 0; i < partyIndexList.Count; i++)
            {
                _view.StartAnimationActor(partyIndexList[i],animation);
                _view.StartSkillDamageActor(partyIndexList[i],actionInfo.Master.DamageTiming,(targetIndex) => StartSkillDamage(targetIndex));
            }
        }
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
                        if (actionResultInfos[i].Target.isActor){
                            _view.StartDamageActor(targetIndex,DamageType.HpDamage,actionResultInfos[i].HpDamage);
                        } else{
                            _view.StartDamageEnemy(targetIndex,DamageType.HpDamage,actionResultInfos[i].HpDamage);
                        }
                    }
                }
            }
        }
    }

    private void CommandEndAnimation()
    {
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
        _model.TurnEnd();
        _view.SetBusy(false);
        /*
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
        /*
        if (_nextCommandType == Battle.CommandType.StartDamage)
        {
            ActionInfo actionInfo = _model.CurrentActionInfo();
            if (actionInfo != null)
            {
                List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
                for (int i = 0; i < actionResultInfos.Count; i++)
                {
                    if (actionResultInfos[i].HpDamage > 0)
                    {
                        _view.StartDamage(actionResultInfos[i].TargetIndex,DamageType.HpDamage,actionResultInfos[i].HpDamage);
                    }
                }
            }
        }
        */
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
