using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

public class GameSystem : MonoBehaviour
{
    [SerializeField] private string version = "";
    [SerializeField] private bool testMode = false;
    [SerializeField] private SceneAssign sceneAssign = null;
    [SerializeField] private PopupAssign popupAssign = null;
    [SerializeField] private GameObject confirmRoot = null;
    [SerializeField] private GameObject transitionRoot = null;
    [SerializeField] private Fade transitionFade = null;
    [SerializeField] private GameObject statusRoot = null;
    [SerializeField] private GameObject statusPrefab = null;
    [SerializeField] private GameObject enemyInfoPrefab = null;
    [SerializeField] private LoadingView loadingView = null;
    [SerializeField] private AdvEngine advEngine = null;
    [SerializeField] private AdvController advController = null;
    [SerializeField] private DebugBattleData debugBattleData = null;
    [SerializeField] private HelpWindow helpWindow = null;
    [SerializeField] private HelpWindow advHelpWindow = null;
    
    private BaseView _currentScene = null;
    private BaseView _popupView = null;
    private BaseView _statusView = null;

    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static SaveConfigInfo ConfigData = null;

    private bool _busy = false;
    public bool Busy => _busy;

    public static string Version;
    public static DebugBattleData DebugBattleData;
    private void Awake() 
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FireBaseController.Instance.Initialize();
#endif
        Application.targetFrameRate = 60;
        advController.Initialize();
        advController.SetHelpWindow(advHelpWindow);
        transitionRoot.SetActive(false);
        loadingView.gameObject.SetActive(false);
        _model = new BaseModel();
        GameSystem.Version = version;
#if UNITY_EDITOR
        GameSystem.DebugBattleData = debugBattleData;
#endif
        CommandSceneChange(Scene.Boot);
    }

    private void CreateStatus(bool isActor)
    {
        var basePrefab = isActor ? statusPrefab : enemyInfoPrefab;
        var prefab = Instantiate(basePrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        if (isActor)
        {
            _statusView = prefab.GetComponent<StatusView>();
        } else
        {
            _statusView = prefab.GetComponent<EnemyInfoView>();
        }
        _statusView.SetHelpWindow(helpWindow);
        _statusView.Initialize();
        statusRoot.gameObject.SetActive(true);
    }

    private void CreateLoading()
    {
        loadingView.Initialize();
    }

    private void UpdateCommand(ViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        switch (viewEvent.commandType)
        {
            case Base.CommandType.SceneChange:
                if (testMode && (Scene)viewEvent.template == Scene.Battle)
                {
                    if (debugBattleData.AdvName != "")
                    {
                        StartCoroutine(JumpScenarioAsync(debugBattleData.AdvName,null));
                    } else
                    {
                        debugBattleData.MakeBattleActor();
                        CommandSceneChange((Scene)viewEvent.template);
                    }
                } else{
                    CommandSceneChange((Scene)viewEvent.template);
                }
                break;
            case Base.CommandType.CallConfirmView:
                CommandConfirmView((ConfirmInfo)viewEvent.template);
                break;
            case Base.CommandType.CallSkillDetailView:
                CommandSkillDetailView((ConfirmInfo)viewEvent.template);
                break;
            case Base.CommandType.CloseConfirm:
                confirmRoot.gameObject.SetActive(false);
                _currentScene.SetBusy(false);
                if (_statusView) _statusView.SetBusy(false);
                break;
            case Base.CommandType.CallRulingView:
                CommandRulingView((System.Action)viewEvent.template);
                break;
            case Base.CommandType.CallOptionView:
                CommandOptionView((System.Action)viewEvent.template);
                break;
            case Base.CommandType.CallRankingView:
                CommandRankingView((System.Action)viewEvent.template);
                break;
            case Base.CommandType.CallCreditView:
                CommandCreditView((System.Action)viewEvent.template);
                break;
            case Base.CommandType.CallStatusView:
                if (_statusView != null)
                {
                    DestroyImmediate(_statusView.gameObject);
                }
                CreateStatus(true);
                var statusViewInfo = (StatusViewInfo)viewEvent.template;
                (_statusView as StatusView).SetViewInfo(statusViewInfo);
                _statusView.SetEvent((type) => UpdateCommand(type));
                _currentScene.SetBusy(true);
                break;
            case Base.CommandType.CloseStatus:
                if (_statusView != null)
                {
                    DestroyImmediate(_statusView.gameObject);
                }
                statusRoot.gameObject.SetActive(false);
                _currentScene.SetBusy(false);
                break;
            case Base.CommandType.CallEnemyInfoView:
                if (_statusView != null)
                {
                    DestroyImmediate(_statusView.gameObject);
                }
                CreateStatus(false);
                var enemyStatusInfo = (StatusViewInfo)viewEvent.template;
                var enemyInfoView = _statusView as EnemyInfoView;
                enemyInfoView.Initialize(enemyStatusInfo.EnemyInfos,_model.BattleCursorEffects(),enemyStatusInfo.IsBattle);
                enemyInfoView.SetBackEvent(enemyStatusInfo.BackEvent);
                enemyInfoView.SetEvent((type) => UpdateCommand(type));
                _currentScene.SetBusy(true);
                break;
            case Base.CommandType.CallAdvScene:
                statusRoot.gameObject.SetActive(false);
                if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(true);
                if (_statusView) _statusView.SetBusy(true);
                AdvCallInfo advCallInfo = viewEvent.template as AdvCallInfo;
                _currentScene.SetBusy(true);
                if (!this.gameObject.activeSelf)
                {
                    this.gameObject.SetActive(true);
                }
                //_currentScene.SetActiveUi(false);
                StartCoroutine(JumpScenarioAsync(advCallInfo.Label,advCallInfo.CallEvent));
                break;
            case Base.CommandType.DecidePlayerName:
                string playerName = (string)advEngine.Param.GetParameter("PlayerName");
                advEngine.Param.SetParameterString("PlayerName",(string)viewEvent.template);
                break;
            case Base.CommandType.CallLoading:
                if (loadingView == null)
                {
                    CreateLoading();
                }
                loadingView.gameObject.SetActive(true);
                _currentScene.SetBusy(true);
                break;
            case Base.CommandType.CloseLoading:
                loadingView.gameObject.SetActive(false);
                _currentScene.SetBusy(false);
                break;
            case Base.CommandType.SetRouteSelect:
                int routeSelect = (int)advEngine.Param.GetParameter("RouteSelect");
                CurrentData.CurrentStage.SetRouteSelect(routeSelect);
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
            case Base.CommandType.ChangeEventSkipIndex:
                advEngine.Config.IsSkip = (bool)viewEvent.template;
                break;
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
    }

    private void CommandSkillDetailView(ConfirmInfo confirmInfo)
    {
        if (_popupView != null)
        {
            DestroyImmediate(_popupView.gameObject);
        }
        var prefab = popupAssign.CreatePopup(PopupType.SkillDetail);
        _popupView = prefab.GetComponent<ConfirmView>();
        _popupView.SetHelpWindow(helpWindow);
        var confirmView = (_popupView as ConfirmView);
        confirmView.Initialize();
        confirmRoot.gameObject.SetActive(true);
        confirmView.SetViewInfo(confirmInfo);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
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
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
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
                Ryneus.SoundManager.Instance.BGMVolume,
                Ryneus.SoundManager.Instance.BGMMute,
                Ryneus.SoundManager.Instance.SeVolume,
                Ryneus.SoundManager.Instance.SeMute
            );
            SaveSystem.SaveConfigStart(GameSystem.ConfigData);
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        optionView.SetEvent((type) => UpdateCommand(type));
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
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
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        confirmRoot.gameObject.SetActive(true);
        _currentScene.SetBusy(true);
        if (_statusView) _statusView.SetBusy(true);
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
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
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
        _currentScene.SetEvent((type) => UpdateCommand(type));
        _currentScene.Initialize();
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