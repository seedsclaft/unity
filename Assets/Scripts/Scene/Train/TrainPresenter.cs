using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Train;

namespace Ryneus
{
    public class TrainPresenter : BasePresenter
    {
        TrainModel _model = null;
        TrainView _view = null;

        private bool _busy = true;

        private CommandType _backCommand = CommandType.None;
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
            _view.SetStatusEvent((type) => UpdateStatusCommand(type));
            _view.SetSelectCharacter(_model.TacticsCharacterData(),_model.NoChoiceConfirmCommand());
            _view.SetAttributeList(GetListData(_model.AttributeTabList()));
            _view.SetStatusButtonEvent(() => CommandStatus(_view.CharacterSelectIndex));
            CommandRefresh();
            _view.OpenAnimation();
            _view.ChangeUIActive(true);
        }

        private void UpdateCommand(TrainViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            _view.UpdateInputKeyActive(viewEvent,_model.TacticsCommandType);
            Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case CommandType.SelectTacticsCommand:
                    CommandSelectTacticsCommand((TacticsCommandType)viewEvent.template);
                    break;
                case CommandType.DecideTacticsCommand:
                    if (_model.TacticsCommandType == TacticsCommandType.Paradigm)
                    {
                        CommandDecideTacticsCommandEnemy((ConfirmCommandType)viewEvent.template);
                    } else
                    {
                        CommandCancelTacticsCommand();
                    }
                    break;
                case CommandType.SelectTacticsActor:
                    CommandSelectTacticsActor((int)viewEvent.template);
                    break;
                case CommandType.SelectFrontBattleIndex:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSelectFrontBattleIndex((int)viewEvent.template);
                    break;
                case CommandType.SelectBackBattleIndex:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSelectBackBattleIndex((int)viewEvent.template);
                    break;
                case CommandType.SelectSkillTrigger:
                    if (_model.CurrentStageTutorialDates.Count > 0) return;
                    CommandSelectSkillTrigger((int)viewEvent.template);
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
                case CommandType.ChangeSelectTacticsActor:
                    CommandChangeSelectTacticsActor((int)viewEvent.template);
                    break;
                case CommandType.SelectAttribute:
                    CommandSelectAttribute((int)viewEvent.template);
                    break;
                case CommandType.BattleReplay:
                    CommandBattleReplay();
                    break;
                case CommandType.ShowTacticsCharacter:
                    _view.ShowSelectCharacter(_model.TacticsBattleCharacterData(_view.CharacterSelectIndex),_model.TacticsCommandData());
                    break;
                case CommandType.ActorLearnMagic:
                    if (_model.TacticsCommandType == TacticsCommandType.Alchemy)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Decide);
                        // 魔法習得
                        CommandActorLearnMagic((SkillInfo)viewEvent.template);  
                    }
                    break;
                case CommandType.CommandHelp:
                    CommandCommandHelp();
                    break;
                case CommandType.EnemyInfo:
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    CommandEnemyInfo();
                    break;
            }
        }

        private void UpdateStatusCommand(StatusViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case Status.CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case Status.CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case Status.CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)viewEvent.template);
                    return;
                case Status.CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
            }
        }

        private void CommandBack()
        {
            if (_backCommand != CommandType.None)
            {
                var eventData = new TrainViewEvent(_backCommand);
                eventData.template = _model.TacticsCommandType;
                UpdateCommand(eventData);
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _backCommand = CommandType.None;
            }
        }

        private void CommandSelectTacticsCommand(TacticsCommandType tacticsCommandType)
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
                    _view.ChangeBackCommandActive(true);
                    _view.ShowSelectCharacter(_model.TacticsCharacterData(_view.CharacterSelectIndex),_model.TacticsCommandData());
                    if (tacticsCommandType == TacticsCommandType.Alchemy)
                    {                    
                        ShowLearningSkillInfos();
                    } else
                    {
                        ShowCharacterDetail();
                    }
                    _view.ShowBattleReplay(false);
                    _backCommand = CommandType.DecideTacticsCommand;
                    break;
                case TacticsCommandType.Status:
                    CommandStatus();
                    break;
            }
        }

        private void CommandStatus(int startIndex = -1)
        {
            int actorId = -1;
            if (startIndex != -1)
            {
                // actorIdに変換
                var actor = _model.TacticsActor();
                if (actor != null)
                {
                    actorId = _model.TacticsActor().ActorId;
                }
            }

            CommandStatusInfo(_model.PastActorInfos(),false,true,true,false,actorId,() => 
            {
                _view.SetHelpInputInfo("TACTICS");
                _view.SetNuminous(_model.Currency);
                _view.SetBusy(false);
                _view.CallChangeSelectTacticsActor();
            });
        }

        private void CommandSelectActorTrain()
        {
            _busy = true;
            _view.SetBusy(true);
            CommandActorLevelUp(_model.TacticsActor(),() => 
            {
                _busy = false;
                _view.SetBusy(false);
                CommandSelectTacticsCommand(_model.TacticsCommandType);
                ShowCharacterDetail();
                CommandRefresh();
            });
        }

        private void CommandCancelTacticsCommand()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.ChangeBackCommandActive(false);
            _view.HideSelectCharacter();
        }

        private void CommandSelectTacticsActor(int actorId)
        {
            _model.SetSelectActorId(actorId);
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Train:
                CommandSelectActorTrain();
                break;
                case TacticsCommandType.Alchemy:
                _view.CommandSelectActorAlchemy();
                _backCommand = CommandType.SelectTacticsCommand;
                break;
                case TacticsCommandType.Paradigm:
                CommandSelectActorParadigm();
                break;
            }
        }

        private void CommandChangeSelectTacticsActor(int actorId)
        {
            _model.SetSelectActorId(actorId);
            _view.SetLvUpCost(_model.ActorLevelUpCost(_model.TacticsActor()));
            _view.SetToLvText(_model.TacticsActor().LinkedLevel());
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                case TacticsCommandType.Train:
                    ShowCharacterDetail();
                    break;
                case TacticsCommandType.Alchemy:
                    ShowLearningSkillInfos();
                    break;
            }
        }

        private void ShowCharacterDetail()
        {
            _view.ShowCharacterDetail(_model.TacticsActor(),_model.StageMembers(),_model.SkillActionListData(_model.TacticsActor()));  
        }

        private void CommandSelectAttribute(int attribute)
        {
            _model.SetSelectAttribute(attribute);
            ShowLearningSkillInfos();
        }

        private void CommandBattleReplay()
        {
            _busy = true;
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
            if (_model.EnableAddInBattle() == false)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetInBattle();
            var listData = _model.TacticsBattleCharacterData(_view.CharacterSelectIndex);
            _view.RefreshTacticsActor(listData);
            CommandChangeSelectTacticsActor(_model.SelectedActorIdBySelectIndex(listData,_view.CharacterSelectIndex));
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

        private void CommandSelectSkillTrigger(int actorId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var skillTriggerViewInfo = new SkillTriggerViewInfo(actorId,() => 
            {
                _busy = false;
                CommandRefresh();
            });
            _view.CommandCallSkillTrigger(skillTriggerViewInfo);
        }

        private void CommandActorLearnMagic(SkillInfo skillInfo)
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,skillInfo.LearningCost.ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var from = _model.SelectActorEvaluate();
                var skillInfo = _view.SelectMagic;
                _model.LearnMagic(skillInfo.Id);
                var to = _model.SelectActorEvaluate();

                var learnSkillInfo = new LearnSkillInfo(from,to,skillInfo);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        var backEvent = new TrainViewEvent(CommandType.SelectTacticsCommand)
                        {
                            template = _model.TacticsCommandType
                        };
                        UpdateCommand(backEvent);
                        CommandRefresh();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            } else
            {
                var backEvent = new TrainViewEvent(CommandType.SelectTacticsActor)
                {
                    template = _model.TacticsActor().ActorId
                };
                UpdateCommand(backEvent);
            }
        }

        private void CommandDecideTacticsCommandEnemy(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                if (_model.BattleMembers().Count > 0)
                {
                    _model.SaveTempBattleMembers();
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
                    var battleSceneInfo = new BattleSceneInfo
                    {
                        ActorInfos = _model.BattleMembers(),
                        EnemyInfos = _model.CurrentTroopInfo().BattlerInfos
                    };
                    _view.CommandSceneChange(Scene.Battle,battleSceneInfo);
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
            CommandCautionInfo(DataSystem.GetText(11160));
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

        private void CommandRefresh()
        {
            _view.SetNuminous(_model.Currency);
            _view.SetEvaluate(_model.PartyEvaluate(),_model.TroopEvaluate());
            _view.CommandRefresh();
            ShowCharacterDetail();
            _view.SetLvUpCost(_model.ActorLevelUpCost(_model.TacticsActor()));
            _view.SetToLvText(_model.TacticsActor().LinkedLevel());
        }

        private void CommandCommandHelp()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var helpKey = "";
            switch (_model.TacticsCommandType)
            {
                case TacticsCommandType.Paradigm:
                    helpKey = "Battle";
                    break;
                case TacticsCommandType.Train:
                    helpKey = "LevelUp";
                    break;
                case TacticsCommandType.Alchemy:
                    helpKey = "Alchemy";
                    break;
            }
            if (helpKey != "")
            {
                _view.CommandHelpList(DataSystem.HelpText(helpKey));
            }
        }

        private void CommandEnemyInfo()
        {
            var enemyInfos = _model.CurrentTroopInfo().BattlerInfos;
            _busy = true;
            CommandEnemyInfo(enemyInfos,false,() => {_busy = false;});
        }
        
        private void CommandLevelUp()
        {
            _busy = true;
            _view.SetBusy(true);
            CommandLevelUp(_model.TacticsActor(),() => 
            {
                _busy = false;
                _view.SetBusy(false);
                CommandRefresh();
            });
        }

        private void CommandShowLearnMagic()
        {
            _view.SetLearnMagicButtonActive(true);
            var lastSelectSkillId = -1;
            var lastSelectSkill = _view.SelectMagic;
            if (lastSelectSkill != null)
            {
                lastSelectSkillId = lastSelectSkill.Id;
            }
            _view.ShowLeaningList(_model.SelectActorLearningMagicList(lastSelectSkillId));            
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            CommandLearnMagic(_model.TacticsActor(),skillInfo,() => 
            {
                _view.SetNuminous(_model.Currency);
                _view.CommandRefresh();
                CommandShowLearnMagic();
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
        }

        private void CommandHideLearnMagic()
        {
            _view.SetLearnMagicButtonActive(false);
            CommandRefresh();
        }
    }
}