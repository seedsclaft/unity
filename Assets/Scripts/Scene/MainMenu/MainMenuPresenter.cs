﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainMenu;

public class MainMenuPresenter 
{
    MainMenuModel _model = null;
    MainMenuView _view = null;

    private bool _busy = true;
    public MainMenuPresenter(MainMenuView view)
    {
        _view = view;
        _model = new MainMenuModel();

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));

        var menuCommand = await _model.MenuCommand();
        _view.SetCommandData(menuCommand);
        List<ActorsData.ActorData> actors = DataSystem.Actors;
        var actorImages = _model.ActorsImage(actors);
        _view.SetActorsData(actors,actorImages);

        var bgm = await _model.BgmData();

        Sound.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(ViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.MainMenuCommand)
        {
            if (viewEvent.templete != null)
            {
                var selectedMenuCommand = viewEvent.templete;
                _model.SetSelectedMenuCommand((MenuComandType)selectedMenuCommand);
                _view.CommandSkill();
            }
        }
    }

    public async void CrossFade()
    {
        var bgm = await _model.BgmData2();

        Sound.SoundManager.Instance.CrossFadeBgm(bgm,1.0f,true);
    }
}
