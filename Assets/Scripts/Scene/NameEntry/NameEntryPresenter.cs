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
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);

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
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
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
        ConfirmInfo popupInfo = new ConfirmInfo("あなたの名前を聞かせてください",(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
        popupInfo.SetIsNoChoise(true);
        _view.CommandCallConfirm(popupInfo);
    }

    private void CommandEntryEnd(string nameText)
    {
        if (nameText == "")
        {
            ConfirmInfo popupInfo = new ConfirmInfo("名前を入力してください",(menuCommandInfo) => updatePopup((ConfirmComandType)menuCommandInfo));
            popupInfo.SetIsNoChoise(true);
            _view.CommandCallConfirm(popupInfo);
        } else{
            _view.CommandDecidePlayerName(nameText);
            _view.CommandSceneChange(Scene.MainMenu);
        }
    }
}