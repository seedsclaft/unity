using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SideMenu;
using Ryneus;

namespace Ryneus
{
    public class SideMenuPresenter : BasePresenter
    {
        SideMenuModel _model = null;
        SideMenuView _view = null;

        private bool _busy = true;
        public SideMenuPresenter(SideMenuView view)
        {
            _model = new SideMenuModel();
            _view = view;
            SetModel(_model);
            SetView(_view);
            Initialize();
        }

        private void Initialize()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = false;
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("SideMenu");
        }

        private void UpdateCommand(SideMenuViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.SelectSideMenu:
                CommandSelectSideMenu();
                break;
            }
        }

        private void CommandSelectSideMenu()
        {
            var data = _view.SideMenuCommand;
            if (data != null)
            {
                switch (data.Key)
                {
                    case "Retire":
                    CommandDropout();
                    break;
                    case "Help":
                    CommandRule();
                    break;
                    case "Save":
                    CommandSave(false);
                    break;
                    case "License":
                    CommandCredit();
                    break;
                    case "InitializeData":
                    CommandInitializeData();
                    break;
                }
            }
        }

        private void CommandDropout()
        {  
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(1100).Text,(a) => UpdatePopupDropout((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupDropout(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _model.SavePlayerStageData(false);
                _view.CommandGotoSceneChange(Scene.MainMenu);
            } else{
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            //_view.ActivateCommandList();
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }
        
        private void CommandRule()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            //_view.SetHelpInputInfo("RULING");
            _view.CommandCallRuling(() => {
                _busy = false;
                //_view.SetHelpInputInfo("OPTION");
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
        }

        private void CommandCredit()
        {
            _busy = true;
            _view.CommandCallCredit(() => {
                _busy = false;
            });
        }

        private void CommandInitializeData()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(581).Text,(a) => UpdatePopupDeletePlayerData((ConfirmCommandType)a));
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupDeletePlayerData(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.DeletePlayerData();
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                var popupInfo = new ConfirmInfo(DataSystem.GetTextData(582).Text,(a) => {
                    SoundManager.Instance.StopBgm();
                    _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                    _view.CommandGotoSceneChange(Scene.Boot);
                });
                popupInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(popupInfo);
            }
        }
    }
}