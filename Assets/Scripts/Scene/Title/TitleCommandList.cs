using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TitleCommandList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SystemData.MenuCommandData> _data = new List<SystemData.MenuCommandData>();

    private HelpWindow _helpWindow = null;
    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<SystemData.MenuCommandData> menuCommands ,System.Action<TitleComandType> callEvent)
    {
        InitializeListView(menuCommands.Count);
        for (var i = 0; i < menuCommands.Count;i++){
            _data.Add(menuCommands[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            titleCommand.SetData(menuCommands[i],i);
            titleCommand.SetCallHandler(callEvent);
            titleCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void SetHelpWindow(HelpWindow helpWindow){
        _helpWindow = helpWindow;
    }

    public void InputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        int selectIndex = -1;
        if (keyType == InputKeyType.Down){
            selectIndex = Index + 1;
            if (selectIndex > _data.Count-1){
                selectIndex = 0;
            }
        } else
        if (keyType == InputKeyType.Up){
            selectIndex = Index - 1;
            if (selectIndex < 0){
                selectIndex = _data.Count-1;
            }
        } 
        UpdateSelectIndex(selectIndex);
        ResetInputFrame();
    }

    void UpdateSelectIndex(int index){
        SelectIndex(index);
        _helpWindow.SetHelpText(_data[index].Help);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var titleCommand = ObjectList[i].GetComponent<TitleCommand>();
            if (index == i){
                titleCommand.SetSelect();
            } else{
                titleCommand.SetUnSelect();
            }
        }
    }
}
