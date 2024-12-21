using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    using Boot;
    public class BootView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button logoButton = null;
        private new System.Action<ViewEvent> _commandData = null;
        public new void SetEvent(System.Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType bootCommand)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.Boot,bootCommand);
            var eventData = new ViewEvent(commandType);
            _commandData(eventData);
        }

        public override void Initialize() 
        {
            base.Initialize();
            new BootPresenter(this);
            if (TestMode == false)
            {
                logoButton.onClick.AddListener(() => CallLogoClick());
            }
            logoButton.gameObject.SetActive(TestMode == false);
        }


        private void CallLogoClick()
        {
            CallEvent(CommandType.LogoClick);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (keyType != InputKeyType.None)
            {
                CallLogoClick();
            }
        }

    }
    namespace Boot
    {    
        public enum CommandType
        {
            None = 0,
            LogoClick,
        }
    }
}