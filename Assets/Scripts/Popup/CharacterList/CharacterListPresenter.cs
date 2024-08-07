using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListPresenter :BasePresenter
    {
        CharacterListModel _model = null;
        CharacterListView _view = null;

        private bool _busy = true;
        public CharacterListPresenter(CharacterListView view,List<ActorInfo> actorInfos)
        {
            _view = view;
            _model = new CharacterListModel(actorInfos);

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetCharacterList(GetListData(_model.ActorInfos));
            _view.OpenAnimation();
        }

        private void UpdateCommand(CharacterListViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
        }
    }

    public class CharacterListInfo
    {
        private System.Action<int> _callEvent;
        public System.Action<int> CallEvent => _callEvent;
        public CharacterListInfo(System.Action<int> callEvent,System.Action backEvent)
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