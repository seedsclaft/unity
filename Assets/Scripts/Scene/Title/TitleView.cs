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
        public override void Initialize() 
        {
            base.Initialize();
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            rankingButton?.OnClickAddListener(() => 
            {
                var eventData = new TitleViewEvent(CommandType.Ranking);
                _commandData(eventData);
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

        private void CallSideMenu()
        {
            var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        public void InputHandler(InputKeyType keyType, bool pressed)
        {
            if (keyType == InputKeyType.Decide || keyType == InputKeyType.Start)
            {
                OnClickTitle();
            } else
            if (keyType == InputKeyType.Option1)
            {
                CallSideMenu();
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