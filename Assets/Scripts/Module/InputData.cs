using UnityEngine;

namespace Ryneus
{
    public class InputData
    {
        private bool _onDown = false; // 押された
        private bool _onPress = false; // 押している状態
        private bool _onLeft = false; // 離れた

        private InputKeyType _inputKeyType; // 担当しているキー
        public InputKeyType InputKeyType => _inputKeyType;

        public InputData(InputKeyType inputKeyType)
        {
            _inputKeyType = inputKeyType;
        }

        public bool IsTrigger()
        {
            return _onDown || _onPress;
        }

        public bool IsPress()
        {
            return _onPress;
        }

        public bool IsRelease()
        {
            return _onLeft;
        }

        public void OnDown()
        {
            _onDown = true;
            _onPress = false;
            _onLeft = false;
        }

        public void OnPress()
        {
            _onDown = true;
            _onPress = true;
            _onLeft = false;
        }

        public void OnLeft()
        {
            _onDown = false;
            _onPress = false;
            _onLeft = true;
        }

        public void OnLeftEnd()
        {
            _onDown = false;
            _onPress = false;
            _onLeft = false;
        }
    }
}
