using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopInfo 
{
    public TroopData Master => DataSystem.Troops.Find(a => a.TroopId == _troopId);
    public List<TroopData> MasterAll => DataSystem.Troops.FindAll(a => a.TroopId == _troopId);
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

    public void MakeEnemyTroopDates(int level)
    {
        foreach (var troopData in MasterAll)
        {
            var enemyData = DataSystem.Enemies.Find(a => a.Id == troopData.EnemyId);
            var battlerInfo = new BattlerInfo(enemyData,troopData.Lv + level,troopData.Id - 1,troopData.Line,troopData.BossFlag);
            AddEnemy(battlerInfo);
        }
        MakeGetItemInfos();
    }

    public void AddEnemy(BattlerInfo battlerInfo)
    {
        _battlerInfos.Add(battlerInfo);
    }

    public void MakeGetItemInfos()
    {
        var getItemDates = new List<GetItemData>();
        foreach (var master in MasterAll)
        {
            foreach (var getItemData in master.GetItemDates)
            {
                getItemDates.Add(getItemData);
            }
        }
        for (int i = 0;i < getItemDates.Count;i++)
        {
            var getItemInfo = new GetItemInfo(getItemDates[i]);
            if (getItemDates[i].Type == GetItemType.Skill)
            {
                var skillData = DataSystem.Skills.Find(a => a.Id == getItemDates[i].Param1);
                getItemInfo.SetResultData(skillData.Name);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
            }
            if (getItemDates[i].Type == GetItemType.Numinous)
            {
                getItemInfo.SetResultData("+" + getItemDates[i].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
            }
            if (getItemDates[i].Type == GetItemType.Demigod)
            {
                getItemInfo.SetResultData(DataSystem.System.GetTextData(14042).Text + "+" + (getItemInfo.Param1).ToString());
            }
            if ((int)getItemDates[i].Type >= (int)GetItemType.AttributeFire && (int)getItemDates[i].Type <= (int)GetItemType.AttributeDark)
            {
                string text = DataSystem.System.GetReplaceText(14051,DataSystem.System.GetTextData(330 + (int)getItemDates[i].Type - 11).Text);
                getItemInfo.SetResultData(text);
                getItemInfo.SetSkillElementId((int)AttributeType.Fire + (int)getItemDates[i].Type - 11);
            }
            if (getItemDates[i].Type == GetItemType.Ending)
            {
                getItemInfo.SetResultData(DataSystem.System.GetTextData(14060).Text);
            }
            if (getItemDates[i].Type == GetItemType.StatusUp)
            {
                getItemInfo.SetResultData(DataSystem.System.GetReplaceText(14070,getItemDates[i].Param1.ToString()));
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
