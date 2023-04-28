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
        _view.SetAttributeTypes(_model.AttributeTypes(),_model.CurrentAttributeType);
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
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandLeftActor()
    {
        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(-1);
        _view.StartEnemyInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private void CommandRightActor()
    {
        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(1);
        _view.StartEnemyInfo(_model.CurrentActor);
        CommandAttributeType(_model.CurrentAttributeType);
    }

    private void CommandAttributeType(AttributeType attributeType)
    {
        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        List<SkillInfo> skillInfos = _model.SkillActionList(attributeType);
        _view.RefreshSkillActionList(skillInfos);
    }

    private void CommandBack()
    {
        _view.CommandBack();
    }

    private void CommandRefresh()
    {
        _view.StartEnemyInfo(_model.CurrentActor);
        List<SkillInfo> skillInfos = _model.SkillActionList(_model.CurrentAttributeType);
        _view.RefreshSkillActionList(skillInfos);
    }
}
