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

    [SerializeField] private List<Toggle> graphicToggles = null;
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new OptionPresenter(this);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
    
    public void InitializeVolume(float bgmVolume,bool bgmMute,float seVolume,bool seMute)
    {
        optionBgmVolume.Initialize(bgmVolume,bgmMute,(a) => CallChangeBGMVolume(a),(a) => CallChangeBGMMute(a));
        optionSeVolume.Initialize(seVolume,seMute,(a) => CallChangeSEVolume(a),(a) => CallChangeSEMute(a));
    }
    
    public void InitializeGraphic(int graphicIndex)
    {
        UpdateGraphicIndex(graphicIndex);
        for (int i = 0;i < graphicToggles.Count;i++)
        {
            int j = graphicToggles.Count-i;
            graphicToggles[i].onValueChanged.AddListener((a) => ChangeGraphicIndex(a,j));
        }
    }


    public void SetEvent(System.Action<OptionViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetBackEvent(System.Action backEvent)
    {
        CreateBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
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

    private void ChangeGraphicIndex(bool isChange,int toggleIndex)
    {
        if (isChange == false) return;
        var eventData = new OptionViewEvent(CommandType.ChangeGraphicIndex);
        eventData.templete = toggleIndex;
        _commandData(eventData);
    }


    private void UpdateGraphicIndex(int graphicIndex)
    {
        for (int i = 0;i < graphicToggles.Count;i++)
        {
            graphicToggles[i].isOn = (graphicToggles.Count-i) == graphicIndex;
        }
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
        ChangeGraphicIndex = 1021,
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
