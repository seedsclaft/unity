using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsEnemyList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private TacticsCommandList tacticsCommandList;
    public TacticsCommandList TacticsCommandList {get {return tacticsCommandList;}}
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<TroopInfo> _troopInfos = new List<TroopInfo>();

    public int selectIndex{
        get {return Index;}
    }
    public void Initialize(System.Action<int> callEvent,System.Action cancelEvent)
    {
        InitializeListView(cols);
        for (int i = 0; i < cols;i++)
        {
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (callEvent != null)
            {
                tacticsEnemy.SetCallHandler(callEvent);
                tacticsEnemy.SetSelectHandler((data) => UpdateSelectIndex(data));
            }
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent));
    }

    public void Refresh(List<TroopInfo> troopInfos)
    {
        _troopInfos = troopInfos;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (i < _troopInfos.Count)
            {
                tacticsEnemy.SetData(_troopInfos[i].BossEnemy,i);
                tacticsEnemy.SetGetItemList(_troopInfos[i].GetItemInfos);
            }
            ObjectList[i].SetActive(i < _troopInfos.Count);
        }
        UpdateSelectIndex(-1);
        Refresh();
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
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

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (callEvent != null)
            callEvent(Index);
        }
        if (keyType == InputKeyType.Cancel)
        {
            if (cancelEvent != null)
            cancelEvent();
        }
    }
}
