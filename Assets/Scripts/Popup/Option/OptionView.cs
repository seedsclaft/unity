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
    private new System.Action<OptionViewEvent> _commandData = null;
    [SerializeField] private OptionVolume optionBgmVolume = null;
    [SerializeField] private OptionVolume optionSeVolume = null;

    [SerializeField] private List<Toggle> graphicToggles = null;
    [SerializeField] private List<Toggle> eventSkipToggles = null;
    [SerializeField] private List<Toggle> commandEndCheckToggles = null;
    [SerializeField] private List<Toggle> battleWaitToggles = null;
    [SerializeField] private List<Toggle> battleAnimationToggles = null;
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

    public void InitializeEventSkip(int eventSkipIndex)
    {
        UpdateEventSkipIndex(eventSkipIndex);
        for (int i = 0;i < eventSkipToggles.Count;i++)
        {
            int j = eventSkipToggles.Count-i;
            eventSkipToggles[i].onValueChanged.AddListener((a) => ChangeEventSkipIndex(a,j));
        }
    }

    public void InitializeCommandEndCheck(int commandEndCheckIndex)
    {
        UpdateCommandEndCheck(commandEndCheckIndex);
        for (int i = 0;i < commandEndCheckToggles.Count;i++)
        {
            int j = commandEndCheckToggles.Count-i;
            commandEndCheckToggles[i].onValueChanged.AddListener((a) => ChangeCommandEndCheck(a,j));
        }
    }

    public void InitializeBattleWait(int battleWaitIndex)
    {
        UpdateBattleWait(battleWaitIndex);
        for (int i = 0;i < battleWaitToggles.Count;i++)
        {
            int j = battleWaitToggles.Count-i;
            battleWaitToggles[i].onValueChanged.AddListener((a) => ChangeBattleWait(a,j));
        }
    }

    public void InitializeBattleAnimation(int animationIndex)
    {
        UpdateBattleAnimation(animationIndex);
        for (int i = 0;i < battleAnimationToggles.Count;i++)
        {
            int j = battleAnimationToggles.Count-i;
            battleAnimationToggles[i].onValueChanged.AddListener((a) => ChangeBattleAnimation(a,j));
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

    private void ChangeEventSkipIndex(bool isChange,int toggleIndex)
    {
        if (isChange == false) return;
        var eventData = new OptionViewEvent(CommandType.ChangeEventSkipIndex);
        eventData.templete = toggleIndex;
        _commandData(eventData);
    }

    private void UpdateEventSkipIndex(int eventSkipIndex)
    {
        for (int i = 0;i < eventSkipToggles.Count;i++)
        {
            eventSkipToggles[i].isOn = (eventSkipToggles.Count-i) == eventSkipIndex;
        }
    }

    private void ChangeCommandEndCheck(bool isChange,int toggleIndex)
    {
        if (isChange == false) return;
        var eventData = new OptionViewEvent(CommandType.ChangeCommandEndCheck);
        eventData.templete = toggleIndex;
        _commandData(eventData);
    }

    private void UpdateCommandEndCheck(int eventSkipIndex)
    {
        for (int i = 0;i < commandEndCheckToggles.Count;i++)
        {
            commandEndCheckToggles[i].isOn = (commandEndCheckToggles.Count-i) == eventSkipIndex;
        }
    }

    private void ChangeBattleWait(bool isChange,int toggleIndex)
    {
        if (isChange == false) return;
        var eventData = new OptionViewEvent(CommandType.ChangeBattleWait);
        eventData.templete = toggleIndex;
        _commandData(eventData);
    }

    private void UpdateBattleWait(int battleWaitIndex)
    {
        for (int i = 0;i < battleWaitToggles.Count;i++)
        {
            battleWaitToggles[i].isOn = (battleWaitToggles.Count-i) == battleWaitIndex;
        }
    }

    private void ChangeBattleAnimation(bool isChange,int toggleIndex)
    {
        if (isChange == false) return;
        var eventData = new OptionViewEvent(CommandType.ChangeBattleAnimation);
        eventData.templete = toggleIndex;
        _commandData(eventData);
    }

    private void UpdateBattleAnimation(int battleAnimationIndex)
    {
        for (int i = 0;i < battleAnimationToggles.Count;i++)
        {
            battleAnimationToggles[i].isOn = (battleAnimationToggles.Count-i) == battleAnimationIndex;
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
        ChangeEventSkipIndex = 1031,
        ChangeCommandEndCheck = 1041,
        ChangeBattleWait = 1051,
        ChangeBattleAnimation = 1061,
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
