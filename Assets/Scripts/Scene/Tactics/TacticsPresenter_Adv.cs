using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class TacticsPresenter : BasePresenter
    {
        
        private void CheckStageEvent()
        {
            // イベントチェック
            var stageEvents = _model.StageEvents(EventTiming.StartTactics);
            foreach (var stageEvent in stageEvents)
            {
                if (_eventBusy)
                {
                    continue;
                }
                switch (stageEvent.Type)
                {
                    case StageEventType.CommandDisable:
                        _model.SetTacticsCommandEnables((TacticsCommandType)stageEvent.Param + 1,false);
                        break;
                    case StageEventType.NeedAllTactics:
                        break;
                    case StageEventType.IsSubordinate:
                        break;
                    case StageEventType.IsAlcana:
                        //_model.SetIsAlcana(stageEvent.Param == 1);
                        break;
                    case StageEventType.SelectAddActor:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        var selectAddActor = new ConfirmInfo(DataSystem.GetText(11050),(menuCommandInfo) => UpdatePopupSelectAddActor((ConfirmCommandType)menuCommandInfo));
                        selectAddActor.SetIsNoChoice(true);
                        selectAddActor.SetSelectIndex(0);
                        _view.CommandCallConfirm(selectAddActor);
                        _view.ChangeUIActive(false);
                        PlayTacticsBgm();
                        
                        break;
                    case StageEventType.SaveCommand:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        CommandSave(true);
                        break;
                    case StageEventType.SetDefineBossIndex:
                        break;
                    case StageEventType.SetRouteSelectParam:
                        _view.CommandSetRouteSelect();
                        break;
                    case StageEventType.ClearStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _model.StageClear();
                        _view.CommandGotoSceneChange(Scene.MainMenu);
                        break;
                    case StageEventType.ChangeRouteSelectStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _view.CommandGotoSceneChange(Scene.Tactics);
                        break;
                    case StageEventType.SetDisplayTurns:
                        break;
                    case StageEventType.MoveStage:
                        break;
                    case StageEventType.SetDefineBoss:
                        //_model.SetDefineBoss(stageEvent.Param);
                        break;
                    case StageEventType.SurvivalMode:
                        _model.SetSurvivalMode();
                        break;
                }
            }
        }

        private bool CheckAdvEvent()
        {
            var StartTacticsAdvData = _model.StartTacticsAdvData();
            if (StartTacticsAdvData != null)
            {
                var advInfo = new AdvCallInfo();
                advInfo.SetLabel(_model.GetAdvFile(StartTacticsAdvData.Id));
                advInfo.SetCallEvent(() => {
                    if (StartTacticsAdvData.EndJump != Scene.None)
                    {
                        _view.CommandSceneChange(StartTacticsAdvData.EndJump);
                    }   
                });
                _view.CommandCallAdv(advInfo);
                _view.ChangeUIActive(false);
            }
            return StartTacticsAdvData != null;
        }

        private bool CheckBeforeTacticsAdvEvent()
        {
            var isAbort = CheckAdvStageEvent(EventTiming.BeforeTactics,() => {
                _view.CommandGotoSceneChange(Scene.Tactics);
            },_model.CurrentStage.SelectActorIdsClassId(0));
            if (isAbort)
            {
                _view.ChangeUIActive(false);
            }
            return isAbort;
        }

        private bool CheckRebornEvent()
        {
            var isReborn = CheckRebornEvent(EventTiming.BeforeTactics,() => {
                _view.CommandGotoSceneChange(Scene.RebornResult);
            });
            if (isReborn)
            {
                _view.ChangeUIActive(false);
            }
            return isReborn;
        }
    }
}
