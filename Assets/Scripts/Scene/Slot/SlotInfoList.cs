using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotInfoList : ListWindow , IInputHandlerEvent
{
    private Dictionary<int,SlotInfo> _data = new ();

    public void Initialize(Dictionary<int,SlotInfo> slotInfos,System.Action callEvent)
    {
        InitializeListView(slotInfos.Count);
        // スクロールするものはObjectList.CountでSetSelectHandlerを登録する
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var skillAction = ObjectList[i].GetComponent<SlotInfoComponent>();
            
            skillAction.SetSelectHandler((data) => 
            {
                UpdateSelectIndex(data);
            }); 
            skillAction.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            skillAction.SetCallInfoHandler(() => CallListInputHandler(InputKeyType.Option1));
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
        _data = slotInfos;
        SetDataCount(slotInfos.Count);
    }
    
    public void SetData(Dictionary<int,SlotInfo> slotInfos)
    {
        _data = slotInfos;
    }

    public void Refresh(int selectIndex = 0)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _data.Count) 
            {
                var skillAction = ObjectList[i].GetComponent<SlotInfoComponent>();
                skillAction.SetData(_data[i],i);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        UpdateSelectIndex(selectIndex);
        //ResetScrollPosition();
        UpdateAllItems();
    }
}
