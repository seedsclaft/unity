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

    private void Initialize()
    {
        _busy = true;
        _view.SetHelpWindow();

        //_view.SetActors(_model.TempData.TempResultActorInfos);
        _view.SetBackGround(_model.CurrentStage.Master.BackGround);
        _view.SetEvent((type) => UpdateCommand(type));

        _view.SetSymbolRecords(_model.SymbolRecords());
        _view.SetParallelCommand(_model.ParallelCommand());
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
        if (viewEvent.commandType == CommandType.DecideRecord)
        {
            CommandDecideRecord();
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == CommandType.Parallel)
        {
            CommandParallel();
        }
    }

    private void CommandDecideRecord()
    {
        var index = _view.SymbolListIndex;
        var popupInfo = new ConfirmInfo(DataSystem.GetTextData(23010).Text,(a) => UpdatePopupCheckStartRecord((ConfirmCommandType)a));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupCheckStartRecord(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var index = _view.SymbolListIndex;
            _model.MakeSymbolRecordStage(index);
            _view.CommandSceneChange(Scene.Tactics);
        }
    }

    private void CommandBack()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        _view.CommandSceneChange(Scene.MainMenu);
    }

    private void CommandParallel()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        if (_model.CanParallel())
        {
            var index = _view.SymbolListIndex;
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23020,_model.ParallelCost().ToString()),(a) => UpdatePopupCheckParallelRecord((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        } else
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(23030,_model.ParallelCost().ToString()),(a) => UpdatePopupNoParallelRecord());
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        }
    }

    private void UpdatePopupCheckParallelRecord(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            var index = _view.SymbolListIndex;
            _model.MakeSymbolRecordStage(index);
            _model.SetParallelMode();
            _view.CommandSceneChange(Scene.Tactics);
        }
    }

    private void UpdatePopupNoParallelRecord()
    {
        _view.CommandGameSystem(Base.CommandType.CloseConfirm);
    }

    public void CommandRefresh()
    {
        _view.SetTurns(_model.RemainTurns);
        _view.SetNuminous(_model.Currency);
        _view.SetStageInfo(_model.CurrentStage);
        _view.SetSymbols(_model.StageSymbolInfos(_view.SymbolListIndex));
        _view.SetTacticsCharaLayer(_model.SymbolActorIdList(_view.SymbolListIndex));
    }

    private enum SymbolRecordState{
        None = 0,
        BattleResult = 1,
        TacticsResult = 2,
    }
}
