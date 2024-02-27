using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainMenu;
using Ryneus;

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
        _view.SetEvent((type) => UpdateCommand(type));

        _view.SetStagesData(_model.Stages());
        _view.SetSideMenu(_model.SideMenu());
        _view.SetNuminous(_model.Currency);
        _view.SetTotalScore(_model.TotalScore);
        //_model.InitStageData();

        var bgm = await _model.GetBgmData("MAINMENU");
        SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        //SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        _view.UpdateMainMenuStage();
        _busy = false;
    }

    private void UpdateCommand(MainMenuViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == CommandType.StageSelect)
        {
            CommandStageSelect((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.Option)
        {
            CommandOption();
        }
        if (viewEvent.commandType == CommandType.Ranking)
        {
            CommandRanking((int)viewEvent.template);
        }
        if (viewEvent.commandType == CommandType.SelectSideMenu)
        {
            CommandSelectSideMenu((SystemData.CommandData)viewEvent.template);
        }
    }

    private void CommandStageSelect(int stageId)
    {
        //_model.InitializeStageData(stageId);
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        if (_model.NeedSlotData(stageId))
        {
            _view.CommandSceneChange(Scene.Slot);
        } else
        {
            if (_model.ClearedStage(stageId))
            {
                _model.StartSymbolRecordStage(stageId);
                _view.CommandSceneChange(Scene.SymbolRecord);
            } else
            {
                _model.StartSelectStage(stageId);
                _view.CommandGotoSceneChange(Scene.Tactics);
            }
            /*
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.SetInitHelpText();
                _view.ChangeUIActive(true);
            });
            statusViewInfo.SetDisplayDecideButton(true);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
            */
        }
    }

    private void CommandRule()
    {
        _busy = true;
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.DeactivateSideMenu();
        _view.SetHelpInputInfo("RULING");
        _view.CommandCallRuling(() => {
            _busy = false;
            _view.ActivateSideMenu();
            _view.SetHelpInputInfo("OPTION");
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
    }

    private void CommandRanking(int stageId)
    {
        _busy = true;
        var rankingViewInfo = new RankingViewInfo();
        rankingViewInfo.EndEvent = () => {
            //_view.ActivateSideMenu();
            _busy = false;
        };
        rankingViewInfo.StageId = stageId;
        _view.CommandCallRanking(rankingViewInfo);
    }
    
    private void CommandSlotPopup()
    {
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _view.CommandSceneChange(Scene.Slot);
    }

    private void CommandSelectSideMenu(SystemData.CommandData sideMenu)
    {
        if (sideMenu.Key == "Help")
        {
            CommandRule();
        }
        if (sideMenu.Key == "Slot")
        {
            CommandSlotPopup();
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
