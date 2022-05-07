using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseView : MonoBehaviour
{
    private InputSystem _input;
    private List<IInputHandlerEvent> _inputHandler = new List<IInputHandlerEvent>();
    private System.Action<BaseViewEvent> _commandData = null;
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
            handler.InputHandler(keyType);
        }
    }

    private void Update()
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

    public void SetEvent(System.Action<BaseViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void CallSceneChangeCommand(BaseViewEvent eventData)
    {
        _commandData(eventData);
    }
}

namespace Base
{
    public enum CommandType
    {
        None = 0,
        SceneChange,
        InitSaveInfo,
    }
}

public class BaseViewEvent
{
    public Base.CommandType commandType;
    public object templete;

    public BaseViewEvent(Base.CommandType type)
    {
        commandType = type;
    }
}