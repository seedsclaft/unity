using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class InputSystemModel 
    {
        private List<IInputHandlerEvent> _inputHandler = new ();
        private int _inputBusyFrame = 0;
        private InputKeyType _lastInputKey = InputKeyType.None;
        private int _pressedFrame = 0;
        readonly int _pressFrame = 30;
        private bool _busy = false;
        public void SetBusy(bool busy)
        {
        }

        public void AddInputHandler(IInputHandlerEvent handler)
        {
            _inputHandler.Add(handler);
        }

        public void SetInputFrame(int frame)
        {
            _inputBusyFrame = frame;
        }

        private void InputHandler(InputKeyType keyType,bool pressed)
        {
            if (_inputBusyFrame >= 0) return;
            foreach (var handler in _inputHandler)
            {
                handler?.InputHandler(keyType,pressed);
            }
        }

        public void CallMouseCancel()
        {
            if (_busy) return;
            foreach (var handler in _inputHandler)
            {
                handler?.MouseCancelHandler();
            }
        }

        public void UpdateInputKeyType(InputKeyType keyType)
        {
            if (_lastInputKey != keyType)
            {
                _lastInputKey = keyType;
                _pressedFrame = 0;
            } else
            {
                if (_lastInputKey == keyType)
                {
                    _pressedFrame += 1;
                }
            }
            InputHandler(keyType,_pressedFrame > _pressFrame);
            if (InputSystem.IsMouseRightButtonDown())
            {
                CallMouseCancel();
            }
            if (_inputBusyFrame >= 0)
            {
                _inputBusyFrame--;
            }
        }
    }
}
