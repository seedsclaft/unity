using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

namespace Ryneus
{
    public class TitleView : BaseView 
    {
        [SerializeField] private TextMeshProUGUI versionText = null;
        private new System.Action<TitleViewEvent> _commandData = null;
        [SerializeField] private Button tapTitle = null;
        public override void Initialize() 
        {
            base.Initialize();
            SideMenuButton.onClick.AddListener(() => 
            {
                CallSideMenu();
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
            versionText.text = text;
        }

        private void CallSideMenu()
        {
            var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }
    }


    public class TitleViewEvent
    {
        public Title.CommandType commandType;
        public object template;

        public TitleViewEvent(Title.CommandType type)
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
        TitleCommand,
        Option,
        SelectTitle,
        SelectSideMenu,
    }
}