using System;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Ryneus
{
    public class MapView : BaseView ,IInputHandlerEvent
    {
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType mapCommandType)
        {
            var commandType = new ViewCommandType
            {
                MapCommandType = mapCommandType
            };
            var eventData = new ViewEvent(commandType);
            _commandData(eventData);
        }

        private VirtualModelController virtualModelController = null;

        private GameObject _mapPrefab = null;
        public override void Initialize() 
        {
            base.Initialize();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            
            new MapPresenter(this);
        }

        public void CreateMapLeaderActor(GameObject gameObject)
        {
            var prefab = Instantiate(gameObject);
            virtualModelController = prefab.GetComponent<VirtualModelController>();
            virtualModelController.Initialize(true);
            CommandCreateMapObject(prefab);
            _mapPrefab = prefab;
        }

        private void CallSideMenu()
        {
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (InputSystem.IsGamePad)
            {
                if (InputSystem.GetInputDate(InputKeyType.Decide).IsTrigger())
                {
                    virtualModelController?.Jump();
                    CallEvent(CommandType.BattleStart);
                }

                if (InputSystem.GetInputDate(InputKeyType.LeftStickUp).IsTrigger())
                {
                    virtualModelController?.Forward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.LeftStickDown).IsTrigger())
                {
                    virtualModelController?.BackForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.LeftStickRight).IsTrigger())
                {
                    virtualModelController?.RightForward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.LeftStickLeft).IsTrigger())
                {
                    virtualModelController?.LeftForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.RightStickUp).IsTrigger())
                {
                    virtualModelController?.DownCamera();
                } else
                if (InputSystem.GetInputDate(InputKeyType.RightStickDown).IsTrigger())
                {
                    virtualModelController?.UpCamera();
                }

                if (InputSystem.GetInputDate(InputKeyType.RightStickLeft).IsTrigger())
                {
                    virtualModelController?.LeftCamera();
                } else
                if (InputSystem.GetInputDate(InputKeyType.RightStickRight).IsTrigger())
                {
                    virtualModelController?.RightCamera();
                }
            } else
            {
                if (InputSystem.GetInputDate(InputKeyType.Decide).IsTrigger())
                {
                    virtualModelController?.Jump();
                    CallEvent(CommandType.BattleStart);
                }

                if (InputSystem.GetInputDate(InputKeyType.Up).IsTrigger())
                {
                    virtualModelController?.Forward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.Down).IsTrigger())
                {
                    virtualModelController?.BackForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.Right).IsTrigger())
                {
                    virtualModelController?.RightForward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.Left).IsTrigger())
                {
                    virtualModelController?.LeftForward();
                }

            }
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsTrigger())
            {
                virtualModelController?.RightRotation();
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsTrigger())
            {
                virtualModelController?.LeftRotation();
            }

            if (InputSystem.GetInputDate(InputKeyType.SideRight2).IsTrigger())
            {
                virtualModelController?.RightCamera();
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideLeft2).IsTrigger())
            {
                virtualModelController?.LeftCamera();
            }
            switch (keyType)
            {
                case InputKeyType.None:
                    virtualModelController?.Stop();
                    return;
                case InputKeyType.Cancel:
                    CallEvent(CommandType.CallStatus);
                    return;
            }
        }

        public new void MouseMoveHandler(Vector3 position)
        {
            virtualModelController?.MouseMove(position);
        }

        public new void MouseWheelHandler(Vector2 position)
        {
            virtualModelController?.MouseWheel(position);
        }

        public void ClearMap()
        {
            CommandGameSystem(Base.CommandType.MapClear);   
        }
    }
}

namespace Ryneus
{    
    public partial class ViewCommandType
    {
        public CommandType MapCommandType;
    }
}

namespace Map
{
    public enum CommandType
    {
        None = 0,
        BattleStart,
        CallStatus
    }
}