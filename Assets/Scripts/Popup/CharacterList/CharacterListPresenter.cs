using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterList;

public class CharacterListPresenter 
{
    CharacterListModel _model = null;
    CharacterListView _view = null;

    private bool _busy = true;
    public CharacterListPresenter(CharacterListView view)
    {
        _view = view;
        _model = new CharacterListModel();

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetHelpInputInfo("CHARACTER_LIST");
        _view.SetCharacterList(_model.CharacterList());
    }

    private void UpdateCommand(CharacterListViewEvent viewEvent)
    {
        if (_busy){
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
    
}