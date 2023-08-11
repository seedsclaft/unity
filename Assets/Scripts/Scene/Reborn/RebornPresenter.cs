using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reborn;

public class RebornPresenter :BasePresenter
{
    RebornModel _model = null;
    RebornView _view = null;

    private bool _busy = true;
    public RebornPresenter(RebornView view)
    {
        _view = view;
        _model = new RebornModel();
        SetModel(_model);
        SetView(_view);

        Initialize();
    }

    private void Initialize()
    {
        _view.SetEvent((type) => updateCommand(type));
        _view.SetHelpInputInfo("REBORN");
        _view.SetActorList(_model.ActorInfos());
        CommandUpdateActor();
        _view.SetHelpText(DataSystem.System.GetTextData(17010).Text);
        _busy = false;
    }

    private void updateCommand(RebornViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.DecideActor)
        {
           CommandDecideActor((int)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.CancelActor)
        {
           CommandCancelActor();
        }
        if (viewEvent.commandType == CommandType.UpdateActor)
        {
           CommandUpdateActor();
        }
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            _model.OnRebornSkill();
            _view.CommandConfirmClose();
            _view.CommandSceneChange(Scene.Tactics);
        }
        _view.CommandConfirmClose();
    }

    private void CommandDecideActor(int index)
    {
        _model.SetRebornActorIndex(index);
        var rebornActor = _model.RebornActorInfo();
        if (rebornActor != null)
        {
            ConfirmInfo confirmInfo = new ConfirmInfo("継承しますか？",(a) => UpdatePopup(a));
            _view.CommandCallConfirm(confirmInfo);
        }
    }

    private void CommandCancelActor()
    {

    }

    private void CommandUpdateActor()
    {
        _model.SetRebornActorIndex(_view.ActorInfoListIndex);
        var rebornActor = _model.RebornActorInfo();
        if (rebornActor != null)
        {
            _view.UpdateActor(rebornActor);
        }
    }
}
