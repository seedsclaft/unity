using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Confirm;

public class ConfirmView : BaseView,IInputHandlerEvent
{
    [SerializeField] private ConfirmCommandList commandList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    [SerializeField] private GameObject skillInfoRoot = null;
    [SerializeField] private GameObject skillInfoPrefab = null;
    private System.Action<ConfirmComandType> _confirmEvent = null;
    private new System.Action<ConfirmViewEvent> _commandData = null;
    private ConfirmInfo _confirmInfo = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new ConfirmPresenter(this);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetSkillInfo(List<SkillInfo> skillInfos)
    {
        if (skillInfos == null) return;
        foreach(Transform child in skillInfoRoot.transform){
            Destroy(child.gameObject);
        }
        foreach (var skillInfo in skillInfos)
        {
            GameObject prefab = Instantiate(skillInfoPrefab);
            prefab.transform.SetParent(skillInfoRoot.transform, false);
            SkillAction skillAction = prefab.GetComponent<SkillAction>();
            skillAction.SetData(skillInfo,0);
            skillAction.UpdateViewItem();
        }
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

    public void  SetViewInfo(ConfirmInfo confirmInfo)
    {
        _confirmInfo = confirmInfo;
        SetIsNoChoice(confirmInfo.IsNoChoise);
        SetTitle(confirmInfo.Title);
        SetSkillInfo(confirmInfo.SkillInfos);
        SetConfirmEvent(confirmInfo.CallEvent);
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

    public void InputHandler(InputKeyType keyType)
    {

    }

    public new void MouseCancelHandler()
    {
        if (_confirmInfo.IsNoChoise)
        {
            CallConfirmCommand(ConfirmComandType.Yes);
        } else
        {
            CallConfirmCommand(ConfirmComandType.No);
        }
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
