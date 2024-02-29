using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Title;

namespace Ryneus
{
    public class TitlePresenter : BasePresenter
    {
        TitleModel _model = null;
        TitleView _view = null;
        private bool _busy = true;
        public TitlePresenter(TitleView view)
        {
            _view = view;
            SetView(_view);
            _model = new TitleModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;
            _model.ApplyConfigData();

            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetVersion(_model.VersionText());
            _view.SetHelpWindow();
            _view.SetTitleCommand(_model.TitleCommand());
            CommandRefresh();
            var bgm = await _model.GetBgmData("TITLE");
            Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,false);
            _busy = false;
        }

        private void UpdateCommand(TitleViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            if (viewEvent.commandType == CommandType.TitleCommand)
            {
                CommandTitle((int) viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.SelectSideMenu)
            {
                CommandSelectSideMenu();
            }
            if (viewEvent.commandType == CommandType.SelectTitle)
            {
                var loadFile = SaveSystem.ExistsLoadPlayerFile();
                if (loadFile)
                {
                    CommandContinue();
                } else
                {
                    CommandNewGame();
                }
            }
        }

        private void CommandTitle(int commandIndex){
            _busy = true;
            switch ((TitleCommandType)commandIndex){
                case TitleCommandType.NewGame:
                    CommandNewGame();
                break;
                case TitleCommandType.Continue:
                    CommandContinue();
                    break;
            }
        }

        private void CommandNewGame()
        {
            _model.InitSaveInfo();
            _view.CommandGotoSceneChange(Scene.NameEntry);
        }

        private void CommandContinue()
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var loadSuccess = SaveSystem.LoadPlayerInfo();
            if (loadSuccess == false)
            {
                var popupInfo = new ConfirmInfo("セーブデータを読み込めませんでした。\n誠に申し訳ないですがNewGameから開始をお願いします。",(menuCommandInfo) => updatePopup((ConfirmCommandType)menuCommandInfo));
                popupInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(popupInfo);
                return;
            }
            // プレイヤーネームを設定しなおし
            _view.CommandDecidePlayerName(GameSystem.CurrentData.PlayerInfo.PlayerName);
            
            var loadStage = SaveSystem.ExistsStageFile();
            if (loadStage)
            {
                SaveSystem.LoadStageInfo();
                // 習得データを変更
                GameSystem.CurrentStageData.Party.ActorInfos.ForEach(a => a.UpdateLearningDates(DataSystem.Actors.Find(b => b.Id == a.ActorId).LearningSkills));
            } else
            {
                _model.InitSaveStageInfo();
            }
            if (GameSystem.CurrentStageData.ResumeStage)
            {
                _view.CommandGotoSceneChange(Scene.Tactics);
            } else{
                _view.CommandGotoSceneChange(Scene.MainMenu);
            }
        }

        private void CommandRefresh(){
            int selectIndex = 0;
            if (_model.ExistsLoadFile())
            {
                selectIndex = 1;
            }
            _view.RefreshCommandIndex(selectIndex);
            //_view.RefreshView();
        }

        private void CommandSelectSideMenu()
        {
            var sideMenuViewInfo = new SideMenuViewInfo();
            sideMenuViewInfo.EndEvent = () => {

            };
            sideMenuViewInfo.CommandLists = _model.SideMenu();
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }

        private void updatePopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}