using System.Collections;
using System.Collections.Generic;
using EnemyInfo;

namespace Ryneus
{
    public class EnemyInfoPresenter : BasePresenter
    {
        EnemyInfoModel _model = null;
        EnemyInfoView _view = null;

        private bool _busy = true;
        public EnemyInfoPresenter(EnemyInfoView view,List<BattlerInfo> enemyInfos)
        {
            _view = view;
            _model = new EnemyInfoModel(enemyInfos);

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetEnemies(GetListData(_model.EnemyBattlerInfos));
            CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(EnemyInfoViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
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
            _view.CommandBack();
        }

        private void CommandRefresh()
        {
            var skillInfos = _model.SkillActionList();
            var lastSelectIndex = 0;
            _view.SetCondition(GetListData(_model.SelectCharacterConditions()));
            _view.CommandRefreshStatus(GetListData(skillInfos,0),_model.CurrentEnemy,GetListData(_model.EnemySkillTriggerInfo(),0),_model.EnemyIndexes(),lastSelectIndex);
        }
    }
}