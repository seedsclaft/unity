using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class BaseView : MonoBehaviour
{
    private bool _testMode = false;
    public bool TestMode => _testMode;
    private InputSystem _input;
    private List<IInputHandlerEvent> _inputHandler = new List<IInputHandlerEvent>();
    private bool _busy = false;
    public bool Busy { get {return _busy;}}
    public System.Action<ViewEvent> _commandData = null;
    private Button _backCommand = null;
    private System.Action _backEvent = null;
    public System.Action BackEvent => _backEvent;
    [SerializeField] private GameObject uiRoot = null;
    [SerializeField] private GameObject backPrefab = null;
    [SerializeField] private GameObject backRoot = null;

    private HelpWindow _helpWindow = null;
    public HelpWindow HelpWindow => _helpWindow;
    public void SetHelpInputInfo(string key)
    {
        HelpWindow.SetInputInfo(key);
    }
    public void SetHelpText(string text)
    {
        HelpWindow.SetHelpText(text);
    }

    private int _inputBusyFrame = 0;
    private InputKeyType _lastInputKey = InputKeyType.None;
    private int _pressedFrame = 0;
    private int _pressFrame = 30;
    public void SetInputFrame(int frame)
    {
        _inputBusyFrame = frame;
    }

    public virtual void Initialize()
    {
        
    }

    public void InitializeInput()
    {    
        _input = new InputSystem();
    }

    public void SetHelpWindow(HelpWindow helpWindow)
    {
        _helpWindow = helpWindow;
    }

    public void SetInputHandler(IInputHandlerEvent handler)
    {
        _inputHandler.Add(handler);
    }

    private void InputHandler(InputKeyType keyType,bool pressed)
    {
        if (_busy) return;
        for (int i = _inputHandler.Count-1;i >= 0;i--)
        {
            if (_inputHandler[i] != null && _inputBusyFrame < 0){
                _inputHandler[i].InputHandler(keyType,pressed);
            }
        }
    }

    private void CallMouseCancel()
    {
        if (_busy) return;
        foreach (var handler in _inputHandler)
        {
            if (handler != null){
                handler.MouseCancelHandler();
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
            //if (keyType != InputKeyType.None)
            //{
                InputHandler(keyType,_pressedFrame > _pressFrame);
            //}
            if (InputSystem.IsMouseRightButtonDown())
            {
                CallMouseCancel();
            }
            if (_lastInputKey != keyType)
            {
                _lastInputKey = keyType;
                _pressedFrame = 0;
            } else
            {
                _pressedFrame += 1;
            }
        }
        UpdateInputFrame();
    }

    private void UpdateInputFrame()
    {
        if (_inputBusyFrame >= 0)
        {
            _inputBusyFrame--;
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
        var eventData = new ViewEvent(Base.CommandType.SceneChange);
        eventData.templete = scene;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallConfirm(ConfirmInfo popupInfo)
    {
        var eventData = new ViewEvent(Base.CommandType.CallConfirmView);
        eventData.templete = popupInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandConfirmClose()
    {
        var eventData = new ViewEvent(Base.CommandType.CloseConfirm);
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallRuling(System.Action endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.CallRulingView);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallOption(System.Action endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.CallOptionView);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallRanking(System.Action endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.CallRankingView);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallCredit(System.Action endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.CallCreditView);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallStatus(StatusViewInfo statusViewInfo)
    {
        var eventData = new ViewEvent(Base.CommandType.CallStatusView);
        eventData.templete = statusViewInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandStatusClose()
    {
        var eventData = new ViewEvent(Base.CommandType.CloseStatus);
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallEnemyInfo(StatusViewInfo statusViewInfo)
    {
        var eventData = new ViewEvent(Base.CommandType.CallEnemyInfoView);
        eventData.templete = statusViewInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallAdv(AdvCallInfo advCallInfo)
    {
        var eventData = new ViewEvent(Base.CommandType.CallAdvScene);
        eventData.templete = advCallInfo;
        CallSceneChangeCommand(eventData);
    }

    public void CommandDecidePlayerName(string nameText)
    {
        var eventData = new ViewEvent(Base.CommandType.DecidePlayerName);
        eventData.templete = nameText;
        CallSceneChangeCommand(eventData);
    }

    public void CommandCallLoading()
    {
        var eventData = new ViewEvent(Base.CommandType.CallLoading);
        CallSceneChangeCommand(eventData);
    }

    public void CommandLoadingClose()
    {
        var eventData = new ViewEvent(Base.CommandType.CloseLoading);
        CallSceneChangeCommand(eventData);
    }

    public void CommandSetRouteSelect()
    {
        var eventData = new ViewEvent(Base.CommandType.SetRouteSelect);
        CallSceneChangeCommand(eventData);
    }

    public void CommandChangeViewToTransition(System.Action<string> endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.ChangeViewToTransition);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandStartTransition(System.Action endEvent)
    {
        var eventData = new ViewEvent(Base.CommandType.StartTransition);
        eventData.templete = endEvent;
        CallSceneChangeCommand(eventData);
    }

    public void CommandChangeEventSkipIndex(bool isSkip)
    {
        var eventData = new ViewEvent(Base.CommandType.ChangeEventSkipIndex);
        eventData.templete = isSkip;
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
        _backEvent = callEvent;
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

    public void MouseCancelHandler()
    {

    }
}

namespace Base
{
    public enum CommandType
    {
        None = 0,
        SceneChange,
        CallConfirmView,
        CloseConfirm,
        CallRulingView,
        CallOptionView,
        CallRankingView,
        CallCreditView,
        CallStatusView,
        CloseStatus,
        CallAdvScene,
        CallEnemyInfoView,
        DecidePlayerName,
        CallLoading,
        CloseLoading,
        SetRouteSelect,
        ChangeViewToTransition,
        StartTransition,
        ChangeEventSkipIndex,
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