using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ryneus
{
    public class InputSystem
    {
        public static bool IsGamePad = false;

        public static List<InputData> _inputDates = new();
        public static void Initialize()
        {
            _inputDates.Clear();
            var enums = System.Enum.GetValues(typeof(InputKeyType));
            foreach (var e in enums)
            {
                var keyData = new InputData((InputKeyType)e);
                _inputDates.Add(keyData);
            }
        }

        public static InputData GetInputDate(InputKeyType inputKeyType)
        {
            var data = _inputDates.Find(a => a.InputKeyType == inputKeyType);
            return data;
        }

        public InputKeyType Update()
        {
            if (GameSystem.ConfigData.InputType == InputType.MouseOnly)
            {
                return InputKeyType.None;
            }
            UpdateGamePadData();
            var gamePadKey = UpdateGamePad();
            if (gamePadKey != InputKeyType.None)
            {
                IsGamePad = true;
                return gamePadKey;
            }
            UpdateKeyBoardData();
            return UpdateKeyBoard();
        }

        private void UpdateInputKeyData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.KeyControl keyControl,Key key)
        {
            var keyData = GetInputDate(inputKeyType);
            if((keyControl != null && keyControl.wasPressedThisFrame) || Keyboard.current[key].wasPressedThisFrame) 
            {
                keyData.OnDown();
            } else
            if((keyControl != null && keyControl.isPressed) || Keyboard.current[key].isPressed) 
            {
                keyData.OnPress();
            } else
            if((keyControl != null && keyControl.wasReleasedThisFrame) || Keyboard.current[key].wasReleasedThisFrame) 
            {
                keyData.OnLeft();
            } else
            {
                keyData.OnLeftEnd();
            }
        }

        private void UpdateKeyBoardData()
        {
            UpdateInputKeyData(InputKeyType.Up,Keyboard.current.upArrowKey,Key.W);
            UpdateInputKeyData(InputKeyType.Down,Keyboard.current.downArrowKey,Key.S);
            UpdateInputKeyData(InputKeyType.Left,Keyboard.current.leftArrowKey,Key.A);
            UpdateInputKeyData(InputKeyType.Right,Keyboard.current.rightArrowKey,Key.D);
            UpdateInputKeyData(InputKeyType.Decide,null,Key.Space);
            UpdateInputKeyData(InputKeyType.Cancel,null,Key.LeftShift);
            UpdateInputKeyData(InputKeyType.Option1,null,Key.R);
            UpdateInputKeyData(InputKeyType.Option2,null,Key.T);
            UpdateInputKeyData(InputKeyType.SideLeft1,null,Key.Q);
            UpdateInputKeyData(InputKeyType.SideRight1,null,Key.E);
            UpdateInputKeyData(InputKeyType.SideLeft2,null,Key.PageDown);
            UpdateInputKeyData(InputKeyType.SideRight2,null,Key.PageUp);
            UpdateInputKeyData(InputKeyType.Start,null,Key.Enter);
            UpdateInputKeyData(InputKeyType.Select,null,Key.RightShift);
        }

        private void UpdateGamePadData()
        {
            var gamePad = Gamepad.current;
            if (gamePad == null)
            {
                return;
            }
            UpdateInputGamePadData(InputKeyType.Up,gamePad.dpad.up,null);
            UpdateInputGamePadData(InputKeyType.Down,gamePad.dpad.down,null);
            UpdateInputGamePadData(InputKeyType.Left,gamePad.dpad.left,null);
            UpdateInputGamePadData(InputKeyType.Right,gamePad.dpad.right,null);
            UpdateInputGamePadData(InputKeyType.Decide,gamePad.bButton,null);
            UpdateInputGamePadData(InputKeyType.Cancel,gamePad.aButton,null);
            UpdateInputGamePadData(InputKeyType.Option1,gamePad.yButton,null);
            UpdateInputGamePadData(InputKeyType.Option2,gamePad.xButton,null);
            UpdateInputGamePadData(InputKeyType.SideLeft1,gamePad.leftTrigger,null);
            UpdateInputGamePadData(InputKeyType.SideRight1,gamePad.rightTrigger,null);
            UpdateInputGamePadData(InputKeyType.SideLeft2,gamePad.leftShoulder,null);
            UpdateInputGamePadData(InputKeyType.SideRight2,gamePad.rightShoulder,null);
            UpdateInputGamePadData(InputKeyType.Start,gamePad.startButton,null);
            UpdateInputGamePadData(InputKeyType.Select,gamePad.selectButton,null);
            UpdateInputStickGamePadData(InputKeyType.LeftStickUp,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickDown,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickLeft,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.LeftStickRight,gamePad.leftStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickUp,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickDown,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickLeft,gamePad.rightStick);
            UpdateInputStickGamePadData(InputKeyType.RightStickRight,gamePad.rightStick);
        }

        private InputKeyType UpdateKeyBoard()
        {
            InputKeyType keyType = InputKeyType.None;
            if(Keyboard.current.upArrowKey.isPressed || Keyboard.current[Key.W].isPressed) 
            {
                keyType = InputKeyType.Up;
            } else
            if(Keyboard.current.downArrowKey.isPressed || Keyboard.current[Key.S].isPressed) 
            {
                keyType = InputKeyType.Down;
            } else
            if(Keyboard.current.leftArrowKey.isPressed || Keyboard.current[Key.A].isPressed) 
            {
                keyType = InputKeyType.Left;
            } else
            if(Keyboard.current.rightArrowKey.isPressed || Keyboard.current[Key.D].isPressed) 
            {
                keyType = InputKeyType.Right;
            } else
            if(Keyboard.current[Key.Space].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Decide;
            } else
            if(Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.Escape].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Cancel;
            } else
            if(Keyboard.current[Key.R].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Option1;
            } else
            if(Keyboard.current[Key.T].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Option2;
            } else
            if(Keyboard.current[Key.Q].isPressed) 
            {
                keyType = InputKeyType.SideLeft1;
            } else
            if(Keyboard.current[Key.E].isPressed) 
            {
                keyType = InputKeyType.SideRight1;
            }  else
            if(Keyboard.current[Key.PageDown].isPressed) 
            {
                keyType = InputKeyType.SideLeft2;
            } else
            if(Keyboard.current[Key.PageUp].isPressed) 
            {
                keyType = InputKeyType.SideRight2;
            } else
            if(Keyboard.current[Key.Enter].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Start;
            } else
            if(Keyboard.current[Key.RightShift].wasPressedThisFrame) 
            {
                keyType = InputKeyType.Select;
            }
            if (keyType != InputKeyType.None)
            {
                IsGamePad = false;
            }
            return keyType;
        }

        private void UpdateInputGamePadData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.ButtonControl keyControl,UnityEngine.InputSystem.Controls.ButtonControl stick)
        {
            var keyData = GetInputDate(inputKeyType);
            if((keyControl != null && keyControl.wasPressedThisFrame) || (stick != null && stick.wasPressedThisFrame)) 
            {
                keyData.OnDown();
            } else
            if((keyControl != null && keyControl.isPressed) || (stick != null && stick.isPressed)) 
            {
                keyData.OnPress();
            } else
            if((keyControl != null && keyControl.wasReleasedThisFrame) || (stick != null && stick.wasReleasedThisFrame)) 
            {
                keyData.OnLeft();
            } else
            {
                keyData.OnLeftEnd();
            }
        }
        
        private void UpdateInputStickGamePadData(InputKeyType inputKeyType,UnityEngine.InputSystem.Controls.StickControl stickControl)
        {
            var keyData = GetInputDate(inputKeyType);
            switch (inputKeyType)
            {
                case InputKeyType.LeftStickUp:
                case InputKeyType.RightStickUp:
                    if (stickControl.y.value > 0)
                    {
                        keyData.SetValue(stickControl.y.value);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickDown:
                case InputKeyType.RightStickDown:
                    if (stickControl.y.value < 0)
                    {
                        keyData.SetValue(stickControl.y.value * -1);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickLeft:
                case InputKeyType.RightStickLeft:
                    if (stickControl.x.value < 0)
                    {
                        keyData.SetValue(stickControl.x.value * -1);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
                case InputKeyType.LeftStickRight:
                case InputKeyType.RightStickRight:
                    if (stickControl.x.value > 0)
                    {
                        keyData.SetValue(stickControl.x.value);
                    } else
                    {
                        keyData.SetValue(0);
                    }
                    break;
            }
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
            if (gamePad.leftStick.value.y > 0)
            {
                return InputKeyType.LeftStickUp;
            }
            if (gamePad.leftStick.value.y < 0)
            {
                return InputKeyType.LeftStickDown;
            }

            if (gamePad.leftStick.value.x < 0)
            {
                return InputKeyType.LeftStickLeft;
            }
            if (gamePad.leftStick.value.x > 0)
            {
                return InputKeyType.LeftStickRight;
            }
            return InputKeyType.None;
        }

        public static bool IsMouseRightButtonDown()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.GetMouseButtonDown(1);
            }
            return false;
        }
        
        public static Vector3 MouseMovePosition()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.mousePosition;
            }
            return new Vector3(0,0,0);
        }

        public static Vector2 MouseWheelPosition()
        {
            if (IsPlatformStandAloneOrEditor() || EnableWebGLInput())
            { 
                return Input.mouseScrollDelta;
            }
            return new Vector2(0,0);
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
        None = -1,
        Up = 1,
        Down = 0,
        Left = 3,
        Right = 2,
        Decide = 4,
        Cancel = 5,
        Option1 = 6, // □,A
        Option2 = 7, // △,S
        SideLeft1 = 9, // L1
        SideRight1 = 8, // R1
        SideLeft2 = 11, // L2
        SideRight2 = 10, // R2
        Start = 12,
        LeftStickUp,
        LeftStickDown,
        LeftStickLeft,
        LeftStickRight,
        RightStickUp,
        RightStickDown,
        RightStickLeft,
        RightStickRight,
        Select,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }
}