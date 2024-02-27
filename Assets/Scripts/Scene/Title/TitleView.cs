using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Title;

public class TitleView : BaseView 
{
    [SerializeField] private TextMeshProUGUI versionText = null;
    private new System.Action<TitleViewEvent> _commandData = null;
    [SerializeField] private BaseList titleCommandList = null;
    [SerializeField] private Button tapTitle = null;
    public override void Initialize() 
    {
        base.Initialize();
        titleCommandList.Initialize();
        SideMenuButton.onClick.AddListener(() => {
            CallSideMenu();
        });
        new TitlePresenter(this);
        tapTitle.onClick.AddListener(() => OnClickTitle());
    }

    private void OnClickTitle()
    {
        var eventData = new TitleViewEvent(CommandType.SelectTitle);
        _commandData(eventData);
    }

    public void SetHelpWindow(){
    }

    public void SetEvent(System.Action<TitleViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetVersion(string text)
    {
        versionText.text = text;
    }

    public void SetTitleCommand(List<ListData> titleCommands){
        titleCommandList.SetData(titleCommands);
        titleCommandList.SetInputHandler(InputKeyType.Decide,() => CallTitleCommand());
        titleCommandList.SetInputHandler(InputKeyType.Option1,() => CommandOpenSideMenu());
        titleCommandList.SetSelectedHandler(() => UpdateHelpWindow());
        SetInputHandler(titleCommandList.GetComponent<IInputHandlerEvent>());
        titleCommandList.SetHelpWindow(HelpWindow);
    }

    public void RefreshCommandIndex(int selectIndex)
    {
        titleCommandList.Refresh(selectIndex);
        titleCommandList.Activate();
    }

    private void CallTitleCommand(){
        var listData = titleCommandList.ListData;
        if (listData != null && listData.Enable)
        {
            var eventData = new TitleViewEvent(CommandType.TitleCommand);
            var commandData = (SystemData.CommandData)listData.Data;
            eventData.template = commandData.Id;
            _commandData(eventData);
        }
    }

    private void CallSideMenu()
    {
        var eventData = new TitleViewEvent(CommandType.SelectSideMenu);
        _commandData(eventData);
    }
    
    private void UpdateHelpWindow()
    {
        var listData = titleCommandList.ListData;
        if (listData != null)
        {
            var commandData = (SystemData.CommandData)listData.Data;
            HelpWindow.SetHelpText(commandData.Help);
        }
    }
}

namespace Title
{
    public enum CommandType
    {
        None = 0,
        TitleCommand,
        Option,
        SelectTitle,
        SelectSideMenu,
    }
}

public class TitleViewEvent
{
    public Title.CommandType commandType;
    public object template;

    public TitleViewEvent(Title.CommandType type)
    {
        commandType = type;
    }
}