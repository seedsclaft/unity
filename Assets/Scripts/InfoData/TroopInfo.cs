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
        var troopData = Master;
        if (troopData.StageTurn == 0 || troopData.StageTurn >= displayTurn)
        {
            foreach (var troopEnemies in troopData.TroopEnemies)
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
            if (getItemData.Type == GetItemType.Skill)
            {
                var skillData = DataSystem.FindSkill(getItemData.Param1);
                getItemInfo.SetResultData(skillData.Name);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
            }
            if (getItemData.Type == GetItemType.Numinous)
            {
                getItemInfo.SetResultData("+" + getItemData.Param1.ToString() + DataSystem.GetTextData(1000).Text);
            }
            if (getItemData.Type == GetItemType.Demigod)
            {
                getItemInfo.SetResultData(DataSystem.GetTextData(14042).Text + "+" + (getItemInfo.Param1).ToString());
            }
            if ((int)getItemData.Type >= (int)GetItemType.AttributeFire && (int)getItemData.Type <= (int)GetItemType.AttributeDark)
            {
                string text = DataSystem.GetReplaceText(14051,DataSystem.GetTextData(330 + (int)getItemData.Type - 11).Text);
                getItemInfo.SetResultData(text);
                getItemInfo.SetSkillElementId((int)AttributeType.Fire + (int)getItemData.Type - 11);
            }
            if (getItemData.Type == GetItemType.Ending)
            {
                getItemInfo.SetResultData(DataSystem.GetTextData(14060).Text);
            }
            if (getItemData.Type == GetItemType.StatusUp)
            {
                getItemInfo.SetResultData(DataSystem.GetReplaceText(14070,getItemData.Param1.ToString()));
            }
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
