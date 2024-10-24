using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SideMenu;

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
            ClosePopup();
            _view.SetEvent((type) => UpdateCommand(type));
            CommandRefresh();
            _view.OpenAnimation();
        }

        private void UpdateCommand(SideMenuViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
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
                    case "Option":
                        CommandOption();
                        break;
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
                    case "DeleteStage":
                        CommandDeleteStage();
                        break;
                    case "Title":
                        CommandTitle();
                        break;
                    case "EndGame":
                        CommandEndGame();
                        break;
                    case "Dictionary":
                        CommandDictionary();
                        break;
                    
                }
            }
        }

        private void CommandOption()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CommandCallOption(() => 
            {
                ClosePopup();
                //_view.CommandGameSystem(Base.CommandType.ClosePopup);
            });
        }

        private void CommandDropout()
        {  
            if (_model.BrunchMode)
            {
                CommandCautionInfo(DataSystem.GetText(19360));
                ClosePopup();
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(1100),(a) => UpdatePopupDropout(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupDropout(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _model.SavePlayerStageData(false);
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.CommandGotoSceneChange(Scene.MainMenu);
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
            ClosePopup();
            //_view.ActivateCommandList();
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }
        
        private void CommandRule()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Ruling,
                EndEvent = () =>
                {
                    ClosePopup();
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandCredit()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Credit,
                EndEvent = () =>
                {
                    ClosePopup();
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void CommandInitializeData()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13300),(a) => UpdatePopupDeletePlayerData(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandDeleteStage()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13301),(a) => UpdatePopupDeleteStageData(a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandTitle()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(13320),(a) => UpdatePopupTitle((ConfirmCommandType)a));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupDeletePlayerData(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.DeletePlayerData();
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(13310),(a) => 
                {
                    SoundManager.Instance.StopBgm();
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.CommandGotoSceneChange(Scene.Boot);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            } else
            {            
                ClosePopup();
            }
        }

        private void UpdatePopupDeleteStageData(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _model.DeleteStageData();
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                var confirmInfo = new ConfirmInfo(DataSystem.GetText(13310),(a) => 
                {
                    SoundManager.Instance.StopBgm();
                    _view.CommandGameSystem(Base.CommandType.CloseStatus);
                    _view.CommandGotoSceneChange(Scene.Boot);
                });
                confirmInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo);
            } else
            {            
                ClosePopup();
            }
        }

        private void UpdatePopupTitle(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
                _view.CommandGotoSceneChange(Scene.Title);
            }
            ClosePopup();
        }

        private void CommandEndGame()
        {
            _busy = true;
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }

        private void CommandDictionary()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Dictionary,
                EndEvent = () =>
                {
                    ClosePopup();
                }
            };
            _view.CommandCallPopup(popupInfo);
        }

        private void ClosePopup()
        {
            _busy = false;
            _view.ActivateSideMenu();
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.SetHelpInputInfo("SIDEMENU");
        }
    }
}