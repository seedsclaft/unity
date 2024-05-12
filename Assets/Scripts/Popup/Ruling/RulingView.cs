using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ruling;

namespace Ryneus
{
    public class RulingView : BaseView
    {
        [SerializeField] private BaseList commandList = null;
        
        [SerializeField] private ToggleSelect toggleSelect = null;
        
        [SerializeField] private BaseList ruleList = null;
        [SerializeField] private TextMeshProUGUI titleText = null;
        private new System.Action<RulingViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            ruleList.Initialize();
            commandList.Initialize();
            toggleSelect.Initialize(new List<string>()
            {
                DataSystem.GetText(900),
                DataSystem.GetText(901),
                DataSystem.GetText(902)
            });
            toggleSelect.SetClickHandler(() => 
            {
                var eventData = new RulingViewEvent(CommandType.SelectCategory);
                var data = toggleSelect.SelectTabIndex;
                eventData.template = data;
                _commandData(eventData);
            });
            new RulingPresenter(this);
        }
        
        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        public void SetEvent(System.Action<RulingViewEvent> commandData)
        {
            _commandData = commandData;
        }

        
        public void SetRuleCategory(List<ListData> ruleList)
        {
            //categoryList.SetSelectedHandler(() => CallRulingCommand());
            SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        }

        public void SetRuleCommand(List<ListData> ruleList)
        {
            commandList.SetData(ruleList);
            commandList.SetInputHandler(InputKeyType.Down,() => CallRulingCommand());
            commandList.SetInputHandler(InputKeyType.Up,() => CallRulingCommand());
            commandList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            commandList.SetSelectedHandler(() => CallRulingCommand());
            SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        }

        private void CallRulingCommand()
        {
            var listData = commandList.ListData;
            if (listData != null)
            {
                var eventData = new RulingViewEvent(CommandType.SelectRule);
                var data = (SystemData.CommandData)listData.Data;
                eventData.template = data.Id;
                _commandData(eventData);
            }
        }

        public void CommandSelectRule(List<ListData> helpList )
        {
            ruleList.SetData(helpList);
        }

        public void CommandRefresh(List<ListData> helpList)
        {
            commandList.Refresh();
            ruleList.SetData(helpList);
        }
    }
}

namespace Ruling
{
    public enum CommandType
    {
        None = 0,
        SelectRule = 1,
        SelectCategory = 2,
        Back
    }
}

public class RulingViewEvent
{
    public CommandType commandType;
    public object template;

    public RulingViewEvent(CommandType type)
    {
        commandType = type;
    }
}