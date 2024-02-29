using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reborn;
using Ryneus;

namespace Ryneus
{
    public class RebornPresenter :BasePresenter
    {
        RebornModel _model = null;
        RebornView _view = null;

        private bool _busy = true;
        public RebornPresenter(RebornView view)
        {
            _view = view;
            _model = new RebornModel();
            SetModel(_model);
            SetView(_view);

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("REBORN");
            _view.SetActorList(_model.ActorInfos());
            CommandUpdateActor();
            _view.SetHelpText(DataSystem.GetTextData(17010).Text);
            _view.SetBackEvent(() => {
                CommandBackEvent();
            });
            // 初回なら
            /*
            if (GameSystem.LastScene == Scene.MainMenu)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(17010).Text,(a) => UpdatePopupStart(a));
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            }
            */
            _busy = false;
        }

        private void UpdateCommand(RebornViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            if (viewEvent.commandType == CommandType.DecideActor)
            {
            CommandDecideActor((int)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.CancelActor)
            {
            CommandCancelActor();
            }
            if (viewEvent.commandType == CommandType.UpdateActor)
            {
            CommandUpdateActor();
            }
            if (viewEvent.commandType == CommandType.Back)
            {
            CommandBackEvent();
            }
            if (viewEvent.commandType == CommandType.LeftActor)
            {
            CommandLeftActor();
            }
            if (viewEvent.commandType == CommandType.RightActor)
            {
            CommandRightActor();
            }
        }

        private void UpdatePopupStart(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _model.OnRebornSkill();
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                var NeedAlcana = _model.NeedAlcana();
                if (NeedAlcana)
                {
                    _view.CommandSceneChange(Scene.AlcanaSelect);
                } else
                {
                    _model.SavePlayerStageData(true);
                    _view.CommandSceneChange(Scene.Tactics);
                }
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
            } else{
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }

        private void CommandDecideActor(int index)
        {
            _model.SetRebornActorIndex(index);
            var rebornActor = _model.RebornActorInfo();
            if (rebornActor != null)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(17020).Text,(a) => UpdatePopup(a));
                _view.CommandCallConfirm(confirmInfo);
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
        }

        private void CommandCancelActor()
        {
            CommandBackEvent();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandUpdateActor()
        {
            _model.SetRebornActorIndex(_view.ActorInfoListIndex);
            var rebornActor = _model.RebornActorInfo();
            if (rebornActor != null)
            {
                //_view.UpdateActor(rebornActor);
            }
            CommandRefresh();
        }

        public void CommandBackEvent()
        {
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.CommandSceneChange(Scene.MainMenu);
            });
            _model.InitializeStageData(_model.CurrentStage.Id);
            statusViewInfo.SetDisplayDecideButton(true);
            _view.ChangeUIActive(false);
            _view.CommandCallStatus(statusViewInfo);
        }
        
        private void CommandLeftActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeActorIndex(-1);
            CommandRefresh();
        }

        private void CommandRightActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeActorIndex(1);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            var skillInfos = _model.RebornSkillInfos(_model.CurrentActor);
            var lastSelectIndex = skillInfos.FindIndex(a => ((SkillInfo)a.Data).Id == _model.CurrentActor.LastSelectSkillId);
            if (lastSelectIndex == -1)
            {
                lastSelectIndex = 0;
            }
            _view.CommandRefreshStatus(skillInfos,_model.CurrentActor,_model.PartyMembers(),lastSelectIndex);
        }
    }
}