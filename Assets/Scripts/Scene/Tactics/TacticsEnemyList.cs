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

    private int _getItemIndex = -1;
    public void Initialize(System.Action<int> callEvent,System.Action cancelEvent,System.Action<int> getItemEvent)
    {
        InitializeListView(cols);
        for (int i = 0; i < cols;i++)
        {
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (callEvent != null)
            {
                tacticsEnemy.SetCallHandler(callEvent);
                tacticsEnemy.SetGetItemCallHandler(getItemEvent);
                tacticsEnemy.SetSelectHandler((data) => UpdateSelectIndex(data));
            }
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,getItemEvent));
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

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent,System.Action<int> getItemEvent)
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
        if (getItemEvent != null)
        {
            if (keyType == InputKeyType.Option1)
            {
                if (_troopInfos[Index].GetItemInfos[_getItemIndex].IsSkill())
                {
                    getItemEvent(_troopInfos[Index].GetItemInfos[_getItemIndex].Param1);
                }
            }
            if (keyType == InputKeyType.Left || keyType == InputKeyType.Right)
            {            
                _getItemIndex = -1;
                SetAllUnselectGetItem();
                UpdateSelectIndex(Index);
            }
            if (keyType == InputKeyType.Up)
            {
                SetAllUnselect();
                _getItemIndex--;
                TacticsEnemy tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                if (_getItemIndex < 0)
                {
                    _getItemIndex = _troopInfos[Index].GetItemInfos.Count;
                } else{
                    if (_getItemIndex == -1)
                    {
                        UpdateSelectIndex(Index);
                    }
                }
                tacticsEnemy.SetSelectGetItem(_getItemIndex);
            }
            if (keyType == InputKeyType.Down)
            {
                SetAllUnselect();
                _getItemIndex++;
                TacticsEnemy tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                if (_troopInfos[Index].GetItemInfos.Count > _getItemIndex)
                {
                } else{
                    _getItemIndex = -1;
                    UpdateSelectIndex(Index);
                }
                tacticsEnemy.SetSelectGetItem(_getItemIndex);
            }
        }
    }

    private void SetAllUnselect()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            ListItem listItem = ObjectList[i].GetComponent<ListItem>();
            if (listItem == null) continue;
            listItem.SetUnSelect();
        }
    }

    private void SetAllUnselectGetItem()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (tacticsEnemy == null) continue;
            tacticsEnemy.SetSelectGetItem(_getItemIndex);
        }
    }
}
