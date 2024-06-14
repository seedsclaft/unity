﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Boot;

namespace Ryneus
{
    public class BootView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button logoButton = null;
        private new System.Action<BootViewEvent> _commandData = null;
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

        public void SetEvent(System.Action<BootViewEvent> commandData)
        {
            _commandData = commandData;
        }

        private void CallLogoClick()
        {
            var eventData = new BootViewEvent(CommandType.LogoClick);
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

namespace Boot
{
    public enum CommandType
    {
        None = 0,
        LogoClick,
    }
}

public class BootViewEvent
{
    public CommandType commandType;
    public object template;

    public BootViewEvent(CommandType type)
    {
        commandType = type;
    }
}