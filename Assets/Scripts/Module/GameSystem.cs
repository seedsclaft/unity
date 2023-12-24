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
    [SerializeField] private StatusAssign statusAssign = null;

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
    private void Awake() 
    {
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
        FirebaseController.Instance.Initialize();
#endif
        Application.targetFrameRate = 60;
        advController.Initialize();
        advController.SetHelpWindow(advHelpWindow);
        transitionRoot.SetActive(false);
        loadingView.Initialize();
        loadingView.gameObject.SetActive(false);
        TempData = new TempInfo();
        _model = new BaseModel();
        GameSystem.Version = version;
#if UNITY_EDITOR
        GameSystem.DebugBattleData = debugBattleData;
#endif
#if UNITY_ANDROID
        AdMobController.Instance.Initialize(() => {CommandSceneChange(Scene.Boot);});
#else
        CommandSceneChange(Scene.Boot);
#endif
    }

    private BaseView CreateStatus(StatusType statusType)
    {
        var prefab = statusAssign.CreatePopup(statusType,helpWindow);
        if (statusType == StatusType.Status)
        {
            prefab.GetComponent<StatusView>().Initialize();
        } else
        {
            prefab.GetComponent<EnemyInfoView>().Initialize();
        }
        return prefab.GetComponent<BaseView>();
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
                popupAssign.CloseConfirm();
                SetIsNotBusyMainAndStatus();
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
            case Base.CommandType.CallCharacterListView:
                CommandCharacterListView((CharacterListInfo)viewEvent.template);
                break;
            case Base.CommandType.CallStatusView:
                var statusView = CreateStatus(StatusType.Status) as StatusView;
                var statusViewInfo = (StatusViewInfo)viewEvent.template;
                statusView.SetViewInfo(statusViewInfo);
                statusView.SetEvent((type) => UpdateCommand(type));
                _currentScene.SetBusy(true);
                break;
            case Base.CommandType.CloseStatus:
                statusAssign.CloseStatus();
                _currentScene.SetBusy(false);
                break;
            case Base.CommandType.CallEnemyInfoView:
                var enemyInfoView = CreateStatus(StatusType.EnemyDetail) as EnemyInfoView;
                var enemyStatusInfo = (StatusViewInfo)viewEvent.template;
                enemyInfoView.Initialize(enemyStatusInfo.EnemyInfos,_model.BattleCursorEffects(),enemyStatusInfo.IsBattle);
                enemyInfoView.SetBackEvent(enemyStatusInfo.BackEvent);
                enemyInfoView.SetEvent((type) => UpdateCommand(type));
                _currentScene.SetBusy(true);
                break;
            case Base.CommandType.CallAdvScene:
                SetIsBusyMainAndStatus();
                var advCallInfo = viewEvent.template as AdvCallInfo;
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
            case Base.CommandType.ChangeEventSkipIndex:
                advEngine.Config.IsSkip = (bool)viewEvent.template;
                break;
        }
    }

    private void CommandConfirmView(ConfirmInfo confirmInfo)
    {
        var prefab = popupAssign.CreatePopup(PopupType.Confirm,helpWindow);
        var confirmView = prefab.GetComponent<ConfirmView>();
        confirmView.Initialize();
        confirmView.SetViewInfo(confirmInfo);
        SetIsBusyMainAndStatus();
    }

    private void CommandSkillDetailView(ConfirmInfo confirmInfo)
    {
        var prefab = popupAssign.CreatePopup(PopupType.SkillDetail,helpWindow);
        var confirmView = prefab.GetComponent<ConfirmView>();
        confirmView.Initialize();
        confirmView.SetViewInfo(confirmInfo);
        SetIsBusyMainAndStatus();
    }

    private void CommandRulingView(System.Action endEvent)
    {
        var prefab = popupAssign.CreatePopup(PopupType.Ruling,helpWindow);
        var rulingView = prefab.GetComponent<RulingView>();
        rulingView.Initialize();
        rulingView.SetBackEvent(() => 
        {
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        SetIsBusyMainAndStatus();
    }
    
    private void CommandOptionView(System.Action endEvent)
    {
        var prefab = popupAssign.CreatePopup(PopupType.Option,helpWindow);
        var optionView = prefab.GetComponent<OptionView>();
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
        SetIsBusyMainAndStatus();
    }
    
    private void CommandRankingView(System.Action endEvent)
    {
        var prefab = popupAssign.CreatePopup(PopupType.Ranking,helpWindow);
        var rankingView = prefab.GetComponent<RankingView>();
        rankingView.Initialize();
        rankingView.SetBackEvent(() => 
        {
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        SetIsBusyMainAndStatus();
    }

    private void CommandCreditView(System.Action endEvent)
    {
        var prefab = popupAssign.CreatePopup(PopupType.Credit,helpWindow);
        var creditView = prefab.GetComponent<CreditView>();
        creditView.Initialize();
        creditView.SetBackEvent(() => 
        {
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
            if (endEvent != null) endEvent();
        });
        SetIsBusyMainAndStatus();
    }

    private void CommandCharacterListView(CharacterListInfo characterListInfo)
    {
        var prefab = popupAssign.CreatePopup(PopupType.CharacterList,helpWindow);
        var characterListView = prefab.GetComponent<CharacterListView>();
        characterListView.Initialize();
        characterListView.SetViewInfo(characterListInfo);
        characterListView.SetBackEvent(() => 
        {
            UpdateCommand(new ViewEvent(Base.CommandType.CloseConfirm));
        });
        SetIsBusyMainAndStatus();
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
        SetIsNotBusyMainAndStatus();
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
        var prefab = sceneAssign.CreateScene(scene,helpWindow);
        _currentScene = prefab.GetComponent<BaseView>();
        _currentScene.SetTestMode(testMode);
        _currentScene.SetEvent((type) => UpdateCommand(type));
        _currentScene.Initialize();
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