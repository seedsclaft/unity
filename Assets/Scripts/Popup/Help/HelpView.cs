using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class HelpView : BaseView
    {
        [SerializeField] private BaseList helpTextList = null;
        private new System.Action<HelpViewEvent> _commandData = null;
        private System.Action<int> _callEvent = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            new HelpPresenter(this);
        }
        
        public void SetHelp(List<ListData> helpText)
        {
            helpTextList.Initialize();
            helpTextList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(helpTextList.gameObject);
            helpTextList.SetData(helpText);
            helpTextList.Activate();
        }
        
        public void SetEvent(System.Action<HelpViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetBackEvent(System.Action backEvent)
        {
            SetBackCommand(() => 
            {    
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                if (backEvent != null) backEvent();
            });
            ChangeBackCommandActive(true);
        }
    }

    namespace Help
    {
        public enum CommandType
        {
            None = 0,
        }

        public enum HelpType
        {
            None = 0,
            Battle = 1,
        }
    }

    public class HelpViewEvent
    {
        public Help.CommandType commandType;
        public object template;

        public HelpViewEvent(Help.CommandType type)
        {
            commandType = type;
        }
    }
}