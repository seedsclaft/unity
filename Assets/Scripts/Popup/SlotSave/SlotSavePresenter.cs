using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotSave;

namespace Ryneus
{
    public class SlotSavePresenter 
    {
        SlotSaveModel _model = null;
        SlotSaveView _view = null;

        private bool _busy = true;
        public SlotSavePresenter(SlotSaveView view)
        {
            _view = view;
            _model = new SlotSaveModel();

            Initialize();
        }

        private void Initialize()
        {
            _busy = false;
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("SLOT");
            CommandRefresh();
        }

        private void UpdateCommand(SlotSaveViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.SlotSaveOpen:
                    CommandSlotSaveOpen((SlotInfo)viewEvent.template);
                    break;
                case CommandType.SlotSave:
                    CommandSlotSave((SlotInfo)viewEvent.template);
                    break;
                case CommandType.Status:
                    CommandStatus((int)viewEvent.template);
                    break;
            }
        }

        private void CommandSlotSaveOpen(SlotInfo slotInfo)
        {
            _model.CurrentSlotInfo = slotInfo;
            _view.SetCurrentSlotInfo(slotInfo);
            CommandRefresh();
        }

        private void CommandSlotSave(SlotInfo slotInfo)
        {
            var confirmView = new ConfirmInfo(DataSystem.GetText(22010),(a) => UpdatePopupSlotSave(a));
            _model.SetBaseSlotSaveInfo(slotInfo);
            _view.CommandCallConfirm(confirmView);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void UpdatePopupSlotSave(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var slotIndex = _view.SlotListIndex;
                _model.SaveSlotInfo(slotIndex);
                _view.BackEvent();
            }
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }
        
        private void CommandStatus(int index)
        {
            if (index > -1)
            {
                _model.ClearActorsData();
                _model.SetActorsData(index);
                var statusViewInfo = new StatusViewInfo(() => {
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.ChangeUIActive(true);
                });
                statusViewInfo.SetDisplayDecideButton(false);
                _view.CommandCallStatus(statusViewInfo);
                _view.ChangeUIActive(false);
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh(_model.SlotList());
        }
    }
}