using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootPresenter 
{
    private BootView _view = null;
    private BootModel _model = null;
    private bool _busy = true;
    public BootPresenter(BootView view)
    {
        _view = view;
        _model = new BootModel();

        Initialize();
    }

    private async void Initialize()
    {

        await _model.LoadData();

        _busy = false;
        Debug.Log("Boot Success");
        _view.CommandSceneChange();
        _view.CommandInitSaveInfo();
        //SaveSystem.SaveStart();
    }
}
