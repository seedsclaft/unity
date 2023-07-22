using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

public class TitlePresenter : BasePresenter
{
    TitleModel _model = null;
    TitleView _view = null;
    private bool _busy = true;
    public TitlePresenter(TitleView view)
    {
        _view = view;
        _model = new TitleModel();

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
        if (viewEvent.commandType == CommandType.Option)
        {
            CommandOption();
        }
    }

    private void CommandTitle(int commandIndex){
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
        _view.CommandCallCredit(() => {
            _busy = false;
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
        _view.CommandLogoClick();
        var bgm = await _model.GetBgmData("TITLE");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,false);
    }
}
