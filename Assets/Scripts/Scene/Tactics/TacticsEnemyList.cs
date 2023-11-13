using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsEnemyList : BaseList
{
    [SerializeField] private BaseList tacticsCommandList;
    public BaseList TacticsCommandList => tacticsCommandList;
    
    public bool IsSelectEnemy(){
        var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
        if (tacticsEnemy.GetItemIndex == -1)
        {
            return true;
        }
        return false;
    }

    public GetItemInfo GetItemInfo(){
        var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
        if (tacticsEnemy.GetItemIndex != -1)
        {
            return tacticsEnemy.GetItemInfo();
        }
        return null;
    }

    public void SetInputCallHandler()
    {
        SetInputCallHandler((a) => CallInputHandler(a));
    }

    private void CallInputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Right || keyType == InputKeyType.Left)
        {
            if (DataCount > 1)
            {
                var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
                tacticsEnemy.SetItemIndex(-1);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
            tacticsEnemy.UpdateItemIndex(-1);
        }
        if (keyType == InputKeyType.Down)
        {
            var tacticsEnemy = ObjectList[Index].GetComponent<TacticsEnemy>();
            tacticsEnemy.UpdateItemIndex(1);
        }
    }

    public new void SetData(List<ListData> troopInfos)
    {
        base.SetData(troopInfos);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsEnemy = ObjectList[i].GetComponent<TacticsEnemy>();
            tacticsEnemy.SetCallHandler(() => CallSelectHandler(InputKeyType.Decide));
            tacticsEnemy.SetEnemyInfoCallHandler(() => CallListInputHandler(InputKeyType.Option1));
            tacticsEnemy.SetItemIndex(-1);
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
}
