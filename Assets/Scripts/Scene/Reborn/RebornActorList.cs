using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornActorList : ListWindow , IInputHandlerEvent
{
    private List<ActorInfo> _data = new List<ActorInfo>();
    private List<int> _disableIndexs = new List<int>();
    public void Initialize(List<ActorInfo> actorInfos,List<int> disableIndexs)
    {
        _data = actorInfos;
        _disableIndexs = disableIndexs;
        InitializeListView(actorInfos.Count);
        for (int i = actorInfos.Count-1; i >= 0;i--)
        {
            RebornActor skillAction = ObjectList[i].GetComponent<RebornActor>();
            skillAction.SetCallHandler(() => CallSelectHandler(InputKeyType.Decide));
            skillAction.SetSelectHandler((data) => 
                {
                    UpdateSelectIndex(data);
                    CallSelectHandler(InputKeyType.Down);
                });
            //ObjectList[i].SetActive(false);
        } 
        SetInputCallHandler((a) => CallSelectHandler(a));
    }

    public void Refresh()
    {
        var selectIndex = 0;
        for (int i = ObjectList.Count-1; i >= 0;i--)
        {
            if (i < _data.Count) 
            {
                RebornActor rebornActor = ObjectList[i].GetComponent<RebornActor>();
                rebornActor.SetData(_data[i],i);
                rebornActor.SetDisable(i,_disableIndexs.Contains(i));
                if (_disableIndexs.Contains(i))
                {
                    selectIndex++;
                }
        
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        //ResetScrollPosition();
        UpdateSelectIndex(selectIndex);
        UpdateAllItems();
    }
}
