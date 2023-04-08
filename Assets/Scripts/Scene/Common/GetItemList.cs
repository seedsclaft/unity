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
    public TacticsCommandList TacticsCommandList {get {return tacticsCommandList;}}
    private System.Action<TacticsComandType> _confirmEvent = null;
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
        _data = skillInfoData;
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
        //UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        _confirmEvent = callEvent;
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
        tacticsCommandList.UpdateSelectIndex(0);
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<TacticsComandType> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            _confirmEvent((TacticsComandType)tacticsCommandList.Index);
        }
        if (keyType == InputKeyType.Cancel)
        {
            _confirmEvent(TacticsComandType.Train);
        }
    }
}
