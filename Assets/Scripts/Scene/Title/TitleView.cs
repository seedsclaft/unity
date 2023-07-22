using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

public class TitleView : BaseView
{
    [SerializeField] private Button creditButton = null;
    [SerializeField] private TextMeshProUGUI versionText = null;
    [SerializeField] private TitleCommandList commandList = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private GameObject helpRoot = null;
    [SerializeField] private GameObject helpPrefab = null;
    [SerializeField] private Button logoButton = null;
    [SerializeField] private Button optionButton = null;
    private HelpWindow _helpWindow = null;
    
    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new TitlePresenter(this);
        creditButton.onClick.AddListener(() => CallCredit());
        logoButton.onClick.AddListener(() => CallLogoClick());
        logoButton.gameObject.SetActive(true);
        optionButton.onClick.AddListener(() => OnClickOption());
    }

    private void CallCredit()
    {
        var eventData = new TitleViewEvent(CommandType.Credit);
        _commandData(eventData);
    }

    private void CallLogoClick()
    {
        var eventData = new TitleViewEvent(CommandType.LogoClick);
        _commandData(eventData);
    }

    private void OnClickOption()
    {
        var eventData = new TitleViewEvent(CommandType.Option);
        _commandData(eventData);
    }

    public void SetHelpWindow(){
        GameObject prefab = Instantiate(helpPrefab);
        prefab.transform.SetParent(helpRoot.transform, false);
        _helpWindow = prefab.GetComponent<HelpWindow>();
        commandList.SetHelpWindow(_helpWindow);
    }

    public void SetEvent(System.Action<TitleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetVersion(string text)
    {
        versionText.text = text;
    }
    public void SetTitleCommand(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallTitleCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    public void RefreshCommandIndex(int selectIndex)
    {
        commandList.Refresh(selectIndex);
    }

    private void CallTitleCommand(TitleComandType commandType){
        var eventData = new TitleViewEvent(CommandType.TitleCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }

    public void SetCommandDisable(int commandId)
    {
        commandList.SetDisable(DataSystem.TacticsCommand[commandId],true);
    }

    public void CommandLogoClick()
    {
        logoButton.gameObject.SetActive(false);
    }
}

namespace Title
{
    public enum CommandType
    {
        None = 0,
        TitleCommand,
        Credit,
        LogoClick,
        Option
    }
}

public class TitleViewEvent
{
    public Title.CommandType commandType;
    public object templete;

    public TitleViewEvent(Title.CommandType type)
    {
        commandType = type;
    }
}