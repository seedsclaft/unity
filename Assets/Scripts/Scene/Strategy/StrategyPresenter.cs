using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Strategy;

public class StrategyPresenter : BasePresenter
{
    StrategyModel _model = null;
    StrategyView _view = null;

    private bool _busy = true;


    private StrategyState _strategyState = StrategyState.None;
    public StrategyPresenter(StrategyView view)
    {
        _view = view;
        SetView(_view);
        _model = new StrategyModel();
        SetModel(_model);
        Initialize();
    }

    private async void Initialize()
    {
        _busy = true;
        _model.LoadActorResources();
        _model.LoadEnemyResources();
        _view.SetHelpWindow();
        _view.SetEnemyList(_model.ResultCommand());

        _view.SetActors(_model.StageMembers());
        _view.SetResultList(_model.ResultCommand());
        var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => UpdateCommand(type));

        CommandStartStrategy();
        _busy = false;
    }

    private void UpdateCommand(StrategyViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        Debug.Log(viewEvent.commandType);
        if (viewEvent.commandType == CommandType.StartStrategy)
        {
            CommandStartStrategy();
        }
        if (viewEvent.commandType == CommandType.EndAnimation)
        {
            CommandEndAnimation();
        }
        if (viewEvent.commandType == CommandType.CallEnemyInfo)
        {
            CommandCallEnemyInfo();
        }
        if (viewEvent.commandType == CommandType.PopupSkillInfo)
        {
            CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.ResultClose)
        {
            CommandResultClose((ConfirmCommandType)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.BattleClose)
        {
            CommandBattleClose((ConfirmCommandType)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.EndLvUpAnimation)
        {
            CommandEndLvUpAnimation();
        }
        if (viewEvent.commandType == CommandType.LvUpNext)
        {
            CommandLvUpNext();
        }
        if (viewEvent.commandType == CommandType.ChangeSkipToggle)
        {
            CommandChangeSkipToggle((bool)viewEvent.template);
        }
    }

    private void UpdatePopupSkillInfo(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
    }

    private void UpdatePopupLost(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        _model.LostActors(_model.LostMembers());
    }

    private void CommandStartStrategy(){
        var battledResultActors = _model.BattleResultActors();
        if (battledResultActors.Count > 0)
        {
            _strategyState = StrategyState.BattleResult;
            SeekStrategyState();
        } else
        {
            CheckTacticsActors();
        }
    }

    private void CheckTacticsActors()
    {
        var tacticsActors = _model.TacticsActors();
        if (tacticsActors.Count > 0)
        {
            _strategyState = StrategyState.TacticsResult;
            SeekStrategyState();
        } else{
            CheckNextBattle();
        }
    }

    private void CheckNextBattle()
    {
        var battleMembers = _model.CheckNextBattleActors();
        if (battleMembers != null && battleMembers.Count > 0)
        {
            _strategyState = StrategyState.InBattle;
            SeekStrategyState();
        } else{
            EndStrategy();
        }
    }

    private void SeekStrategyState()
    {
        if (_strategyState == StrategyState.BattleResult)
        {
            var battledResultActors = _model.BattleResultActors();
            _model.MakeResult();
            var bonusList = new List<bool>();
            foreach (var item in battledResultActors)
            {
                bonusList.Add(false);
            }
            _view.SetTitle(DataSystem.System.GetTextData(14030).Text);
            _view.StartResultAnimation(_model.CastActorInfos(battledResultActors),bonusList);
        } else
        if (_strategyState == StrategyState.TacticsResult)
        {
            var tacticsActors = _model.TacticsActors();
            _model.SetLvUp();
            _model.MakeResult();
            var bonusList = new List<bool>();
            foreach (var item in tacticsActors)
            {
                bonusList.Add(_model.IsBonusTactics(item.ActorId));
            }
            _view.SetTitle(DataSystem.System.GetTextData(14020).Text);
            _view.StartResultAnimation(_model.CastActorInfos(tacticsActors),bonusList);
            if (_model.LevelUpData.Count > 0)
            {
                _view.StartLvUpAnimation();
            }
        }
        if (_strategyState == StrategyState.InBattle)
        {
            var battleMembers = _model.CheckNextBattleActors();
            _view.HideResultList();
            _model.SetBattleMembers(battleMembers);
            var bonusList = new List<bool>();
            foreach (var item in battleMembers)
            {
                bonusList.Add(false);
            }
            _view.SetTitle(DataSystem.System.GetTextData(4).Text);
            _view.StartResultAnimation(_model.CastActorInfos(battleMembers),bonusList);
        }
    }


    private void CommandEndAnimation()
    {
        if (_strategyState == StrategyState.BattleResult)
        {
            var getItemInfos = _model.SetBattleResult();
            _view.ShowResultList(getItemInfos);
            // ロスト判定
            var lostMembers = _model.LostMembers();
            if (lostMembers.Count > 0)
            {
                var text = DataSystem.System.GetTextData(3060).Text;
                var lostMembersText = "";
                for (int i = 0;i < lostMembers.Count;i++)
                {
                    lostMembersText += lostMembers[i].Master.Name;
                    if (i != lostMembers.Count-1)
                    {
                        lostMembersText += ",";
                    }
                }
                text = text.Replace("\\d",lostMembersText);
                var popupInfo = new ConfirmInfo(text,(a) => UpdatePopupLost((ConfirmCommandType)a));
                popupInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(popupInfo);
            }
        }
        if (_strategyState == StrategyState.TacticsResult)
        {
            if (_model.LevelUpData.Count == 0)
            {
                _view.ShowResultList(_model.ResultGetItemInfos);
            }
        }
        if (_strategyState == StrategyState.InBattle)
        {
            _view.ShowEnemyList(_model.TroopInfoListDates(_model.CurrentTroopInfo()),_model.EnableBattleSkip());
            SetHelpInputSkipEnable();
        }
        var stageEvents = _model.StageEvents(EventTiming.StartStrategy);
        if (stageEvents.Count > 0)
        {
            for (int i = 0;i < stageEvents.Count;i++)
            {
                if (stageEvents[i].Type == StageEventType.CommandDisable)
                {
                    _model.AddEventReadFlag(stageEvents[i]);
                }
                if (stageEvents[i].Type == StageEventType.NeedUseSp)
                {
                    _model.SetNeedUseSpCommand(true);
                    _model.AddEventReadFlag(stageEvents[i]);
                }
                if (stageEvents[i].Type == StageEventType.AdvStart)
                {
                    AdvCallInfo advInfo = new AdvCallInfo();
                    advInfo.SetLabel(_model.GetAdvFile(stageEvents[i].Param));
                    advInfo.SetCallEvent(() => {   
                        _busy = false;
                    });
                    _view.CommandCallAdv(advInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    _busy = true;
                    break;
                }
            }
        }
    }

    private void CommandEndLvUpAnimation()
    {
        _view.ShowLvUpActor(_model.LevelUpData[0],_model.LevelUpActorStatus(0));
    }

    private void CommandLvUpNext()
    {
        _model.SetLevelUpStatus();
        if (_model.LevelUpData.Count > 0)
        {
            CommandEndLvUpAnimation();
        } else
        {
            _view.ShowResultList(_model.ResultGetItemInfos);
        }
    }

    private void CommandResultClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            if (_strategyState == StrategyState.BattleResult)
            {
                var battledMembers = _model.BattleResultActors();
                if (battledMembers != null && battledMembers.Count > 0)
                {
                    _model.ClearBattleData(battledMembers);
                }
            }
            CheckTacticsActors();
        } else
        {
            ShowStatus();
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandBattleClose(ConfirmCommandType confirmCommandType)
    {
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            BattleStart();
        } else{
            ShowStatus();
        }
    }

    private void BattleStart()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.SetHelpText("");
        _view.SetHelpInputInfo("");
        _view.CommandCallLoading();
        _view.CommandChangeViewToTransition(null);
        if (_model.BattleSkip)
        {
            _view.CommandSceneChange(Scene.FastBattle);
        } else
        {
            _view.CommandSceneChange(Scene.Battle);
        }
    }

    private void ShowStatus()
    {
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            if (_strategyState != StrategyState.InBattle){
                _view.SetHelpInputInfo("STRATEGY");
            } else{
                SetHelpInputSkipEnable();
            }
            _view.SetHelpText(DataSystem.System.GetTextData(14010).Text);
            _view.ChangeUIActive(true);
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.ChangeUIActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        var popupInfo = new ConfirmInfo("",(menuCommandInfo) => UpdatePopupSkillInfo((ConfirmCommandType)menuCommandInfo));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoice(true);
        _view.CommandCallSkillDetail(popupInfo);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandCallEnemyInfo()
    {
        var enemyIndex = _model.CurrentStage.CurrentBattleIndex;
        var enemyInfos = _model.TacticsTroops()[enemyIndex].BattlerInfos;
        
        var statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.ChangeUIActive(true);
            if (_strategyState != StrategyState.InBattle){
                _view.SetHelpInputInfo("STRATEGY");
            } else{
                SetHelpInputSkipEnable();
            }
            _view.SetHelpText(DataSystem.System.GetTextData(14010).Text);
        });
        statusViewInfo.SetEnemyInfos(enemyInfos,false);
        _view.CommandCallEnemyInfo(statusViewInfo);
        _view.ChangeUIActive(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);    
    }

    private void CommandChangeSkipToggle(bool needChangeView)
    {
        if (_model.EnableBattleSkip())
        {
            var check = _view.BattleSkipToggle.isOn;
            if (needChangeView)
            {
                check = !check;
            }
            _model.ChangeBattleSkip(check);
            if (needChangeView)
            {
                _view.CommandChangeSkipToggle(_model.BattleSkip);
            }
        }
    }

    private void SetHelpInputSkipEnable()
    {
        if (_model.EnableBattleSkip())
        {
            _view.SetHelpInputInfo("STRATEGY_BATTLE_SKIP");
        } else
        {
            _view.SetHelpInputInfo("STRATEGY_BATTLE");
        }
    }

    private void EndStrategy()
    {
        _model.EndStrategy();
        _view.EndShinyEffect();
        _view.CommandSceneChange(Scene.Tactics);
    }

    private enum StrategyState{
        None = 0,
        BattleResult = 1,
        TacticsResult = 2,
        InBattle = 3,
    }
}
