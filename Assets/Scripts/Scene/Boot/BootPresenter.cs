﻿using System.Collections;
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

    private void Initialize()
    {

        _model.LoadData();

        _busy = false;
        Debug.Log("Boot Success");
        Application.targetFrameRate = 60;
        if (SaveSystem.ExistsConfigFile())
        {
            SaveSystem.LoadConfigStart();
        } else
        {
            _view.CommandInitConfigInfo();
        }
        if (_view.TestMode)
        {
            _view.CommandInitSaveInfo();
            _view.CommandSceneChange(Scene.Battle);
        } else{
            _view.CommandSceneChange(Scene.Title);
        }
        //SaveSystem.SaveStart();
    }
}
