using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainMenu;

public class MainMenuPresenter : BasePresenter
{
    MainMenuModel _model = null;
    MainMenuView _view = null;

    private bool _busy = true;
    public MainMenuPresenter(MainMenuView view)
    {
        _view = view;
        SetView(_view);
        _model = new MainMenuModel();
        SetModel(_model);

        Initialize();
    }

    private async void Initialize()
    {
        _view.SetHelpWindow();
        _view.SetEvent((type) => updateCommand(type));

        List<StageInfo> stages = _model.Stages();
        _view.SetStagesData(stages);
        _view.SetSideMenu(_model.SideMenu());
        _model.InitPartyInfo();

        var bgm = await _model.GetBgmData("MAINMENU");
        Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);

        _busy = false;
    }

    private void updateCommand(MainMenuViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.StageSelect)
        {
            _model.SetStageId((int)viewEvent.templete);
            StatusViewInfo statusViewInfo = new StatusViewInfo(() => {
                _view.CommandStatusClose();
                _view.SetInitHelpText();
                _view.SetActiveUi(true);
            });
            statusViewInfo.SetDisplayDecideButton(true);
            statusViewInfo.SetDisableStrength(true);
            _view.CommandCallStatus(statusViewInfo);
            _view.SetActiveUi(false);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        if (viewEvent.commandType == CommandType.Rule)
        {
            CommandRule();
        }
        if (viewEvent.commandType == CommandType.Option)
        {
            CommandOption();
        }
        if (viewEvent.commandType == CommandType.Ranking)
        {
            CommandRanking();
        }
        if (viewEvent.commandType == CommandType.OpenSideMenu)
        {
            CommandOpenSideMenu();
        }
        if (viewEvent.commandType == CommandType.CloseSideMenu)
        {
            CommandCloseSideMenu();
        }
        if (viewEvent.commandType == CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.MenuCommandData)viewEvent.templete);
        }
    }

    private void CommandRule()
    {
        _busy = true;
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.DeactivateSideMenu();
        _view.SetHelpInputInfo("RULING");
        _view.CommandCallRuling(() => {
            _busy = false;
            _view.ActivateSideMenu();
            _view.SetHelpInputInfo("OPTION");
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
    }

    private void CommandRanking()
    {
        var popupInfo = new ConfirmInfo(DataSystem.System.GetTextData(11100).Text,(menuCommandInfo) => UpdatePopup((ConfirmComandType)menuCommandInfo));
        _view.CommandCallConfirm(popupInfo);
    }

    private void UpdatePopup(ConfirmComandType confirmComandType)
    {
        if (confirmComandType == ConfirmComandType.Yes)
        {
            CommandRankingPopup();
        } else
        {
            _view.CommandConfirmClose();
        }
    }
    
    private void CommandRankingPopup()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallRanking(() => {
            _view.ActivateSideMenu();
            _busy = false;
        });
    }

    private void CommandOpenSideMenu()
    {
        _view.CommandOpenSideMenu();
    }

    private void CommandCloseSideMenu()
    {
        _view.CommandCloseSideMenu();
    }

    private void CommandSelectSideMenu(SystemData.MenuCommandData sideMenu)
    {
        if (sideMenu.Key == "Help")
        {
            CommandRule();
        }
        if (sideMenu.Key == "Ranking")
        {
            CommandRankingPopup();
        }
    }    
    
    public void CommandOption()
    {
        _busy = true;
        _view.DeactivateSideMenu();
        _view.CommandCallOption(() => {
            _busy = false;
            _view.ActivateSideMenu();
        });
    }
}
