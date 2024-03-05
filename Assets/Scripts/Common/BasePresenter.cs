using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BasePresenter 
    {
        private BaseView _view = null;
        public void SetView(BaseView view)
        {
            _view = view;
        }

        private BaseModel _model = null;
        public void SetModel(BaseModel model)
        {
            _model = model;
        }

        public bool CheckAdvStageEvent(EventTiming eventTiming,System.Action endCall,int selectActorId = 0)
        {
            var isAbort = false;
            var advId = -1;
            var stageEvents = _model.StageEvents(eventTiming);
            foreach (var stageEvent in stageEvents)
            {
                if (stageEvent.Type == StageEventType.AdvStart)
                {
                    advId = stageEvent.Param;
                    _model.AddEventReadFlag(stageEvent);
                    isAbort = true;
                    break;
                }
                if (stageEvent.Type == StageEventType.SelectActorAdvStart)
                {
                    advId = stageEvent.Param + selectActorId;
                    _model.AddEventReadFlag(stageEvent);
                    isAbort = true;
                    break;
                }
            }
            if (isAbort)
            {
                var advInfo = new AdvCallInfo();
                advInfo.SetLabel(_model.GetAdvFile(advId));
                advInfo.SetCallEvent(() => {                
                    if (endCall != null) endCall();
                });
                _view.CommandCallAdv(advInfo);
            }
            return isAbort;
        }

        public bool CheckRebornEvent(EventTiming eventTiming,System.Action endCall)
        {
            var isReborn = false;
            var stageEvents = _model.StageEvents(eventTiming);
            foreach (var stageEvent in stageEvents)
            {
                if (stageEvent.Type == StageEventType.RebornSkillEffect)
                {
                    _model.AddEventReadFlag(stageEvent);
                    isReborn = true;
                    break;
                }
            }
            if (isReborn)
            {
                if (endCall != null) endCall();
            }
            return isReborn;
        }

        public async void PlayTacticsBgm()
        {
            var bgmKey = _model.TacticsBgmKey();
            var bgmData = DataSystem.Data.GetBGM(bgmKey);
            var bgm = await _model.GetBgmData(bgmKey);
            if (bgmData.CrossFade != "")
            {
                Ryneus.SoundManager.Instance.PlayCrossFadeBgm(bgm,1.0f);
            } else
            {
                Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f);
            }
        }

        public async void PlayBossBgm()
        {
            var bgmData = DataSystem.Data.GetBGM(_model.CurrentStage.Master.BossBGMId);
            var bgm = await _model.GetBgmData(bgmData.Key);
            Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f);
        }
        
        public void CommandSave(bool isReturnScene)
        {
    #if UNITY_ANDROID
            var savePopupTitle = _model.SavePopupTitle();
            var saveNeedAds = _model.NeedAdsSave();
            var popupInfo = new ConfirmInfo(savePopupTitle,(a) => UpdatePopupSaveCommand((ConfirmCommandType)a,isReturnScene));
            
            popupInfo.SetSelectIndex(1);
            if (saveNeedAds)
            {
                //popupInfo.SetDisableIds(new List<int>(){1});
                popupInfo.SetCommandTextIds(_model.SaveAdsCommandTextIds());
            } else
            {
            }
            _view.CommandCallConfirm(popupInfo);
            _view.ChangeUIActive(false);
    #elif UNITY_WEBGL
            SuccessSave(isReturnScene);
    #endif
        }

        private void SuccessSave(bool isReturnScene)
        {
            // ロード非表示
            _view.CommandGameSystem(Base.CommandType.CloseLoading);
            _model.GainSaveCount();
            _model.SavePlayerStageData(true);
            // 成功表示
            var confirmInfo = new ConfirmInfo(DataSystem.GetTextData(11084).Text,(a) => {
                _view.CommandGameSystem(Base.CommandType.CloseConfirm);
                if (isReturnScene)
                {
                    _view.CommandGotoSceneChange(Scene.Tactics);
                } else
                {        
                    _view.ChangeUIActive(true);
                }
            });
            confirmInfo.SetIsNoChoice(true);
            _view.CommandCallConfirm(confirmInfo);
        }

    }
}