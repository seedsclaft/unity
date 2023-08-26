using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;
using Cysharp.Threading.Tasks;

public class GameSystem : MonoBehaviour
{
    [SerializeField] private float version = 0.01f;
    [SerializeField] private bool testMode = false;
    [SerializeField] private SceneAssign sceneAssign = null;
    [SerializeField] private PopupAssign popupAssign = null;
    [SerializeField] private GameObject confirmRoot = null;
    [SerializeField] private GameObject transitionRoot = null;
    [SerializeField] private Fade transitionFade = null;
    [SerializeField] private GameObject statusRoot = null;
    [SerializeField] private GameObject statusPrefab = null;
    [SerializeField] private GameObject enemyInfoPrefab = null;
    [SerializeField] private GameObject loadingRoot = null;
    [SerializeField] private GameObject loadingPrefab = null;
    [SerializeField] private AdvEngine advEngine = null;
    [SerializeField] private AdvController advController = null;
    [SerializeField] private DebugBattleData debugBattleData = null;
    [SerializeField] private HelpWindow helpWindow = null;
    [SerializeField] private HelpWindow advHelpWindow = null;
    
    private BaseView _currentScene = null;
    private BaseView _popupView = null;
    private StatusView _statusView = null;
    private EnemyInfoView _enemyInfoView = null;
    private LoadingView _loadingView = null;
    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static SaveConfigInfo ConfigData = null;
    public static TempInfo CurrentTempData = null;

    private bool _busy = false;
    public bool Busy => _busy;

    public static float Version;
    public static DebugBattleData DebugBattleData;
    private void Awake() 
    {
        Application.targetFrameRate = 60;
        advController.Initialize();
        advController.SetHelpWindow(advHelpWindow);
        transitionRoot.SetActive(false);
        _model = new BaseModel();
        GameSystem.Version = version;
#if UNITY_EDITOR
        GameSystem.DebugBattleData = debugBattleData;
#endif
        CommandSceneChange(Scene.Boot);
    }

    private void CreateStatus()
    {
        var prefab = Instantiate(statusPrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        _statusView = prefab.GetComponent<StatusView>();
        _statusView.Initialize();
        _statusView.SetHelpWindow(helpWindow);
        statusRoot.gameObject.SetActive(false);
    }

    private void CreateEnemyInfo()
    {
        var prefab = Instantiate(enemyInfoPrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        _enemyInfoView = prefab.GetComponent<EnemyInfoView>();
        _enemyInfoView.Initialize();
        _enemyInfoView.SetHelpWindow(helpWindow);
        statusRoot.gameObject.SetActive(false);
    }

    private void CreateLoading()
    {
        var prefab = Instantiate(loadingPrefab);
        prefab.transform.SetParent(loadingRoot.transform, false);
        _loadingView = prefab.GetComponent<LoadingView>();
        _loadingView.Initialize();
        loadingRoot.gameObject.SetActive(false);
    }

    private void updateCommand(ViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Base.CommandType.SceneChange)
        {
            if (testMode && (Scene)viewEvent.templete == Scene.Battle)
            {
                if (debugBattleData.AdvName != "")
                {
                    StartCoroutine(JumpScenarioAsync(debugBattleData.AdvName,null));
                } else
                {
                    debugBattleData.MakeBattleActor();
                    CommandSceneChange((Scene)viewEvent.templete);
                }
            } else{
                CommandSceneChange((Scene)viewEvent.templete);
            }
        } else
        if (viewEvent.commandType == Base.CommandType.CallConfirmView)
        {
            CommandConfirmView((ConfirmInfo)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CloseConfirm)
        {
            confirmRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
            if (_statusView) _statusView.SetBusy(false);
            if (_enemyInfoView) _enemyInfoView.SetBusy(false);
        } else
        if (viewEvent.commandType == Base.CommandType.CallRulingView)
        {
            CommandRulingView((System.Action)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CallOptionView)
        {
            CommandOptionView((System.Action)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CallRankingView)
        {
            CommandRankingView((System.Action)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CallCreditView)
        { 
            CommandCreditView((System.Action)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CallStatusView)
        {
            if (_statusView != null)
            {
                DestroyImmediate(_statusView.gameObject);
            }
            CreateStatus();
            statusRoot.gameObject.SetActive(true);
            var popupInfo = (StatusViewInfo)viewEvent.templete;
            _statusView.SetViewInfo(popupInfo);
            _statusView.SetEvent((type) => updateCommand(type));
            _currentScene.SetBusy(true);
        } else
        if (viewEvent.commandType == Base.CommandType.CloseStatus)
        {
            DestroyImmediate(_statusView.gameObject);
            statusRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        } else
        if (viewEvent.commandType == Base.CommandType.CallEnemyInfoView)
        {
            if (_enemyInfoView != null)
            {
                DestroyImmediate(_enemyInfoView.gameObject);
            }
            CreateEnemyInfo();
            statusRoot.gameObject.SetActive(true);
            var popupInfo = (StatusViewInfo)viewEvent.templete;
            _enemyInfoView.Initialize(popupInfo.EnemyInfos,popupInfo.IsBattle);
            _enemyInfoView.SetBackEvent(popupInfo.BackEvent);
            _enemyInfoView.SetEvent((type) => updateCommand(type));
            _currentScene.SetBusy(true);
        } else
        if (viewEvent.commandType == Base.CommandType.CloseEnemyInfo)
        {
            DestroyImmediate(_enemyInfoView.gameObject);
            statusRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        } else
        if (viewEvent.commandType == Base.CommandType.CallAdvScene)
        {
            statusRoot.gameObject.SetActive(false);
            if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(true);
            if (_statusView) _statusView.SetBusy(true);
            if (_enemyInfoView) _enemyInfoView.SetBusy(true);
            AdvCallInfo advCallInfo = viewEvent.templete as AdvCallInfo;
            _currentScene.SetBusy(true);
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
            //_currentScene.SetActiveUi(false);
            StartCoroutine(JumpScenarioAsync(advCallInfo.Label,advCallInfo.CallEvent));
        } else
        if (viewEvent.commandType == Base.CommandType.DecidePlayerName)
        {
            string playerName = (string)advEngine.Param.GetParameter("PlayerName");
            advEngine.Param.SetParameterString("PlayerName",(string)viewEvent.templete);
        } else
        if (viewEvent.commandType == Base.CommandType.CallLoading)
        {
            if (_loadingView == null)
            {
                CreateLoading();
            }
            loadingRoot.gameObject.SetActive(true);
            _currentScene.SetBusy(true);
        } else
        if (viewEvent.commandType == Base.CommandType.CloseLoading)
        {
            /*
            if (_loadingView != null)
            {
                DestroyImmediate(_loadingView.gameObject);
            }
            */
            loadingRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        } else
        if (viewEvent.commandType == Base.CommandType.SetRouteSelect)
        {
            int routeSelect = (int)advEngine.Param.GetParameter("RouteSelect");
            CurrentData.CurrentStage.SetRouteSelect(routeSelect);
        } else
        if (viewEvent.commandType == Base.CommandType.ChangeViewToTransition)
        {
            transitionRoot.SetActive(true);
            _currentScene.gameObject.transform.SetParent(transitionRoot.transform, false);
            _currentScene = null;
        } else
        if (viewEvent.commandType == Base.CommandType.StartTransition)
        {
            transitionFade.FadeIn(0.8f,() => {
                foreach(Transform child in transitionRoot.transform){
                    var endEvent = (System.Action)viewEvent.templete;
                    if ((System.Action)viewEvent.templete != null) endEvent();
                    Destroy(child.gameObject);
                    transitionFade.FadeOut(0);
                    transitionRoot.SetActive(false);
                }
            });
        } else
        if (viewEvent.commandType == Base.CommandType.ChangeEventSkipIndex)
        {
            advEngine.Config.IsSkip = (bool)viewEvent.templete;
        }
    }

    private void CommandConfirmView(ConfirmInfo confirmInfo)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.Confirm);
        _popupView = prefab.GetComponent<ConfirmView>();
        _popupView.SetHelpWindow(helpWindow);
        var confirmView = (_popupView as ConfirmView);
        confirmView.Initialize();
        confirmRoot.gameObject.SetActive(true);
        confirmView.SetViewInfo(confirmInfo);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
        if (_enemyInfoView) _enemyInfoView.SetBusy(true);
    }

    private void CommandRulingView(System.Action endEvent)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.Ruling);
        _popupView = prefab.GetComponent<RulingView>();
        _popupView.SetHelpWindow(helpWindow);
        var rulingView = (_popupView as RulingView);
        rulingView.Initialize();
        rulingView.SetBackEvent(() => 
        {
            updateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
        if (_enemyInfoView) _enemyInfoView.SetBusy(true);
    }
    
    private void CommandOptionView(System.Action endEvent)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.Option);
        _popupView = prefab.GetComponent<OptionView>();
        _popupView.SetHelpWindow(helpWindow);
        var optionView = (_popupView as OptionView);
        optionView.Initialize();
        optionView.SetBackEvent(() => 
        {
            GameSystem.ConfigData.UpdateSoundParameter(
                Ryneus.SoundManager.Instance._bgmVolume,
                Ryneus.SoundManager.Instance._bgmMute,
                Ryneus.SoundManager.Instance._seVolume,
                Ryneus.SoundManager.Instance._seMute
            );
            SaveSystem.SaveConfigStart(GameSystem.ConfigData);
            updateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        optionView.SetEvent((type) => updateCommand(type));
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
        if (_enemyInfoView) _enemyInfoView.SetBusy(true);
    }
    
    private void CommandRankingView(System.Action endEvent)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.Ranking);
        _popupView = prefab.GetComponent<RankingView>();
        _popupView.SetHelpWindow(helpWindow);
        var rankingView = (_popupView as RankingView);
        rankingView.Initialize();
        rankingView.SetBackEvent(() => 
        {
            updateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
        if (_enemyInfoView) _enemyInfoView.SetBusy(true);
    }

    private void CommandCreditView(System.Action endEvent)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.Credit);
        _popupView = prefab.GetComponent<CreditView>();
        _popupView.SetHelpWindow(helpWindow);
        var creditView = (_popupView as CreditView);
        creditView.Initialize();
        creditView.SetBackEvent(() => 
        {
            updateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
    }

    IEnumerator JumpScenarioAsync(string label, System.Action onComplete)
    {
        _busy = true;
        advHelpWindow.SetInputInfo("ADV_READING");
        while (advEngine.IsWaitBootLoading) yield return null;
        while (advEngine.GraphicManager.IsLoading) yield return null;
        while (advEngine.SoundManager.IsLoading) yield return null;
        advEngine.JumpScenario(label);
        advController.StartAdv();
        while (!advEngine.IsEndOrPauseScenario)
        {
            yield return null;
        }
        if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(false);
        if (_statusView) _statusView.SetBusy(false);
        if (_enemyInfoView) _enemyInfoView.SetBusy(false);
        advController.EndAdv();
        advHelpWindow.SetInputInfo("");
        
        //_currentScene.SetActiveUi(true);
        _busy = false;
        if(onComplete !=null) onComplete();
    }

    private void CommandSceneChange(Scene scene)
    {
        if (_currentScene != null)
        { 
            Destroy(_currentScene.gameObject);
            ResourceSystem.ReleaseAssets();
            ResourceSystem.ReleaseScene();
            Resources.UnloadUnusedAssets();
        }
        GameObject prefab = sceneAssign.CreateScene(scene);
        _currentScene = prefab.GetComponent<BaseView>();
        _currentScene.SetTestMode(testMode);
        _currentScene.SetHelpWindow(helpWindow);
        _currentScene.SetEvent((type) => updateCommand(type));
        _currentScene.Initialize();
    }

    private void CommandSetTemplete(TempInfo templete){
        CurrentTempData = templete;
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