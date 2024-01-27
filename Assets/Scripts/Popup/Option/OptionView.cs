using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Option;

public class OptionView : BaseView
{
    private new System.Action<OptionViewEvent> _commandData = null;
    [SerializeField] private BaseList optionList = null;
    public ListData CurrentOptionCommand => optionList.ListData;

    public override void Initialize() 
    {
        base.Initialize();
        optionList.Initialize();
        new OptionPresenter(this);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
    
    public void SetOptionList(List<ListData> optionData)
    {
        optionList.SetData(optionData);
        SetInputHandler(optionList.GetComponent<IInputHandlerEvent>());
        optionList.SetInputHandler(InputKeyType.Right,() => CallChangeOptionValue(InputKeyType.Right));
        optionList.SetInputHandler(InputKeyType.Left,() => CallChangeOptionValue(InputKeyType.Left));
        optionList.SetInputHandler(InputKeyType.Option1,() => CallChangeOptionValue(InputKeyType.Option1));
        optionList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
    }

    public void CommandRefresh()
    {
        optionList.UpdateAllItems();
    }

    public void SetHelpWindow()
    {
        SetHelpText(DataSystem.GetTextData(500).Help);
        SetHelpInputInfo("OPTION");
    }

    public void SetEvent(System.Action<OptionViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        SetBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            GameSystem.ConfigData.InputType = GameSystem.TempData.TempInputType;
            if (GameSystem.ConfigData.InputType == false)
            {
                SetHelpInputInfo("");
            }
            if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
    }

    public void CallChangeOptionValue(InputKeyType inputKeyType)
    {
        var listData = optionList.ListData;
        if (listData == null)
        {
            return;
        }
        var optionResultInfo = (OptionInfo)listData.Data;
        var eventData = new OptionViewEvent(CommandType.ChangeOptionValue);
        var optionInfo = new OptionInfo();
        optionInfo.OptionCommand = optionResultInfo.OptionCommand;
        optionInfo.keyType = inputKeyType;
        eventData.template = optionInfo;
        _commandData(eventData);
    }
}

namespace Option
{
    public enum CommandType
    {
        None = 0,
        SelectCategory = 101,
        ChangeOptionValue = 2000,
    }
}
public class OptionViewEvent
{
    public Option.CommandType commandType;
    public object template;

    public OptionViewEvent(Option.CommandType type)
    {
        commandType = type;
    }
}
