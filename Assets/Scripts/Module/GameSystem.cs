using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    
    [SerializeField] private GameObject mainRoot = null;
    [SerializeField] private GameObject uiRoot = null;
    [SerializeField] private GameObject confirmRoot = null;
    [SerializeField] private GameObject confirmPrefab = null;
    [SerializeField] private GameObject statusRoot = null;
    [SerializeField] private GameObject statusPrefab = null;
    
    private BaseView _currentScene = null;
    private ConfirmView _confirmView = null;
    private StatusView _statusView = null;
    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static TempInfo CurrentTempData = null;


    private bool _busy = false;
    public bool Busy {get {return _busy;}}
    private void Awake() 
    {
        _model = new BaseModel();
        CommandSceneChange(Scene.Boot);
        CreateConfirm();
    }

    private void CreateConfirm()
    {
        var prefab = Instantiate(confirmPrefab);
        prefab.transform.SetParent(confirmRoot.transform, false);
        _confirmView = prefab.GetComponent<ConfirmView>();
        confirmRoot.gameObject.SetActive(false);
    }
    
    private void CreateStatus()
    {
        var prefab = Instantiate(statusPrefab);
        prefab.transform.SetParent(statusRoot.transform, false);
        _statusView = prefab.GetComponent<StatusView>();
        statusRoot.gameObject.SetActive(false);
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
            CommandSceneChange((Scene)viewEvent.templete);
            Debug.Log("Start Change Scene");
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
            confirmRoot.gameObject.SetActive(true);
            var popupInfo = (ConfirmInfo)viewEvent.templete;
            _confirmView.SetTitle(popupInfo.Title);
            _confirmView.SetEvent(popupInfo.CallEvent);
            if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(true);
            if (_statusView) _statusView.SetBusy(true);
        }
        if (viewEvent.commandType == Base.CommandType.CloseConfirm)
        {
            confirmRoot.gameObject.SetActive(false);
            if (!statusRoot.gameObject.activeSelf) _currentScene.SetBusy(false);
            if (_statusView) _statusView.SetBusy(false);
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
    }

    private void CommandSetTemplete(TempInfo templete){
        CurrentTempData = templete;
    }
}

