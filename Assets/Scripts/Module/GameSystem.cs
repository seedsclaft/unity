using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    
    [SerializeField] private GameObject mainRoot = null;
    [SerializeField] private GameObject uiRoot = null;
    
    private BaseView _currentScene = null;
    private BaseModel _model = null;
    
    public static SavePlayInfo CurrentData = null;
    public static TempInfo CurrentTempData = null;


    private bool _busy = false;
    private void Awake() 
    {
        _model = new BaseModel();
        CommandSceneChange(Scene.Boot);
    }

    private void updateCommand(ViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == (int)Base.CommandType.SetTemplete)
        {
            CommandSetTemplete((TempInfo)viewEvent.templete);
        }
        if (viewEvent.commandType == (int)Base.CommandType.SceneChange)
        {
            CommandSceneChange((Scene)viewEvent.templete);
            Debug.Log("Start Change Scene");
        }
        if (viewEvent.commandType == (int)Base.CommandType.InitSaveInfo)
        {
            var playInfo = new SavePlayInfo();
            playInfo.InitSaveData();
            CurrentData = playInfo;
            Debug.Log("InitSaveInfo");
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

