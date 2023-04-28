using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NameEntry;

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

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));



        var bgm = await _model.GetBgmData("MAINMENU");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        //_view.CommandLoadingClose();
        // 
        CommandStartEntry();
        _busy = false;
    }

    private void updateCommand(NameEntryViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.StartEntry)
        {
           CommandStartEntry();
        }
        if (viewEvent.commandType == CommandType.EntryEnd)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandEntryEnd((string)viewEvent.templete);
        }
    }

    private void updatePopup(ConfirmComandType confirmComandType)
    {
        _view.CommandConfirmClose();
        if (confirmComandType == ConfirmComandType.Yes)
        {
        }
        _view.StartNameEntry();
    }

    private void CommandStartEntry()
    {
        ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(5000).Text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        popupInfo.SetIsNoChoise(true);
        _view.CommandCallConfirm(popupInfo);
        _view.ShowNameEntry(DataSystem.System.GetTextData(5001).Text);
    }

    private void CommandEntryEnd(string nameText)
    {
        if (nameText == "")
        {
            ConfirmInfo popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(5002).Text,(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
            popupInfo.SetIsNoChoise(true);
            _view.CommandCallConfirm(popupInfo);
        } else{
            _model.SetPlayerName(nameText);
            _view.CommandDecidePlayerName(nameText);
            _view.CommandSceneChange(Scene.MainMenu);
        }
    }
}
