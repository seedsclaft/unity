using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Option;

public class OptionView : BaseView
{
    [SerializeField] private ConfirmCommandList commandList = null;
    [SerializeField] private List<GameObject> optionObjs = null;
    [SerializeField] private TextMeshProUGUI subText = null;
    private new System.Action<OptionViewEvent> _commandData = null;
    [SerializeField] private OptionVolume optionBgmVolume = null;
    [SerializeField] private OptionVolume optionSeVolume = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new OptionPresenter(this);
    }
    
    public void InitializeVolume(float bgmVolume,bool bgmMute,float seVolume,bool seMute)
    {
        optionBgmVolume.Initialize(bgmVolume,bgmMute,(a) => CallChangeBGMVolume(a),(a) => CallChangeBGMMute(a));
        optionSeVolume.Initialize(seVolume,seMute,(a) => CallChangeSEVolume(a),(a) => CallChangeSEMute(a));
    }

    public void SetEvent(System.Action<OptionViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        CreateBackCommand(() => 
        {    
            if (backEvent != null) backEvent();
        });
        SetActiveBack(true);
    }

    public void SetOptionCommand(List<SystemData.MenuCommandData> menuCommands)
    {
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallConfirmCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallConfirmCommand(ConfirmComandType commandType)
    {
        var eventData = new OptionViewEvent(CommandType.SelectCategory);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    private void CallChangeBGMVolume(float bgmVolume)
    {
        var eventData = new OptionViewEvent(CommandType.ChangeBGMValue);
        eventData.templete = bgmVolume;
        _commandData(eventData);
    }

    private void CallChangeBGMMute(bool bgmMute)
    {
        var eventData = new OptionViewEvent(CommandType.ChangeBGMMute);
        eventData.templete = bgmMute;
        _commandData(eventData);
    }

    private void CallChangeSEVolume(float seVolume)
    {
        var eventData = new OptionViewEvent(CommandType.ChangeSEValue);
        eventData.templete = seVolume;
        _commandData(eventData);
    }

    private void CallChangeSEMute(bool seMute)
    {
        var eventData = new OptionViewEvent(CommandType.ChangeSEMute);
        eventData.templete = seMute;
        _commandData(eventData);
    }

    public void CommandSelectCategory(int optionIndex)
    {
        for (int i = 0;i < optionObjs.Count ; i++)
        {
            optionObjs[i].SetActive(i == optionIndex);
        }
    }

}

namespace Option
{
    public enum CommandType
    {
        None = 0,
        SelectCategory = 101,
        ChangeBGMValue = 1001,
        ChangeBGMMute = 1002,
        ChangeSEValue = 1011,
        ChangeSEMute = 1012,
    }
}
public class OptionViewEvent
{
    public Option.CommandType commandType;
    public object templete;

    public OptionViewEvent(Option.CommandType type)
    {
        commandType = type;
    }
}
