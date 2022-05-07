using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuActorList: ListWindow , IInputHandlerEvent
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<ActorsData.ActorData> _data = new List<ActorsData.ActorData>();

    public void Initialize(List<ActorsData.ActorData> actors,List<Sprite> images,System.Action<ActorsData.ActorData> callEvent)
    {
        InitializeListView(actors.Count);
        for (var i = 0; i < actors.Count;i++){
            _data.Add(actors[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var actor = ObjectList[i].GetComponent<MainMenuActor>();
            actor.SetData(actors[i],images[i]);
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
