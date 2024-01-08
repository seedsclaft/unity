using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsEnemyList : BaseList
{
    [SerializeField] private BaseList tacticsCommandList;
    public BaseList TacticsCommandList => tacticsCommandList;
    
    public bool IsSelectEnemy(){
        if (ObjectList.Count > Index)
        {
            var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
            if (tacticsEnemy.Selectable)
            {
                return true;
            }
        }
        return false;
    }

    public GetItemInfo GetItemInfo(){
        var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
        if (tacticsEnemy.GetItemList.Index != -1)
        {
            return tacticsEnemy.GetItemInfo();
        }
        return null;
    }

    public void SetInputCallHandler()
    {
        //SetInputCallHandler((a) => CallInputHandler(a));
        SetInputHandler(InputKeyType.Right,() => CallInputHandler(InputKeyType.Right));
        SetInputHandler(InputKeyType.Left,() => CallInputHandler(InputKeyType.Left));
        SetInputHandler(InputKeyType.Down,() => CallInputHandler(InputKeyType.Down));
        SetInputHandler(InputKeyType.Up,() => CallInputHandler(InputKeyType.Up));
    }

    private void CallInputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Right || keyType == InputKeyType.Left)
        {
            if (DataCount > 1)
            {
                var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                tacticsEnemy.UpdateItemIndex(-1);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
            tacticsEnemy.UpdateItemIndex(tacticsEnemy.GetItemIndex-1);
            if (tacticsEnemy.GetItemIndex == -1)
            {
                tacticsEnemy.SetSelectable(true);
            }
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
        if (keyType == InputKeyType.Down)
        {
            var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
            tacticsEnemy.UpdateItemIndex(tacticsEnemy.GetItemIndex+1);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
    }

    public new void SetData(List<ListData> troopInfos)
    {
        base.SetData(troopInfos);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            tacticsEnemy.SetCallHandler(() => CallSelectHandler(InputKeyType.Decide));
            tacticsEnemy.SetSelectHandler((System.Action<int>)((a) => {
                UpdateUnSelectAll();
                UpdateSelectIndex(a);
                tacticsEnemy.SetSelectable(true);
            }));
            tacticsEnemy.SetGetItemInfoCallHandler(() => 
            {
                CallListInputHandler(InputKeyType.Decide);
            });
            //tacticsEnemy.SetEnemyInfoCallHandler((a) => CallListInputHandler(InputKeyType.Option1));
            tacticsEnemy.SetGetItemInfoSelectHandler((a) => {
                for (int i = 0; i < ObjectList.Count;i++)
                {
                    var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
                    if (a != i)
                    {
                        tacticsEnemy.UpdateItemIndex(-1);
                    }
                    tacticsEnemy.SetSelectable(false);
                }
                UpdateSelectIndex(a);
            });
            tacticsEnemy.UpdateItemIndex(-1);
            tacticsEnemy.SetSelectable(i == 0);
        }
    }

    public void SetInfoHandler(System.Action<int> infoHandler)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            tacticsEnemy.SetEnemyInfoCallHandler((a) => infoHandler(a));
        }
    }

    public void InitializeConfirm(List<ListData> confirmCommands ,System.Action<ConfirmCommandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands.Count);
        tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => 
        {
            var data = (SystemData.CommandData)tacticsCommandList.ListData.Data;
            if (data.Key == "Yes")
            {
                callEvent(ConfirmCommandType.Yes);
            } else
            if (data.Key == "No")
            {
                callEvent(ConfirmCommandType.No);
            }
        });
        tacticsCommandList.SetData(confirmCommands);
    }

    private void UpdateUnSelectAll()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            tacticsEnemy.UpdateItemIndex(-1);
            tacticsEnemy.SetSelectable(false);
        }
    }
}
