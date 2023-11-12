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
        _view.SetEvent((type) => UpdateCommand(type));
        CommandRefresh();
        _busy = false;
    }

    private void UpdateCommand(EnemyInfoViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.SelectEnemy)
        {
            CommandSelectEnemy((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.LeftEnemy)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == CommandType.RightEnemy)
        {
            CommandRightActor();
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandSelectEnemy(int enemyIndex)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        _model.SelectEnemy(enemyIndex);
        CommandRefresh();
    }

    private void CommandLeftActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(-1);
         CommandRefresh();
    }

    private void CommandRightActor()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
         _model.ChangeActorIndex(1);
         CommandRefresh();
    }

    private void CommandBack()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _view.CommandBack();
    }

    private void CommandRefresh()
    {
        var skillInfos = _model.SkillActionList();
        var lastSelectIndex = 0;
        _view.SetCondition(_model.SelectCharacterConditions());
        _view.CommandRefreshStatus(skillInfos,_model.CurrentEnemy,_model.EnemyIndexes(),lastSelectIndex);
    }
}
