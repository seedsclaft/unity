using System.Collections;
using System.Collections.Generic;

public class BattlePresenter : BasePresenter
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
    
    private void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));

        _view.SetBattleActorData(_model.BattleActors());
        _view.SetBattleEnemyData(_model.BattleEnemies());
        _busy = false;
    }

    private void updateCommand(BattleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == (int)Battle.CommandType.None)
        {
            if (viewEvent.templete != null)
            {
            }
        }
    }

}
