using System;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Ryneus
{
    public class MapView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private SymbolList symbolInfoList;
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType mapCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType
            {
                MapCommandType = mapCommandType
            };
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }

        private VirtualModelController _virtualModelController = null;

        private bool _busy = false;
        public new void SetBusy(bool isBusy)
        {
            _busy = isBusy;
        }

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
            symbolInfoList.SetInputHandler(InputKeyType.Decide,OnClickSymbol);
            symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelSymbol);
            symbolInfoList.SetInputHandler(InputKeyType.Right,() => OnSelectSymbolIndex(1));
            symbolInfoList.SetInputHandler(InputKeyType.Left,() => OnSelectSymbolIndex(-1));
            symbolInfoList.SetSelectedHandler(OnSelectListSymbolList);
            //symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelActor);
            AddViewActives(symbolInfoList);
        }

        public void SetSymbolList(List<ListData> symbolList)
        {
            symbolInfoList.SetData(symbolList,false,() => 
            {
            });
        }

        public void UpdateSelectSymbolList(SymbolInfo symbolInfo)
        {
            symbolInfoList.UpdateAllItems();
            symbolInfoList.UpdateSymbolInfo(symbolInfo);
        }

        public void SetPositionSymbolRecords(int selectIndex)
        {
            var resultIndex = symbolInfoList.DataCount - selectIndex;
            if (resultIndex < 0)
            {
                resultIndex = 0;
            }
            symbolInfoList.UpdateSelectIndex(resultIndex - 1);
            symbolInfoList.UpdateScrollRect(resultIndex + 1);
        }

        private void OnClickSymbol()
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            var data = symbolInfoList.SelectSymbolInfo();
            if (data != null)
            {
                CallEvent(CommandType.OnClickSymbol);
            }
        }

        private void OnCancelSymbol()
        {
            symbolInfoList.gameObject.SetActive(false);
            CallEvent(CommandType.OnCancelSymbol);
        }

        private void OnSelectSymbolIndex(int selectIndex)
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            CallEvent(CommandType.OnSelectSymbolIndex,selectIndex);
        }

        private void OnSelectListSymbolList()
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            var data = symbolInfoList.SelectSymbolInfo();
            if (data != null)
            {
                CallEvent(CommandType.OnSelectSymbolList,data);
            }
        }

        public void CreateMapLeaderActor(GameObject gameObject)
        {
            var prefab = Instantiate(gameObject);
            _virtualModelController = prefab.GetComponent<VirtualModelController>();
            _virtualModelController.Initialize(true);
            CommandCreateMapObject(prefab);
            _mapPrefab = prefab;
        }

        public void UpdatePartyInfo(PartyInfo partyInfo)
        {
            symbolInfoList.UpdatePartyInfo(partyInfo);
        }

        private void CallSideMenu()
        {
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (_busy == true)
            {
                return;
            }
            if (InputSystem.IsGamePad)
            {
                if (InputSystem.GetInputDate(InputKeyType.Decide).IsTrigger())
                {
                    _virtualModelController?.Jump();
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
                case InputKeyType.Option1:
                    CallEvent(CommandType.CallSymbol);
                    symbolInfoList.gameObject.SetActive(true);
                    return;
            }
        }

        public new void MouseMoveHandler(Vector3 position)
        {
            if (_busy == true)
            {
                return;
            }
            _virtualModelController?.MouseMove(position);
        }

        public new void MouseWheelHandler(Vector2 position)
        {
            if (_busy == true)
            {
                return;
            }
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
        CallStatus,
        CallSymbol,
        OnClickSymbol,
        OnCancelSymbol,
        OnSelectSymbolIndex,
        OnSelectSymbolList,
    }
}