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
            SlotInfoComponent skillAction = ObjectList[i].GetComponent<SlotInfoComponent>();
            
            skillAction.SetSelectHandler((data) => 
            {
                UpdateSelectIndex(data);
            }); 
            skillAction.SetCallHandler(() => CallInputHandler(InputKeyType.Decide));
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
        _data = slotInfos;
        SetDataCount(slotInfos.Count);
    }
    
    public void SetData(Dictionary<int,SlotInfo> slotInfos)
    {
        _data = slotInfos;
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _data.Count) 
            {
                SlotInfoComponent skillAction = ObjectList[i].GetComponent<SlotInfoComponent>();
                skillAction.SetData(_data[i],i);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    private void CallSelectHandler(InputKeyType keyType)
    {
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                UpdateScrollRect(keyType,4,_data.Count);
            }
            if (keyType == InputKeyType.Up)
            {
                UpdateScrollRect(keyType,4,_data.Count);
            }
        }
    }
}
