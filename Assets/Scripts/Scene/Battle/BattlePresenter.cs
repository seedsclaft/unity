using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePresenter 
{
    BattleModel _model = null;
    BattleView _view = null;

    private bool _busy = true;
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
            _view.ShowSkillActionList(_model.CurrentBattler.ActorInfo);
            CommandAttributeType(_model.CurrentAttributeType);
        }
    }

    private void CommandSkillAction(int skillId)
    {
        _model.ClearActionInfo();
        ActionInfo actionInfo = _model.MakeActionInfo(skillId);
        if (actionInfo.TargetType == TargetType.Opponent)
        {
            _view.HideSkillActionList();
            _view.ShowEnemyTarget(actionInfo);
        }
        //
    }

    private void CommandEnemyLayer(List<int> enemyIndexList)
    {
        ActionInfo actionInfo = _model.CurrentActionInfo();
        if (actionInfo != null)
        {
            _model.MakeActionResultInfo(actionInfo,enemyIndexList);
        }
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
