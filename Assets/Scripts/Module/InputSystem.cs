using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputSystem 
{
    public InputKeyType Update()
    {
        UpdateGamePad();
        if(Keyboard.current.upArrowKey.isPressed) 
        {
            Debug.Log("up");
            return InputKeyType.Up;
        } else
        if(Keyboard.current.downArrowKey.isPressed) 
        {
            Debug.Log("down");
            return InputKeyType.Down;
        } else
        if(Keyboard.current.leftArrowKey.isPressed) 
        {
            Debug.Log("left");
            return InputKeyType.Left;
        } else
        if(Keyboard.current.rightArrowKey.isPressed) 
        {
            Debug.Log("right");
            return InputKeyType.Right;
        } else
        if(Keyboard.current[Key.Z].isPressed) 
        {
            Debug.Log("decide");
            return InputKeyType.Decide;
        } else
        if(Keyboard.current[Key.X].isPressed) 
        {
            Debug.Log("cancel");
            return InputKeyType.Cancel;
        }
        return InputKeyType.None;
    }

    private void UpdateGamePad()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            Debug.Log("ゲームパッドがありません。");
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
    Cancel
}
