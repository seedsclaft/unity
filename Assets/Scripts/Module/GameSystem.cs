using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;

public class GameSystem : MonoBehaviour
{
    [SerializeField] private bool testMode = false;
    [SerializeField] private GameObject uiRoot = null;
    [SerializeField] private GameObject confirmRoot = null;
    [SerializeField] private GameObject confirmPrefab = null;
    [SerializeField] private GameObject statusRoot = null;
    [SerializeField] private GameObject statusPrefab = null;
    [SerializeField] private GameObject enemyInfoPrefab = null;
    [SerializeField] private GameObject loadingRoot = null;
    [SerializeField] private GameObject loadingPrefab = null;
    [SerializeField] private AdvEngine advEngine = null;
    [SerializeField] private AdvController advController = null;
    [SerializeField] private DebugBattleData debugBattleData = null;
    
    private BaseView _currentScene = null;
    private ConfirmView _confirmView = null;
    private StatusView _statusView = null;
    private EnemyInfoView _enemyInfoView = null;
    private LoadingView _loadingView = null;
    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static TempInfo CurrentTempData = null;


    private bool _busy = false;
    public bool Busy {get {return _busy;}}
    private void Awake() 
    {
        advController.Initialize();
        _model = new BaseModel();
        CommandSceneChange(Scene.Boot);
    }

    private void CreateConfirm()
    {
        var prefab = Instantiate(confirmPrefab);
        prefab.transform.SetParent(confirmRoot.transform, false);
        _confirmView = prefab.GetComponent<ConfirmView>();
        _confirmView.Initialize();
        confirmRoot.gameObject.SetActive(false);
    }
    
    private void CreateStatus()
    {
        var prefab = Instantiate(statusPrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        _statusView = prefab.GetComponent<StatusView>();
        _statusView.Initialize();
        statusRoot.gameObject.SetActive(false);
    }

    private void CreateEnemyInfo()
    {
        var prefab = Instantiate(enemyInfoPrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        _enemyInfoView = prefab.GetComponent<EnemyInfoView>();
        _enemyInfoView.Initialize();
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
        if (viewEvent.commandType == Base.CommandType.SetTemplete)
        {
            CommandSetTemplete((TempInfo)viewEvent.templete);
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
        }
        if (viewEvent.commandType == Base.CommandType.InitSaveInfo)
        {
            var playInfo = new SavePlayInfo();
            playInfo.InitSaveData();
            CurrentData = playInfo;
            Debug.Log("InitSaveInfo");
        }
        if (viewEvent.commandType == Base.CommandType.CallConfirmView)
        {
            if (_confirmView != null)
            {
                DestroyImmediate(_confirmView.gameObject);
            }
            CreateConfirm();
            confirmRoot.gameObject.SetActive(true);
            var popupInfo = (ConfirmInfo)viewEvent.templete;
            _confirmView.SetIsNoChoice(popupInfo.IsNoChoise);
            _confirmView.SetTitle(popupInfo.Title);
            _confirmView.SetSkillInfo(popupInfo.SkillInfo);
            _confirmView.SetConfirmEvent(popupInfo.CallEvent);
            _currentScene.SetBusy(true);
            if (_statusView) _statusView.SetBusy(true);
            if (_enemyInfoView) _enemyInfoView.SetBusy(true);
        }
        if (viewEvent.commandType == Base.CommandType.CloseConfirm)
        {
            confirmRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
            if (_statusView) _statusView.SetBusy(false);
            if (_enemyInfoView) _enemyInfoView.SetBusy(false);
        }
        if (viewEvent.commandType == Base.CommandType.CallStatusView)
        {
            if (_statusView != null)
            {
                DestroyImmediate(_statusView.gameObject);
            }
            CreateStatus();
            statusRoot.gameObject.SetActive(true);
            var popupInfo = (StatusViewInfo)viewEvent.templete;
            _statusView.DisplayDecideButton(popupInfo.DisplayDecideButton);
            _statusView.DisplayBackButton(popupInfo.DisplayBackButton);
            _statusView.DisableStrength(popupInfo.DisableStrength);
            _statusView.SetBackEvent(popupInfo.BackEvent);
            _statusView.SetEvent((type) => updateCommand(type));
            _currentScene.SetBusy(true);
        }
        if (viewEvent.commandType == Base.CommandType.CloseStatus)
        {
            DestroyImmediate(_statusView.gameObject);
            statusRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        }
        if (viewEvent.commandType == Base.CommandType.CallEnemyInfoView)
        {
            if (_enemyInfoView != null)
            {
                DestroyImmediate(_enemyInfoView.gameObject);
            }
            CreateEnemyInfo();
            statusRoot.gameObject.SetActive(true);
            var popupInfo = (StatusViewInfo)viewEvent.templete;
            _enemyInfoView.Initialize(popupInfo.EnemyInfos);
            _enemyInfoView.SetBackEvent(popupInfo.BackEvent);
            _enemyInfoView.SetEvent((type) => updateCommand(type));
            _currentScene.SetBusy(true);
        }
        if (viewEvent.commandType == Base.CommandType.CloseEnemyInfo)
        {
            DestroyImmediate(_enemyInfoView.gameObject);
            statusRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        }
        if (viewEvent.commandType == Base.CommandType.CallAdvScene)
        {
            statusRoot.gameObject.SetActive(false);
            if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(true);
            if (_statusView) _statusView.SetBusy(true);
            if (_enemyInfoView) _enemyInfoView.SetBusy(true);
            AdvCallInfo advCallInfo = viewEvent.templete as AdvCallInfo;
            _currentScene.SetBusy(true);
            //_currentScene.SetActiveUi(false);
            StartCoroutine(JumpScenarioAsync(advCallInfo.Label,advCallInfo.CallEvent));
        }
        if (viewEvent.commandType == Base.CommandType.DecidePlayerName)
        {
            string playerName = (string)advEngine.Param.GetParameter("PlayerName");
            advEngine.Param.SetParameterString("PlayerName",(string)viewEvent.templete);
        }
        if (viewEvent.commandType == Base.CommandType.CallLoading)
        {
            CreateLoading();
            loadingRoot.gameObject.SetActive(true);
            _currentScene.SetBusy(true);
        }
        if (viewEvent.commandType == Base.CommandType.CloseLoading)
        {
            if (_loadingView != null)
            {
                DestroyImmediate(_loadingView.gameObject);
            }
            loadingRoot.gameObject.SetActive(false);
            _currentScene.SetBusy(false);
        }
    }

    IEnumerator JumpScenarioAsync(string label, System.Action onComplete)
    {
        _busy = true;
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
        
        //_currentScene.SetActiveUi(true);
        _busy = false;
        if(onComplete !=null) onComplete();
    }

    private async void CommandSceneChange(Scene scene)
    {
        if (_currentScene != null)
        { 
            DestroyImmediate(_currentScene.gameObject);
        }
        var loadScene = await ResourceSystem.LoadScene<GameObject>(scene);
        var prefab = Instantiate(loadScene);
        prefab.transform.SetParent(uiRoot.transform, false);
        _currentScene = prefab.GetComponent<BaseView>();
        _currentScene.SetEvent((type) => updateCommand(type));
        _currentScene.Initialize();
        _currentScene.SetTestMode(testMode);
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