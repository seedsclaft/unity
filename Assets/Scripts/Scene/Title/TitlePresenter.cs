using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

public class TitlePresenter
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
        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));
        _view.SetTitleCommand(_model.TitleCommand);

        var bgm = await _model.GetBgmData("TITLE");
        SoundManager.Instance.PlayBgm(bgm,1.0f,false);
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
    }

    void CommandTitle(int commandIndex){
        _busy = true;
        switch ((TitleComandType)commandIndex){
            case TitleComandType.NewGame:
            _view.CommandInitSaveInfo();
            _view.CommandSceneChange(Scene.MainMenu);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //_view.CommandSceneChange(Scene.Battle);
            break;
            case TitleComandType.Continue:
            SaveSystem.LoadStart();
            _view.CommandSceneChange(Scene.Tactics);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            break;
            case TitleComandType.Option:
            break;
        }
    }
}
