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
        return Actors();
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
            GetItemInfo getItemInfo = new GetItemInfo(null);
            if (actorInfos[i].TacticsComandType == TacticsComandType.Train)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",actorInfos[i].Master.Name));
                actorInfos[i].LevelUp();
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3001).Text.Replace("\\d",actorInfos[i].Level.ToString()));
                actorInfos[i].ClearTacticsCommand();
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Alchemy)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",actorInfos[i].Master.Name));
                actorInfos[i].LearnSkill(actorInfos[i].NextLearnSkillId);
                SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == actorInfos[i].NextLearnSkillId);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
                getItemInfo.SetResultData(skillData.Name + DataSystem.System.GetTextData(3002).Text);
                actorInfos[i].ClearTacticsCommand();
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Recovery)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3010).Text.Replace("\\d",actorInfos[i].Master.Name));
                int Hp = Mathf.Min(actorInfos[i].CurrentHp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxHp);
                int Mp = Mathf.Min(actorInfos[i].CurrentMp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxMp);
                actorInfos[i].ChangeHp(Hp);
                actorInfos[i].ChangeMp(Mp);
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3011).Text);
                actorInfos[i].ClearTacticsCommand();
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Resource)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3020).Text.Replace("\\d",actorInfos[i].Master.Name));
                PartyInfo.ChangeCurrency(Currency + TacticsUtility.ResourceCost(actorInfos[i]));
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3021).Text.Replace("\\d",TacticsUtility.ResourceCost(actorInfos[i]).ToString()));
                actorInfos[i].ClearTacticsCommand();
            }
            PartyInfo.AddActor(actorInfos[i].ActorId);
            getItemInfos.Add(getItemInfo);
        }
        return getItemInfos;
    }

    public List<GetItemInfo> SetBattleResult()
    {
        List<GetItemInfo> strategyGetItemInfos = new List<GetItemInfo>();
        int enemyIndex = BattleEnemyIndex(true);
        foreach (GetItemInfo getItemInfo in TacticsGetItemInfos()[enemyIndex])
        {
            if (getItemInfo.GetItemType == GetItemType.Skill)
            {
                PartyInfo.AddAlchemy(getItemInfo.Param1);
                getItemInfo.SetTitleData("新たな魔法技術を獲得！");
            }
            if (getItemInfo.GetItemType == GetItemType.Numinous)
            {
                PartyInfo.ChangeCurrency(PartyInfo.Currency + getItemInfo.Param1);
                getItemInfo.SetTitleData("Numinousを獲得！");
            }
        }


        return TacticsGetItemInfos()[enemyIndex];
    }

    public int BattleEnemyIndex(bool inBattle)
    {
        int enemyIndex = -1;
        List<ActorInfo> actorInfos = TacticsBattleActors();
        for (int i = 0;i < actorInfos.Count;i++)
        {
            if (enemyIndex == -1)
            {
                if (actorInfos[i].TacticsComandType == TacticsComandType.Battle)
                {
                    if (actorInfos[i].InBattle == inBattle && actorInfos[i].NextBattleEnemyIndex > enemyIndex)
                    {
                        enemyIndex = actorInfos[i].NextBattleEnemyIndex;
                    }
                }
            }
        }
        return enemyIndex;
    }

    public List<ActorInfo> CheckNextBattleActors()
    {
        int enemyIndex = BattleEnemyIndex(false);
        List<ActorInfo> actorInfos = TacticsBattleActors();
        if (enemyIndex >= 0)
        {
            return actorInfos.FindAll(a => a.NextBattleEnemyIndex == enemyIndex);
        }
        return null;
    }

    public List<ActorInfo> CheckInBattleActors()
    {
        return TacticsBattleActors().FindAll(a => a.InBattle);
    }

    public void SetBattleMembers(List<ActorInfo> actorInfos)
    {
        PartyInfo.InitActors();
        actorInfos.ForEach(a => PartyInfo.AddActor(a.ActorId));
        actorInfos.ForEach(a => a.InBattle = true);
    }

    public void ClearBattleData(List<ActorInfo> actorInfos)
    {
        actorInfos.ForEach(a => a.SetNextBattleEnemyIndex(-1,0));
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
    
    public async Task<List<AudioClip>> BgmData(){
        BGMData bGMData = DataSystem.Data.GetBGM("TACTICS1");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + ".ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        return new List<AudioClip>(){
            result1,null
        };
    }
    
    public void EndStrategy()
    {
        GameSystem.CurrentData.CurrentStage.ClearTacticsEnemies();
    }

}
