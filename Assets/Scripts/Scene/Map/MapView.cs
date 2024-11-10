using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Ryneus
{
    public class MapView : BaseView ,IInputHandlerEvent
    {
        private new System.Action<MapViewEvent> _commandData = null;

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
            virtualModelController.Initialize();
            CommandCreateMapObject(prefab);
            _mapPrefab = prefab;
        }

        public void SetEvent(System.Action<MapViewEvent> commandData)
        {
            _commandData = commandData;
        }

        private void CallSideMenu()
        {
            var eventData = new MapViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {

            if (InputSystem.GetInputDate(InputKeyType.Decide).IsTrigger())
            {
                virtualModelController?.Jump();
                _commandData(new MapViewEvent(CommandType.BattleStart));
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

    public class MapViewEvent
    {
        public CommandType commandType;
        public object template;

        public MapViewEvent(CommandType type)
        {
            commandType = type;
        }
    }
}


namespace Map
{
    public enum CommandType
    {
        None = 0,
        SelectMap,
        SelectSideMenu,
        Ranking,
        BattleStart,
    }
}