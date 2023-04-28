using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Confirm;

public class ConfirmView : BaseView
{
    [SerializeField] private ConfirmCommandList commandList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    [SerializeField] private TextMeshProUGUI subText = null;
    [SerializeField] private GameObject skillInfoRoot = null;
    [SerializeField] private GameObject skillInfoPrefab = null;
    private System.Action<ConfirmComandType> _confirmEvent = null;
    private new System.Action<ConfirmViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new ConfirmPresenter(this);
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetSkillInfo(SkillInfo skillInfo)
    {
        if (skillInfo == null) return;
        GameObject prefab = Instantiate(skillInfoPrefab);
        prefab.transform.SetParent(skillInfoRoot.transform, false);
        SkillAction skillAction = prefab.GetComponent<SkillAction>();
        skillAction.SetData(skillInfo,0);
        skillAction.UpdateViewItem();
    }

    public void SetIsNoChoice(bool isNoChoice)
    {
        if (isNoChoice)
        {
            var eventData = new ConfirmViewEvent(CommandType.IsNoChoise);
            _commandData(eventData);
        }
    }

    public void SetConfirmEvent(System.Action<ConfirmComandType> commandData)
    {
        _confirmEvent = commandData;
    }

    public void SetEvent(System.Action<ConfirmViewEvent> commandData)
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
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        _confirmEvent(commandType);
    }
}

namespace Confirm
{
    public enum CommandType
    {
        None = 0,
        IsNoChoise = 101,
    }
}
public class ConfirmViewEvent
{
    public Confirm.CommandType commandType;
    public object templete;

    public ConfirmViewEvent(Confirm.CommandType type)
    {
        commandType = type;
    }
}
