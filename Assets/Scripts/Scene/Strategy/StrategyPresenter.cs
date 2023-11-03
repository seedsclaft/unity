using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Strategy;

public class StrategyPresenter : BasePresenter
{
    StrategyModel _model = null;
    StrategyView _view = null;

    private bool _busy = true;

    private bool _isBattle = false;
    private bool _isBattleEnded = false;
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
        _view.SetUiView();
        _view.SetEnemyList(_model.ResultCommand());

        _view.SetActors(_model.StageMembers());
        _view.SetResultList(_model.ResultCommand());
        var bgm = await _model.GetBgmData(_model.TacticsBgmFilename());
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.SetEvent((type) => updateCommand(type));

        CommandStartStretegy();
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
        if (viewEvent.commandType == CommandType.CallEnemyInfo)
        {
            CommandCallEnemyInfo();
        }
        if (viewEvent.commandType == CommandType.PopupSkillInfo)
        {
            CommandPopupSkillInfo((GetItemInfo)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.ResultClose)
        {
            CommandResultClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.BattleClose)
        {
            CommandBattleClose((ConfirmComandType)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.EndLvupAnimation)
        {
            CommandEndLvupAnimation();
        }
        if (viewEvent.commandType == CommandType.LvUpNext)
        {
            CommandLvUpNext();
        }
        if (viewEvent.commandType == CommandType.ChangeSkipToggle)
        {
            CommandChangeSkipToggle((bool)viewEvent.templete);
        }
    }

    private void UpdatePopupSkillInfo(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
        }
        _model.LostActors(_model.LostMembers());
    }

    private void CommandStartStretegy(){
        List<ActorInfo> battledMembers = _model.CheckInBattleActors();
        if (battledMembers != null && battledMembers.Count > 0)
        {
            _model.SetResult();
            var bonusList = new List<bool>();
            foreach (var item in battledMembers)
            {
                bonusList.Add(false);
            }
            _isBattleEnded = true;
            _view.SetTitle(DataSystem.System.GetTextData(14030).Text);
            _view.StartResultAnimation(battledMembers,bonusList);
        } else
        {
            CheckTacticsActors();
        }
    }

    private void CommandEndAnimation()
    {
        if (_isBattleEnded == true)
        {
            List<GetItemInfo> getItemInfos = _model.SetBattleResult();
            _view.ShowResultList(getItemInfos);
            // ロスト判定
            List<ActorInfo> lostMembers = _model.LostMembers();
            if (lostMembers.Count > 0)
            {
                string text = DataSystem.System.GetTextData(3060).Text;
                string lostMembersText = "";
                for (int i = 0;i < lostMembers.Count;i++)
                {
                    lostMembersText += lostMembers[i].Master.Name;
                    if (i != lostMembers.Count-1)
                    {
                        lostMembersText += ",";
                    }
                }
                text = text.Replace("\\d",lostMembersText);
                ConfirmInfo popupInfo = new ConfirmInfo(text,(a) => UpdatePopup((ConfirmComandType)a));
                popupInfo.SetIsNoChoise(true);
                _view.CommandCallConfirm(popupInfo);
            }
        } else 
        if (_isBattle == false){
            if (_model.LevelUpData.Count == 0)
            {
                _view.ShowResultList(_model.ResultGetItemInfos);
            }
        } else{
            _view.ShowEnemyList(_model.CurrentTroopInfo(),_model.EnableBattleSkip());
            SetHelpInputSkipEnable();
        }
        var stageEvents = _model.StageEvents(EventTiming.StartStarategy);
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

    private void CommandEndLvupAnimation()
    {
        _view.ShowLvUpActor(_model.LevelUpData[0]);
    }

    private void CommandLvUpNext()
    {
        _model.SetLevelUpStatus();
        if (_model.LevelUpData.Count > 0)
        {
            CommandEndLvupAnimation();
        } else
        {
            _view.ShowResultList(_model.ResultGetItemInfos);
        }
    }

    private void CommandResultClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            ShowStatus();
        } else
        {
            if (_isBattleEnded == true)
            {
                List<ActorInfo> battledMembers = _model.CheckInBattleActors();
                if (battledMembers != null && battledMembers.Count > 0)
                {
                    _model.ClearBattleData(battledMembers);
                }
            }
            _isBattleEnded = false;
            CheckTacticsActors();
        }
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandBattleClose(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            ShowStatus();
        } else{
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
    }

    private void ShowStatus()
    {
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            if (_isBattle == false){
                _view.SetHelpInputInfo("STRATEGY");
            } else{
                SetHelpInputSkipEnable();
            }
            _view.SetHelpText(DataSystem.System.GetTextData(14010).Text);
            _view.SetActiveUi(true);
        });
        statusViewInfo.SetDisplayDecideButton(false);
        _view.CommandCallStatus(statusViewInfo);
        _view.SetActiveUi(false);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CheckTacticsActors()
    {
        List<ActorInfo> tacticsActors = _model.TacticsActors();
        if (tacticsActors.Count > 0)
        {
            _model.SetLvup();
            _model.SetResult();
            var bonusList = new List<bool>();
            foreach (var item in tacticsActors)
            {
                bonusList.Add(_model.IsBonusTactics(item.ActorId));
            }
            _view.SetTitle(DataSystem.System.GetTextData(14020).Text);
            _view.StartResultAnimation(tacticsActors,bonusList);
            if (_model.LevelUpData.Count > 0)
            {
                _view.StartLvUpAnimation();
            }
        } else{
            CheckNextBattle();
        }
    }

    private void CheckNextBattle()
    {
        List<ActorInfo> battleMembers = _model.CheckNextBattleActors();
        if (battleMembers != null && battleMembers.Count > 0)
        {
            StartNextBattle(battleMembers);
        } else{
            EndStrategy();
        }
    }

    private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
    {
        ConfirmInfo popupInfo = new ConfirmInfo("",(menuCommandInfo) => UpdatePopupSkillInfo((ConfirmComandType)menuCommandInfo));
        popupInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
        popupInfo.SetIsNoChoise(true);
        _view.CommandCallConfirm(popupInfo);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandCallEnemyInfo()
    {
        var enemyIndex = _model.CurrentStage.CurrentBattleIndex;
        List<BattlerInfo> enemyInfos = _model.TacticsTroops()[enemyIndex].BattlerInfos;
        
        StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
            _view.CommandStatusClose();
            _view.SetActiveUi(true);
            if (_isBattle == false){
                _view.SetHelpInputInfo("STRATEGY");
            } else{
                SetHelpInputSkipEnable();
            }
            _view.SetHelpText(DataSystem.System.GetTextData(14010).Text);
        });
        statusViewInfo.SetEnemyInfos(enemyInfos,false);
        _view.CommandCallEnemyInfo(statusViewInfo);
        _view.SetActiveUi(false);
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

    private void StartNextBattle(List<ActorInfo> battleMembers)
    {
        _isBattle = true;
        _view.HideResultList();
        _model.SetBattleMembers(battleMembers);
        _view.SetTitle(DataSystem.System.GetTextData(4).Text);
        var bonusList = new List<bool>();
        foreach (var item in battleMembers)
        {
            bonusList.Add(false);
        }
        _view.StartResultAnimation(battleMembers,bonusList);
    }

    private void EndStrategy()
    {
        _model.EndStrategy();
        _view.EndShinyEffect();
        _view.CommandSceneChange(Scene.Tactics);
    }
}
