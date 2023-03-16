using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NameEntry;
using TMPro;

public class NameEntryView : BaseView
{
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private Button decideButton = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    private HelpWindow _helpWindow = null;

    private new System.Action<NameEntryViewEvent> _commandData = null;

    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new NameEntryPresenter(this);
        decideButton.onClick.AddListener(() => OnClickDecide());
        inputField.gameObject.SetActive(false);
        decideButton.gameObject.SetActive(false);
    }

    public void SetHelpWindow(){
    }

    public void SetEvent(System.Action<NameEntryViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    private void OnClickDecide()
    {
        var eventData = new NameEntryViewEvent(CommandType.EntryEnd);
        eventData.templete = inputField.text;
        _commandData(eventData);
        if (inputField != null) inputField.gameObject.SetActive(false);
        if (decideButton != null) decideButton.gameObject.SetActive(false);
    }

    public void StartNameEntry(){
        inputField.gameObject.SetActive(true);
        decideButton.gameObject.SetActive(true);
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
    public NameEntry.CommandType commandType;
    public object templete;

    public NameEntryViewEvent(NameEntry.CommandType type)
    {
        commandType = type;
    }
}
