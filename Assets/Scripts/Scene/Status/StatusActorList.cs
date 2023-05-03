using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class StatusActorList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;

    public void Initialize(System.Action leftEvent,System.Action rightEvent,System.Action decideEvent,System.Action cancelEvent)
    {
        InitializeListView(1);
        SetInputHandler((a) => CallInputHandler(a,leftEvent,rightEvent,decideEvent,cancelEvent));
        SetInputFrame(30);
    }

    public void Refresh(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var StatusActor = ObjectList[i].GetComponent<ActorInfoComponent>();
            StatusActor.UpdateInfo(actorInfo,actorInfos);
        }
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action leftEvent, System.Action rightEvent,System.Action decideEvent,System.Action cancelEvent)
    {
        if (keyType == InputKeyType.SideLeft1)
        {
            leftEvent();
        }
        if (keyType == InputKeyType.SideRight1)
        {
            rightEvent();
        }
        if (keyType == InputKeyType.Start)
        {
            decideEvent();
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
    }
}
