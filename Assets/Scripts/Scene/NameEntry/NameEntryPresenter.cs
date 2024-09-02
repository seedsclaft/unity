using System.Collections;
using System.Collections.Generic;
using NameEntry;

namespace Ryneus
{
    public class NameEntryPresenter 
    {
        NameEntryModel _model = null;
        NameEntryView _view = null;

        private bool _busy = true;
        public NameEntryPresenter(NameEntryView view)
        {
            _view = view;
            _model = new NameEntryModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));



            //var bgm = await _model.GetBgmData("MAINMENU");
            //Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

            //_view.CommandGameSystem(Base.CommandType.CloseLoading);
            // 
            CommandStartEntry();
            _busy = false;
        }

        private void UpdateCommand(NameEntryViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.commandType == CommandType.StartEntry)
            {
                CommandStartEntry();
            }
            if (viewEvent.commandType == CommandType.EntryEnd)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CommandEntryEnd((string)viewEvent.template);
            }
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
            }
            _view.StartNameEntry();
        }

        private void CommandStartEntry()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(5000),(a) => UpdatePopup(a));
            confirmInfo.SetIsNoChoice(true);
            confirmInfo.SetBackEvent(() => UpdatePopup(ConfirmCommandType.Yes));
            _view.CommandCallConfirm(confirmInfo);
            _view.ShowNameEntry("");
        }

        private void CommandEntryEnd(string nameText)
        {
            if (nameText == "")
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(5002),(menuCommandInfo) => UpdatePopup((ConfirmCommandType)menuCommandInfo));
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            } else
            {
                _model.SetPlayerName(nameText);
                _view.CommandDecidePlayerName(nameText);
                _model.StartOpeningStage();
                _view.CommandGotoSceneChange(Scene.Tactics);
            }
        }
    }
}