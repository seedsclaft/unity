using System.Collections.Generic;
using Ryneus;

namespace Ryneus
{
    public class StatusPresenter : BasePresenter
    {
        private StatusModel _model = null;
        private StatusView _view = null;
        private Status.CommandType _popupCommandType = Status.CommandType.None;
        public StatusPresenter(StatusView view)
        {
            _view = view;
            SetView(_view);
            _model = new StatusModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetHelpWindow(_model.HelpText());
            _view.SetUIButton();
            _view.SetEvent((type) => UpdateCommand(type));

            CommandRefresh();
            if (_model.StatusActors().Count == 1) _view.HideArrows();
        }

        private void UpdateCommand(StatusViewEvent viewEvent)
        {
            if (_view.Busy){
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
                case Status.CommandType.LvReset:
                    CommandLvReset();
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
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var characterListInfo = new CharacterListInfo((a) => {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.SelectActor(a);
                CommandRefresh();
            },
            () => {
                CommandRefresh();
            });
            _view.CommandCallCharacterList(characterListInfo);
        }

        private void CommandLvReset()
        {
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
            }
        }

        private void UpdatePopupActorLvReset(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var currency = _model.ActorLvReset();
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(14170,currency.ToString()),(a) => {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    CommandRefresh();
                    _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
        }

        private void CommandDecideActor()
        {
            var actorInfo = _model.CurrentActor;
            var text = DataSystem.GetReplaceText(14180,actorInfo.Master.Name);
            var confirmInfo = new ConfirmInfo(text,(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
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
                } else{
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
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
            var skillInfos = _model.SkillActionList();
            var lastSelectIndex = skillInfos.FindIndex(a => ((SkillInfo)a.Data).Id == _model.CurrentActor.LastSelectSkillId);
            if (lastSelectIndex == -1)
            {
                lastSelectIndex = 0;
            }
            _view.CommandRefreshStatus(skillInfos,_model.CurrentActor,_model.PartyMembers(),lastSelectIndex,_model.SkillTrigger());
        }

        private void SaveSelectedSkillId()
        {
            var selectedSkillId = _view.SelectedSkillId();
            if (selectedSkillId > -1)
            {
                _model.SetActorLastSkillId(selectedSkillId);
            }
        }
    }
}