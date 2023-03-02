using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class StrategyModel : BaseModel
{

    private TacticsComandType _currentCommandType = TacticsComandType.None;
    public TacticsComandType CurrentCommandType{
        get { return _currentCommandType;}
    }

    private List<ActorInfo> StrategyActors()
    {
        return GameSystem.CurrentData.Actors;
    }

    public List<ActorInfo> TacticsActors()
    {
        List<ActorInfo> actorInfos = StrategyActors().FindAll(a => a.TacticsComandType != TacticsComandType.None && a.TacticsComandType != TacticsComandType.Battle);
        return actorInfos;
    }


    public List<ActorInfo> TacticsBattleActors()
    {
        List<ActorInfo> actorInfos = StrategyActors().FindAll(a => a.TacticsComandType == TacticsComandType.Battle);
        return actorInfos;
    }


    public List<GetItemInfo> SetResult()
    {
        // Party初期化
        PartyInfo.InitActors();
        List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
        List<ActorInfo> actorInfos = TacticsActors();
        for (int i = 0;i < actorInfos.Count;i++)
        {
            GetItemInfo getItemInfo = new GetItemInfo();
            if (actorInfos[i].TacticsComandType == TacticsComandType.Train)
            {
                actorInfos[i].LevelUp();
                getItemInfo.SetTitleData(actorInfos[i].Master.Name);
                getItemInfo.SetResultData("レベルアップ！");
                actorInfos[i].ClearTacticsCommand();
                PartyInfo.AddActor(actorInfos[i].ActorId);
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Alchemy)
            {
                actorInfos[i].LearnSkill(actorInfos[i].NextLearnSkillId);
                getItemInfo.SetTitleData(actorInfos[i].Master.Name);
                SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == actorInfos[i].NextLearnSkillId);
                getItemInfo.SetSkillData((int)skillData.Attribute,skillData.Name);
                getItemInfo.SetResultData("を習得！");
                actorInfos[i].ClearTacticsCommand();
                PartyInfo.AddActor(actorInfos[i].ActorId);
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Recovery)
            {
                getItemInfo.SetTitleData(actorInfos[i].Master.Name);
                int Hp = Mathf.Min(actorInfos[i].CurrentHp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxHp);
                int Mp = Mathf.Min(actorInfos[i].CurrentMp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxMp);
                actorInfos[i].ChangeHp(Hp);
                actorInfos[i].ChangeMp(Mp);
                getItemInfo.SetResultData("回復！");
                actorInfos[i].ClearTacticsCommand();
                PartyInfo.AddActor(actorInfos[i].ActorId);
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Resource)
            {
                getItemInfo.SetTitleData(actorInfos[i].Master.Name);
                PartyInfo.ChangeCurrency(Currency + TacticsUtility.ResourceCost(actorInfos[i]));
                getItemInfo.SetResultData("Numinouseが回復！");
                actorInfos[i].ClearTacticsCommand();
                PartyInfo.AddActor(actorInfos[i].ActorId);
            }
            getItemInfos.Add(getItemInfo);
        }
        return getItemInfos;
    }

    public List<GetItemInfo> SetBattleResult()
    {
        List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
        return getItemInfos;
    }

    public List<ActorInfo> CheckNonBattleActors()
    {
        int enemyIndex = -1;
        List<ActorInfo> actorInfos = TacticsBattleActors();
        for (int i = 0;i < actorInfos.Count;i++)
        {
            if (actorInfos[i].TacticsComandType == TacticsComandType.Battle)
            {
                if (actorInfos[i].InBattle == false && actorInfos[i].NextBattleEnemyIndex > enemyIndex)
                {
                    enemyIndex = actorInfos[i].NextBattleEnemyIndex;
                }
            }
        }
        if (enemyIndex > 0)
        {
            return actorInfos.FindAll(a => a.NextBattleEnemyIndex == enemyIndex);
        }
        return null;
    }

    public List<ActorInfo> CheckInBattleActors()
    {
        
        return TacticsBattleActors().FindAll(a => a.InBattle);
    }

    public List<BattlerInfo> EnemyInfo()
    {
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == 1);
        BattlerInfo battlerInfo = new BattlerInfo(enemyData,1,0,0);
        battlerInfos.Add(battlerInfo);
        return battlerInfos;
    }

    public void SetBattleData(List<ActorInfo> actorInfos)
    {
        PartyInfo.InitActors();
        actorInfos.ForEach(a => PartyInfo.AddActor(a.ActorId));
        actorInfos.ForEach(a => a.SetNextBattleEnemyIndex(0));
        actorInfos.ForEach(a => a.InBattle = true);
    }

    public void ClearBattleData(List<ActorInfo> actorInfos)
    {
        actorInfos.ForEach(a => a.InBattle = false);
    }

    public List<SystemData.MenuCommandData> ResultCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = "ステータス確認";
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = "次へ";
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
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
    
    public async Task<List<AudioClip>> BgmData(){
        BGMData bGMData = DataSystem.Data.GetBGM("TACTICS1");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + ".ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        return new List<AudioClip>(){
            result1,null
        };
    }
    


}
