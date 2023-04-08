using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyModel : BaseModel
{
    private List<ActorInfo> StrategyActors()
    {
        return StageMembers();
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
        List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
        foreach (GetItemInfo getItemInfo in CurrentTroopInfo().GetItemInfos)
        {
            if (getItemInfo.GetItemType == GetItemType.Skill)
            {
                int rand = UnityEngine.Random.Range (0,100);
                if ((int)(getItemInfo.Param2 * 0.01f) > rand)
                {
                    PartyInfo.AddAlchemy(getItemInfo.Param1);
                    getItemInfo.SetTitleData(DataSystem.System.GetTextData(14040).Text);
                    getItemInfos.Add(getItemInfo);
                }
            }
            if (getItemInfo.GetItemType == GetItemType.Numinous)
            {
                PartyInfo.ChangeCurrency(PartyInfo.Currency + getItemInfo.Param1);
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(14041).Text);
                getItemInfos.Add(getItemInfo);
            }
        }
        CurrentStage.AddClearTroopId(CurrentTroopInfo().TroopId);
        CurrentStage.GainClearCount(1);
        CurrentStage.ChangeSubordinate(15);

        return getItemInfos;
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
            CurrentStage.SetBattleIndex(enemyIndex);
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
        actorInfos.ForEach(a => a.ClearTacticsCommand());
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
    
    public void EndStrategy()
    {
        CurrentAlcana.UseAlcana(false);
        foreach (var actorInfo in StageMembers())
        {
            actorInfo.ChangeTacticsCostRate(1);
        }
        CurrentStage.ClearTacticsEnemies();
        CurrentStage.ChangeSubordinate(-5);
        CurrentAlcana.SetAlacanaState(null);
    }
}
