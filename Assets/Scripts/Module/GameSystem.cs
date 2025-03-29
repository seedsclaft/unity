using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Utage;

namespace Ryneus
{
    public class GameSystem : MonoBehaviour
    {
        [SerializeField] private string version = "";
        [SerializeField] private bool testMode = false;
        [SerializeField] private SceneAssign sceneAssign = null;
        [SerializeField] private MapAssign mapAssign = null;
        [SerializeField] private PopupAssign popupAssign = null;
        [SerializeField] private StatusAssign statusAssign = null;
        [SerializeField] private ConfirmAssign confirmAssign = null;

        [SerializeField] private Canvas uiCanvas = null;
        public static Canvas UiCanvas;
        [SerializeField] private GameObject transitionRoot = null;
        [SerializeField] private Fade transitionFade = null;
        [SerializeField] private LoadingView loadingView = null;
        [SerializeField] private TutorialView tutorialView = null;
        [SerializeField] private AdvEngine advEngine = null;
        [SerializeField] private AdvController advController = null;

        [SerializeField] private EventSystem eventSystem = null;
        
        private BaseView _currentScene = null;

        private BaseModel _model = null;
        
        public static SaveInfo CurrentData = null;
        public static SaveGameInfo CurrentStageData = null;
        public static SaveConfigInfo ConfigData = null;
        public static TempInfo TempData = null;
        private static TutorialData _lastTutorialData = null;

        private bool _busy = false;
        public bool Busy => _busy;

        public static string Version;

        private static SceneStackManager _sceneStackManager = new SceneStackManager();
        public static SceneStackManager SceneStackManager => _sceneStackManager;


        private void Awake() 
        {
    #if UNITY_WEBGL || UNITY_ANDROID || UNITY_STANDALONE_WIN// && !UNITY_EDITOR
            //FirebaseController.Instance.Initialize();
    #endif
            Application.targetFrameRate = 60;
            advController.Initialize();
            transitionRoot.SetActive(false);
            loadingView.Initialize();
            loadingView.gameObject.SetActive(false);
            transitionFade.Init();
            tutorialView.Initialize();
            statusAssign.CloseStatus();
            InputSystem.Initialize();
            UiCanvas = uiCanvas;
            TempData = new TempInfo();
            _model = new BaseModel();
            Version = version;
    #if UNITY_EDITOR
    #endif
    #if UNITY_ANDROID
            AdMobController.Instance.Initialize(() => {CommandSceneChange(Scene.Boot);});
    #else
            CommandSceneChange(new SceneInfo(){ToScene = Scene.Boot});
    #endif
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
                    {
                        CommandSceneChange(sceneInfo);
                    }
                    break;
                case Base.CommandType.MapChange:
                    var mapType = (MapType)viewEvent.template; 
                    CommandMapChange(mapType);
                    break;
                case Base.CommandType.MapClear:
                    CommandMapClear();
                    break;
                case Base.CommandType.CreateMapObject:
                    var mapObject = (GameObject)viewEvent.template; 
                    CommandCreateMapObject(mapObject);
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
                case Base.CommandType.ClosePopupAll:
                    popupAssign.ClosePopupAll();
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
                    break;
                case Base.CommandType.CallCharacterListView:
                    break;
                case Base.CommandType.CallHelpView:
                    CommandHelpView((List<ListData>)viewEvent.template);
                    break;
                case Base.CommandType.CallSlotSaveView:
                    break;
                case Base.CommandType.CallSkillTriggerView:
                    break;
                case Base.CommandType.CallSkillLogView:
                    break;
                case Base.CommandType.CallStatusView:
                    break;
                case Base.CommandType.CloseStatus:
                    statusAssign.CloseStatus();
                    _currentScene.SetBusy(false);
                    break;
                case Base.CommandType.CallEnemyInfoView:
                    break;
                case Base.CommandType.CallTacticsStatusView:

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
                    break;
                case Base.CommandType.CloseTutorialFocus:
                    if (popupAssign.StackPopupView != null)
                    {
                        if (popupAssign.StackPopupView.Find(a => a.GetType() == typeof(TutorialView)) != null) 
                        {                
                            popupAssign.CloseTutorialPopup();
                        }
                    }
                    break;
                case Base.CommandType.CheckTutorialState:
                    CheckTutorialState((TutorialViewInfo)viewEvent.template);
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
            var prefab = confirmAssign.CreateConfirm(confirmInfo.ConfirmType);
            var confirmView = prefab.GetComponent<ConfirmView>();
            confirmView.Initialize();
            confirmView.SetViewInfo(confirmInfo);
            confirmView.SetBackEvent(() => 
            {
                confirmInfo.BackEvent?.Invoke();
                UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            });
            SetIsBusyMainAndStatus();
        }

        private void CommandCautionView(CautionInfo confirmInfo)
        {
            var prefab = confirmAssign.CreateConfirm(ConfirmType.Caution);
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

        private async void CommandPopupView(PopupInfo popupInfo)
        {
            var prefab = popupAssign.CreatePopup(popupInfo.PopupType);
            var baseView = prefab.GetComponent<BaseView>();
            baseView.SetEvent((type) => UpdateCommand(type));
            baseView.Initialize();
            baseView.SetBackEvent(() => 
            {
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                popupInfo.EndEvent?.Invoke();
            });
            SetIsBusyMainAndStatus();
        }
        
        private void CommandOptionView(System.Action endEvent)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Option);
            var optionView = prefab.GetComponent<OptionView>();
            optionView.Initialize();
            optionView.SetBackEvent(() => 
            {
                ConfigData.UpdateSoundParameter(
                    SoundManager.Instance.BgmVolume,
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



        private void CommandSideMenu(SideMenuViewInfo sideMenuViewInfo)
        {
            var prefab = popupAssign.CreatePopup(PopupType.SideMenu);
            var sideMenuView = prefab.GetComponent<SideMenuView>();
            sideMenuView.Initialize();
            sideMenuView.SetEvent((type) => UpdateCommand(type));
            sideMenuView.SetBackEvent(() => 
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                UpdateCommand(new ViewEvent(Base.CommandType.ClosePopup));
                sideMenuViewInfo.EndEvent?.Invoke();
            });
            sideMenuView.SetSideMenuViewInfo(sideMenuViewInfo);
        }
        


        private void CommandHelpView(List<ListData> helpTextList)
        {
            var prefab = popupAssign.CreatePopup(PopupType.Help);
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
            var prefab = sceneAssign.CreateScene(sceneInfo.ToScene);
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(sceneInfo);
            _currentScene.Initialize();
            //tutorialView.HideFocusImage();
        }

        public void CommandMapChange(MapType mapType)
        {
            var prefab = mapAssign.CreateMap(mapType);
            /*
            _currentScene = prefab.GetComponent<BaseView>();
            _currentScene.SetTestMode(testMode);
            _currentScene.SetBattleTestMode(debugBattleData.TestBattle);
            _currentScene.SetEvent((type) => UpdateCommand(type));
            _sceneStackManager.PushSceneInfo(mapType);
            _currentScene.Initialize();
            */
            //tutorialView.HideFocusImage();
        }

        private void CommandMapClear()
        {
            mapAssign.ClearMap();
        }

        private void CommandCreateMapObject(GameObject mapObject)
        {
            mapAssign.CreateMapObject(mapObject);
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

        private void CheckTutorialState(TutorialViewInfo tutorialViewInfo)
        {
            if (ConfigData.TutorialCheck == false)
            {
                return;
            }
            var TutorialDates = _model.SceneTutorialDates(tutorialViewInfo.SceneType);
            var tutorialData = TutorialDates.Count > 0 ? TutorialDates[0] : null;
            var checkEndFlag = _lastTutorialData != null && tutorialViewInfo.CheckEndMethod != null ? tutorialViewInfo.CheckEndMethod(_lastTutorialData) : false;
            if (checkEndFlag)
            {
                tutorialView.gameObject.SetActive(false);
            }
            if (tutorialData != null)
            {
                var checkFlag = tutorialViewInfo.CheckMethod(tutorialData);
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
                tutorialViewInfo.CheckTrueAction?.Invoke();
                _lastTutorialData = tutorialData;
                tutorialView.gameObject.SetActive(true);
                tutorialView.SetTutorialData(tutorialData);
                tutorialView.SetBackEvent(() => 
                {
                    tutorialView.OnClickBack();
                    tutorialView.gameObject.SetActive(false);
                    tutorialViewInfo.EndEvent?.Invoke();
                });
                _model.ReadTutorialData(tutorialData);
            }
        }
    }


    public class AdvCallInfo
    {
        private string _label;
        public string Label => _label;
        public void SetLabel(string label)
        {
            _label = label;
        }
        private Action _callEvent;
        public Action CallEvent => _callEvent;
        public void SetCallEvent(Action callEvent)
        {
            _callEvent = callEvent;
        }
    }
}