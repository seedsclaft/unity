using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BattleActorList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<BattlerInfo> _data = new List<BattlerInfo>();

    public int selectIndex{
        get {return Index;}
    }


    public void Initialize(System.Action<int> callEvent)
    {
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            var battleActor = ObjectList[i].GetComponent<BattleActor>();
            battleActor.SetCallHandler(callEvent);
            battleActor.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(false);
        }
    }

    public void Refresh(List<BattlerInfo> battlerInfos)
    {
        _data.Clear();
        for (var i = 0; i < battlerInfos.Count;i++)
        {
            _data.Add(battlerInfos[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _data.Count) 
            {
                var statusCommand = ObjectList[i].GetComponent<BattleActor>();
                statusCommand.SetData(battlerInfos[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
}
