using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseView : MonoBehaviour
{
    private InputSystem _input;
    private List<IInputHandlerEvent> _inputHandler = new List<IInputHandlerEvent>();
    public System.Action<ViewEvent> _commandData = null;
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
}

namespace Base
{
    public enum CommandType
    {
        None = 0,
        SetTemplete,
        SceneChange,
        InitSaveInfo,
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