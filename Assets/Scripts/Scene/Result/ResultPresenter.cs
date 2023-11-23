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
        _view.SetEvent((type) => UpdateCommand(type));
        _busy = true;

        _view.StartAnimation();
        _view.StartResultAnimation(_model.CastActorInfos(_model.ResultMembers()));
        _busy = false;
    }

    private void UpdateCommand(ResultViewEvent viewEvent)
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
            CommandResultClose((ConfirmCommandType)viewEvent.template);
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
        _model.ApplyScore();
        _model.SetResumeStageFalse();
        _view.SetEndingType(_model.EndingType());
        _view.SetEvaluate(_model.TotalEvaluate(),_model.IsNewRecord());
        _view.SetPlayerName(_model.PlayerName());
    }

    private void CommandResultClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
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
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16010).Text,(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
        popupInfo.SetSelectIndex(1);
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopup(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
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
            _view.ChangeUIActive(true);
            _view.SetHelpInputInfo("RESULT");
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.ChangeUIActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandActorAssign()
    {
        _model.GetRebornSkills();
        int textId = 16040;
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(textId).Text,(a) => UpdatePopupReborn((ConfirmCommandType)a));
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopupReborn(ConfirmCommandType confirmCommandType)
    {
        _view.CommandActorAssign();
        _view.SetActorList(_model.ActorInfos());
        CommandUpdateActor();
        _view.CommandConfirmClose();
        if (_model.ActorInfos().Count > 10)
        {
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16051).Text,(a) => UpdatePopupRebornEraseCheck((ConfirmCommandType)a));
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        } else
        {        
            _model.SetResumeStageFalse();
        }
    }

    private void UpdatePopupRebornEraseCheck(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
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
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(16060).Text,(a) => UpdatePopupRebornErase((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
            return;
        }
        CommandEndGame();
    }

    private void UpdatePopupRebornErase(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
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
        _model.SaveSlotData();
        _model.SetResumeStageFalse();
        _view.CommandSceneChange(Scene.MainMenu);
    }
}
