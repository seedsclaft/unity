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
        _view.SetEvent((type) => updateCommand(type));
        _view.SetHelpInputInfo("SLOT");
        _view.SetHelpText(DataSystem.System.GetTextData(18010).Text);
        _view.SetBackEvent();
        _view.SetSlotInfo(_model.SlotInfos());
        _busy = false;
    }

    private void updateCommand(SlotViewEvent viewEvent)
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
            CommandLock((int)viewEvent.templete);
        }
        if (viewEvent.commandType == CommandType.Status)
        {
            CommandStatus((int)viewEvent.templete);
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
            _view.CommandRefresh(_model.SlotInfos());
        }
    }
    
    private void CommandStatus(int index)
    {
        if (index > -1)
        {
            _model.ClearActorsData();
            _model.SetActorsData(index);
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                _view.SetActiveUi(true);
            });
            statusViewInfo.SetDisplayDecideButton(false);
            statusViewInfo.SetDisableStrength(true);
            _view.CommandCallStatus(statusViewInfo);
            _view.SetActiveUi(false);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
    }
}