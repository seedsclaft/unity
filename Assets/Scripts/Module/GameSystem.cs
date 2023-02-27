using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    
    [SerializeField] private GameObject mainRoot = null;
    [SerializeField] private GameObject uiRoot = null;
    [SerializeField] private GameObject popupRoot = null;
    [SerializeField] private GameObject popupPrefab = null;
    
    private BaseView _currentScene = null;
    private ConfirmView _popupView = null;
    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static TempInfo CurrentTempData = null;


    private bool _busy = false;
    public bool Busy {get {return _busy;}}
    private void Awake() 
    {
        _model = new BaseModel();
        CommandSceneChange(Scene.Boot);
        CreatePopup();
    }

    private void CreatePopup()
    {
        var prefab = Instantiate(popupPrefab);
        prefab.transform.SetParent(popupRoot.transform, false);
        _popupView = prefab.GetComponent<ConfirmView>();
        popupRoot.gameObject.SetActive(false);
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
        if (viewEvent.commandType == Base.CommandType.CallPopupView)
        {
            popupRoot.gameObject.SetActive(true);
            var popupInfo = (PopupInfo)viewEvent.templete;
            _popupView.SetTitle(popupInfo.Title);
            _popupView.SetEvent(popupInfo.CallEvent);
        }
        if (viewEvent.commandType == Base.CommandType.ClosePopupView)
        {
            popupRoot.gameObject.SetActive(false);
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

