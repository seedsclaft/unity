using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopInfo 
{
    private int _troopId = 0;
    private List< BattlerInfo > _battlerInfos = new List<BattlerInfo>(); 
    public TroopInfo(int troopId){
        _troopId = troopId;
        _battlerInfos.Clear();
    }

    public void AddEnemy(BattlerInfo battlerInfo){
        _battlerInfos.Add(battlerInfo);
    }
}
