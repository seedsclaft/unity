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

    private void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));
        _view.SetTitleCommand(_model.TitleCommand);

        CommandRefresh();
        var bgm = _model.GetBgmData("TITLE");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,false);

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

    private void CommandTitle(int commandIndex){
        _busy = true;
        switch ((TitleComandType)commandIndex){
            case TitleComandType.NewGame:
            _view.CommandInitSaveInfo();
            _view.CommandSceneChange(Scene.NameEntry);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //_view.CommandSceneChange(Scene.Battle);
            break;
            case TitleComandType.Continue:
            SaveSystem.LoadStart();
            _view.CommandSceneChange(Scene.Tactics);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            break;
            case TitleComandType.Option:
            break;
        }
    }

    private void CommandRefresh(){
        int selectIndex = (int)TitleComandType.NewGame;
        if (_model.ExistsLoadFile())
        {
            selectIndex = (int)TitleComandType.Continue;
        }
        _view.RefreshCommandIndex(selectIndex);
    }
}
