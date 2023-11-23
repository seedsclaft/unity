using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Credit;

public class CreditPresenter 
{
    private CreditView _view = null;
    private bool _busy = true;
    public CreditPresenter(CreditView view)
    {
        _view = view;

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => UpdateCommand(type));
        _busy = false;
    }

    private void UpdateCommand(CreditViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
    }
}

public class CreditInfo
{
}