using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Boot;

public class BootPresenter : BasePresenter
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
        DataSystem.LoadData();
        Debug.Log("Boot Success");
        Application.targetFrameRate = 60;
#if UNITY_ANDROID && !UNITY_EDITOR
        var width = Screen.width;
        var height = Screen.height;
        var rate = 1280f / (float)width;
        Screen.SetResolution((int)(width * rate), (int)(height * rate), true);
#endif
        var gamePad = Gamepad.current;
        if (gamePad != null)
        {
            InputSystem.IsGamePad = true;
        }
        if (SaveSystem.ExistsConfigFile())
        {
            SaveSystem.LoadConfigStart();
        } else
        {
            _model.InitConfigInfo();
        }
        if (_view.TestMode)
        {
            _model.InitSaveInfo();
            _view.CommandSceneChange(Scene.Battle);
        } else{
            _view.SetEvent((type) => UpdateCommand(type));
        }
        _busy = false;
        //SaveSystem.SaveStart();
    }
    
    private void UpdateCommand(BootViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.LogoClick)
        {
            CommandLogoClick();
        }
    }

    private void CommandLogoClick()
    {
        _view.CommandSceneChange(Scene.Title);
    }
}
