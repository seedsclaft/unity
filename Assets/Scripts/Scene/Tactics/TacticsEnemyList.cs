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
    public TacticsCommandList TacticsCommandList => tacticsCommandList;
    [SerializeField] private int cols = 0;
    private List<TroopInfo> _troopInfos = new List<TroopInfo>();

    private int _getItemIndex = -1;
    public void Initialize(System.Action<int> callEvent,System.Action cancelEvent,System.Action<GetItemInfo> getItemEvent,System.Action<int> enemyInfoEvent)
    {
        InitializeListView(cols);
        for (int i = 0; i < cols;i++)
        {
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (callEvent != null)
            {
                tacticsEnemy.SetCallHandler(callEvent);
                tacticsEnemy.SetSelectHandler((data) => 
                {
                    SetUnselectGetItem();
                    UpdateSelectIndex(data);
                });
            }
            if (getItemEvent != null)
            {
                tacticsEnemy.SetGetItemCallHandler(getItemEvent);
                tacticsEnemy.SetGetItemSelectHandler((a,b) => UpdateGetItemIndex(a,b));
            }
            if (enemyInfoEvent != null)
            {
                tacticsEnemy.SetEnemyInfoCallHandler(enemyInfoEvent);
            }
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,getItemEvent,enemyInfoEvent));
        SetCancelEvent(() => cancelEvent());
    }

    public void Refresh(List<TroopInfo> troopInfos)
    {
        SetDataCount(troopInfos.Count);
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
        UpdateSelectIndex(0);
        Refresh();
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent,System.Action<int> enemyInfoEvent)
    {
        tacticsCommandList.Initialize(callEvent,null,null,null,enemyInfoEvent);
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

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent,System.Action<GetItemInfo> getItemEvent,System.Action<int> enemyInfoEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (callEvent != null && Index > -1 && _getItemIndex == -1)
            {
                callEvent(Index);
            }

            if (getItemEvent != null && _getItemIndex > -1)
            {
                if (_troopInfos[Index].GetItemInfos[_getItemIndex].IsSkill())
                {
                    getItemEvent(_troopInfos[Index].GetItemInfos[_getItemIndex]);
                }
                if (_troopInfos[Index].GetItemInfos[_getItemIndex].IsAttributeSkill())
                {
                    getItemEvent(_troopInfos[Index].GetItemInfos[_getItemIndex]);
                }
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            if (cancelEvent != null)
            cancelEvent();
        }

        if (keyType == InputKeyType.Option1)
        {
            if (_getItemIndex < 0)
            {
                if (enemyInfoEvent != null && Index >= 0)
                {
                    enemyInfoEvent(Index);
                }
            }
        }
        if (keyType == InputKeyType.Left || keyType == InputKeyType.Right)
        {            
            _getItemIndex = -1;
            UpdateSelectGetItem();
            UpdateSelectIndex(Index);
        }
        if (keyType == InputKeyType.Up)
        {
            SetAllUnselect();
            if (Index >= 0)
            {
                _getItemIndex--;
                if (_getItemIndex == -2)
                {
                    if (_troopInfos.Count <= Index)
                    {
                        return;
                    }
                    _getItemIndex = _troopInfos[Index].GetItemInfos.Count - 1;
                }
                TacticsEnemy tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                if (_getItemIndex < 0)
                {
                    UpdateSelectIndex(Index);
                }
                tacticsEnemy.SetSelectGetItem(_getItemIndex);
            }
        }
        if (keyType == InputKeyType.Down)
        {
            SetAllUnselect();
            if (Index >= 0)
            {
                _getItemIndex++;
                TacticsEnemy tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                
                if (_troopInfos.Count <= Index)
                {
                    return;
                }
                if (_troopInfos[Index].GetItemInfos.Count > _getItemIndex)
                {
                } else{
                    _getItemIndex = -1;
                    UpdateSelectIndex(Index);
                }
                tacticsEnemy.SetSelectGetItem(_getItemIndex);
            }
        }
        Debug.Log(_getItemIndex.ToString());
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

    private void UpdateSelectGetItem()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (tacticsEnemy == null) continue;
            tacticsEnemy.SetSelectGetItem(_getItemIndex);
        }
    }

    private void SetUnselectGetItem()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            TacticsEnemy tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            if (tacticsEnemy == null) continue;
            tacticsEnemy.SetSelectGetItem(-1);
        }
    }

    private void UpdateGetItemIndex(TacticsEnemy thisTacticsEnemy ,int index)
    {
        SetAllUnselect();
        _getItemIndex = index;
        
        SetUnselectGetItem();
        thisTacticsEnemy.SetSelectGetItem(_getItemIndex);
    }
}
