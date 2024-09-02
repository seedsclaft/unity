using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckConflictPresenter :BasePresenter
    {
        CheckConflictModel _model = null;
        CheckConflictView _view = null;

        private bool _busy = true;
        public CheckConflictPresenter(CheckConflictView view)
        {
            _view = view;
            _model = new CheckConflictModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetCheckConflict(GetListData(_model.ResultInfos()));
            _view.SetMainActorInfos(GetListData(_model.MainActorInfos()));
            _view.SetBrunchActorInfos(GetListData(_model.BrunchActorInfos()));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(CheckConflictViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CheckConflict.CommandType.MainActorStatus:
                    CommandMainActorStatus();
                    break;
                case CheckConflict.CommandType.BrunchActorStatus:
                    CommandBrunchActorStatus();
                    break;
                case CheckConflict.CommandType.MainToggle:
                    CommandMainToggle((bool)viewEvent.template);
                    break;
                case CheckConflict.CommandType.BrunchToggle:
                    CommandBrunchToggle((bool)viewEvent.template);
                    break;
            }
        }

        private void CommandMainActorStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.MainActorInfos(),false,true,false,false,-1,() => 
            {
            });
        }

        private void CommandBrunchActorStatus()
        {   
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.BrunchActorInfos(),false,true,false,false,-1,() => 
            {
            });
        }

        private void CommandMainToggle(bool isChecked)
        {
            _view.CheckBrunchToggle(!isChecked);
            var confirmInfo = new ConfirmInfo("更新せずに元の進行状態に戻りますか？",(ConfirmCommandType a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    _model.ReverseBrunch();
                    _view.CommandGameSystem(Base.CommandType.ClosePopup);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _view.CheckMainToggle(false);
                    _view.CheckBrunchToggle(false);
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandBrunchToggle(bool isChecked)
        {
            _view.CheckMainToggle(!isChecked);
            var confirmInfo = new ConfirmInfo("更新した内容を元の進行状態に統合しますか？",(ConfirmCommandType a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    _model.MergeBrunch();
                    _view.CommandGameSystem(Base.CommandType.ClosePopup);
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {
                    _view.CheckMainToggle(false);
                    _view.CheckBrunchToggle(false);
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }
    }

    public class CheckConflictInfo
    {
        private System.Action<int> _callEvent;
        public System.Action<int> CallEvent => _callEvent;
        public CheckConflictInfo(System.Action<int> callEvent,System.Action backEvent)
        {
            _callEvent = callEvent;
            _backEvent = backEvent;
        }
        private System.Action _backEvent;
        public System.Action BackEvent => _backEvent;
        
        private List<ActorInfo> _actorInfos;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
    }
}