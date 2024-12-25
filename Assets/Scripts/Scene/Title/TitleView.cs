using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    using Title;
    public class TitleView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        [SerializeField] private BaseList titleCommandList = null;
        public SystemData.CommandData TitleCommand => titleCommandList.ListItemData<SystemData.CommandData>();
        
        private new System.Action<ViewEvent> _commandData = null;
        public new void SetEvent(System.Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType titleCommandType)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.Title,titleCommandType);
            var eventData = new ViewEvent(commandType);
            _commandData(eventData);
        }

        public override void Initialize() 
        {
            base.Initialize();
            InitializeTitleCommand();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            new TitlePresenter(this);
        }

        private void InitializeTitleCommand()
        {
            titleCommandList.Initialize();
            SetInputHandler(titleCommandList.gameObject);
            titleCommandList.SetInputHandler(InputKeyType.Decide,OnClickTitle);
        }        
        
        public void SetTitleCommand(List<ListData> titleCommand)
        {
            titleCommandList.SetData(titleCommand);
            titleCommandList.Activate();
        }

        public void SetVersion(string text)
        {
            versionText.SetText(text);
        }

        private void OnClickTitle()
        {
            CallEvent(CommandType.SelectTitle);
        }

        private void CallSideMenu()
        {
            CallEvent(CommandType.SelectSideMenu);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
        }
    }

    namespace Title
    {    
        public enum CommandType
        {
            None = 0,
            SelectTitle,
            SelectSideMenu,
        }
    }
}