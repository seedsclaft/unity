using System.Collections.Generic;
using UnityEngine;
using SymbolRecord;

public class SymbolRecordPresenter : BasePresenter
{
    SymbolRecordModel _model = null;
    SymbolRecordView _view = null;

    private bool _busy = true;


    private SymbolRecordState _strategyState = SymbolRecordState.None;
    public SymbolRecordPresenter(SymbolRecordView view)
    {
        _view = view;
        SetView(_view);
        _model = new SymbolRecordModel();
        SetModel(_model);
        Initialize();
    }

    private async void Initialize()
    {
        _busy = true;
        _view.SetHelpWindow();

        //_view.SetActors(_model.TempData.TempResultActorInfos);
        //var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        //Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => UpdateCommand(type));

        _view.SetSymbolRecords(_model.SymbolRecords());
        CommandRefresh();
        _busy = false;
    }

    private void UpdateCommand(SymbolRecordViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        Debug.Log(viewEvent.commandType);
        if (viewEvent.commandType == CommandType.SelectRecord)
        {
            CommandRefresh();
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
    }

    public void CommandRefresh()
    {
        _view.SetTurns(_model.RemainTurns);
        _view.SetNuminous(_model.Currency);
        _view.SetSymbols(_model.Symbols(_view.SymbolListIndex));
        _view.SetTacticsCharaLayer(_model.SymbolActors(_view.SymbolListIndex));
        //_view.SetStageInfo(_model.CurrentStage);
        
        //_view.CommandRefresh();
    }

    private void CommandBack()
    {
        _view.CommandSceneChange(Scene.MainMenu);
    }



    private enum SymbolRecordState{
        None = 0,
        BattleResult = 1,
        TacticsResult = 2,
    }
}
