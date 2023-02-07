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

        var bgm = await _model.BgmData();
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _busy = false;
    }

    private void updateCommand(TitleViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.TitleCommand)
        {
            CommandTitle();

            /*
            _view.CommandInitSaveInfo();
            var temp = DataSystem.Enemies;
            TempInfo info = new TempInfo(temp);
            _view.CommandSetTemplete(info);
            _view.CommandSceneChange(Scene.Map);
            */
        }
    }

    void CommandTitle(){
        _busy = true;
        switch ((TitleComandType)_view.titleCommandIndex){
            case TitleComandType.NewGame:
            break;
            case TitleComandType.Continue:
            break;
            case TitleComandType.Option:
            break;
        }
    }
}
