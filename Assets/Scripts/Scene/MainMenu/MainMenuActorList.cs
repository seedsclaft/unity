using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuActorList: ListWindow , IInputHandlerEvent
{   
    private List<ActorInfo> _data = new List<ActorInfo>();

    public void Initialize(List<ActorInfo> actors,System.Action<ActorInfo> callEvent)
    {
        InitializeListView(actors.Count);
        _data = actors;
        for (int i = 0; i < _data.Count;i++)
        {
            var actor = ObjectList[i].GetComponent<MainMenuActor>();
            actor.SetData(actors[i]);
            actor.SetCallHandler(callEvent);
        }
        UpdateAllItems();
    }
    public new void InputHandler(InputKeyType keyType)
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

    public void UpdateActorStatus()
    {

    }
}
