using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmPresenter 
{
    ConfirmView _view = null;

    ConfirmModel _model = null;
    private bool _busy = true;
    public ConfirmPresenter(ConfirmView view)
    {
        _view = view;
        _model = new ConfirmModel();

        Initialize();
    }

    private void Initialize()
    {
        _view.SetConfirmCommand(_model.ConfirmCommand());
        _busy = false;
    }
}

public class ConfirmInfo
{
    private string _title = "";
    public string Title {get {return _title;}}
    private System.Action<ConfirmComandType> _callEvent = null;
    public System.Action<ConfirmComandType> CallEvent {get {return _callEvent;}}

    public ConfirmInfo(string title,System.Action<ConfirmComandType> callEvent)
    {
        _title = title;
        _callEvent = callEvent;
    }
}