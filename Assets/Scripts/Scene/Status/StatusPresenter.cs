using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPresenter 
{
    StatusModel _model = null;
    StatusView _view = null;

    private bool _busy = true;
    public StatusPresenter(StatusView view)
    {
        _view = view;
        _model = new StatusModel();

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetUIButton();
        _view.SetEvent((type) => updateCommand(type));

        //List<ActorInfo> actorInfos = _model.Actors();
        _view.SetActorInfo(_model.CurrentActor);


        _view.SetTitleCommand(_model.StatusCommand);
        //var bgm = await _model.BgmData();
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(StatusViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Status.CommandType.LeftActor)
        {
            CommandLeftActor();
        }
        if (viewEvent.commandType == Status.CommandType.RightActor)
        {
            CommandRightActor();
        }
    }

    private void CommandLeftActor()
    {
         _model.ChangeActorIndex(-1);
        _view.SetActorInfo(_model.CurrentActor);
    }

    private void CommandRightActor()
    {
         _model.ChangeActorIndex(1);
        _view.SetActorInfo(_model.CurrentActor);
    }

    public async void CrossFade()
    {

    }
}
