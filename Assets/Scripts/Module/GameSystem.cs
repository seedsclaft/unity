using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

namespace Ryneus
{
    public class GameSystem : MonoBehaviour
    {
        [SerializeField] private string version = "";
        [SerializeField] private bool testMode = false;
        [SerializeField] private SceneAssign sceneAssign = null;
        [SerializeField] private PopupAssign popupAssign = null;
        [SerializeField] private StatusAssign statusAssign = null;
        [SerializeField] private ConfirmAssign confirmAssign = null;

        [SerializeField] private GameObject transitionRoot = null;
        [SerializeField] private Fade transitionFade = null;
        [SerializeField] private LoadingView loadingView = null;
        [SerializeField] private AdvEngine advEngine = null;
        [SerializeField] private AdvController advController = null;

        [SerializeField] private DebugBattleData debugBattleData = null;
        [SerializeField] private HelpWindow helpWindow = null;
        [SerializeField] private HelpWindow advHelpWindow = null;
        [SerializeField] private TutorialView tutorialView = null;
        
        private BaseView _currentScene = null;

        private BaseModel _model = null;
        
        public static SaveInfo CurrentData = null;
        public static SaveStageInfo CurrentStageData = null;
        public static SaveConfigInfo ConfigData = null;
        public static TempInfo TempData = null;

        private bool _busy = false;
        public bool Busy => _busy;

        public static string Version;
        public static DebugBattleData DebugBattleData;

        private static SceneStackManager _sceneStackManager = new SceneStackManager();
        public static SceneStackManager SceneStackManager => _sceneStackManager;


        private void Awake() 
        {
    #if UNITY_WEBGL || UNITY_ANDROID || UNITY_STANDALONE_WIN// && !UNITY_EDITOR
            FirebaseController.Instance.Initialize();
    #endif
            Application.targetFrameRate = 60;
            advController.Initialize();
            advController.SetHelpWindow(advHelpWindow);
            transitionRoot.SetActive(false);
            loadingView.Initialize();
            loadingView.gameObject.SetActive(false);
            tutorialView.Initialize();
            tutorialView.HideFocusImage();
            transitionFade.Init();
            statusAssign.CloseStatus();
            TempData = new TempInfo();
            _model = new BaseModel();
            Version = version;
    #if UNITY_EDITOR
            DebugBattleData = debugBattleData;
    #endif
    #if UNITY_ANDROID
            AdMobController.Instance.Initialize(() => {CommandSceneChange(Scene.Boot);});
    #else
            CommandSceneChange(new SceneInfo(){ToScene = Scene.Boot});
    #endif
        }

        private BaseView CreateStatus(StatusType statusType,StatusViewInfo statusViewInfo)
        {
            var prefab = statusAssign.CreatePopup(statusType,helpWindow);
            if (statusType == StatusType.Status)
            {
                prefab.GetComponent<StatusView>().Initialize(statusViewInfo.ActorInfos);
            } else
            {
                prefab.GetComponent<EnemyInfoView>().Initialize(statusViewInfo.EnemyInfos,statusViewInfo.IsBattle);
            }
            return prefab.GetComponent<BaseView>();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case Base.CommandType.SceneChange:
                    var sceneInfo = (SceneInfo)viewEvent.template; 
                    if (testMode && sceneInfo.ToScene == Scene.Battle)
                    {
                        if (debugBattleData.AdvName != "")
                        {
                            StartCoroutine(JumpScenarioAsync(debugBattleData.AdvName,null));
                        } else
                        {
                            debugBattleData.MakeBattleActor();
                            CommandSceneChange(sceneInfo);
                        }
                    } else
                    {
                        CommandSceneChange(sceneInfo);
                    }
                    break;
                case Base.CommandType.CallConfirmView:
                case Base.CommandType.CallSkillDetailView:
                    CommandConfirmView((ConfirmInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallCautionView:
                    CommandCautionView((CautionInfo)viewEvent.template);
                    break;
                case Base.CommandType.ClosePopup:
                    popupAssign.ClosePopup();
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.CloseConfirm:
                    confirmAssign.CloseConfirm();
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.CallPopupView:
                    CommandPopupView((PopupInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallOptionView:
                    CommandOptionView((System.Action)viewEvent.template);
                    break;
                case Base.CommandType.CallSideMenu:
                    CommandSideMenu((SideMenuViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallRankingView:
                    CommandRankingView((RankingViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallCharacterListView:
                    CommandCharacterListView((CharacterListInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallHelpView:
                    CommandHelpView((List<ListData>)viewEvent.template);
                    break;
                case Base.CommandType.CallSlotSaveView:
                    break;
                case Base.CommandType.CallSkillTriggerView:
                    CommandSkillTriggerView((SkillTriggerViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallSkillLogView:
                    CommandCallSkillLogView((SkillLogViewInfo)viewEvent.template);
                    break;
                case Base.CommandType.CallStatusView:
                    var statusViewInfo = (StatusViewInfo)viewEvent.template;
                    var statusView = CreateStatus(StatusType.Status,statusViewInfo) as StatusView;
                    statusView.SetEvent((type) => UpdateCommand(type));
                    statusView.SetViewInfo(statusViewInfo);
                    _currentScene.SetBusy(true);
                    break;
                case Base.CommandType.CloseStatus:
                    statusAssign.CloseStatus();
                    _currentScene.SetBusy(false);
                    break;
                case Base.CommandType.CallEnemyInfoView:
                    var enemyStatusInfo = (StatusViewInfo)viewEvent.template;
                    var enemyInfoView = CreateStatus(StatusType.EnemyDetail,enemyStatusInfo) as EnemyInfoView;
                    enemyInfoView.SetEvent((type) => UpdateCommand(type));
                    enemyInfoView.SetBackEvent(enemyStatusInfo.BackEvent);
                    _currentScene.SetBusy(true);
                    break;
                case Base.CommandType.CallAdvScene:
                    SetIsBusyMainAndStatus();
                    var advCallInfo = viewEvent.template as AdvCallInfo;
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                    //_currentScene.SetActiveUi(false);
                    StartCoroutine(JumpScenarioAsync(advCallInfo.Label,advCallInfo.CallEvent));
                    break;
                case Base.CommandType.DecidePlayerName:
                    string playerName = (string)advEngine.Param.GetParameter("PlayerName");
                    advEngine.Param.SetParameterString("PlayerName",(string)viewEvent.template);
                    break;
                case Base.CommandType.CallLoading:
                    loadingView.gameObject.SetActive(true);
                    SetIsBusyMainAndStatus();
                    break;
                case Base.CommandType.CloseLoading:
                    loadingView.gameObject.SetActive(false);
                    SetIsNotBusyMainAndStatus();
                    break;
                case Base.CommandType.SetRouteSelect:
                    int routeSelect = (int)advEngine.Param.GetParameter("RouteSelect");
                    CurrentStageData.CurrentStage.SetRouteSelect(routeSelect);
                    break;
                case Base.CommandType.ChangeViewToTransition:
                    transitionRoot.SetActive(true);
                    _currentScene.gameObject.transform.SetParent(transitionRoot.transform, false);
                    _currentScene = null;
                    break;
                case Base.CommandType.StartTransition:
                    transitionFade.FadeIn(0.8f,() => {
                        foreach(Transform child in transitionRoot.transform){
                            var endEvent = (System.Action)viewEvent.template;
                            if ((System.Action)viewEvent.template != null) endEvent();
                            Destroy(child.gameObject);
                            transitionFade.FadeOut(0);
                            transitionRoot.SetActive(false);
                        }
                    });
                    break;
                case Base.CommandType.CallTutorialFocus:
                    var stageTutorialData = (StageTutorialData)viewEvent.template;
                    tutorialView.SeekFocusImage(stageTutorialData);
                    break;
                case Base.CommandType.CloseTutorialFocus:
                    tutorialView.HideFocusImage();
                    break;
                case Base.CommandType.SceneHideUI:
                    SceneHideUI();
                    break;
                case Base.CommandType.SceneShowUI:
                    SceneShowUI();
                    break;
            }
        }

        private void CommandConfirmView(ConfirmInfo confirmInfo)
        {
            var prefab = confirmAssign.CreateConfirm(confirmInfo.ConfirmType,helpWindow);
            var confirmView = prefab.GetComponent<ConfirmView>();
            confirmView.Initialize();
            confirmView.SetViewInfo(confirmInfo);
            SetIsBusyMainAndStatus();
        }

        private void CommandCautionView(CautionInfo confirmInfo)
        {
            var prefab = confirmAssign.CreateConfirm(ConfirmType.Caution,helpWindow);
            var confirmView = prefab.GetComponent<CautionView>();
            confirmView.Initialize();
            if (confirmInfo.Title != null)
            {
                confirmView.SetTitle(confirmInfo.Title);
            }
            if (confirmInfo.From > 0 && confirmInfo.To > 0 )
            {
                confirmView.SetLevelup(confirmInfo.From,confirmInfo.To);
            }
            //SetIsBusyMainAndStatus();
        }

        private void CommandPopupView(PopupInfo popupInfo)
        {
            var prefab = popupAssign.CreatePopup(popupInfo.PopupType,helpWindow);
            var baseView = prefab.GetComponent<BaseView>();
            baseView.SetEvent((type) => UpdateCommand(type));
            baseView.Initialize();
            baseView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                if (popupInfo.EndEvent != null) popupInfo.EndEvent();
            });
            if (popupInfo.PopupType == PopupType.LearnSkill)
            {
                var learnSkill = prefab.GetComponent<LearnSkillView>();
                learnSkill.SetLearnSkillInfo((LearnSkillInfo)popupInfo.template);
            }
            SetIsBusyMainAndStatus();
        }
        
        private void CommandOptionView(System.Action endEvent)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Option,helpWindow);
            var optionView = prefab.GetComponent<OptionView>();
            optionView.Initialize();
            optionView.SetBackEvent(() => 
            {
                ConfigData.UpdateSoundParameter(
                    SoundManager.Instance.BGMVolume,
                    SoundManager.Instance.BGMMute,
                    SoundManager.Instance.SeVolume,
                    SoundManager.Instance.SeMute
                );
                SaveSystem.SaveConfigStart(ConfigData);
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                if (endEvent != null) endEvent();
            });
            optionView.SetEvent((type) => UpdateCommand(type));
            SetIsBusyMainAndStatus();
        }

        private void CommandSkillTriggerView(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SkillTrigger,helpWindow);
            var skillTriggerView = prefab.GetComponent<SkillTriggerView>();
            skillTriggerView.SetSkillTriggerViewInfo(skillTriggerViewInfo);
            skillTriggerView.Initialize();
            skillTriggerView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                if (skillTriggerViewInfo.EndEvent != null) skillTriggerViewInfo.EndEvent();
            });
            skillTriggerView.SetEvent((type) => UpdateCommand(type));
            SetIsBusyMainAndStatus();
        }

        private void CommandCallSkillLogView(SkillLogViewInfo skillLogViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SkillLog,helpWindow);
            var skillLogView = prefab.GetComponent<SkillLogView>();
            skillLogView.Initialize();
            skillLogView.SetSkillLogViewInfo(skillLogViewInfo.SkillLogListInfos);
            skillLogView.SetEvent((type) => UpdateCommand(type));
            skillLogView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                if (skillLogViewInfo.EndEvent != null) skillLogViewInfo.EndEvent();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandSideMenu(SideMenuViewInfo sideMenuViewInfo)
        {
            var prefab = statusAssign.CreatePopup(StatusType.SideMenu,helpWindow);
            var sideMenuView = prefab.GetComponent<SideMenuView>();
            sideMenuView.Initialize();
            sideMenuView.SetEvent((type) => UpdateCommand(type));
            sideMenuView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.CloseStatus));
                sideMenuViewInfo.EndEvent?.Invoke();
            });
            sideMenuView.SetSideMenuViewInfo(sideMenuViewInfo);
            _currentScene.SetBusy(true);
        }
        
        private void CommandRankingView(RankingViewInfo rankingViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Ranking,helpWindow);
            var rankingView = prefab.GetComponent<RankingView>();
            rankingView.Initialize();
            rankingView.SetEvent((type) => UpdateCommand(type));
            rankingView.SetRankingViewInfo(rankingViewInfo);
            rankingView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                if (rankingViewInfo.EndEvent != null) rankingViewInfo.EndEvent();
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandCharacterListView(CharacterListInfo characterListInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.CharacterList,helpWindow);
            var characterListView = prefab.GetComponent<CharacterListView>();
            characterListView.Initialize(characterListInfo.ActorInfos);
            characterListView.SetViewInfo(characterListInfo);
            characterListView.SetBackEvent(() => 
            {
                if (characterListInfo.BackEvent != null)
                {
                    characterListInfo.BackEvent();
                }
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandHelpView(List<ListData> helpTextList)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Help,helpWindow);
            var helpView = prefab.GetComponent<HelpView>();
            helpView.Initialize();
            helpView.SetHelp(helpTextList);
            helpView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
            });
            SetIsBusyMainAndStatus();
        }

        IEnumerator JumpScenarioAsync(string label, System.Action onComplete)
        {
            _busy = true;
            advHelpWindow.SetInputInfo("ADV_READING");
            while (advEngine.IsWaitBootLoading) yield return null;
            while (advEngine.IsLoading) yield return null;
            advEngine.JumpScenario(label);
            advEngine.Config.IsSkip = ConfigData.EventSkipIndex;
            advController.StartAdv();
            while (!advEngine.IsEndOrPauseScenario)
            {
                yield return null;
            }
            SetIsNotBusyMainAndStatus();
            advController.EndAdv();
            advHelpWindow.SetInputInfo("");
            
            //_currentScene.SetActiveUi(true);
            _busy = false;
            if(onComplete != null) onComplete();
        }

        public void CommandSceneChange(SceneInfo sceneInfo)
        {
            if (_currentScene != null)
            { 
                Destroy(_currentScene.gameObject);
                ResourceSystem.ReleaseAssets();
                ResourceSystem.ReleaseScene();
                Resources.UnloadUnusedAssets();
            }
            if (sceneInfo.SceneChangeType == SceneChangeType.Pop)
            {
                sceneInfo.FromScene = _sceneStackManager.LastScene;
                sceneInfo.ToScene = _sceneStackManager.LastScene;
            } else
            {
                sceneInfo.FromScene = _sceneStackManager.Current;
            }
            var prefab = sceneAssign.CreateScene(sceneInfo.ToScene,helpWindow);
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetBattleTestMode(debugBattleData.TestBattle);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(sceneInfo);
            _currentScene.Initialize();
            tutorialView.HideFocusImage();
        }

        private void SetIsBusyMainAndStatus()
        {
            _currentScene.SetBusy(true);
            statusAssign.SetBusy(true);
        }

        private void SetIsNotBusyMainAndStatus()
        {
            if (!statusAssign.StatusRoot.gameObject.activeSelf) _currentScene.SetBusy(false);
            statusAssign.SetBusy(false);
        }

        private void SceneShowUI()
        {
            sceneAssign.ShowUI();
        }

        private void SceneHideUI()
        {
            sceneAssign.HideUI();
        }
    }


    public class AdvCallInfo{
        private string _label;
        public string Label { get {return _label;}}
        public void SetLabel(string label)
        {
            _label = label;
        }
        private System.Action _callEvent;
        public System.Action CallEvent { get {return _callEvent;}}
        public void SetCallEvent(System.Action callEvent)
        {
            _callEvent = callEvent;
        }
    }
}