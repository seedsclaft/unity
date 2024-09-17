using System.Collections.Generic;

namespace Ryneus
{
    public class TacticsStatusPresenter : BasePresenter
    {
        private TacticsStatusModel _model = null;
        private TacticsStatusView _view = null;
        private TacticsStatus.CommandType _popupCommandType = TacticsStatus.CommandType.None;
        private bool _busy = false;
        public TacticsStatusPresenter(TacticsStatusView view,List<ActorInfo> actorInfos)
        {
            _view = view;
            SetView(_view);
            _model = new TacticsStatusModel(actorInfos);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetHelpWindow(_model.HelpText());
            _view.SetUIButton(GetListData(_model.TacticsStatusCommand()));
            _view.SetEvent((type) => UpdateCommand(type));

            CommandRefresh();
            if (_model.ActorInfos.Count == 1) _view.HideArrows();
            _view.OpenAnimation();
        }

        private void UpdateCommand(TacticsStatusViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case TacticsStatus.CommandType.DecideActor:
                    CommandDecideActor();
                    return;
                case TacticsStatus.CommandType.LeftActor:
                    CommandLeftActor();
                    return;
                case TacticsStatus.CommandType.RightActor:
                    CommandRightActor();
                    return;
                case TacticsStatus.CommandType.Back:
                    CommandBack();
                    return;
                case TacticsStatus.CommandType.CharacterList:
                    CommandCharacterList();
                    return;
                case TacticsStatus.CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.template);
                    return;
                case TacticsStatus.CommandType.LvReset:
                    CommandLvReset();
                    return;
                case TacticsStatus.CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case TacticsStatus.CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case TacticsStatus.CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)viewEvent.template);
                    return;
                case TacticsStatus.CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
                case TacticsStatus.CommandType.SelectCommandList:
                    CommandSelectCommandList((SystemData.CommandData)viewEvent.template);
                    return;
                case TacticsStatus.CommandType.CallHelp:
                    CommandCallHelp();
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
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(18300,_model.CurrentActor.Master.Name),(a) => UpdatePopupActorLvReset(a));
                _view.CommandCallConfirm(confirmInfo);
                _popupCommandType = TacticsStatus.CommandType.DecideStage;
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(18340));
                SetBusy(false);
            }
        }

        private void CommandLevelUp()
        {
            _busy = true;
            _view.SetBusy(true);
            CommandLevelUp(_model.CurrentActor,() => 
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
            CommandLearnMagic(_model.CurrentActor,skillInfo,() => 
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

        private void UpdatePopupActorLvReset(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currency = _model.ActorLvReset();
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(18310,currency.ToString()),(a) => 
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    CommandRefresh();
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
            var text = DataSystem.GetReplaceText(18320,actorInfo.Master.Name);
            var confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => UpdatePopup(menuCommandInfo));
            _view.CommandCallConfirm(confirmInfo);
            _popupCommandType = TacticsStatus.CommandType.DecideStage;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            if (_popupCommandType == TacticsStatus.CommandType.SelectSkillAction)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    CommandRefresh();
                }
                _view.ActivateSkillActionList();
            }


            if (_popupCommandType == TacticsStatus.CommandType.DecideStage)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);

                    var makeSelectActorInfos = _model.MakeSelectActorInfos();
                    var makeSelectGetItemInfos = _model.MakeSelectGetItemInfos();
                    var strategySceneInfo = new StrategySceneInfo
                    {
                        GetItemInfos = makeSelectGetItemInfos,
                        ActorInfos = makeSelectActorInfos,
                        InBattle = false
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
            _view.SetToLvText(_model.CurrentActor.LinkedLevel());
            _view.CommandRefresh();
            var skillListData = _model.SkillActionListData(_model.CurrentActor);
            if (skillListData.Count > 0)
            {
                skillListData[0].SetSelected(true);
            }
            _view.CommandRefreshTacticsStatus(skillListData,_model.CurrentActor,_model.PartyMembers(),GetListData(_model.SkillTrigger()));
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

        private void CommandCallHelp()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "TacticsStatus",
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }
    }
}