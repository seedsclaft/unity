using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopInfo 
{
    public TroopData Master => DataSystem.Troops.Find(a => a.TroopId == _troopId);
    private int _troopId = 0;
    public int TroopId => _troopId;
    private List<BattlerInfo> _battlerInfos = new(); 
    public List<BattlerInfo> BattlerInfos => _battlerInfos;
    public BattlerInfo BossEnemy {
        get {
            var boss = _battlerInfos.Find(a => a.BossFlag == true);
            if (boss != null) return boss;
            if (_battlerInfos.Count > 0)
            {
                return _battlerInfos[_battlerInfos.Count-1];
            }
            return null;
        }
    }
    private List<GetItemInfo> _getItemInfos = new (); 
    public List<GetItemInfo> GetItemInfos => _getItemInfos;

    private bool _escapeEnable = false;
    public bool EscapeEnable => _escapeEnable;
    public TroopInfo(int troopId,bool escapeEnable)
    {
        _troopId = troopId;
        _battlerInfos.Clear();
        _getItemInfos.Clear();
        _escapeEnable = escapeEnable;
    }

    public void MakeEnemyTroopDates(int level,int displayTurn = 1)
    {
        if (Master.StageTurn == 0 || Master.StageTurn >= displayTurn)
        {
            foreach (var troopEnemies in Master.TroopEnemies)
            {
                var enemyData = DataSystem.Enemies.Find(a => a.Id == troopEnemies.EnemyId);
                var battlerInfo = new BattlerInfo(enemyData,troopEnemies.Lv + level,_battlerInfos.Count,troopEnemies.Line,troopEnemies.BossFlag);
                AddEnemy(battlerInfo);
            }
        }
        MakeGetItemInfos();
    }

    public void AddEnemy(BattlerInfo battlerInfo)
    {
        _battlerInfos.Add(battlerInfo);
    }

    public void MakeGetItemInfos()
    {
        var prizeSetId = Master.PrizeSetId;
        var prizeSetDates = DataSystem.PrizeSets.FindAll(a => a.Id == prizeSetId);
        for (int i = 0;i < prizeSetDates.Count;i++)
        {
            var getItemData = prizeSetDates[i].GetItem;
            var getItemInfo = new GetItemInfo(getItemData);
            AddGetItemInfo(getItemInfo);
        }
    }

    public void RemoveAtEnemyIndex(int enemyIndex)
    {
        var battler = _battlerInfos.Find(a => a.Index == enemyIndex);
        if (battler != null)
        {
            _battlerInfos.Remove(battler);
        }
    }
    
    public void AddGetItemInfo(GetItemInfo getItemInfo)
    {
        _getItemInfos.Add(getItemInfo);
    }
}
