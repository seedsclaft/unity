using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterList;

public class CharacterListView : BaseView
{
    [SerializeField] private BaseList characterList = null;
    private new System.Action<CharacterListViewEvent> _commandData = null;
    private System.Action<int> _callEvent = null;
    
    public override void Initialize() 
    {
        base.Initialize();
        new CharacterListPresenter(this);
        characterList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        characterList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor());
        SetInputHandler(characterList.GetComponent<IInputHandlerEvent>());
    }

    public void SetViewInfo(CharacterListInfo characterListInfo)
    {
        _callEvent = characterListInfo.CallEvent;
    }
    
    public void SetEvent(System.Action<CharacterListViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        SetBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
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
            var data = (ActorInfo)listData.Data;
            _callEvent(data.ActorId);
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