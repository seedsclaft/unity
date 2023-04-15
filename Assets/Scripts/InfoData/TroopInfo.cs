using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopInfo 
{
    private int _troopId = 0;
    public int TroopId { get {return _troopId;}}
    private List<BattlerInfo> _battlerInfos = new List<BattlerInfo>(); 
    public List<BattlerInfo> BattlerInfos {get {return _battlerInfos;}}
    public BattlerInfo BossEnemy {
        get {
            var boss = _battlerInfos.Find(a => a.BossFlag == true);
            if (boss != null) return _battlerInfos.Find(a => a.BossFlag == true);
            return _battlerInfos[_battlerInfos.Count-1];
        }
    }
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

    public void MakeEnemyData(TroopsData.TroopData troopData,int index,int gainLevel){
        EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == troopData.EnemyId);
        BattlerInfo battlerInfo = new BattlerInfo(enemyData,troopData.Lv + gainLevel,index,troopData.Line,troopData.BossFlag);
        AddEnemy(battlerInfo);
        
        List<GetItemData> getItemDatas = troopData.GetItemDatas;
        for (int i = 0;i < getItemDatas.Count;i++)
        {
            GetItemInfo getItemInfo = new GetItemInfo(getItemDatas[i]);
            if (getItemDatas[i].Type == GetItemType.Skill)
            {
                SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[i].Param1);
                getItemInfo.SetResultData(skillData.Name);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
            }
            if (getItemDatas[i].Type == GetItemType.Numinous)
            {
                getItemInfo.SetResultData("+" + getItemDatas[i].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
            }
            AddGetItemInfo(getItemInfo);
        }
    }

    public void RemoveAtEnemyIndex(int enemyIndex){
        var battler = _battlerInfos.Find(a => a.Index == enemyIndex);
        if (battler != null)
        {
            _battlerInfos.Remove(battler);
        }
    }
    
    public void AddGetItemInfo(GetItemInfo getItemInfo){
        _getItemInfos.Add(getItemInfo);
    }


}
