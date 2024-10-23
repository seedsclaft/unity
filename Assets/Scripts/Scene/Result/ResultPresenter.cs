using System.Collections.Generic;
using UnityEngine;
using Result;
using Ryneus;
using Unity.VisualScripting;

namespace Ryneus
{
    public class ResultPresenter : BasePresenter
    {
        ResultModel _model = null;
        ResultView _view = null;

        private bool _busy = true;
        private bool _isRankingEnd = false;
        private bool _isClearPrizeEnd = false;


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

            _view.InitActors();
            _view.InitResultList(GetListData(_model.ResultCommand()));
            _view.SetBackGround(_model.CurrentStage.Master.BackGround);
            var bgm = await _model.GetBgmData(_model.TacticsBgmKey());
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _view.SetEvent((type) => UpdateCommand(type));

            CommandStartResult();
            _busy = false;
        }

        private void UpdateCommand(ResultViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.StartResult:
                    CommandStartResult();
                    break;
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.ResultClose:
                    CommandResultClose((SystemData.CommandData)viewEvent.template);
                    break;
            }
        }

        private void CommandStartResult()
        {
            _view.StartResultAnimation(_model.MakeListData(_model.TacticsActors()));
        }

        private void CommandEndAnimation()
        {
            _view.ShowResultList(GetListData(_model.ResultViewInfos),
                _model.BattleTotalScore());
        }

        private void CommandResultClose(SystemData.CommandData commandData)
        {
            if (commandData.Key == "Yes")
            {
                if (_isRankingEnd == false)
                {
                    CheckRanking();
                } else
                {
                    // 獲得ボーナス表示
                    CheckClearPrize();
                }
            } else
            {
                ShowStatus();
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
    
        private void ShowStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CommandStatusInfo(_model.StageMembers(),false,true,false,false,-1,() => 
            {
                _view.SetHelpText(DataSystem.GetText(20020));
            });
        }

        private void CheckRanking()
        {
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(23040),(a) => 
            {
                if (a == ConfirmCommandType.Yes)
                {
                    // ランキングに登録
                    _view.CommandGameSystem(Base.CommandType.CallLoading);
                    _busy = true;
                    _model.CurrentRankingData((a) => 
                    {
                        _view.SetRanking(a);
                        _view.CommandGameSystem(Base.CommandType.CloseLoading);
                        _busy = false;
                        _isRankingEnd = true;
                        SaveSystem.DeleteStageData();
                    });
                } else
                {
                    _isRankingEnd = true;
                    SaveSystem.DeleteStageData();
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CheckClearPrize()
        {
            _isClearPrizeEnd = true;
            _model.ClearGame();
            _model.SavePlayerData();
            //_model.SavePlayerStageData(false);
            EndStage();
        }

        private void EndStage()
        {
            _view.CommandGotoSceneChange(Scene.Title);
        }
    }
}