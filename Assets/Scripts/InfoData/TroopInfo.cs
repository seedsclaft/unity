using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopInfo 
{
    private int _troopId = 0;
    private List<BattlerInfo> _battlerInfos = new List<BattlerInfo>(); 
    public List<BattlerInfo> BattlerInfos {get {return _battlerInfos;}}
    public BattlerInfo BossEnemy {get {return _battlerInfos[_battlerInfos.Count-1];}}
    private List<GetItemInfo> _getItemInfos = new List<GetItemInfo>(); 
    public List<GetItemInfo> GetItemInfos {get {return _getItemInfos;}} 
    public TroopInfo(int troopId){
        _troopId = troopId;
        _battlerInfos.Clear();
        _getItemInfos.Clear();
    }

    public void AddEnemy(BattlerInfo battlerInfo){
        _battlerInfos.Add(battlerInfo);
    }

    public void RemoveAtEnemyIndex(int enemyIndex){
        var battler = _battlerInfos.Find(a => a.Index == enemyIndex);
        _battlerInfos.Remove(battler);
    }
    
    public void AddGetItemInfo(GetItemInfo getItemInfo){
        _getItemInfos.Add(getItemInfo);
    }


}
