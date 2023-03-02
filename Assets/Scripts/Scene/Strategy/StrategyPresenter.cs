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
    private bool _isBattled = false;
    public StrategyPresenter(StrategyView view)
    {
        _view = view;
        _model = new StrategyModel();

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();

        var bgm = await _model.BgmData();
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetActors(_model.Actors());
        _view.SetEvent((type) => updateCommand(type));
        _busy = false;
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
            List<ActorInfo> battleMembers = _model.CheckNonBattleActors();
            if (battleMembers != null && battleMembers.Count > 0)
            {
                _isBattle = true;
                _view.StartResultAnimation(battleMembers);
            } else{
                List<ActorInfo> battledMembers = _model.CheckInBattleActors();
                if (battledMembers != null && battledMembers.Count > 0)
                {
                    _isBattled = true;
                    _model.ClearBattleData(battledMembers);
                    _view.StartResultAnimation(battledMembers);
                }
            }
        }
    }

    private void CommandEndAnimation()
    {
        if (_isBattle == false){
            List<GetItemInfo> getItemInfos = _model.SetResult();
            _view.ShowResultList(getItemInfos,_model.ResultCommand());
        } else{
            if (_isBattled == false)
            {
                _view.ShowEnemyList(_model.EnemyInfo(),_model.ResultCommand());
            } else{        
                List<GetItemInfo> getItemInfos = _model.SetBattleResult();
                _view.ShowResultList(getItemInfos,_model.ResultCommand());
            }
        }
    }

    private void CommandResultClose(ConfirmComandType confirmComandType)
    {
        if (_isBattled == false)
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandStatusClose();
                });
                statusViewInfo.SetDisplayDecideButton(false);
                _view.CommandCallStatus(statusViewInfo);
            } else{
                List<ActorInfo> battleMembers = _model.CheckNonBattleActors();
                if (battleMembers != null && battleMembers.Count > 0)
                {
                    _isBattle = true;
                    _view.StartResultAnimation(battleMembers);
                } else{
                    _view.CommandSceneChange(Scene.Tactics);
                }
            }
        } else
        {
            if (confirmComandType == ConfirmComandType.Yes)
            {
                StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandStatusClose();
                });
                statusViewInfo.SetDisplayDecideButton(false);
                _view.CommandCallStatus(statusViewInfo);
            } else{
                List<ActorInfo> battleMembers = _model.CheckNonBattleActors();
                if (battleMembers != null && battleMembers.Count > 0)
                {
                    _isBattle = true;
                    _view.StartResultAnimation(battleMembers);
                } else
                {                
                    _view.CommandSceneChange(Scene.Tactics);
                }
            }
        }
    }

    private void CommandBattleClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
            });
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
        } else{
            List<ActorInfo> battleMembers = _model.CheckNonBattleActors();
            _model.SetBattleData(battleMembers);
            if (battleMembers != null && battleMembers.Count > 0)
            {
                _view.CommandSceneChange(Scene.Battle);
            }
        }
    }
}
