using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainMenu;

public class MainMenuView : BaseView
{
    [SerializeField] private Image backGround = null; 
    [SerializeField] private MainMenuCommandList commandList = null;
    [SerializeField] private MainMenuActorList actorList = null;


    protected void Awake(){
        InitializeInput();
        Initialize();
    }

    void Initialize(){
        new MainMenuPresenter(this);
    }

    public void SetImage(string str){
    }

    public new void SetEvent(System.Action<ViewEvent> commandData)
    {
        _commandData = commandData;
    }

    public void SetCommandData(List<SystemData.MenuCommandData> menuCommands){
        commandList.Initialize(menuCommands,(menuCommandInfo) => CallMainMenuCommand(menuCommandInfo));
        SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
    }

    private void CallMainMenuCommand(MenuComandType commandType){
        var eventData = new ViewEvent(Scene.MainMenu, CommandType.MainMenuCommand);
        eventData.templete = commandType;
        _commandData(eventData);
    }
    
    public void SetActorsData(List<ActorInfo> actors){
        actorList.Initialize(actors,(actorInfo) => CallMainMenuActor(actorInfo));
        SetInputHandler(actorList.GetComponent<IInputHandlerEvent>());
    }
    
    private void CallMainMenuActor(ActorInfo actor){
        var eventData = new ViewEvent(Scene.MainMenu, CommandType.ActorSelect);
        eventData.templete = actor;
        _commandData(eventData);
    }

    public void CommandSkill()
    {
        commandList.Deactivate();
        actorList.Activate();
    }

    public void UpdateActorStatus()
    {
        actorList.UpdateAllItems();
    }

}

namespace MainMenu
{
    public enum CommandType
    {
        None = 0,
        MainMenuCommand,
        ActorSelect
    }
}
