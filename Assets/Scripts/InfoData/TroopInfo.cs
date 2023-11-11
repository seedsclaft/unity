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

    public void MakeEnemyTroopDatas(int level)
    {
        foreach (var troopData in MasterAll)
        {
            EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == troopData.EnemyId);
            BattlerInfo battlerInfo = new BattlerInfo(enemyData,troopData.Lv + level,troopData.Id,troopData.Line,troopData.BossFlag);
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
        List<GetItemData> getItemDatas = Master.GetItemDatas;
        for (int i = 0;i < getItemDatas.Count;i++)
        {
            GetItemInfo getItemInfo = new GetItemInfo(getItemDatas[i]);
            if (getItemDatas[i].Type == GetItemType.Skill)
            {
                SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[i].Param1);
                getItemInfo.SetResultData(skillData.Name);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
            }
            if (getItemDatas[i].Type == GetItemType.Numinous)
            {
                getItemInfo.SetResultData("+" + getItemDatas[i].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
            }
            if (getItemDatas[i].Type == GetItemType.Demigod)
            {
                getItemInfo.SetResultData(DataSystem.System.GetTextData(14042).Text + "+" + (getItemInfo.Param1).ToString());
            }
            if ((int)getItemDatas[i].Type >= (int)GetItemType.AttributeFire && (int)getItemDatas[i].Type <= (int)GetItemType.AttributeDark)
            {
                string text = DataSystem.System.GetReplaceText(14051,DataSystem.System.GetTextData(330 + (int)getItemDatas[i].Type - 11).Text);
                getItemInfo.SetResultData(text);
                getItemInfo.SetSkillElementId((int)AttributeType.Fire + (int)getItemDatas[i].Type - 11);
            }
            if (getItemDatas[i].Type == GetItemType.Ending)
            {
                getItemInfo.SetResultData(DataSystem.System.GetTextData(14060).Text);
            }
            if (getItemDatas[i].Type == GetItemType.StatusUp)
            {
                getItemInfo.SetResultData(DataSystem.System.GetReplaceText(14070,getItemDatas[i].Param1.ToString()));
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
