using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Result;

public class ResultPresenter : BasePresenter
{
    ResultModel _model = null;
    ResultView _view = null;

    private bool _busy = true;

    private bool _isRankingEnd = false;
    public ResultPresenter(ResultView view)
    {
        _view = view;
        SetView(_view);
        _model = new ResultModel();
        SetModel(_model);
        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetUiView();
        _view.SetResultList(_model.ResultCommand());
        _view.SetActors(_model.ResultMembers());
        var bgm = await _model.GetBgmData("TACTICS1");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => updateCommand(type));
        _busy = true;

        _view.StartAnimation();
        _view.StartResultAnimation(_model.ResultMembers());
        _busy = false;
    }

    private void updateCommand(ResultViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == CommandType.ResultClose)
        {
            CommandResultClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.DecideActor)
        {
            CommandDecideActor(0);
        }
        if (viewEvent.commandType == CommandType.UpdateActor)
        {
            CommandUpdateActor();
        }
    }

    private void CommandEndAnimation()
    {
        _model.ApllyScore();
        _model.SetResumeStageFalse();
        _view.SetEndingType(_model.EndingType());
        _view.SetEvaluate(_model.TotalEvaluate(),_model.IsNewRecord());
        _view.SetPlayerName(_model.PlayerName());
    }

    private void CommandResultClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            ShowStatus();
        } else
        {
            if (_isRankingEnd)
            {
                CommandActorAssign();
            } else
            {
                CommandRanking();
            }
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandRanking()
    {
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16010).Text,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
        popupInfo.SetSelectIndex(1);
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.GetSelfRankingData((a) => 
            {
                _isRankingEnd = true;
                _view.CommandConfirmClose();
                _view.SetRanking(a);
                UpdateResultCommand();
            });
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _isRankingEnd = true;
            _view.CommandConfirmClose();
            UpdateResultCommand();
        }
    }

    private void UpdateResultCommand()
    {
        _view.UpdateResultCommand(_model.StageEndCommand());
    }

    private void ShowStatus()
    {
        _model.SetActors();
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.SetActiveUi(true);
            _view.SetHelpInputInfo("RESULT");
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.SetActiveUi(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandActorAssign()
    {
        _model.GetRebornSkills();
        int textId = 16040;
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(textId).Text,(a) => UpdatePopupReborn((ConfirmComandType)a));
        popupInfo.SetIsNoChoise(true);
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupReborn(ConfirmComandType confirmComandType)
    {
        _view.CommandActorAssign();
        _view.SetActorList(_model.ActorInfos(),_model.DisableActorIndexs());
        CommandUpdateActor();
        _view.CommandConfirmClose();
        if (_model.ActorInfos().Count > 10)
        {
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16051).Text,(a) => UpdatePopupRebornEraseCheck((ConfirmComandType)a));
            popupInfo.SetIsNoChoise(true);
            _view.CommandCallConfirm(popupInfo);
        } else
        {        
            _model.SetResumeStageFalse();
        }
    }

    private void UpdatePopupRebornEraseCheck(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        } else
        {        
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        _view.CommandConfirmClose();
    }
    
    private void CommandDecideActor(int index)
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        if (_model.ActorInfos().Count > 10)
        {
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16060).Text,(a) => UpdatePopupRebornErase((ConfirmComandType)a));
            _view.CommandCallConfirm(popupInfo);
            return;
        }
        CommandEndGame();
    }

    private void UpdatePopupRebornErase(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.EraseReborn();
            CommandEndGame();
        } else{        
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
    }

    private void CommandUpdateActor()
    {
        _model.SetRebornActorIndex(_view.ActorInfoListIndex);
        var rebornActor = _model.RebornActorInfo();
        if (rebornActor != null)
        {
            _view.UpdateActor(rebornActor);
        }
    }
    private void CommandEndGame()
    {
        _model.SetResumeStageFalse();
        _view.CommandSceneChange(Scene.MainMenu);
    }
}
