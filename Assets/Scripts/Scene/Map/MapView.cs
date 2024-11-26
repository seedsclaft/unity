using System;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Ryneus
{
    public class MapView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList symbolInfoList;
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

        private VirtualModelController _virtualModelController = null;

        private GameObject _mapPrefab = null;
        public override void Initialize() 
        {
            base.Initialize();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            InitializeSymbolInfoList();
            
            new MapPresenter(this);
        }

        private void InitializeSymbolInfoList()
        {
            symbolInfoList.Initialize();
            SetInputHandler(symbolInfoList.gameObject);
            //symbolInfoList.SetInputHandler(InputKeyType.Decide,OnSelectActor);
            //symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelActor);
            AddViewActives(symbolInfoList);
        }

        public void SetSymbolList(List<ListData> symbolList)
        {
            symbolInfoList.SetData(symbolList,false,() => 
            {
                var SymbolRecordDates = symbolInfoList.GetComponentsInChildren<SymbolRecordData>();
                foreach (var SymbolRecordData in SymbolRecordDates)
                {
                    //SymbolRecordData.SetSymbolItemCallHandler((a) => OnClickSymbol());
                }
                symbolInfoList.SetSelectedHandler(() => 
                {
                    // 消さないこと
                });
            });
        }

        public void CreateMapLeaderActor(GameObject gameObject)
        {
            var prefab = Instantiate(gameObject);
            _virtualModelController = prefab.GetComponent<VirtualModelController>();
            _virtualModelController.Initialize(true);
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
                    _virtualModelController?.Jump();
                    CallEvent(CommandType.BattleStart);
                }

                if (InputSystem.GetInputDate(InputKeyType.LeftStickUp).IsTrigger())
                {
                    _virtualModelController?.Forward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.LeftStickDown).IsTrigger())
                {
                    _virtualModelController?.BackForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.LeftStickRight).IsTrigger())
                {
                    _virtualModelController?.RightForward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.LeftStickLeft).IsTrigger())
                {
                    _virtualModelController?.LeftForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.RightStickUp).IsTrigger())
                {
                    _virtualModelController?.DownCamera();
                } else
                if (InputSystem.GetInputDate(InputKeyType.RightStickDown).IsTrigger())
                {
                    _virtualModelController?.UpCamera();
                }

                if (InputSystem.GetInputDate(InputKeyType.RightStickLeft).IsTrigger())
                {
                    _virtualModelController?.LeftCamera();
                } else
                if (InputSystem.GetInputDate(InputKeyType.RightStickRight).IsTrigger())
                {
                    _virtualModelController?.RightCamera();
                }
            } else
            {
                if (InputSystem.GetInputDate(InputKeyType.Decide).IsTrigger())
                {
                    _virtualModelController?.Jump();
                    CallEvent(CommandType.BattleStart);
                }

                if (InputSystem.GetInputDate(InputKeyType.Up).IsTrigger())
                {
                    _virtualModelController?.Forward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.Down).IsTrigger())
                {
                    _virtualModelController?.BackForward();
                }
                
                if (InputSystem.GetInputDate(InputKeyType.Right).IsTrigger())
                {
                    _virtualModelController?.RightForward();
                } else
                if (InputSystem.GetInputDate(InputKeyType.Left).IsTrigger())
                {
                    _virtualModelController?.LeftForward();
                }

            }
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsTrigger())
            {
                _virtualModelController?.RightRotation();
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsTrigger())
            {
                _virtualModelController?.LeftRotation();
            }

            if (InputSystem.GetInputDate(InputKeyType.SideRight2).IsTrigger())
            {
                _virtualModelController?.RightCamera();
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideLeft2).IsTrigger())
            {
                _virtualModelController?.LeftCamera();
            }
            switch (keyType)
            {
                case InputKeyType.None:
                    _virtualModelController?.Stop();
                    return;
                case InputKeyType.Cancel:
                    CallEvent(CommandType.CallStatus);
                    return;
            }
        }

        public new void MouseMoveHandler(Vector3 position)
        {
            _virtualModelController?.MouseMove(position);
        }

        public new void MouseWheelHandler(Vector2 position)
        {
            _virtualModelController?.MouseWheel(position);
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