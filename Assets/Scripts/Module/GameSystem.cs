﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    
    [SerializeField] private GameObject mainRoot = null;
    [SerializeField] private GameObject uiRoot = null;
    
    private BaseView _currentScene = null;
    private BaseModel _model = null;


    private bool _busy = false;
    private void Awake() 
    {
        _model = new BaseModel();
        CommandSceneChange(Scene.Boot);
    }

    private void updateCommand(BaseViewEvent viewEvent)
    {
        if (_busy){
            return;
        }
        if (viewEvent.commandType == Base.CommandType.SceneChange)
        {
            CommandSceneChange(Scene.Title);
            Debug.Log("Start Change Scene");
        }
        if (viewEvent.commandType == Base.CommandType.InitSaveInfo)
        {
            var playInfo = new SavePlayInfo();
            playInfo.InitSaveData();
            _model.PlayInfo = playInfo;
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
}

