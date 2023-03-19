using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class BaseView : MonoBehaviour
{
    private bool _testMode = false;
    public bool TestMode { get {return _testMode;}}
    private InputSystem _input;
    private List<IInputHandlerEvent> _inputHandler = new List<IInputHandlerEvent>();
    private bool _busy = false;
    public bool Busy { get {return _busy;}}
    public System.Action<ViewEvent> _commandData = null;
    private Button _backCommand = null;
    [SerializeField] private GameObject uiRoot = null;
    [SerializeField] private GameObject backPrefab = null;
    [SerializeField] private GameObject backRoot = null;

    public void InitializeInput()
    {    
        _input = new InputSystem();
    }

    public void SetInputHandler(IInputHandlerEvent handler)
    {
        _inputHandler.Add(handler);
    }

    private void InputHandler(InputKeyType keyType)
    {
        foreach (var handler in _inputHandler)
        {
            if (handler != null){
                handler.InputHandler(keyType);
            }
        }
    }

    public void SetBusy(bool isBusy)
    {
        _busy = isBusy;
    }

    public void Update()
    {
        if (_input != null)
        {
            InputKeyType keyType = _input.Update();
            if (keyType != InputKeyType.None)
            {
                InputHandler(keyType);
            }
        }
    }

    public void SetEvent(System.Action<ViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void CallSceneChangeCommand(ViewEvent eventData)
    {
        _commandData(eventData);
    }

    public void CommandSceneChange(Scene scene)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.SceneChange);
        eventData.templete = scene;
        CallSceneChangeCommand(eventData);
    }

    public void CommandSetTemplete(TempInfo temp)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.SetTemplete);
        eventData.templete = temp;
        CallSceneChangeCommand(eventData);
    }
    
    public void CommandInitSaveInfo()
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.InitSaveInfo);
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallConfirm(ConfirmInfo popupInfo)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.CallConfirmView);
        eventData.templete = popupInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandConfirmClose()
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.CloseConfirm);
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallStatus(StatusViewInfo statusViewInfo)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.CallStatusView);
        eventData.templete = statusViewInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandStatusClose()
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.CloseStatus);
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallAdv(AdvCallInfo advCallInfo)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.CallAdvScene);
        eventData.templete = advCallInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandDecidePlayerName(string nameText)
    {
        var eventData = new ViewEvent(Scene.Base, Base.CommandType.DecidePlayerName);
        eventData.templete = nameText;
        CallSceneChangeCommand(eventData);
    }

    public void CreateBackCommand(System.Action callEvent)
    {
        GameObject prefab = Instantiate(backPrefab);
        prefab.transform.SetParent(backRoot.transform, false);
        _backCommand = prefab.GetComponent<Button>();
        _backCommand.onClick.AddListener(() => {        
            if (!_backCommand.gameObject.activeSelf) return;
            callEvent();
        });
    }

    public void SetActiveBack(bool IsActive)
    {
        _backCommand.gameObject.SetActive(IsActive);
    }

    public void SetActiveUi(bool IsActive)
    {
        uiRoot.gameObject.SetActive(IsActive);
    }

    public void SetTestMode(bool isTest)
    {
        _testMode = isTest;
    }
}

namespace Base
{
    public enum CommandType
    {
        None = 0,
        SetTemplete,
        SceneChange,
        InitSaveInfo,
        CallConfirmView,
        CloseConfirm,  
        CallStatusView,
        CloseStatus,
        CallAdvScene,
        DecidePlayerName

    }
}

public class BaseViewEvent
{
    public Base.CommandType commandType;
    public Scene templete;

    public BaseViewEvent(Base.CommandType type)
    {
        commandType = type;
    }
}