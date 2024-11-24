using System;
using System.Collections.Generic;
using Status;

namespace Ryneus
{
    public class StatusPresenter : BasePresenter
    {
        private StatusModel _model = null;
        private StatusView _view = null;
        private CommandType _popupCommandType = CommandType.None;
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
            _view.SetCommandList(GetListData(_model.StatusCommand()));
            _view.SetMemberList(GetListData(_model.StageMembers()));
            _view.SetStatusEvent((type) => UpdateCommand(type));

            _view.CommandTopLayer();
            CommandRefresh();
            _view.OpenAnimation(() => 
            {
                CheckTutorialState();
            });
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            UnityEngine.Debug.Log(viewEvent.commandType);
            if (_busy /*|| _view.AnimationBusy*/)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.ViewCommandType.StatusCommandType)
            {
                case CommandType.LeftActor:
                    CommandLeftActor();
                    return;
                case CommandType.RightActor:
                    CommandRightActor();
                    return;
                case CommandType.Back:
                    CommandBack();
                    return;
                case CommandType.SelectActor:
                    CommandSelectActor();
                    return;
                case CommandType.CancelActor:
                    CommandCancelActor();
                    return;
                case CommandType.SelectSkill:
                    CommandSelectSkill((SkillInfo)viewEvent.template);
                    return;
                case CommandType.CancelSkill:
                    CommandCancelSkill();
                    return;
                case CommandType.CharacterList:
                    CommandCharacterList();
                    return;
                case CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.template);
                    return;
                case CommandType.LvReset:
                    CommandLvReset();
                    return;
                case CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)viewEvent.template);
                    return;
                case CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
                case CommandType.SelectCommandList:
                    CommandSelectCommandList((SystemData.CommandData)viewEvent.template);
                    return;
                case CommandType.CallHelp:
                    CommandCallHelp();
                    return;
            }
            //CheckTutorialState(viewEvent.commandType);
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 1000)
                {
                    // 初めて仲間加入画面を開く
                    checkFlag = _view.DisplayDecide;
                }
                if (tutorialData.Param1 == 1200)
                {
                    // Activeの魔法を初めて入手するかステージ3の最初
                    checkFlag = _model.StageMembers().Find(a => a.LearnSkillIds().FindAll(b => DataSystem.FindSkill(b).SkillType == SkillType.Active).Count > 0) != null || _model.CurrentStage.Id == 3;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)StatusType.Status + 200,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }

        private void CommandBack()
        {
            _view.CommandBack();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandSelectSkill(SkillInfo skillInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.SelectingSkill == null)
            {
                // 選択する
                _model.SetSelectingSkillId(skillInfo);
                _view.SetSelectingSkill(skillInfo);
            } else
            {
                // 変更する
                _model.SetActorSkillSlot(skillInfo.Id);
                _model.SetSelectingSkillId(null);
                _view.SetSelectingSkill(null);
                CommandRefreshMagicList();
            }
        }

        private void CommandCancelSkill()
        {
            _view.CommandTopLayer();
            _model.SetSelectingSkillId(null);
            _view.SetSelectingSkill(null);
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
            CheckTutorialState();
        }

        public void CommandSelectCharacter(int actorId)
        {
            _model.SelectActor(actorId);
            CommandRefresh();
        }

        private void CommandLvReset()
        {
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
            CommandRefresh();
            _view.ShowLeaningList(_model.SelectActorLearningMagicList(lastSelectSkillId));
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            CommandLearnMagic(_model.CurrentActor,skillInfo,() => 
            {
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
        }

        private void CommandSelectCommandList(SystemData.CommandData commandData)
        {
            switch (commandData.Key)
            {
                case "STATUS":
                    CallMemberList();
                    break;
                case "PARTY_EDIT":
                    break;
                case "RECORD":
                    break;
                case "SYSTEM":
                    break;
            }
        }

        private void CallMemberList()
        {
            _view.CallMemberList(_model.CurrentIndex);
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

        private void CommandSelectActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandRefreshMagicList();
            _view.CommandStatusLayer();
        }

        private void CommandCancelActor()
        {
            _view.CallCommandList();
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            if (_popupCommandType == CommandType.SelectSkill)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    CommandRefresh();
                }
                _view.ActivateSkillActionList();
            }


            if (_popupCommandType == CommandType.DecideStage)
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
            _view.CommandRefresh();
            List<ListData> skillListData;
            if (_view.IsRanking)
            {
                skillListData = GetListData(_model.CurrentActor.SkillInfos());
            } else
            {
                skillListData = _model.SkillActionListData(_model.CurrentActor);
            }
            if (skillListData.Count > 0)
            {
                skillListData[0].SetSelected(true);
            }
            _view.CommandRefreshStatus(skillListData,_model.CurrentActor,_model.PartyMembers(),GetListData(_model.SkillTrigger()));
        }

        private void CommandRefreshMagicList()
        {
            _view.SetMagicList(GetListData(_model.SlotSkills()));
            _view.SetActorInfo(_model.CurrentActor,_model.ActorInfos);
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
                template = "Status",
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