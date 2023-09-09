using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsEnemyList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private TacticsCommandList tacticsCommandList;
    public TacticsCommandList TacticsCommandList => tacticsCommandList;
    [SerializeField] private int cols = 0;
    private List<TroopInfo> _troopInfos = new List<TroopInfo>();

    private bool _disableLeftRight = false;
    public void SetDisableLeftRight() { _disableLeftRight = true;}
    public int EnemyIndex
    {
        get {
            if (_getItemIndex < 0)
            {
                return Index;
            }
            return -1;
        }
    }

    public GetItemInfo GetItemInfo
    {
        get {
            if (_getItemIndex > -1)
            {
                var data = _troopInfos[Index].GetItemInfos[_getItemIndex];
                if (data.IsSkill() || data.IsAttributeSkill())
                {
                    return data;
                }
            }
            return null;
        }
    }

    private int _getItemIndex = -1;
    public void Initialize(System.Action<int> callEvent)
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
                    _getItemIndex = -1;
                });
            }
            tacticsEnemy.SetGetItemCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            tacticsEnemy.SetGetItemSelectHandler((a,b) => UpdateGetItemIndex(a,b));

            tacticsEnemy.SetEnemyInfoCallHandler(() => CallListInputHandler(InputKeyType.Option1));
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent));
        //SetCancelEvent(() => cancelEvent());
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
        _getItemIndex = -1;
        SetUnselectGetItem();
    }

    public void InitializeConfirm(List<SystemData.CommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
    }


    public void Refresh()
    {
        UpdateAllItems();
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (callEvent != null && Index > -1 && _getItemIndex == -1)
            {
                callEvent(Index);
            }
        }
        if (_disableLeftRight)
        {
            UpdateSelectIndex(-1);
            return;
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
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
