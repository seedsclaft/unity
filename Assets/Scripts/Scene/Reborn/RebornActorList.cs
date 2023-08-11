using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornActorList : ListWindow , IInputHandlerEvent
{
    private List<ActorInfo> _data = new List<ActorInfo>();
    public void Initialize(List<ActorInfo> actorInfos,System.Action<int> callEvent,System.Action cancelEvent,System.Action updateEvent)
    {
        _data = actorInfos;
        InitializeListView(actorInfos.Count);
        for (int i = actorInfos.Count-1; i >= 0;i--)
        {
            RebornActor skillAction = ObjectList[i].GetComponent<RebornActor>();
            skillAction.SetCallHandler(callEvent);
            skillAction.SetSelectHandler((data) => 
                {
                    UpdateSelectIndex(data);
                    if (updateEvent != null)
                    {
                        updateEvent();
                    }
                });
            //ObjectList[i].SetActive(false);
        } 
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,updateEvent));
    }

    public void Refresh()
    {
        for (int i = ObjectList.Count-1; i >= 0;i--)
        {
            if (i < _data.Count) 
            {
                RebornActor rebornActor = ObjectList[i].GetComponent<RebornActor>();
                rebornActor.SetData(_data[i],i);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent,System.Action updateEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (callEvent != null && Index >= 0)
            {
                callEvent(Index);
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                UpdateScrollRect(keyType,4,_data.Count);
                if (updateEvent != null)
                {
                    updateEvent();
                }
            }
            if (keyType == InputKeyType.Up)
            {
                UpdateScrollRect(keyType,4,_data.Count);
                if (updateEvent != null)
                {
                    updateEvent();
                }
            }
        }
    } 

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<RebornActor>();
        skillAction.SetData(_data[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
