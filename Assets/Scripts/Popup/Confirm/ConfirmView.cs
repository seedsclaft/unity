using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmView : BaseView
{
    [SerializeField] private ConfirmCommandList commandList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    [SerializeField] private TextMeshProUGUI subText = null;
    private new System.Action<ConfirmComandType> _commandData = null;

    protected void Awake()
    {
        InitializeInput();
        Initialize();
    }

    void Initialize()
    {
        new ConfirmPresenter(this);
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetEvent(System.Action<ConfirmComandType> commandData)
    {
        _commandData = commandData;
    }
        
    public void SetConfirmCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallConfirmCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallConfirmCommand(ConfirmComandType commandType)
    {
        SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _commandData(commandType);
    }
}