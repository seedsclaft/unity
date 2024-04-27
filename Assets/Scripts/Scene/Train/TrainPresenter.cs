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
            
            CommandRefresh();
            _view.ChangeUIActive(true);
            _view.StartAnimation();
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
                    if (_model.TacticsCommandType != TacticsCommandType.Paradigm)
                    {
                        CommandTacticsCommandClose();
                    }
                    break;
                case Train.CommandType.SelectTacticsActor:
                    CommandSelectTacticsActor((TacticsActorInfo)viewEvent.template);
                    break;
                case Train.CommandType.PopupSkillInfo:
                    CommandPopupSkillInfo((GetItemInfo)viewEvent.template);
                    break;
                case Train.CommandType.Back:
                    CommandBack();
                    break;
                case Train.CommandType.ChangeSelectTacticsActor:
                    CommandChangeSelectTacticsActor((int)viewEvent.template);
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
                        _view.ShowLeaningList(_model.SelectActorLearningMagicList());
                    } else
                    {
                        _view.ShowCharacterDetail(_model.StageMembers()[0],_model.StageMembers());
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

        private void CommandTacticsCommandClose()
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
                    _view.ShowLeaningList(_model.SelectActorLearningMagicList());
                    break;
            }
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

        private void CommandPopupSkillInfo(GetItemInfo getItemInfo)
        {
            var confirmInfo = new ConfirmInfo("",(a) => UpdatePopupSkillInfo(),ConfirmType.SkillDetail);
            confirmInfo.SetSkillInfo(_model.BasicSkillInfos(getItemInfo));
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallSkillDetail(confirmInfo);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandRefresh()
        {
            _view.SetNuminous(_model.Currency);
            _view.SetEvaluate(_model.PartyEvaluate(),_model.TroopEvaluate());
            _view.CommandRefresh();
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