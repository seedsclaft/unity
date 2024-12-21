using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Map;
    public class MapView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private SymbolList symbolInfoList;
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType mapCommandType,object sendData = null)
        {
        }

        private VirtualModelController _virtualModelController = null;
        public SymbolInfo SelectSymbolInfo => symbolInfoList.SelectSymbolInfo();

        private bool _viewBusy = false;
        public void SetViewBusy(bool isBusy)
        {
            _viewBusy = isBusy;
        }

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
            //symbolInfoList.SetSelectedHandler(OnSelectListSymbolList);
            //symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelActor);
            AddViewActives(symbolInfoList);
            symbolInfoList.gameObject.SetActive(false);
        }

        public void SetSymbolList(List<ListData> symbolList,int seekIndex,int seek)
        {
            symbolInfoList.SetSeekIndex(seekIndex);
            symbolInfoList.SetData(symbolList,true,() => 
            {
                var selectIndex = symbolInfoList.DataCount - seek;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }
                symbolInfoList.UpdateSelectIndex(selectIndex);
                symbolInfoList.UpdateScrollRect(selectIndex + 2);
            });
        }

        private void OnClickSymbol()
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            var data = symbolInfoList.SelectSymbolInfo();
            if (data != null)
            {
                CallEvent(CommandType.OnClickSymbol,data);
            }
        }

        private void OnCancelSymbol()
        {
            symbolInfoList.gameObject.SetActive(false);
            CallEvent(CommandType.OnCancelSymbol);
        }

        public void CreateMapLeaderActor(GameObject gameObject)
        {
            var prefab = Instantiate(gameObject);
            _virtualModelController = prefab.GetComponent<VirtualModelController>();
            _virtualModelController.Initialize(true);
            CommandCreateMapObject(prefab);
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
            if (_viewBusy == true)
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
            if (_viewBusy == true)
            {
                return;
            }
            _virtualModelController?.MouseMove(position);
        }

        public new void MouseWheelHandler(Vector2 position)
        {
            if (_viewBusy == true)
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
        }
    }
}