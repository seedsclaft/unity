﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

public class TitlePresenter : BasePresenter
{
    TitleModel _model = null;
    TitleView _view = null;
    private bool _busy = true;
    private bool _logoChecked = false;
    public TitlePresenter(TitleView view)
    {
        _view = view;
        SetView(_view);
        _model = new TitleModel();
        SetModel(_model);

        Initialize();
    }

    private async void Initialize()
    {
        _model.ApllyConfigData();
        if (GameSystem.ConfigData._eventSkipIndex)
        {
            _view.CommandChangeEventSkipIndex(true);
        }

        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));
        _view.SetTitleCommand(_model.TitleCommand);
        _view.SetSideMenu(_model.SideMenu());
        _view.SetVersion(_model.VersionText());

        CommandRefresh();
        var bgm = await _model.GetBgmData("TITLE");
        //Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,false);

        _busy = false;
    }

    private void updateCommand(TitleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.TitleCommand)
        {
            CommandTitle((int) viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.Credit)
        {
            CommandCredit();
        }
        if (viewEvent.commandType == CommandType.LogoClick)
        {
            CommandLogoClick();
        }
        if (viewEvent.commandType == CommandType.OpenSideMenu)
        {
            CommandOpenSideMenu();
        }
        if (viewEvent.commandType == CommandType.CloseSideMenu)
        {
            CommandCloseSideMenu();
        }
        if (viewEvent.commandType == CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.MenuCommandData)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.Option)
        {
            if (_logoChecked == false)
            {
                CommandLogoClick();
                _logoChecked = true;
                return;
            } else
            {
                CommandOption();
            }
        }
    }

    private void CommandTitle(int commandIndex){
        if (_logoChecked == false)
        {
            CommandLogoClick();
            _logoChecked = true;
            return;
        }
        _busy = true;
        switch ((TitleComandType)commandIndex){
            case TitleComandType.NewGame:
            _model.InitSaveInfo();
            _view.CommandSceneChange(Scene.NameEntry);
            //_view.CommandSceneChange(Scene.Battle);
            break;
            case TitleComandType.Continue:
            SaveSystem.LoadStart();
            if (GameSystem.CurrentData.ResumeStage)
            {
                _view.CommandSceneChange(Scene.Tactics);
            } else{
                _view.CommandSceneChange(Scene.MainMenu);
            }
            break;
            case TitleComandType.Option:
            _view.CommandCallOption(() => {
                _busy = false;
            });
            break;
        }
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }

    private void CommandCredit()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallCredit(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }

    public new void CommandOption()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallOption(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }

    private void CommandRefresh(){
        int selectIndex = (int)TitleComandType.NewGame;
        if (_model.ExistsLoadFile())
        {
            selectIndex = (int)TitleComandType.Continue;
        } else
        {
            _view.SetCommandDisable(1);
        }
        _view.RefreshCommandIndex(selectIndex);
    }

    private async void CommandLogoClick()
    {
        _logoChecked = true;
        _view.CommandLogoClick();
        var bgm = await _model.GetBgmData("TITLE");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,false);
    }

    private void CommandOpenSideMenu()
    {
        _view.CommandOpenSideMenu();
    }

    private void CommandCloseSideMenu()
    {
        _view.CommandCloseSideMenu();
    }

    private void CommandSelectSideMenu(SystemData.MenuCommandData sideMenu)
    {
        if (sideMenu.Key == "Licence")
        {
            CommandCredit();
        }
    }
}
