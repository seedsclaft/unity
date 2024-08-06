using System.Collections.Generic;

namespace Ryneus
{
    public class StatusPresenter : BasePresenter
    {
        private StatusModel _model = null;
        private StatusView _view = null;
        private Status.CommandType _popupCommandType = Status.CommandType.None;
        private bool _busy = false;
        public StatusPresenter(StatusView view,List<ActorInfo> actorInfos)
        {
            _view = view;
            SetView(_view);
            _model = new StatusModel(actorInfos);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetHelpWindow(_model.HelpText());
            _view.SetUIButton(_model.StatusCommand());
            _view.SetEvent((type) => UpdateCommand(type));

            CommandRefresh();
            if (_model.ActorInfos.Count == 1) _view.HideArrows();
        }

        private void UpdateCommand(StatusViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case Status.CommandType.DecideActor:
                    CommandDecideActor();
                    return;
                case Status.CommandType.LeftActor:
                    CommandLeftActor();
                    return;
                case Status.CommandType.RightActor:
                    CommandRightActor();
                    return;
                case Status.CommandType.Back:
                    CommandBack();
                    return;
                case Status.CommandType.CharacterList:
                    CommandCharacterList();
                    return;
                case Status.CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.template);
                    return;
                case Status.CommandType.LvReset:
                    CommandLvReset();
                    return;
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
                case Status.CommandType.SelectCommandList:
                    CommandSelectCommandList((SystemData.CommandData)viewEvent.template);
                    return;
            }
        }

        private void CommandBack()
        {
            _view.CommandBack();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandCharacterList()
        {
            SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var characterListInfo = new CharacterListInfo((a) => 
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.SelectActor(a);
                CommandRefresh();
                SetBusy(false);
            },
            () => 
            {
                CommandRefresh();
                SetBusy(false);
            });
            characterListInfo.SetActorInfos(_model.ActorInfos);
            _view.CommandCallCharacterList(characterListInfo);
        }

        public void CommandSelectCharacter(int actorId)
        {
            _model.SelectActor(actorId);
            CommandRefresh();
        }

        private void CommandLvReset()
        {
            SetBusy(true);
            var enable = _model.EnableLvReset();
            if (enable)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(14160,_model.CurrentActor.Master.Name),(a) => UpdatePopupActorLvReset(a));
                _view.CommandCallConfirm(confirmInfo);
                _popupCommandType = Status.CommandType.DecideStage;
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle(DataSystem.GetText(14150));
                _view.CommandCallCaution(cautionInfo);
                SetBusy(false);
            }
        }

        private void CommandLevelUp()
        {
            if (_model.CheckActorTrain())
            {
                SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
                // 新規魔法取得があるか
                var skills = _model.CurrentActor.LearningSkills(1);
                
                var from = _model.SelectActorEvaluate();
                _model.CommandLevelUp();
                var to = _model.SelectActorEvaluate();
                
                if (skills.Count > 0)
                {
                    _busy = true;
                    _view.SetBusy(true);
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                    var popupInfo = new PopupInfo
                    {
                        PopupType = PopupType.LearnSkill,
                        EndEvent = () =>
                        {
                            _busy = false;
                            _view.SetBusy(false);
                            CommandRefresh();
                            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                        },
                        template = learnSkillInfo
                    };
                    _view.CommandCallPopup(popupInfo);
                } else
                {
                    var cautionInfo = new CautionInfo();
                    cautionInfo.SetLevelUp(from,to);
                    _view.CommandCallCaution(cautionInfo);

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
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new ConfirmInfo(DataSystem.GetReplaceText(11150,skillInfo.LearningCost.ToString()) + DataSystem.GetReplaceText(11151,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill(a,skillInfo));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType,SkillInfo skillInfo)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var from = _model.SelectActorEvaluate();
                _model.LearnMagic(skillInfo.Id);
                var to = _model.SelectActorEvaluate();

                var learnSkillInfo = new LearnSkillInfo(from,to,skillInfo);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        _view.SetNuminous(_model.Currency);
                        _view.CommandRefresh();
                        CommandShowLearnMagic();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            } else
            {
            }
        }

        private void CommandHideLearnMagic()
        {
            _view.SetLearnMagicButtonActive(false);
            CommandRefresh();
        }

        private void UpdatePopupActorLvReset(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currency = _model.ActorLvReset();
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(14170,currency.ToString()),(a) => 
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    CommandRefresh();
                    _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                    SetBusy(false);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
            } else
            {
                SetBusy(false);
            }
        }

        private void CommandSelectCommandList(SystemData.CommandData commandData)
        {
            if (commandData.Key == "LV_RESET")
            {
                CommandLvReset();
            } else
            if (commandData.Key == "SKILL_TRIGGER")
            {
                CommandSelectSkillTrigger(_model.CurrentActor.ActorId);
            }
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

        private void CommandDecideActor()
        {
            SetBusy(true);
            var actorInfo = _model.CurrentActor;
            var text = DataSystem.GetReplaceText(14180,actorInfo.Master.Name);
            var confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => UpdatePopup(menuCommandInfo));
            _view.CommandCallConfirm(confirmInfo);
            _popupCommandType = Status.CommandType.DecideStage;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);

            if (_popupCommandType == Status.CommandType.SelectSkillAction)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    CommandRefresh();
                }
                _view.ActivateSkillActionList();
            }


            if (_popupCommandType == Status.CommandType.DecideStage)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    _model.SelectAddActor();
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);

                    var makeSelectActorInfos = _model.MakeSelectActorInfos();
                    var makeSelectGetItemInfos = _model.MakeSelectGetItemInfos();
                    var strategySceneInfo = new StrategySceneInfo
                    {
                        GetItemInfos = makeSelectGetItemInfos,
                        ActorInfos = makeSelectActorInfos
                    };
                    _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
                } else
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    SetBusy(false);
                }
            }
        }
        
        private void CommandLeftActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(-1);
            CommandRefresh();
        }

        private void CommandRightActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(1);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.SetNuminous(_model.Currency);
            _view.SetLvUpCost(_model.LevelUpCost());
            _view.CommandRefresh();
            var skillListData = _model.SkillActionList(_model.CurrentActor);
            if (skillListData.Count > 0)
            {
                skillListData[0].SetSelected(true);
            }
            _view.CommandRefreshStatus(skillListData,_model.CurrentActor,_model.PartyMembers(),_model.SkillTrigger());
        }

        private void SaveSelectedSkillId()
        {
            var selectedSkillId = _view.SelectedSkillId();
            if (selectedSkillId > -1)
            {
                _model.SetActorLastSkillId(selectedSkillId);
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            _view.SetBusy(busy);
        }
    }
}