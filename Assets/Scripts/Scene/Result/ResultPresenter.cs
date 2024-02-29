using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Result;
using Ryneus;

namespace Ryneus
{
    public class ResultPresenter : BasePresenter
    {
        ResultModel _model = null;
        ResultView _view = null;

        private bool _busy = true;

        private bool _isRankingEnd = false;
        private bool _isRebornEnd = false;
        private bool _isAlcanaEnd = false;
        private bool _isSlotSaveEnd = false;
        public ResultPresenter(ResultView view)
        {
            _view = view;
            SetView(_view);
            _model = new ResultModel();
            SetModel(_model);
            Initialize();
        }

        private async void Initialize()
        {
            _busy = true;
            _view.SetHelpWindow();
            _view.SetResultList(_model.ResultCommand());
            _view.SetActors(_model.ResultMembers());
            var bgm = await _model.GetBgmData("TACTICS1");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.SetEvent((type) => UpdateCommand(type));

            _view.StartAnimation();
            _view.StartResultAnimation(_model.MakeListData(_model.ResultMembers()));
            _busy = false;
        }

        private void UpdateCommand(ResultViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            if (viewEvent.commandType == CommandType.EndAnimation)
            {
                CommandEndAnimation();
            }
            if (viewEvent.commandType == CommandType.ResultClose)
            {
                CommandResultClose((ConfirmCommandType)viewEvent.template);
            }
            if (viewEvent.commandType == CommandType.DecideActor)
            {
                CommandDecideActor(0);
            }
            if (viewEvent.commandType == CommandType.UpdateActor)
            {
                CommandUpdateActor();
            }
            if (viewEvent.commandType == CommandType.DecideAlcana)
            {
                CommandDecideAlcana();
            }
        }

        private void CommandEndAnimation()
        {
            _model.ApplyScore();
            _model.SavePlayerStageData(false);
            _view.SetEndingType(_model.EndingTypeText());
            _view.SetEvaluate(_model.TotalEvaluate(),_model.IsNewRecord());
            _view.SetPlayerName(_model.PlayerName());
            _view.SetRankingTypeText(_model.RankingTypeText(_model.CurrentStage.Master.RankingStage));
        }

        private void CommandResultClose(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                CommandResultNext();
            } else
            {
                ShowStatus();
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandResultNext()
        {
            // ランキング登録確認
            if (_isRankingEnd == false)
            {
                _isRankingEnd = true;
                // ランキング登録アリなら
                if (_model.CurrentStage.Master.RankingStage > 0)
                {
                    CommandRanking();
                    return;
                }
            }

            // 思念継承獲得
            if (_isRebornEnd == false && _model.CurrentStage.Master.Reborn)
            {
                _isRebornEnd = true;
                CommandActorAssign();
                return;
            }

            // アルカナ獲得
            if (_isAlcanaEnd == false && _model.CurrentStage.Master.Alcana)
            {
                _isAlcanaEnd = true;
                CommandGetAlcana();
                return;
            }

            // スロットセーブ
            if (_isSlotSaveEnd == false && _model.CurrentStage.Master.SlotSave)
            {
                _isSlotSaveEnd = true;
                var confirmView = new ConfirmInfo(DataSystem.GetTextData(16110).Text,(a) => UpdatePopupSlotSaveOpen());
                confirmView.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmView);
                return;
            }
            CommandEndGame();
        }

        private void CommandRanking()
        {
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(16010).Text,(a) => UpdateAddRankingPopup((ConfirmCommandType)a));
            popupInfo.SetSelectIndex(1);
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdateAddRankingPopup(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                // ランキングに登録
                _view.CommandGameSystem(Base.CommandType.CallLoading);
                _busy = true;
                _model.CurrentRankingData((a) => 
                {
                    _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                    _view.SetRanking(a);
                    _view.CommandGameSystem(Base.CommandType.CloseLoading);
                    _busy = false;
                    CommandResultNext();
                });
            } else
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                CommandResultNext();
            }
        }

        private void UpdateStageEndCommand()
        {
            _view.UpdateResultCommand(_model.StageEndCommand());
        }

        private void ShowStatus()
        {
            _model.SetActors();
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                _view.SetHelpInputInfo("RESULT");
            });
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void CommandActorAssign()
        {
            _model.GetRebornSkills();
            int textId = 16040;
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(textId).Text,(a) => UpdatePopupReborn((ConfirmCommandType)a));
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdatePopupReborn(ConfirmCommandType confirmCommandType)
        {
            _view.CommandActorAssign();
            _view.SetActorList(_model.EraseActorInfos());
            CommandUpdateActor();
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (_model.ActorInfos().Count > 10)
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetTextData(16051).Text,(a) => UpdatePopupRebornEraseCheck((ConfirmCommandType)a));
                popupInfo.SetIsNoChoice(true);
                _view.CommandCallConfirm(popupInfo);
            }
        }

        private void UpdatePopupRebornEraseCheck(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }
        
        private void CommandDecideActor(int index)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            if (_model.ActorInfos().Count > 10)
            {
                var popupInfo = new ConfirmInfo(DataSystem.GetTextData(16060).Text,(a) => UpdatePopupRebornErase((ConfirmCommandType)a));
                _view.CommandCallConfirm(popupInfo);
                return;
            }
            CommandResultNext();
        }

        private void UpdatePopupRebornErase(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                _model.EraseReborn();
                CommandResultNext();
            } else{        
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }
        }

        private void CommandUpdateActor()
        {
            _model.SetRebornActorIndex(_view.ActorInfoListIndex);
            var rebornActor = _model.RebornActorInfo();
            if (rebornActor != null)
            {
                _view.UpdateActor(ListData.MakeListData(rebornActor.RebornSkillInfos));
            }
        }

        public void CommandGetAlcana()
        {
            // ポップアップ表示        
            var popupInfo = new ConfirmInfo(DataSystem.GetTextData(16100).Text,(a) => UpdateGetAlcanaPopup((ConfirmCommandType)a));
            popupInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(popupInfo);
        }

        private void UpdateGetAlcanaPopup(ConfirmCommandType confirmCommandType)
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            CommandAlcanaRefresh();
        }

        private void CommandDecideAlcana()
        {
            _view.CommandDecideAlcana();
            UpdateStageEndCommand();
        }    
        
        private void UpdatePopupSlotSaveOpen()
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var slotView = new SlotSaveViewInfo();
            slotView.EndEvent = () => {
                _isSlotSaveEnd = true;
                _view.CommandGameSystem(Base.CommandType.ClosePopup);
            };
            slotView.SlotInfo = _model.MakeSlotInfo();
            _view.CommandSlotSave(slotView);
        }

        private void CommandEndGame()
        {
            _model.SavePlayerData();
            _model.SavePlayerStageData(false);
            _view.CommandSceneChange(Scene.MainMenu);
        }

        private void CommandAlcanaRefresh()
        {
            var skillInfos = _model.GetAlcanaSkillInfos();
            _view.CommandAlcanaInfos(skillInfos);
        }

        private void CommandRebornRefresh()
        {
            var skillInfos = _model.RebornSkillInfos(_model.RebornActorInfo());
            var lastSelectIndex = skillInfos.FindIndex(a => ((SkillInfo)a.Data).Id == _model.RebornActorInfo().LastSelectSkillId);
            if (lastSelectIndex == -1)
            {
                lastSelectIndex = 0;
            }
            _view.CommandRefreshStatus(skillInfos,_model.RebornActorInfo(),_model.PartyMembers(),lastSelectIndex);
        }
    }
}