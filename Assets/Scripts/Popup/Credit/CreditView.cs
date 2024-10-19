using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class CreditView : BaseView,IInputHandlerEvent
    {
        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<CreditViewEvent> _commandData = null;
        public override void Initialize() 
        {
            base.Initialize();
            
            SetBaseAnimation(popupAnimation);
            SetHelpInputInfo("CREDIT");
            new CreditPresenter(this);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetEvent(System.Action<CreditViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {
            if (keyType == InputKeyType.Cancel)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                BackEvent();
            }
            if (keyType == InputKeyType.Down)
            {
                var value = scrollRect.normalizedPosition.y - 0.1f;
                scrollRect.normalizedPosition = new Vector2(0,value);
                if (scrollRect.normalizedPosition.y < 0)
                {
                    scrollRect.normalizedPosition = new Vector2(0,0);
                }
            }
            if (keyType == InputKeyType.Up)
            {
                var value = scrollRect.normalizedPosition.y + 0.1f;
                scrollRect.normalizedPosition = new Vector2(0,value);
                if (scrollRect.normalizedPosition.y > 1)
                {
                    scrollRect.normalizedPosition = new Vector2(0,1);
                }
            }
        }
    }

    namespace Credit
    {
        public enum CommandType
        {
            None = 0,
        }
    }

    public class CreditViewEvent
    {
        public Credit.CommandType commandType;
        public object template;

        public CreditViewEvent(Credit.CommandType type)
        {
            commandType = type;
        }
    }
}