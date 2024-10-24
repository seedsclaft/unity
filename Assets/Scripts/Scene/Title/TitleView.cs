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
        private new System.Action<TitleViewEvent> _commandData = null;
        [SerializeField] private Button tapTitle = null;
        [SerializeField] private OnOffButton rankingButton = null;
        [SerializeField] private TextMeshProUGUI playerName = null;
        [SerializeField] private TextMeshProUGUI playerId = null;
        public override void Initialize() 
        {
            base.Initialize();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            rankingButton?.OnClickAddListener(() => 
            {
                CallRanking();
            });
            new TitlePresenter(this);
            tapTitle.onClick.AddListener(() => OnClickTitle());
        }

        private void OnClickTitle()
        {
            var eventData = new TitleViewEvent(CommandType.SelectTitle);
            _commandData(eventData);
        }

        public void SetEvent(System.Action<TitleViewEvent> commandData)
        {
            _commandData = commandData;
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

        private void CallSideMenu()
        {
            var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        private void CallRanking()
        {
            var eventData = new TitleViewEvent(CommandType.Ranking);
            _commandData(eventData);
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

    public class TitleViewEvent
    {
        public CommandType commandType;
        public object template;

        public TitleViewEvent(CommandType type)
        {
            commandType = type;
        }
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