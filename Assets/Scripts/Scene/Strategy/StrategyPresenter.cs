using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Strategy;

public class StrategyPresenter 
{
    StrategyModel _model = null;
    StrategyView _view = null;

    private bool _busy = true;

    private bool _isBattle = false;
    private bool _isBattleEnded = false;
    public StrategyPresenter(StrategyView view)
    {
        _view = view;
        _model = new StrategyModel();

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetUiView();

        _view.SetActors(_model.Actors());
        _view.SetResultList(_model.ResultCommand());
        var bgm = await _model.BgmData();
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => updateCommand(type));
        _busy = false;
        CommandStartStretegy();
    }

    private void updateCommand(StrategyViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.StartStretegy)
        {
            CommandStartStretegy();
        }
        if (viewEvent.commandType == CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == CommandType.ResultClose)
        {
            CommandResultClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.BattleClose)
        {
            CommandBattleClose((ConfirmComandType)viewEvent.templete);
        }
    }

    private void CommandStartStretegy(){
        List<ActorInfo> tacticsActors = _model.TacticsActors();
        if (tacticsActors.Count > 0)
        {
            _view.StartResultAnimation(tacticsActors);
        } else{
            List<ActorInfo> battleMembers = _model.CheckNextBattleActors();
            if (battleMembers != null && battleMembers.Count > 0)
            {
                StartNextBattle(battleMembers);
            } else{
                List<ActorInfo> battledMembers = _model.CheckInBattleActors();
                if (battledMembers != null && battledMembers.Count > 0)
                {
                    _isBattleEnded = true;
                    _model.ClearBattleData(battledMembers);
                    _view.StartResultAnimation(battledMembers);
                } else{
                    EndStrategy();
                }
            }
        }
    }

    private void CommandEndAnimation()
    {
        if (_isBattle == false){
            List<GetItemInfo> getItemInfos = _model.SetResult();
            _view.ShowResultList(getItemInfos);
        } else{
            if (_isBattleEnded == true)
            {
                List<GetItemInfo> getItemInfos = _model.SetBattleResult();
                _view.ShowResultList(getItemInfos);
            } else{
                _view.ShowEnemyList(_model.TacticsEnemies(),_model.TacticsGetItemInfos(),_model.ResultCommand());
            }
        }
    }

    private void CommandResultClose(ConfirmComandType confirmComandType)
    {
        if (_isBattleEnded == false)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                ShowStatus();
            } else{
                List<ActorInfo> battleMembers = _model.CheckNextBattleActors();
                if (battleMembers != null && battleMembers.Count > 0)
                {
                    StartNextBattle(battleMembers);
                } else{
                    EndStrategy();
                }
            }
        } else
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                ShowStatus();
            } else{
                List<ActorInfo> battleMembers = _model.CheckNextBattleActors();
                if (battleMembers != null && battleMembers.Count > 0)
                {
                    StartNextBattle(battleMembers);
                } else
                {                
                    EndStrategy();
                }
            }
        }
    }

    private void CommandBattleClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            ShowStatus();
        } else{
            _view.CommandSceneChange(Scene.Battle);
        }
    }

    private void ShowStatus()
    {
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.SetActiveUi(true);
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.SetActiveUi(false);
    }

    private void StartNextBattle(List<ActorInfo> battleMembers)
    {
        _isBattle = true;
        _model.SetBattleMembers(battleMembers);
        _view.StartResultAnimation(battleMembers);
    }

    private void EndStrategy()
    {
        _model.EndStrategy();
        _view.CommandSceneChange(Scene.Tactics);
    }
}
