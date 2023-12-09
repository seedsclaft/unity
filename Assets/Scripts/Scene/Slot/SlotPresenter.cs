using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slot;

public class SlotPresenter 
{
    SlotModel _model = null;
    SlotView _view = null;

    private bool _busy = true;
    public SlotPresenter(SlotView view)
    {
        _view = view;
        _model = new SlotModel();

        Initialize();
    }

    private void Initialize()
    {
        _busy = true;
        _view.SetEvent((type) => UpdateCommand(type));
        _view.SetHelpInputInfo("SLOT");
        _view.SetHelpText(DataSystem.System.GetTextData(18010).Text);
        _view.SetBackEvent();
        _view.SetSlotInfo(_model.SlotInfos());
        _busy = false;
    }

    private void UpdateCommand(SlotViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == CommandType.Lock)
        {
            CommandLock((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.Status)
        {
            CommandStatus((int)viewEvent.template);
        }
    }

    private void CommandBack()
    {
        _view.CommandSceneChange(Scene.MainMenu);
    }

    private void CommandLock(int index)
    {
        if (index > -1)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SlotLock(index);
            _view.CommandRefresh(_model.SlotList());
        }
    }
    
    private void CommandStatus(int index)
    {
        if (index > -1)
        {
            _model.ClearActorsData();
            _model.SetActorsData(index);
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                _view.ChangeUIActive(true);
            });
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
    }
}
