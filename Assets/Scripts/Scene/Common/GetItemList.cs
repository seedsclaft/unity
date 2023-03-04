using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class GetItemList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<GetItemInfo> _data = new List<GetItemInfo>();

    [SerializeField] private TacticsCommandList tacticsCommandList;
    public int selectIndex{
        get {return Index;}
    }


    public void Initialize()
    {
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            var getItem = ObjectList[i].GetComponent<GetItem>();

            getItem.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(false);
        }
    }

    public void Refresh(List<GetItemInfo> skillInfoData)
    {
        _data.Clear();
        for (var i = 0; i < skillInfoData.Count;i++)
        {
            _data.Add(skillInfoData[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _data.Count) 
            {
                var getItem = ObjectList[i].GetComponent<GetItem>();
                getItem.SetData(skillInfoData[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands,callEvent);
    }
}
