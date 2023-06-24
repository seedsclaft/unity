using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem
{
    public InputKeyType Update()
    {
        /*
        UpdateGamePad();
        if(Keyboard.current.upArrowKey.wasPressedThisFrame) 
        {
            Debug.Log("up");
            return InputKeyType.Up;
        } else
        if(Keyboard.current.downArrowKey.wasPressedThisFrame) 
        {
            Debug.Log("down");
            return InputKeyType.Down;
        } else
        if(Keyboard.current.leftArrowKey.wasPressedThisFrame) 
        {
            Debug.Log("left");
            return InputKeyType.Left;
        } else
        if(Keyboard.current.rightArrowKey.wasPressedThisFrame) 
        {
            Debug.Log("right");
            return InputKeyType.Right;
        } else
        if(Keyboard.current[Key.Z].wasPressedThisFrame) 
        {
            Debug.Log("decide");
            return InputKeyType.Decide;
        } else
        if(Keyboard.current[Key.X].wasPressedThisFrame) 
        {
            Debug.Log("cancel");
            return InputKeyType.Cancel;
        } else
        if(Keyboard.current[Key.A].wasPressedThisFrame) 
        {
            Debug.Log("option1");
            return InputKeyType.Option1;
        } else
        if(Keyboard.current[Key.S].wasPressedThisFrame) 
        {
            Debug.Log("option2");
            return InputKeyType.Option2;
        } else
        if(Keyboard.current[Key.Q].wasPressedThisFrame) 
        {
            Debug.Log("sideLeft1");
            return InputKeyType.SideLeft1;
        } else
        if(Keyboard.current[Key.W].wasPressedThisFrame) 
        {
            Debug.Log("sideRight1");
            return InputKeyType.SideRight1;
        } else
        if(Keyboard.current[Key.Space].wasPressedThisFrame) 
        {
            Debug.Log("start");
            return InputKeyType.Start;
        } else
        if(Keyboard.current[Key.Enter].wasPressedThisFrame) 
        {
            Debug.Log("select");
            return InputKeyType.Select;
        }
        */
        return InputKeyType.None;
    }

    private void UpdateGamePad()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return;
        }
        if (gamepad.aButton.isPressed) Debug.Log($"A");
        if (gamepad.bButton.isPressed) Debug.Log($"B");
        if (gamepad.xButton.isPressed) Debug.Log($"X");
        if (gamepad.yButton.isPressed) Debug.Log($"Y");

        if (gamepad.buttonEast.isPressed) Debug.Log($"East");
        if (gamepad.buttonWest.isPressed) Debug.Log($"West");
        if (gamepad.buttonNorth.isPressed) Debug.Log($"North");
        if (gamepad.buttonSouth.isPressed) Debug.Log($"South");

        if (gamepad.circleButton.isPressed) Debug.Log($"Circle");
        if (gamepad.crossButton.isPressed) Debug.Log($"Cross");
        if (gamepad.triangleButton.isPressed) Debug.Log($"Triangle");
        if (gamepad.squareButton.isPressed) Debug.Log($"Square");

        // コントローラーの中央にあるスタートボタン、セレクトボタン、メニューボタン、ビューボタンなどに該当します。
        if (gamepad.startButton.isPressed) Debug.Log($"Start");
        if (gamepad.selectButton.isPressed) Debug.Log($"Select");

        // 左と右のスティックをまっすぐ押し込んだかどうかを判定します
        if (gamepad.leftStickButton.isPressed) Debug.Log($"LeftStickButton");
        if (gamepad.rightStickButton.isPressed) Debug.Log($"RightStickButton");

        // 左上と右上にあるボタン。PlayStation だと L1 や R1 に該当します
        if (gamepad.leftShoulder.isPressed) Debug.Log($"LeftShoulder");
        if (gamepad.rightShoulder.isPressed) Debug.Log($"RightShoulder");
    }
}

public enum InputKeyType{
    None,
    Up,
    Down,
    Left,
    Right,
    Decide,
    Cancel,
    Option1, // □,Akey
    Option2, // △,Skey
    SideLeft1, // L1
    SideRight1, // R1
    SideLeft2, // L2
    SideRight2, // R2
    Start,
    Select
}
