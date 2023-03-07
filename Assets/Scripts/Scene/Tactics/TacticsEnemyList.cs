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
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<BattlerInfo> _enemyInfos = new List<BattlerInfo>();

    public int selectIndex{
        get {return Index;}
    }
    public void Initialize()
    {
        InitializeListView(cols);
    }

    public void Refresh(List<BattlerInfo> enemyDatas,List<List<GetItemInfo>> getItemInfoLists,System.Action<int> callEvent)
    {
        _enemyInfos = enemyDatas;
        for (int i = 0; i < cols;i++)
        {
            var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (i < _enemyInfos.Count)
            {
                tacticsEnemy.SetData(enemyDatas[i],i);
                tacticsEnemy.SetGetItemList(getItemInfoLists[i]);
            }
            if (callEvent != null)
            {
                tacticsEnemy.SetCallHandler(callEvent);
                tacticsEnemy.SetSelectHandler((data) => UpdateSelectIndex(data));
            }
            ObjectList[i].SetActive(i < _enemyInfos.Count);
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
