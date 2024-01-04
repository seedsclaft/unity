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
        _view.SetHelpText(DataSystem.GetTextData(21020).Text);
        _view.SetBackEvent();
        CommandRefresh();
        _busy = false;
    }

    private void UpdateCommand(SlotViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.Decide)
        {
            CommandDecide((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.Back)
        {
            CommandBack();
        }
        if (viewEvent.commandType == CommandType.Status)
        {
            CommandStatus((int)viewEvent.template);
        }
    }

    private void CommandDecide(int slotIndex)
    {
        if (_model.SelectableSlot(slotIndex))
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(21010).Text,(a) => UpdatePopupStageStart((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }
    }

    private void UpdatePopupStageStart(ConfirmCommandType confirmCommandType)
    {
        _view.CommandConfirmClose();
        if (confirmCommandType == ConfirmCommandType.Yes)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.ClearActorsData();
            var selectIndex = _view.SlotListIndex;
            _model.SetActorsData(selectIndex);
            _model.SetSelectActorIds();
            _model.ResetActors();
            _model.SavePlayerStageData(true);
            _view.CommandSceneChange(Scene.Tactics);
        } else
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
    }

    private void CommandBack()
    {
        _view.CommandSceneChange(Scene.MainMenu);
    }

    private void CommandRefresh()
    {
        _view.CommandRefresh(_model.SlotList());
    }
    
    private void CommandStatus(int index)
    {
        if (_model.SelectableSlot(index))
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
}
