using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Boot;
using Ryneus;

namespace Ryneus
{
    public class BootView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button logoButton = null;
        private new System.Action<ViewEvent> _commandData = null;
        public new void SetEvent(System.Action<ViewEvent> commandData) => _commandData = commandData;

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
            var commandType = new ViewCommandType
            {
                BootCommandType = CommandType.LogoClick
            };
            var eventData = new ViewEvent(commandType);
            _commandData(eventData);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (keyType != InputKeyType.None)
            {
                CallLogoClick();
            }
        }
    }
}

namespace Ryneus
{    
    public partial class ViewCommandType
    {
        public CommandType BootCommandType;
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