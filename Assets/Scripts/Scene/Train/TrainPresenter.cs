using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TrainPresenter :BasePresenter
    {
        TrainModel _model = null;
        TrainView _view = null;

        private bool _busy = true;

        private Train.CommandType _backCommand = Train.CommandType.None;
        public TrainPresenter(TrainView view)
        {
            _view = view;
            SetView(_view);
            _model = new TrainModel();
            SetModel(_model);

            Initialize();
        }

        private void Initialize()
        {
            InitializeView();
            var isAbort = CheckAdvStageEvent(EventTiming.StartTactics,() => {
                _view.CommandGotoSceneChange(Scene.Tactics);
            },_model.CurrentStage.SelectActorIdsClassId(0));
            if (isAbort)
            {
                return;
            }
            _busy = false;
        }

        private void InitializeView()
        {
            _view.ChangeUIActive(false);
            _view.SetHelpWindow();
            _view.ChangeBackCommandActive(false);
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetSelectCharacter(_model.TacticsCharacterData(),_model.NoChoiceConfirmCommand());
            _view.SetAttributeList(_model.AttributeTabList());
            CommandRefresh();
            _view.ChangeUIActive(true);
        }

        private void UpdateCommand(TrainViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case Train.CommandType.TacticsCommand:
                    CommandTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
                case Train.CommandType.TacticsCommandClose:
                    if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
                    {
                        CommandSelectEnemyClose((ConfirmCommandType)viewEvent.template);
                    } else
                    {
                        CommandTacticsCommandClose((ConfirmCommandType)viewEvent.template);
                    }
                    break;
                case Train.CommandType.SelectTacticsActor:
                    CommandSelectTacticsActor((TacticsActorInfo)viewEvent.template);
                    break;
                case Train.CommandType.SelectFrontBattleIndex:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSelectFrontBattleIndex((int)viewEvent.template);
                    break;
                case Train.CommandType.SelectBackBattleIndex:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSelectBackBattleIndex((int)viewEvent.template);
                    break;
                case Train.CommandType.SkillTrigger:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSkillTrigger((int)viewEvent.template);
                    break;
                case Train.CommandType.SymbolClose:
                    CommandSymbolClose();
                    break;
                case Train.CommandType.CallEnemyInfo:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandCallEnemyInfo((SymbolInfo)viewEvent.template);
                    break;
                case Train.CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                    break;
                case Train.CommandType.SelectRecord:
                    break;
                case Train.CommandType.CancelSymbolRecord:
                    CommandCancelSymbolRecord();
                    break;
                case Train.CommandType.Back:
                    CommandBack();
                    break;
                case Train.CommandType.ChangeSelectTacticsActor:
                    CommandChangeSelectTacticsActor((int)viewEvent.template);
                    break;
                case Train.CommandType.SelectAttribute:
                    CommandSelectAttribute((int)viewEvent.template);
                    break;
                case Train.CommandType.BattleReplay:
                    CommandBattleReplay();
                    break;
            }
            if (viewEvent.commandType == Train.CommandType.SkillAlchemy)
            {
                if (_model.TacticsCommandType == TacticsCommandType.Alchemy)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    // 魔法習得
                    CommandLearnSkill((SkillInfo)viewEvent.template);  
                }
            }
            if (viewEvent.commandType == Train.CommandType.SelectSideMenu)
            {
            }
            if (viewEvent.commandType == Train.CommandType.StageHelp)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandStageHelp();
            }
            if (viewEvent.commandType == Train.CommandType.CommandHelp)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandCommandHelp();
            }
            
            if (viewEvent.commandType == Train.CommandType.AlcanaCheck)
            {
                CommandAlcanaCheck();
            }
        }

        private void UpdatePopupSkillInfo()
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }


        private void CommandCancelSymbolRecord()
        {
            _view.ChangeBackCommandActive(false);
            _backCommand = Train.CommandType.None;
        }

        private void CommandBack()
        {
            var eventData = new TrainViewEvent(_backCommand);
            eventData.template = _model.TacticsCommandType;
            UpdateCommand(eventData);
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandTacticsCommand(TacticsCommandType tacticsCommandType)
        {
            _model.SetTacticsCommandType(tacticsCommandType);
            _view.SetHelpInputInfo(_model.TacticsCommandInputInfo());
            switch (tacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                    return;
                case TacticsCommandType.Train:
                case TacticsCommandType.Alchemy:
                    _view.HideConfirmCommand();
                    _view.ShowSelectCharacter(_model.TacticsCharacterData(),_model.TacticsCommandData());
                    _view.ActivateTacticsCommand();
                    _view.ChangeBackCommandActive(true);
                    if (tacticsCommandType == TacticsCommandType.Alchemy)
                    {                    
                        ShowLearningSkillInfos();
                    } else
                    {
                        _view.ShowCharacterDetail(_model.StageMembers()[0],_model.StageMembers(),true);
                    }
                    _backCommand = Train.CommandType.TacticsCommandClose;
                    break;
                case TacticsCommandType.Status:
                    CommandStatus();
                    break;
            }
        }

        private void CommandStatus()
        {
            _model.SetStatusActorInfos();
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                _view.SetHelpInputInfo("TACTICS");
                _view.SetNuminous(_model.Currency);
            });
            _view.ChangeUIActive(false);
            statusViewInfo.SetDisplayDecideButton(false);
            statusViewInfo.SetDisplayLevelResetButton(true);
            _view.CommandCallStatus(statusViewInfo);
        }


        private void CommandSelectActorTrain()
        {
            if (_model.CheckActorTrain())
            {
                SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
                // 新規魔法取得があるか
                var skills = _model.TacticsActor().LearningSkills(1);
                
                var from = _model.SelectActorEvaluate();
                _model.SelectActorTrain();
                var to = _model.SelectActorEvaluate();
                
                if (skills.Count > 0)
                {
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    CommandTacticsCommand(_model.TacticsCommandType);
                    SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
                    
                    var popupInfo = new PopupInfo();
                    popupInfo.PopupType = PopupType.LearnSkill;
                    popupInfo.EndEvent = () => 
                    {
                        UpdatePopupSkillInfo();
                        _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                        CommandRefresh();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    };
                    popupInfo.template = learnSkillInfo;
                    _view.CommandCallPopup(popupInfo);
                } else
                {
                    var cautionInfo = new CautionInfo();
                    cautionInfo.SetLevelUp(from,to);
                    _view.CommandCallCaution(cautionInfo);

                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.CountUp);
                }
            } else
            {
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.System.GetTextData(11170).Text);
                _view.CommandCallCaution(cautionInfo);
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
            }
        }

        private void CommandTacticsCommandClose(ConfirmCommandType confirmCommandType)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.ChangeBackCommandActive(false);
            _view.HideSelectCharacter();
        }

        private void CommandSelectTacticsActor(TacticsActorInfo tacticsActorInfo)
        {
            var actorId = tacticsActorInfo.ActorInfo.ActorId;
            _model.SetSelectActorId(actorId);
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Train)
            {
                CommandSelectActorTrain();
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Alchemy)
            {
            } else
            if (tacticsActorInfo.TacticsCommandType == TacticsCommandType.Paradigm)
            {
                CommandSelectActorParadigm();
            }
        }

        private void CommandChangeSelectTacticsActor(int actorId)
        {
            _model.SetSelectActorId(actorId);
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                case TacticsCommandType.Train:
                    _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers());
                    break;
                case TacticsCommandType.Alchemy:
                    ShowLearningSkillInfos();
                    break;
            }
        }

        private void CommandSelectAttribute(int attribute)
        {
            _model.SetSelectAttribute(attribute);
            ShowLearningSkillInfos();
        }

        private void CommandBattleReplay()
        {
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.ClearParty,
                EndEvent = () =>
                {
                    _busy = false;
                    //_view.SetHelpInputInfo("OPTION");
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
            return;
        }

        private void CommandSelectActorParadigm()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetInBattle();
            CommandRefresh();
        }

        private void CommandSelectFrontBattleIndex(int actorId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeBattleLineIndex(actorId,true);
            CommandRefresh();
        }

        private void CommandSelectBackBattleIndex(int actorId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeBattleLineIndex(actorId,false);
            CommandRefresh();
        }

        private void CommandSkillTrigger(int actorId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var skillTriggerViewInfo = new SkillTriggerViewInfo(actorId,() => {
                CommandRefresh();
            });
            _view.CommandCallSkillTrigger(skillTriggerViewInfo);
        }

        private void CommandLearnSkill(SkillInfo skillInfo)
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,skillInfo.LearningCost.ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var from = _model.SelectActorEvaluate();
                var skillInfo = _view.SelectMagic;
                _model.LearnMagic(skillInfo.Id);
                var to = _model.SelectActorEvaluate();

                var learnSkillInfo = new LearnSkillInfo(from,to,skillInfo);
                CommandTacticsCommand(_model.TacticsCommandType);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);
                
                var popupInfo = new PopupInfo();
                popupInfo.PopupType = PopupType.LearnSkill;
                popupInfo.EndEvent = () => 
                {
                    UpdatePopupSkillInfo();
                    CommandRefresh();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                };
                popupInfo.template = learnSkillInfo;
                _view.CommandCallPopup(popupInfo);
            }
        }

        private void CommandSelectEnemyClose(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                if (_model.BattleMembers().Count > 0)
                {
                    _model.SaveTempBattleMembers();
                    _model.SetStatusActorInfos();
                    _view.CommandChangeViewToTransition(null);
                    // ボス戦なら
                    if (_model.CurrentSelectRecord().SymbolType == SymbolType.Boss)
                    {
                        //SoundManager.Instance.FadeOutBgm();
                        PlayBossBgm();
                    } else
                    {
                        var bgmData = DataSystem.Data.GetBGM(_model.TacticsBgmKey());
                        if (bgmData.CrossFade != "" && SoundManager.Instance.CrossFadeMode)
                        {
                            SoundManager.Instance.ChangeCrossFade();
                        } else
                        {
                            PlayTacticsBgm();
                        }
                    }
                    _model.SetPartyBattlerIdList();
                    SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                    _view.CommandSceneChange(Scene.Battle);
                } else
                {
                    CheckBattleMember();
                }
            } else
            {
                _view.HideSelectCharacter();
            }
        }

        private void CheckBattleMember()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Deny);
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(DataSystem.GetText(11160));
            _view.CommandCallCaution(cautionInfo);
        }

        private void ShowLearningSkillInfos()
        {
            var lastSelectSkillId = -1;
            var lastSelectSkill = _view.SelectMagic;
            if (lastSelectSkill != null)
            {
                lastSelectSkillId = lastSelectSkill.Id;
            }
            _view.ShowLeaningList(_model.SelectActorLearningMagicList(lastSelectSkillId));            
        }

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandSymbolClose()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandRefresh()
        {
            _view.SetNuminous(_model.Currency);
            _view.SetEvaluate(_model.PartyEvaluate(),_model.TroopEvaluate());
            _view.CommandRefresh();
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
                    statusViewInfo.SetDisplayCharacterList(false);
                    _view.CommandCallStatus(statusViewInfo);
                    _view.ChangeUIActive(false);
                    break;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }



        private void CommandStageHelp()
        {
            _view.CommandHelpList(DataSystem.HelpText("Tactics"));
        }

        private void CommandCommandHelp()
        {
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                _view.CommandHelpList(DataSystem.HelpText("Battle"));
                return;
                case TacticsCommandType.Train:
                _view.CommandHelpList(DataSystem.HelpText("LevelUp"));
                return;
                case TacticsCommandType.Alchemy:
                _view.CommandHelpList(DataSystem.HelpText("Alchemy"));
                return;
                /*
                case TacticsCommandType.Recovery:
                _view.CommandHelpList(DataSystem.HelpText("Recovery"));
                return;
                */
            }
            
        }

        private void CommandAlcanaCheck()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new PopupInfo();
            popupInfo.PopupType = PopupType.AlcanaList;
            popupInfo.EndEvent = () => {
                _view.ChangeUIActive(true);
                _busy = false;
            };
            _view.CommandCallPopup(popupInfo);
            _busy = true;
            _view.ChangeUIActive(false);
        }
    }
}