using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Confirm;

public class ConfirmView : BaseView,IInputHandlerEvent
{
    [SerializeField] private BaseList commandList = null;
    [SerializeField] private TextMeshProUGUI titleText = null;
    [SerializeField] private BaseList skillInfoList = null;
    private System.Action<ConfirmCommandType> _confirmEvent = null;
    private new System.Action<ConfirmViewEvent> _commandData = null;
    private ConfirmInfo _confirmInfo = null;

    public override void Initialize() 
    {
        base.Initialize();
        new ConfirmPresenter(this);
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetSkillInfo(List<ListData> skillInfos)
    {
        if (skillInfos == null) return;
        skillInfoList.SetData(skillInfos);
    }

    public void SetIsNoChoice(bool isNoChoice)
    {
        if (isNoChoice)
        {
            var eventData = new ConfirmViewEvent(CommandType.IsNoChoice);
            _commandData(eventData);
        }
    }

    public void SetDisableIds(List<int> disableIds)
    {
        if (disableIds.Count > 0)
        {
            var eventData = new ConfirmViewEvent(CommandType.DisableIds);
            eventData.template = disableIds;
            _commandData(eventData);
        }
    }

    public void SetSelectIndex(int selectIndex)
    {
        commandList.Refresh(selectIndex);
    }

    public void SetConfirmEvent(System.Action<ConfirmCommandType> commandData)
    {
        _confirmEvent = commandData;
    }

    public void SetViewInfo(ConfirmInfo confirmInfo)
    {
        _confirmInfo = confirmInfo;
        SetIsNoChoice(confirmInfo.IsNoChoice);
        SetSelectIndex(confirmInfo.SelectIndex);
        SetTitle(confirmInfo.Title);
        SetSkillInfo(confirmInfo.SkillInfos());
        SetConfirmEvent(confirmInfo.CallEvent);
        SetDisableIds(confirmInfo.DisableIds);
    }

    public void SetEvent(System.Action<ConfirmViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetConfirmCommand(List<ListData> menuCommands)
    {
        commandList.SetData(menuCommands);
        commandList.SetInputHandler(InputKeyType.Decide,() => CallConfirmCommand());
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    public void CommandDisableIds(List<int> disableIds)
    {
        commandList.SetDisableIds(disableIds);
    }

    private void CallConfirmCommand()
    {
        var data = (SystemData.CommandData)commandList.ListData.Data;
        if (data != null)
        {
            var commandType = ConfirmCommandType.No;
            if (data.Key == "Yes")
            {
                commandType = ConfirmCommandType.Yes;
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
            _confirmEvent(commandType);
        }
    }

    public void InputHandler(InputKeyType keyType,bool pressed)
    {

    }

    public new void MouseCancelHandler()
    {
        if (_confirmInfo.IsNoChoice)
        {
            CallConfirmCommand();
        } else
        {
            CallConfirmCommand();
        }
    }
}

namespace Confirm
{
    public enum CommandType
    {
        None = 0,
        IsNoChoice = 101,
        DisableIds = 102,
    }
}
public class ConfirmViewEvent
{
    public Confirm.CommandType commandType;
    public object template;

    public ConfirmViewEvent(Confirm.CommandType type)
    {
        commandType = type;
    }
}
