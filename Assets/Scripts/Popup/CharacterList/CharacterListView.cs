using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class CharacterListView : BaseView
    {
        [SerializeField] private BaseList characterList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<CharacterListViewEvent> _commandData = null;
        private System.Action<int> _callEvent = null;
        
        public void Initialize(List<ActorInfo> actorInfos) 
        {
            base.Initialize();
            characterList.Initialize();
            SetBaseAnimation(popupAnimation);
            new CharacterListPresenter(this,actorInfos);
            characterList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            characterList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor());
            SetInputHandler(characterList.GetComponent<IInputHandlerEvent>());
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void SetViewInfo(CharacterListInfo characterListInfo)
        {
            _callEvent = characterListInfo.CallEvent;
        }
        
        public void SetEvent(System.Action<CharacterListViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetCharacterList(List<ListData> characterLists)
        {
            characterList.SetData(characterLists);
            characterList.Activate();
        }

        private void CallDecideActor()
        {
            var listData = characterList.ListData;
            if (listData != null)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var data = (ActorInfo)listData.Data;
                _callEvent(data.ActorId);
            }
        }
    }
}
namespace CharacterList
{
    public enum CommandType
    {
        None = 0,
        DecideActor = 0,
    }
}

public class CharacterListViewEvent
{
    public CharacterList.CommandType commandType;
    public object template;

    public CharacterListViewEvent(CharacterList.CommandType type)
    {
        commandType = type;
    }
}