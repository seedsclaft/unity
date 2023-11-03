using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyInfo;

public class EnemyInfoPresenter 
{
    EnemyInfoModel _model = null;
    EnemyInfoView _view = null;

    private bool _busy = true;
    public EnemyInfoPresenter(EnemyInfoView view,List<BattlerInfo> enemyInfos)
    {
        _view = view;
        _model = new EnemyInfoModel(enemyInfos);

        Initialize();
    }

    private void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));
        _view.SetAttributeTypes(_model.AttributeAllTypes(null,(int)_model.CurrentAttributeType));
        CommandRefresh();
        _busy = false;
    }

    private void updateCommand(EnemyInfoViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.AttributeType)
        {
            CommandAttributeType((AttributeType)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.LeftActor)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == CommandType.RightActor)
        {
            CommandRightActor();
        }
        if (viewEvent.commandType == CommandType.Condition)
        {
            CommandCondition();
        }
        if (viewEvent.commandType == CommandType.Skill)
        {
            CommandSkill();
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandLeftActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(-1);
        _view.StartEnemyInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private void CommandRightActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(1);
        _view.StartEnemyInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    public void CommandCondition()
    {
        _view.HideSkillActionList();
        _view.ActivateConditionList();
        _view.SetCondition(_model.CurrentActor.StateInfos);
        _view.ShowConditionAll();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
    }

    public void CommandSkill()
    {
        _view.HideCondition();
        CommandRefresh();
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        List<ListData> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
    }

    private void CommandBack()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _view.CommandBack();
    }

    private void CommandRefresh()
    {
        _view.StartEnemyInfo(_model.CurrentActor);
        var skillInfos = _model.SkillActionList(_model.CurrentAttributeType);
        _view.RefreshSkillActionList(skillInfos);
    }
}
