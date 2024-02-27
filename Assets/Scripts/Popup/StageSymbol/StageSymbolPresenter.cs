using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StageSymbol;
using Ryneus;

public class StageSymbolPresenter 
{
    StageSymbolModel _model = null;
    StageSymbolView _view = null;
    private CommandType _backCommand = CommandType.None;

    private bool _busy = true;
    public StageSymbolPresenter(StageSymbolView view)
    {
        _view = view;
        _model = new StageSymbolModel();

        Initialize();
    }

    private void Initialize()
    {
        _busy = false;
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetSymbolRecords(_model.SymbolRecords());
        _view.SetStage(_model.CurrentStage);
        _view.StartAnimation();
    }

    private void UpdateCommand(StageSymbolViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        Debug.Log(viewEvent.commandType);
        switch (viewEvent.commandType)
        {
            case CommandType.Back:
                CommandBack();
                break;
            case CommandType.SelectRecord:
                CommandSelectRecord((SymbolInfo)viewEvent.template);
                break;
            case CommandType.CancelRecord:
                CommandCancelRecord();
                break;
            case CommandType.CallEnemyInfo:
                CommandCallEnemyInfo((SymbolInfo)viewEvent.template);
                break;
        }
    }

    private void CommandSelectRecord(SymbolInfo symbolInfo)
    {
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.SetSymbols(_model.StageSymbolInfos(symbolInfo.StageSymbolData.Seek));
        _backCommand = CommandType.CancelRecord;
        _view.ShowSymbolList();
    }
    
    private void CommandCancelRecord()
    {
        _view.HideSymbolList();
        _backCommand = CommandType.None;
    }    
    
    private void CommandBack()
    {
        if (_backCommand != CommandType.None)
        {
            var eventData = new StageSymbolViewEvent(_backCommand);
            UpdateCommand(eventData);
            return;
        }
        _view.CommandBackEvent();
    }

    private void CommandCallEnemyInfo(SymbolInfo symbolInfo)
    {
        switch (symbolInfo.SymbolType)
        {
            case SymbolType.Battle:
            case SymbolType.Boss:
                var enemyInfos = symbolInfo.BattlerInfos();
                
                var enemyViewInfo = new StatusViewInfo(() => {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                enemyViewInfo.SetEnemyInfos(enemyInfos,false);
                _view.CommandCallEnemyInfo(enemyViewInfo);
                _view.ChangeUIActive(false);
                break;
            case SymbolType.Alcana:
                CommandPopupSkillInfo(symbolInfo.GetItemInfos[0]);
                break;
            case SymbolType.Actor:
                _model.SetTempAddActorStatusInfos(symbolInfo.GetItemInfos[0].Param1);
                var statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                _view.CommandCallStatus(statusViewInfo);
                _view.ChangeUIActive(false);
                break;
        }
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
    
    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo((ConfirmCommandType)a));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallSkillDetail(popupInfo);
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
    
    private void UpdatePopupSkillInfo(ConfirmCommandType confirmCommandType)
    {
        _view.CommandGameSystem(Base.CommandType.ClosePopup);
    }

}
