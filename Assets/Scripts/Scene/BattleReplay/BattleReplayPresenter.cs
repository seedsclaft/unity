using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class BattleReplayPresenter : BasePresenter
    {
        BattleReplayModel _model = null;
        BattleView _view = null;
        BattlePresenter _presenter;
        bool _busy = false;
        private bool _skipBattle = false;
        private bool _triggerAfterChecked = false;
        private bool _slipDamageChecked = false;
        private bool _regenerateChecked = false;
        private bool _battleEnded = false;
        private Battle.CommandType _nextCommandType = Battle.CommandType.None;

        public BattleReplayPresenter(BattleView view)
        {
            _view = view;
            SetView(_view);
            _model = new BattleReplayModel();
            SetModel(_model);
            _presenter = new BattlePresenter(view);
            _view.SetHelpText("");
            _view.CreateBattleBackGround(_model.BattleBackGroundObject());
            Initialize();
        }

        public async void Initialize()
        {
            var replayData = await SaveSystem.LoadReplay(_model.ReplayFilePath());
            await UniTask.WaitUntil(() => replayData != null);
            _view.SetBattleBusy(true);
            _model.CreateBattleData();
            _model.SetSaveBattleInfo(replayData);
            _view.CommandGameSystem(Base.CommandType.CloseLoading);

            ViewInitialize();
            
            _view.CommandStartTransition(() => 
            {
                StartBattle();
            });
        }

        public void ViewInitialize()
        {
            _view.SetUIButton();

            _view.ClearCurrentSkillData();
            _view.CreateObject();
            _view.RefreshTurn(_model.TurnCount);
            _view.SetBattleAutoButton(_model.BattleAutoButton(),GameSystem.ConfigData.BattleAuto == true);
            _view.ChangeBackCommandActive(false);
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
            _view.SetBattleSkipButton(DataSystem.GetText(62));
            _view.SetSkillLogButton(DataSystem.GetText(63));
            _view.SetActors(_model.BattlerActors());
            _view.SetEnemies(_model.BattlerEnemies());
            _view.BattlerBattleClearSelect();

        }
        private async void StartBattle()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.StartBattleStartAnim(_model.BattleStartText());
            _view.StartUIAnimation();
            await UniTask.WaitUntil(() => _view.StartAnimIsBusy == false);

            //_view.SetBattleBusy(false);
            SeekReplayData();
            _busy = false;
        }

        private async void SeekReplayData()
        {
            var actionData = _model.GetSaveActionData();
            if (actionData != null)
            {
                _presenter.StartActionInfoAnimation(actionData);
                _model.SeekReplayCounter();
                return;
            }
            var actionResultData = _model.GetSaveResultData();
            if (actionResultData != null)
            {
                await _presenter.ExecActionResultInfos(actionResultData);
                _model.SeekReplayCounter();
                SeekReplayData();
                return;
            }
            if (IsBattleEnd())
            {
                BattleEnd();
                return;
            }
        }


        private void UpdateCommand(BattleViewEvent viewEvent)
        {
            if (viewEvent.commandType == Battle.CommandType.ChangeBattleSpeed)
            {
                CommandChangeBattleSpeed();
            }
            if (viewEvent.commandType == Battle.CommandType.SkipBattle)
            {
                CommandSkipBattle();
            }
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case Battle.CommandType.UpdateAp:
                    break;
                case Battle.CommandType.ActorList:
                case Battle.CommandType.EnemyLayer:
                    var targetIndexes = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo(),(int)viewEvent.template);
                    //CommandSelectTargetIndexes(targetIndexes);
                    break;
                case Battle.CommandType.SelectActorList:
                case Battle.CommandType.SelectEnemyList:
                    var targetIndexes2 = _model.ActionInfoTargetIndexes(_model.CurrentActionInfo(),(int)viewEvent.template);
                    _view.UpdateSelectIndexList(targetIndexes2);
                    break;
                case Battle.CommandType.AttributeType:
                    break;
                case Battle.CommandType.SelectEnemy:
                    CommandSelectEnemy();
                    break;
                case Battle.CommandType.StartSelect:
                    break;
                case Battle.CommandType.EnemyDetail:
                    break;
                case Battle.CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
                case Battle.CommandType.SkillLog:
                    CommandSkillLog();
                    break;
            }
        }

        private bool IsBattleEnd()
        {
            return _model.CheckVictory() || _model.CheckDefeat();
        }

        private async void BattleEnd()
        {
            if (_battleEnded == true) return;
            var strategySceneInfo = new StrategySceneInfo();
            strategySceneInfo.ActorInfos = _model.BattleMembers();
            if (_model.CheckVictory())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15020));
                strategySceneInfo.GetItemInfos = _model.MakeBattlerResult();
                _model.MakeBattleScore(true);
            } else
            if (_model.CheckDefeat())
            {
                _view.StartBattleStartAnim(DataSystem.GetText(15030)); 
                strategySceneInfo.GetItemInfos = new List<GetItemInfo>();   
                _model.MakeBattleScore(false);       
            }
            _model.EndBattle();
            _battleEnded = true;
            _view.HideStateOverlay();
            if (_skipBattle)
            {
                _view.CommandGameSystem(Base.CommandType.CallLoading);
            }
            await UniTask.DelayFrame(180);
            _view.SetBattleBusy(false);
            if (SoundManager.Instance.CrossFadeMode)
            {
                SoundManager.Instance.ChangeCrossFade();
            } else
            {
                PlayTacticsBgm();
            }
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
        }

        private void CommandSelectEnemy()
        {
        }

        private void CommandSkillLog()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var SkillLogViewInfo = new SkillLogViewInfo(_model.SkillLogs,() => 
            {
                _busy = false;
            });

            _view.CommandCallSkillLog(SkillLogViewInfo);
        }

        private void CommandSelectSideMenu()
        {
            if (_busy) return;
            var sideMenuViewInfo = new SideMenuViewInfo();
            sideMenuViewInfo.EndEvent = () => {

            };
            sideMenuViewInfo.CommandLists = _model.SideMenu();
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }    

        private void CommandChangeBattleSpeed()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            ConfigUtility.ChangeBattleSpeed(1);
            _view.SetBattleSpeedButton(ConfigUtility.CurrentBattleSpeedText());
        }

        private void CommandSkipBattle()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _skipBattle = true;
            _view.CommandGameSystem(Base.CommandType.CallLoading);
        }
    }
}