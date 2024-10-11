using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;

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

        private static TutorialData _lastTutorialData = null;

        public List<ListData> GetListData<T>(List<T> dataList,int selectIndex = 0)
        {
            return _model.MakeListData(dataList,selectIndex);
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

        public async void PlayTacticsBgm()
        {
            var bgmKey = _model.TacticsBgmKey();
            var bgmData = DataSystem.Data.GetBGM(bgmKey);
            var bgm = await _model.GetBgmData(bgmKey);
            if (bgmData.CrossFade != "")
            {
                SoundManager.Instance.PlayCrossFadeBgm(bgm,1.0f);
            } else
            {
                SoundManager.Instance.PlayBgm(bgm,1.0f);
            }
        }

        public async void PlayBossBgm()
        {
            var bgmData = DataSystem.Data.GetBGM(_model.CurrentStage.Master.BossBGMId);
            var bgm = await _model.GetBgmData(bgmData.Key);
            SoundManager.Instance.PlayBgm(bgm,1.0f);
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
#else
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
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(19500),(a) => 
            {
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

        /// <summary>
        /// ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandStatusInfo(List<ActorInfo> actorInfos,bool inBattle,bool backButton = true,bool levelUpObj = true,bool addActor = false,int startIndex = -1,System.Action closeEvent = null)
        {
            var statusViewInfo = new StatusViewInfo(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            statusViewInfo.SetActorInfos(actorInfos,inBattle);
            if (startIndex > -1)
            {
                statusViewInfo.SetStartIndex(startIndex);
            }
            statusViewInfo.SetDisplayDecideButton(addActor);
            statusViewInfo.SetDisplayCharacterList(true);
            statusViewInfo.SetDisplayLevelResetButton(levelUpObj);
            statusViewInfo.SetDisplayBackButton(backButton);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
        }

        /// <summary>
        /// 敵詳細を表示
        /// </summary>
        /// <param name="battlerInfos"></param>
        public void CommandEnemyInfo(List<BattlerInfo> battlerInfos,bool inBattle,System.Action closeEvent = null)
        {
            var enemyViewInfo = new StatusViewInfo(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            enemyViewInfo.SetEnemyInfos(battlerInfos,inBattle);
            _view.CommandCallEnemyInfo(enemyViewInfo);
            _view.ChangeUIActive(false);
        }

        
        /// <summary>
        /// ステータス詳細を表示
        /// </summary>
        /// <param name="actorInfos"></param>
        public void CommandTacticsStatusInfo(List<ActorInfo> actorInfos,bool inBattle,bool backButton = true,bool levelUpObj = true,bool addActor = false,int startIndex = -1,System.Action closeEvent = null,System.Action<int> charaLayerEvent = null)
        {
            var statusViewInfo = new StatusViewInfo(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.ChangeUIActive(true);
                closeEvent?.Invoke();
            });
            statusViewInfo.SetActorInfos(actorInfos,inBattle);
            if (startIndex > -1)
            {
                statusViewInfo.SetStartIndex(startIndex);
            }
            statusViewInfo.SetDisplayDecideButton(addActor);
            statusViewInfo.SetDisplayCharacterList(!addActor);
            statusViewInfo.SetDisplayLevelResetButton(levelUpObj);
            statusViewInfo.SetDisplayBackButton(backButton);
            statusViewInfo.SetCharaLayerEvent(charaLayerEvent);
            _view.CommandCallTacticsStatus(statusViewInfo);
            //_view.ChangeUIActive(false);
        }

        public void CommandCallSideMenu(List<ListData> sideMenuCommands,System.Action closeEvent = null)
        {
            var sideMenuViewInfo = new SideMenuViewInfo
            {
                EndEvent = () =>
                {
                    closeEvent?.Invoke();
                },
                CommandLists = sideMenuCommands
            };
            _view.CommandCallSideMenu(sideMenuViewInfo);
        }

        public void CloseConfirm()
        {
            _view.CommandGameSystem(Base.CommandType.CloseConfirm);
        }

        public void CommandCautionInfo(string title,int from = -1,int to = -1)
        {
            var cautionInfo = new CautionInfo();
            cautionInfo.SetTitle(title);
            if (from != -1 && to != -1)
            {
                cautionInfo.SetLevelUp(from,to);
            }
            _view.CommandCallCaution(cautionInfo);
        }

        public void CommandActorLevelUp(ActorInfo actorInfo,System.Action endEvent = null)
        {
            if (_model.EnableActorLevelUp(actorInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
                // 新規魔法取得があるか
                var skills = actorInfo.LearningSkills(1);
                
                var from = actorInfo.Evaluate();
                _model.ActorLevelUp(actorInfo);
                var to = actorInfo.Evaluate();
                
                if (skills.Count > 0)
                {
                    //_busy = true;
                    _view.SetBusy(true);
                    var learnSkillInfo = new LearnSkillInfo(from,to,skills[0]);
                    SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                    var popupInfo = new PopupInfo
                    {
                        PopupType = PopupType.LearnSkill,
                        EndEvent = () =>
                        {
                            endEvent?.Invoke();
                            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                        },
                        template = learnSkillInfo
                    };
                    _view.CommandCallPopup(popupInfo);
                } else
                {
                    CommandCautionInfo("",from,to);
                    endEvent?.Invoke();
                    SoundManager.Instance.PlayStaticSe(SEType.CountUp);
                }
            } else
            {
                var textId = _model.ActorLevelLinked(actorInfo) ? 19420 : 19410;
                CommandCautionInfo(DataSystem.GetText(textId));
                endEvent?.Invoke();
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
            }
        }        
        
        public void CommandLevelUp(ActorInfo actorInfo,System.Action endEvent = null)
        {
            CommandActorLevelUp(actorInfo,() => 
            {
                endEvent?.Invoke();
            });
        }

        public void CommandLearnMagic(ActorInfo actorInfo,SkillInfo skillInfo,System.Action endEvent = null)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(19510,skillInfo.LearningCost.ToString()) + DataSystem.GetReplaceText(19520,skillInfo.Master.Name),(a) => UpdatePopupLearnSkill(a,actorInfo,skillInfo,endEvent));
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopupLearnSkill(ConfirmCommandType confirmCommandType, ActorInfo actorInfo,SkillInfo skillInfo,System.Action endEvent = null)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                var from = actorInfo.Evaluate();
                _model.ActorLearnMagic(actorInfo,skillInfo.Id);
                var to = actorInfo.Evaluate();

                var learnSkillInfo = new LearnSkillInfo(from,to,skillInfo);
                SoundManager.Instance.PlayStaticSe(SEType.LearnSkill);

                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.LearnSkill,
                    EndEvent = () =>
                    {
                        endEvent?.Invoke();
                        SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    },
                    template = learnSkillInfo
                };
                _view.CommandCallPopup(popupInfo);
            }
        }
    
        public void BaseCheckTutorialState(int sceneType,Func<TutorialData,bool> checkMethod,Action endEvent,Func<TutorialData,bool> checkEndMethod = null,Action checkTrueAction = null)
        {
            var TutorialDates = _model.SceneTutorialDates(sceneType);
            var tutorialData = TutorialDates.Count > 0 ? TutorialDates[0] : null;
            var checkEndFlag = _lastTutorialData != null && checkEndMethod != null ? checkEndMethod(_lastTutorialData) : false;
            if (checkEndFlag)
            {
                _view.CommandCloseTutorialFocus();
            }
            if (tutorialData != null)
            {
                var checkFlag = checkMethod(tutorialData);
                if (!checkFlag)
                {
                    return;
                }
                if (_lastTutorialData?.Id == tutorialData.Id)
                {
                    return;
                }
            }
            if (tutorialData != null)
            {
                checkTrueAction?.Invoke();
                _lastTutorialData = tutorialData;
                if (tutorialData.Param1 != 3)
                {
                    var popupInfo = new PopupInfo
                    {
                        PopupType = PopupType.Tutorial,
                        template = tutorialData,
                        EndEvent = () =>
                        {
                            endEvent?.Invoke();
                        }
                    };
                    _view.CommandCallPopup(popupInfo);
                }
                _model.ReadTutorialData(tutorialData);
            }
        }
    }
}