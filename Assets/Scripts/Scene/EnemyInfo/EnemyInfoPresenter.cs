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
        _view.SetEnemies(_model.EnemyInfoListDates());
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
            CommandSelectEnemy();
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
    }

    private void CommandSelectEnemy()
    {
        var selectIndex = _view.EnemyListIndex;
        _model.SelectEnemyIndex(selectIndex);
        _view.UpdateEnemyList(selectIndex);
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
