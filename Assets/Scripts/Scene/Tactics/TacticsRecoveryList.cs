using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsRecoveryList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<ActorInfo> _actorInfos = new List<ActorInfo>();
    [SerializeField] private TacticsCommandList tacticsCommandList;

    public int selectIndex{
        get {return Index;}
    }


    public void Initialize(List<ActorInfo> actorInfos,System.Action<int> callEvent,System.Action<int> plusEvent,System.Action<int> minusEvent)
    {
        InitializeListView(rows);
        _actorInfos = actorInfos;
        for (int i = 0; i < rows;i++)
        {
            var tacticsRecovery = ObjectList[i].GetComponent<TacticsRecovery>();
            if (i < _actorInfos.Count)
            {
                tacticsRecovery.SetData(_actorInfos[i],i);
            }
            tacticsRecovery.SetCallHandler(callEvent);
            tacticsRecovery.SetPlusHandler(plusEvent);
            tacticsRecovery.SetMinusHandler(minusEvent);
            tacticsRecovery.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(i < _actorInfos.Count);
        }
        UpdateSelectIndex(-1);
        Refresh();
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands,callEvent);
    }

    public void Refresh()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
}
