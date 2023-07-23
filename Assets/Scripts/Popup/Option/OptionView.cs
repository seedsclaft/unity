using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Option;

public class OptionView : BaseView
{
    [SerializeField] private OptionCommandList commandList = null;
    [SerializeField] private List<GameObject> optionObjs = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    private HelpWindow _helpWindow = null;
    private new System.Action<OptionViewEvent> _commandData = null;
    private OptionVolume _optionBgmVolume = null;
    private OptionVolume _optionSeVolume = null;

    private Toggle[] _graphicToggles = null;
    private Toggle[] _eventSkipToggles = null;
    private Toggle[] _commandEndCheckToggles = null;
    private Toggle[] _battleWaitToggles = null;
    private Toggle[] _battleAnimationToggles = null;
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new OptionPresenter(this);
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
    }
    
    public void SetHelpWindow()
    {
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        //_helpWindow.SetHelpText(DataSystem.System.GetTextData(5000).Text);
        _helpWindow.SetInputInfo("OPTION");
    }

    public void InitializeVolume(float bgmVolume,bool bgmMute,float seVolume,bool seMute)
    {
        _optionBgmVolume = commandList.ObjectList[0].GetComponentInChildren<OptionVolume>();
        _optionBgmVolume.Initialize(bgmVolume,bgmMute,(a) => CallChangeBGMVolume(a),(a) => CallChangeBGMMute(a));
        _optionSeVolume = commandList.ObjectList[1].GetComponentInChildren<OptionVolume>();
        _optionSeVolume.Initialize(seVolume,seMute,(a) => CallChangeSEVolume(a),(a) => CallChangeSEMute(a));
    
    }
    
    public void InitializeGraphic(int graphicIndex)
    {
        _graphicToggles = commandList.ObjectList[2].GetComponentsInChildren<Toggle>();
        UpdateGraphicIndex(graphicIndex);
        for (int i = 0;i < _graphicToggles.Length;i++)
        {
            int j = _graphicToggles.Length-i;
            _graphicToggles[i].onValueChanged.AddListener((a) => ChangeGraphicIndex(a,j));
        }
    }

    public void InitializeEventSkip(int eventSkipIndex)
    {
        _eventSkipToggles = commandList.ObjectList[3].GetComponentsInChildren<Toggle>();
        UpdateEventSkipIndex(eventSkipIndex);
        for (int i = 0;i < _eventSkipToggles.Length;i++)
        {
            int j = _eventSkipToggles.Length-i;
            _eventSkipToggles[i].onValueChanged.AddListener((a) => ChangeEventSkipIndex(a,j));
        }
    }

    public void InitializeCommandEndCheck(int commandEndCheckIndex)
    {
        _commandEndCheckToggles = commandList.ObjectList[4].GetComponentsInChildren<Toggle>();
        UpdateCommandEndCheck(commandEndCheckIndex);
        for (int i = 0;i < _commandEndCheckToggles.Length;i++)
        {
            int j = _commandEndCheckToggles.Length-i;
            _commandEndCheckToggles[i].onValueChanged.AddListener((a) => ChangeCommandEndCheck(a,j));
        }
    }

    public void InitializeBattleWait(int battleWaitIndex)
    {
        _battleWaitToggles = commandList.ObjectList[5].GetComponentsInChildren<Toggle>();
        UpdateBattleWait(battleWaitIndex);
        for (int i = 0;i < _battleWaitToggles.Length;i++)
        {
            int j = _battleWaitToggles.Length-i;
            _battleWaitToggles[i].onValueChanged.AddListener((a) => ChangeBattleWait(a,j));
        }
    }

    public void InitializeBattleAnimation(int animationIndex)
    {
        _battleAnimationToggles = commandList.ObjectList[6].GetComponentsInChildren<Toggle>();
        UpdateBattleAnimation(animationIndex);
        for (int i = 0;i < _battleAnimationToggles.Length;i++)
        {
            int j = _battleAnimationToggles.Length-i;
            _battleAnimationToggles[i].onValueChanged.AddListener((a) => ChangeBattleAnimation(a,j));
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

    public void SetOptionCommand(List<SystemData.OptionCommand> menuCommands,System.Action optionEvent)
    {
        commandList.Initialize(menuCommands,(a,b) => CallChangeOptionValue(a,b),optionEvent);
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
        for (int i = 0;i < _graphicToggles.Length;i++)
        {
            _graphicToggles[i].isOn = (_graphicToggles.Length-i) == graphicIndex;
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
        for (int i = 0;i < _eventSkipToggles.Length;i++)
        {
            _eventSkipToggles[i].isOn = (_eventSkipToggles.Length-i) == eventSkipIndex;
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
        for (int i = 0;i < _commandEndCheckToggles.Length;i++)
        {
            _commandEndCheckToggles[i].isOn = (_commandEndCheckToggles.Length-i) == eventSkipIndex;
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
        for (int i = 0;i < _battleWaitToggles.Length;i++)
        {
            _battleWaitToggles[i].isOn = (_battleWaitToggles.Length-i) == battleWaitIndex;
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
        for (int i = 0;i < _battleAnimationToggles.Length;i++)
        {
            _battleAnimationToggles[i].isOn = (_battleAnimationToggles.Length-i) == battleAnimationIndex;
        }
    }

    public void CommandSelectCategory(int optionIndex)
    {
        for (int i = 0;i < optionObjs.Count ; i++)
        {
            optionObjs[i].SetActive(i == optionIndex);
        }
    }

    public void CallChangeOptionValue(InputKeyType inputKeyType,SystemData.OptionCommand optionCommand)
    {
        if (inputKeyType == InputKeyType.Cancel || inputKeyType == InputKeyType.Decide)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            BackEvent();
            return;
        }
        if (optionCommand.Key == "BGM_VOLUME")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                _optionBgmVolume.ChangeValue(Mathf.Min(1, Ryneus.SoundManager.Instance._bgmVolume + 0.05f));
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                _optionBgmVolume.ChangeValue(Mathf.Max(0,Ryneus.SoundManager.Instance._bgmVolume - 0.05f));
            } else
            if (inputKeyType == InputKeyType.Option1)
            {
                _optionBgmVolume.ChangeMute();
                CallChangeBGMMute(!Ryneus.SoundManager.Instance._bgmMute);
            }
        } else
        if (optionCommand.Key == "SE_VOLUME")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                _optionSeVolume.ChangeValue(Mathf.Min(1, Ryneus.SoundManager.Instance._seVolume + 0.05f));
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                _optionSeVolume.ChangeValue(Mathf.Max(0, Ryneus.SoundManager.Instance._seVolume - 0.05f));
            } else
            if (inputKeyType == InputKeyType.Option1)
            {
                _optionSeVolume.ChangeMute();
                CallChangeBGMMute(!Ryneus.SoundManager.Instance._seMute);
            }
        } else
        if (optionCommand.Key == "GRAPHIC_QUALITY")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                UpdateGraphicIndex(1);
                ChangeGraphicIndex(true,1);
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                UpdateGraphicIndex(2);
                ChangeGraphicIndex(true,2);
            };
        } else
        if (optionCommand.Key == "EVENT_SKIP")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                UpdateEventSkipIndex(1);
                ChangeEventSkipIndex(true,1);
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                UpdateEventSkipIndex(2);
                ChangeEventSkipIndex(true,2);
            };
        } else
        if (optionCommand.Key == "COMMAND_END_CHECK")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                UpdateCommandEndCheck(1);
                ChangeCommandEndCheck(true,1);
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                UpdateCommandEndCheck(2);
                ChangeCommandEndCheck(true,2);
            };
        } else
        if (optionCommand.Key == "BATTLE_WAIT")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                UpdateBattleWait(1);
                ChangeBattleWait(true,1);
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                UpdateBattleWait(2);
                ChangeBattleWait(true,2);
            };
        } else
        if (optionCommand.Key == "BATTLE_ANIMATION")
        {
            if (inputKeyType == InputKeyType.Right)
            {
                UpdateBattleAnimation(1);
                ChangeBattleAnimation(true,1);
            } else
            if (inputKeyType == InputKeyType.Left)
            {
                UpdateBattleAnimation(2);
                ChangeBattleAnimation(true,2);
            };
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
