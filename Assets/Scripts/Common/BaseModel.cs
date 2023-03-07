using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BaseModel
{
    public SavePlayInfo CurrentData{get {return GameSystem.CurrentData;}}
    public TempInfo CurrentTempData{get {return GameSystem.CurrentTempData;}}

    public PartyInfo PartyInfo{get {return CurrentData.Party;}}

    public int Currency{get {return PartyInfo.Currency;}}

    public int Turns{get {return CurrentData.CurrentStage.Turns - CurrentData.CurrentStage.CurrentTurn - 1;}}
    
    public List<ActorInfo> Actors()
    {
        return GameSystem.CurrentData.Actors;
    }
    public List<SystemData.MenuCommandData> ConfirmCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = "決定";
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = "中止";
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }
    
    public List<AttributeType> AttributeTypes()
    {
        List<AttributeType> attributeTypes = new List<AttributeType>();
        foreach(var attribute in Enum.GetValues(typeof(AttributeType)))
        {
            if ((int)attribute != 0)
            {
                attributeTypes.Add((AttributeType)attribute);
            }
        } 
        return attributeTypes;
    }
    
    public List<Sprite> ActorsImage(List<ActorInfo> actors){
        var sprites = new List<Sprite>();
        for (var i = 0;i < actors.Count;i++)
        {
            var actorData = DataSystem.Actors.Find(actor => actor.Id == actors[i].ActorId);
            var asset = Addressables.LoadAssetAsync<Sprite>(
                "Assets/Images/Actors/" + actorData.ImagePath + "/main.png"
            );
            asset.WaitForCompletion();
            sprites.Add(asset.Result);
            Addressables.Release(asset);
        }
        return sprites;
    }

    
    public List<BattlerInfo> TacticsEnemies()
    {
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        List<TroopsData.TroopData> tacticsEnemyDatas = GameSystem.CurrentData.CurrentStage.TacticsEnemies();
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == tacticsEnemyDatas[i].EnemyId);
            BattlerInfo battlerInfo = new BattlerInfo(enemyData,tacticsEnemyDatas[i].Lv,0,0);
            battlerInfos.Add(battlerInfo);
        }
        return battlerInfos;
    }

    public List<List<GetItemInfo>> TacticsGetItemInfos()
    {
        List<List<GetItemInfo>> getItemDataLists = new List<List<GetItemInfo>>();
        List<TroopsData.TroopData> tacticsEnemyDatas = GameSystem.CurrentData.CurrentStage.TacticsEnemies();
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
            List<GetItemData> getItemDatas = tacticsEnemyDatas[i].GetItemDatas;
            for (int j = 0;j < getItemDatas.Count;j++)
            {
                GetItemInfo getItemInfo = new GetItemInfo(getItemDatas[j]);
                if (getItemDatas[j].Type == GetItemType.Skill)
                {
                    SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[j].Param1);
                    getItemInfo.SetResultData(skillData.Name);
                    getItemInfo.SetSkillElementId((int)skillData.Attribute);
                }
                if (getItemDatas[j].Type == GetItemType.Numinous)
                {
                    getItemInfo.SetResultData("+" + getItemDatas[j].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
                }
                getItemInfos.Add(getItemInfo);
            }
            getItemDataLists.Add(getItemInfos);
        }
        return getItemDataLists;
    }
}
