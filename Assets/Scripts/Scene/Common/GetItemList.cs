using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GetItemList : ListWindow , IInputHandlerEvent
{
    private List<GetItemInfo> _data = new List<GetItemInfo>();

    [SerializeField] private BaseList tacticsCommandList;
    public BaseList TacticsCommandList {get {return tacticsCommandList;}}
    private System.Action<TacticsComandType> _confirmEvent = null;
    

    public void Initialize()
    {
    }

    public void Refresh(List<GetItemInfo> getItemData)
    {
        InitializeListView(getItemData.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var getItem = ObjectList[i].GetComponent<GetItem>();
            getItem.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(false);
        }

        _data = getItemData;
        SetDataCount(_data.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _data.Count) 
            {
                var getItem = ObjectList[i].GetComponent<GetItem>();
                getItem.SetData(getItemData[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
        //ResetScrollPosition();
        //UpdateSelectIndex(0);
        ResetScrollRect();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    public void InitializeConfirm(List<ListData> confirmCommands ,System.Action<ConfirmComandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands.Count);
        tacticsCommandList.SetData(confirmCommands);
        tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => 
        {
            var data = (SystemData.CommandData)tacticsCommandList.ListData.Data;
            if (data.Key == "Yes")
            {
                callEvent(ConfirmComandType.Yes);
            } else
            if (data.Key == "No")
            {
                callEvent(ConfirmComandType.No);
            }
        });
        tacticsCommandList.UpdateSelectIndex(0);
    }
    
    private new void CallSelectHandler(InputKeyType keyType)
    {
        if (_data != null && _data.Count < 6) return;
        var margin = 1.0f / (_data.Count - 5);
        if (keyType == InputKeyType.Down)
        {
            var value = ScrollRect.normalizedPosition.y - margin;
            ScrollRect.normalizedPosition = new Vector2(0,value);
            if (ScrollRect.normalizedPosition.y < 0)
            {
                ScrollRect.normalizedPosition = new Vector2(0,0);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var value = ScrollRect.normalizedPosition.y + margin;
            ScrollRect.normalizedPosition = new Vector2(0,value);
            if (ScrollRect.normalizedPosition.y > 1)
            {
                ScrollRect.normalizedPosition = new Vector2(0,1);
            }
        }
    }
}
