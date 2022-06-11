using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOrder : ListWindow 
{
    public void Initialize(List<BattlerInfo> battlers)
    {
        InitializeListView(battlers.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var battler = ObjectList[i].GetComponent<BattleOrderItem>();
            battler.SetData(battlers[i]);
        }
        UpdateAllItems();
    }

    public void UpdateBattleMambers(List<BattlerInfo> battlers)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var battler = ObjectList[i].GetComponent<BattleOrderItem>();
            battler.SetData(battlers[i]);
        }
        UpdateAllItems();
    }
}
