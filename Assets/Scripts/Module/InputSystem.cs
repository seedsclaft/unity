using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem
{
    public static bool IsGamePad = false;
    public InputKeyType Update()
    {
        if (GameSystem.ConfigData.InputType == false)
        {
            return InputKeyType.None;
        }
        var gamePadKey = UpdateGamePad();
        if (gamePadKey != InputKeyType.None)
        {
            InputSystem.IsGamePad = true;
            return gamePadKey;
        }
        if(Keyboard.current.upArrowKey.isPressed || Keyboard.current[Key.W].isPressed) 
        {
            Debug.Log("up");
            InputSystem.IsGamePad = false;
            return InputKeyType.Up;
        } else
        if(Keyboard.current.downArrowKey.isPressed || Keyboard.current[Key.S].isPressed) 
        {
            Debug.Log("down");
            InputSystem.IsGamePad = false;
            return InputKeyType.Down;
        } else
        if(Keyboard.current.leftArrowKey.isPressed || Keyboard.current[Key.A].isPressed) 
        {
            Debug.Log("left");
            InputSystem.IsGamePad = false;
            return InputKeyType.Left;
        } else
        if(Keyboard.current.rightArrowKey.isPressed || Keyboard.current[Key.D].isPressed) 
        {
            Debug.Log("right");
            InputSystem.IsGamePad = false;
            return InputKeyType.Right;
        } else
        if(Keyboard.current[Key.Space].wasPressedThisFrame) 
        {
            Debug.Log("decide");
            InputSystem.IsGamePad = false;
            return InputKeyType.Decide;
        } else
        if(Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.Escape].wasPressedThisFrame) 
        {
            Debug.Log("cancel");
            InputSystem.IsGamePad = false;
            return InputKeyType.Cancel;
        } else
        if(Keyboard.current[Key.R].wasPressedThisFrame) 
        {
            Debug.Log("option1");
            InputSystem.IsGamePad = false;
            return InputKeyType.Option1;
        } else
        if(Keyboard.current[Key.T].wasPressedThisFrame) 
        {
            Debug.Log("option2");
            InputSystem.IsGamePad = false;
            return InputKeyType.Option2;
        } else
        if(Keyboard.current[Key.Q].wasPressedThisFrame) 
        {
            Debug.Log("sideLeft1");
            InputSystem.IsGamePad = false;
            return InputKeyType.SideLeft1;
        } else
        if(Keyboard.current[Key.E].wasPressedThisFrame) 
        {
            Debug.Log("sideRight1");
            InputSystem.IsGamePad = false;
            return InputKeyType.SideRight1;
        }  else
        if(Keyboard.current[Key.PageDown].wasPressedThisFrame) 
        {
            Debug.Log("sideLeft2");
            InputSystem.IsGamePad = false;
            return InputKeyType.SideLeft2;
        } else
        if(Keyboard.current[Key.PageUp].wasPressedThisFrame) 
        {
            Debug.Log("sideRight2");
            InputSystem.IsGamePad = false;
            return InputKeyType.SideRight2;
        } else
        if(Keyboard.current[Key.Enter].wasPressedThisFrame) 
        {
            Debug.Log("start");
            InputSystem.IsGamePad = false;
            return InputKeyType.Start;
        } else
        if(Keyboard.current[Key.RightShift].wasPressedThisFrame) 
        {
            Debug.Log("select");
            InputSystem.IsGamePad = false;
            return InputKeyType.Select;
        }
        return InputKeyType.None;
    }

    private InputKeyType UpdateGamePad()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return InputKeyType.None;
        }
        // 十字
        if (gamepad.dpad.up.isPressed) 
        {
            return InputKeyType.Up;
        }
        if (gamepad.dpad.down.isPressed) 
        {
            return InputKeyType.Down;
        }
        if (gamepad.dpad.right.isPressed) 
        {
            return InputKeyType.Right;
        }
        if (gamepad.dpad.left.isPressed) 
        {
            return InputKeyType.Left;
        }

        if (gamepad.aButton.wasPressedThisFrame) 
        {
            return InputKeyType.Decide;
        }
        if (gamepad.bButton.wasPressedThisFrame)
        {
            return InputKeyType.Cancel;
        }
        if (gamepad.xButton.wasPressedThisFrame) 
        {
            return InputKeyType.Option1;
        }
        if (gamepad.yButton.wasPressedThisFrame)
        {
            return InputKeyType.Option2;
        }

        if (gamepad.buttonEast.wasPressedThisFrame) 
        {
            return InputKeyType.Cancel;
        }
        if (gamepad.buttonWest.wasPressedThisFrame) 
        {
            return InputKeyType.Option1;
        }
        if (gamepad.buttonNorth.wasPressedThisFrame)
        {
            return InputKeyType.Option2;
        }
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            return InputKeyType.Decide;
        }

        if (gamepad.circleButton.wasPressedThisFrame) 
        {
            return InputKeyType.Cancel;
        }
        if (gamepad.crossButton.wasPressedThisFrame)
        {
            return InputKeyType.Decide;
        }
        if (gamepad.triangleButton.wasPressedThisFrame) 
        {
            return InputKeyType.Option2;
        }
        if (gamepad.squareButton.wasPressedThisFrame)
        {
            return InputKeyType.Option1;
        }

        // start,select
        if (gamepad.startButton.wasPressedThisFrame) 
        {
            return InputKeyType.Start;
        }
        if (gamepad.selectButton.wasPressedThisFrame) 
        {
            return InputKeyType.Select;
        }


        // L1,R1
        if (gamepad.leftShoulder.wasPressedThisFrame) 
        {
            return InputKeyType.SideLeft1;
        }
        if (gamepad.rightShoulder.wasPressedThisFrame)
        {
            return InputKeyType.SideRight1;
        }

        // L2,R2
        if (gamepad.leftShoulder.isPressed)
        {
            return InputKeyType.SideLeft2;
        }
        if (gamepad.rightShoulder.isPressed)
        {
            return InputKeyType.SideRight2;
        }

        return InputKeyType.None;
    }

    public static bool IsMouseRightButtonDown()
    {
        if ( IsPlatformStandAloneOrEditor() || EnableWebGLInput())
        { 
            return Input.GetMouseButtonDown(1);
        }
        else
        {
            return false;
        }
    }
    
    public static bool EnableWebGLInput()
    {
        return (Application.platform == RuntimePlatform.WebGLPlayer);
    }

    public static bool IsPlatformStandAloneOrEditor()
    {
        return Application.isEditor || IsPlatformStandAlone();
    }

    public static bool IsPlatformStandAlone()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                return true;
            default:
                return false;
        }
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
    Select,
}
