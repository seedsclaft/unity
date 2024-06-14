using UnityEngine;
using UnityEngine.InputSystem;

namespace Ryneus
{
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
                IsGamePad = true;
                return gamePadKey;
            }
            if(Keyboard.current.upArrowKey.isPressed || Keyboard.current[Key.W].isPressed) 
            {
                //Debug.Log("up");
                IsGamePad = false;
                return InputKeyType.Up;
            } else
            if(Keyboard.current.downArrowKey.isPressed || Keyboard.current[Key.S].isPressed) 
            {
                //Debug.Log("down");
                IsGamePad = false;
                return InputKeyType.Down;
            } else
            if(Keyboard.current.leftArrowKey.isPressed || Keyboard.current[Key.A].isPressed) 
            {
                //Debug.Log("left");
                IsGamePad = false;
                return InputKeyType.Left;
            } else
            if(Keyboard.current.rightArrowKey.isPressed || Keyboard.current[Key.D].isPressed) 
            {
                //Debug.Log("right");
                IsGamePad = false;
                return InputKeyType.Right;
            } else
            if(Keyboard.current[Key.Space].wasPressedThisFrame) 
            {
                //Debug.Log("decide");
                IsGamePad = false;
                return InputKeyType.Decide;
            } else
            if(Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.Escape].wasPressedThisFrame) 
            {
                //Debug.Log("cancel");
                IsGamePad = false;
                return InputKeyType.Cancel;
            } else
            if(Keyboard.current[Key.R].wasPressedThisFrame) 
            {
                Debug.Log("option1");
                IsGamePad = false;
                return InputKeyType.Option1;
            } else
            if(Keyboard.current[Key.T].wasPressedThisFrame) 
            {
                Debug.Log("option2");
                IsGamePad = false;
                return InputKeyType.Option2;
            } else
            if(Keyboard.current[Key.Q].wasPressedThisFrame) 
            {
                Debug.Log("sideLeft1");
                IsGamePad = false;
                return InputKeyType.SideLeft1;
            } else
            if(Keyboard.current[Key.E].wasPressedThisFrame) 
            {
                Debug.Log("sideRight1");
                IsGamePad = false;
                return InputKeyType.SideRight1;
            }  else
            if(Keyboard.current[Key.PageDown].wasPressedThisFrame) 
            {
                Debug.Log("sideLeft2");
                IsGamePad = false;
                return InputKeyType.SideLeft2;
            } else
            if(Keyboard.current[Key.PageUp].wasPressedThisFrame) 
            {
                Debug.Log("sideRight2");
                IsGamePad = false;
                return InputKeyType.SideRight2;
            } else
            if(Keyboard.current[Key.Enter].wasPressedThisFrame) 
            {
                Debug.Log("start");
                IsGamePad = false;
                return InputKeyType.Start;
            } else
            if(Keyboard.current[Key.RightShift].wasPressedThisFrame) 
            {
                Debug.Log("select");
                IsGamePad = false;
                return InputKeyType.Select;
            }
            return InputKeyType.None;
        }

        private InputKeyType UpdateGamePad()
        {
            var gamePad = Gamepad.current;
            if (gamePad == null)
            {
                return InputKeyType.None;
            }
            // 十字
            if (gamePad.dpad.up.isPressed) 
            {
                return InputKeyType.Up;
            }
            if (gamePad.dpad.down.isPressed) 
            {
                return InputKeyType.Down;
            }
            if (gamePad.dpad.right.isPressed) 
            {
                return InputKeyType.Right;
            }
            if (gamePad.dpad.left.isPressed) 
            {
                return InputKeyType.Left;
            }

            if (gamePad.aButton.wasPressedThisFrame) 
            {
                return InputKeyType.Decide;
            }
            if (gamePad.bButton.wasPressedThisFrame)
            {
                return InputKeyType.Cancel;
            }
            if (gamePad.xButton.wasPressedThisFrame) 
            {
                return InputKeyType.Option1;
            }
            if (gamePad.yButton.wasPressedThisFrame)
            {
                return InputKeyType.Option2;
            }

            if (gamePad.buttonEast.wasPressedThisFrame) 
            {
                return InputKeyType.Cancel;
            }
            if (gamePad.buttonWest.wasPressedThisFrame) 
            {
                return InputKeyType.Option1;
            }
            if (gamePad.buttonNorth.wasPressedThisFrame)
            {
                return InputKeyType.Option2;
            }
            if (gamePad.buttonSouth.wasPressedThisFrame)
            {
                return InputKeyType.Decide;
            }

            if (gamePad.circleButton.wasPressedThisFrame) 
            {
                return InputKeyType.Cancel;
            }
            if (gamePad.crossButton.wasPressedThisFrame)
            {
                return InputKeyType.Decide;
            }
            if (gamePad.triangleButton.wasPressedThisFrame) 
            {
                return InputKeyType.Option2;
            }
            if (gamePad.squareButton.wasPressedThisFrame)
            {
                return InputKeyType.Option1;
            }

            // start,select
            if (gamePad.startButton.wasPressedThisFrame) 
            {
                return InputKeyType.Start;
            }
            if (gamePad.selectButton.wasPressedThisFrame) 
            {
                return InputKeyType.Select;
            }


            // L1,R1
            if (gamePad.leftShoulder.wasPressedThisFrame) 
            {
                return InputKeyType.SideLeft1;
            }
            if (gamePad.rightShoulder.wasPressedThisFrame)
            {
                return InputKeyType.SideRight1;
            }

            // L2,R2
            if (gamePad.leftShoulder.isPressed)
            {
                return InputKeyType.SideLeft2;
            }
            if (gamePad.rightShoulder.isPressed)
            {
                return InputKeyType.SideRight2;
            }

            return InputKeyType.None;
        }

        public static bool IsMouseRightButtonDown()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
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
            return Application.platform == RuntimePlatform.WebGLPlayer;
        }

        public static bool IsPlatformStandAloneOrEditor()
        {
            return Application.isEditor || IsPlatformStandAlone();
        }

        public static bool IsPlatformStandAlone()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.OSXPlayer or RuntimePlatform.LinuxPlayer => true,
                _ => false,
            };
        }
    }

    public enum InputKeyType
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Decide,
        Cancel,
        Option1, // □,A
        Option2, // △,S
        SideLeft1, // L1
        SideRight1, // R1
        SideLeft2, // L2
        SideRight2, // R2
        Start,
        Select,
    }
}