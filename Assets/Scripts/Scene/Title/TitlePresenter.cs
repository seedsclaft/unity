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
        _view.SetEvent((type) => updateCommand(type));

        var titleCommand = _model.TitleCommand;
        _view.SetTitleCommand(titleCommand);

        var bgm = await _model.BgmData();

        Sound.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(TitleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.TitleCommand)
        {
            if (viewEvent.templete != null)
            {
                /*
                var selectedMenuCommand = viewEvent.templete;
                _model.SetSelectedMenuCommand((MenuComandType)selectedMenuCommand);
                _view.CommandSkill();
                */
            }
        }
    }
}
