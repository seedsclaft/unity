using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<MenuComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        for (var i = 0; i < menuCommands.Count;i++){
            _data.Add(menuCommands[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var mainmenu = ObjectList[i].GetComponent<MainMenuCommand>();
            mainmenu.SetData(menuCommands[i]);
            mainmenu.SetCallHandler(callEvent);
        }
        UpdateAllItems();
    }


    public void InputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        if (keyType == InputKeyType.Down){
            ObjectList[Index].GetComponent<ListItem>().SetUnSelect();
            SelectIndex(Index+1);
            ObjectList[Index].GetComponent<ListItem>().SetSelect();
        } else
        if (keyType == InputKeyType.Up){
            ObjectList[Index].GetComponent<ListItem>().SetUnSelect();
            SelectIndex(Index-1);
            ObjectList[Index].GetComponent<ListItem>().SetSelect();
        } 
        ResetInputFrame();
    }
}
