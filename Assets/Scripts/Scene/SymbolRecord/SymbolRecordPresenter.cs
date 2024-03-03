using System.Collections.Generic;
using UnityEngine;
using SymbolRecord;
using Ryneus;

namespace Ryneus
{
    public class SymbolRecordPresenter : BasePresenter
    {
        SymbolRecordModel _model = null;
        SymbolRecordView _view = null;
        private bool _busy = true;
        private CommandType _backEvent = CommandType.None;
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

            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            _view.SetEvent((type) => UpdateCommand(type));

            _view.SetSymbolRecords(_model.SymbolRecords());
            _view.SetParallelCommand(_model.ParallelCommand());
            CommandRefresh();
            var bgm = await _model.GetBgmData("MAINMENU");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
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
            if (viewEvent.commandType == CommandType.SelectSymbol)
            {
                CommandSelectSymbol((SymbolInfo)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.CancelSymbol)
            {
                CommandCancelSymbol();
            }
        }

        private void CommandDecideRecord()
        {
            _view.HideSymbolBackGround();
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
                _view.CommandGotoSceneChange(Scene.Tactics);
            } else
            {
                _view.ShowSymbolBackGround();
            }
        }

        private void CommandBack()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (_backEvent != CommandType.None)
            {
                var eventData = new SymbolRecordViewEvent(CommandType.SelectRecord);
                eventData.commandType = _backEvent;
                UpdateCommand(eventData);
                return;
            }
            _view.CommandGotoSceneChange(Scene.MainMenu);
        }

        private void CommandParallel()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var ParallelIndex = _view.ParallelListIndex;
            if (ParallelIndex == 0)
            {
                CommandDecideRecord();
                return;
            }
            if (_model.CanParallel())
            {
                _view.HideSymbolBackGround();
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
                _view.CommandGotoSceneChange(Scene.Tactics);
            } else
            {
                _view.ShowSymbolBackGround();
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
            //_view.SetSymbols(_model.StageSymbolInfos(_view.SymbolListIndex));
            //_view.SetTacticsCharaLayer(_model.SymbolActorIdList(_view.SymbolListIndex));
        }

        private void CommandSelectSymbol(SymbolInfo symbolInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.ShowTacticsSymbolList();
            _view.ShowParallelList();
            _view.SetSymbols(_model.StageSymbolInfos(symbolInfo.StageSymbolData.Seek-1));
            _backEvent = CommandType.CancelSymbol;
        }

        private void CommandCancelSymbol()
        {
            _view.CommandCancelSymbol();
            _backEvent = CommandType.None;
        }

    }
}