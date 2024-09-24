using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

namespace Ryneus
{
    public class MainMenuView : BaseView
    {
        [SerializeField] private StageInfoComponent component;
        [SerializeField] private OnOffButton nextStageButton;

        private new System.Action<MainMenuViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            nextStageButton?.SetText(DataSystem.GetText(17010));
            nextStageButton?.SetCallHandler(() => 
            {
                var eventData = new MainMenuViewEvent(CommandType.NextStage);
                _commandData(eventData);
            });
            new MainMenuPresenter(this);
        }

        public void SetInitHelpText()
        {
            HelpWindow.SetHelpText(DataSystem.GetText(11040));
            HelpWindow.SetInputInfo("MAINMENU");
        }

        public void SetHelpWindow()
        {
            SetInitHelpText();
        }

        public void SetEvent(System.Action<MainMenuViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetStageData(StageInfo stageInfo)
        {
            if (stageInfo != null)
            {
                component.UpdateInfo(stageInfo);
            }
        }
    }
}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        NextStage = 100,
    }
}

public class MainMenuViewEvent
{
    public CommandType commandType;
    public object template;

    public MainMenuViewEvent(CommandType type)
    {
        commandType = type;
    }
}