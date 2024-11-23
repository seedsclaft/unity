using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

namespace Ryneus
{
    public class TitleView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        [SerializeField] private Button tapTitle = null;
        [SerializeField] private OnOffButton rankingButton = null;
        [SerializeField] private TextMeshProUGUI playerName = null;
        [SerializeField] private TextMeshProUGUI playerId = null;
        [SerializeField] private BaseList titleCommandList = null;
        public SystemData.CommandData TitleCommand => titleCommandList.ListItemData<SystemData.CommandData>();
        
        private new System.Action<ViewEvent> _commandData = null;
        public new void SetEvent(System.Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType titleCommandType)
        {
            var commandType = new ViewCommandType
            {
                TitleCommandType = titleCommandType
            };
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
            rankingButton?.OnClickAddListener(() => 
            {
                CallRanking();
            });
            new TitlePresenter(this);
            tapTitle.onClick.AddListener(OnClickTitle);
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

        public void SetPlayerData(string name,string id)
        {
            playerName?.SetText(name);
            playerId?.SetText(id);
        }

        private void OnClickTitle()
        {
            CallEvent(CommandType.SelectTitle);
        }

        private void CallSideMenu()
        {
            CallEvent(CommandType.SelectSideMenu);
        }

        private void CallRanking()
        {
            CallEvent(CommandType.Ranking);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            switch (keyType)
            {
                case InputKeyType.Decide:
                case InputKeyType.Start:
                    OnClickTitle();
                    return;
                case InputKeyType.Option1:
                    CallSideMenu();
                    return;
                case InputKeyType.Option2:
                    CallRanking();
                    return;
            }
        }
    }
}

namespace Ryneus
{    
    public partial class ViewCommandType
    {
        public CommandType TitleCommandType;
    }
}

namespace Title
{    
    public enum CommandType
    {
        None = 0,
        SelectTitle,
        SelectSideMenu,
        Ranking,
    }
}