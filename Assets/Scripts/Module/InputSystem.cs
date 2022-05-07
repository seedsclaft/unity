using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputSystem 
{
    public InputKeyType Update()
    {
        if(Keyboard.current.upArrowKey .isPressed) 
        {
            Debug.Log("up");
            return InputKeyType.Up;
        } else
        if(Keyboard.current.downArrowKey.isPressed) 
        {
            Debug.Log("down");
            return InputKeyType.Down;
        }
        return InputKeyType.None;
    }
}

public enum InputKeyType{
    None,
    Up,
    Down
}
