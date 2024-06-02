using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NameEntry;
using TMPro;

namespace Ryneus
{
    public class NameEntryView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private Button decideButton = null;

        private new System.Action<NameEntryViewEvent> _commandData = null;

        private int _inputLateUpdate = -1;
        public override void Initialize() 
        {
            base.Initialize();
            new NameEntryPresenter(this);
            decideButton.onClick.AddListener(() => OnClickDecide());
            inputField.gameObject.SetActive(false);
            decideButton.gameObject.SetActive(false);
            SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetHelpText("");
            HelpWindow.SetInputInfo("");
        }

        public void SetEvent(System.Action<NameEntryViewEvent> commandData)
        {
            _commandData = commandData;
        }
        
        private void OnClickDecide()
        {
            var eventData = new NameEntryViewEvent(CommandType.EntryEnd);
            eventData.template = inputField.text;
            _commandData(eventData);
            if (inputField != null) inputField.gameObject.SetActive(false);
            if (decideButton != null) decideButton.gameObject.SetActive(false);
        }

        public void ShowNameEntry(string defaultName)
        {
            inputField.text = defaultName;
        }

        public void StartNameEntry()
        {
            decideButton.gameObject.SetActive(true);
            inputField.gameObject.SetActive(true);
            inputField.Select();
            _inputLateUpdate = 1;
            HelpWindow.SetHelpText(DataSystem.GetText(5000));
            HelpWindow.SetInputInfo("NAMEENTRY");
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {
            if (inputField.gameObject.activeSelf == true && inputField.IsActive())
            {
                if (keyType == InputKeyType.Start)
                {
                    OnClickDecide();
                }
            }
        }

        private new void Update() 
        {
            if (_inputLateUpdate > -1)
            {
                _inputLateUpdate--;
                if (_inputLateUpdate == -1)
                {
                    inputField.MoveTextEnd(true);
                }
            } else
            {
                base.Update();
            }
        }
    }
}

namespace NameEntry
    {
    public enum CommandType
    {
        None = 0,
        StartEntry = 100,
        EntryEnd = 101,
    }
}
public class NameEntryViewEvent
{
    public CommandType commandType;
    public object template;

    public NameEntryViewEvent(CommandType type)
    {
        commandType = type;
    }
}
