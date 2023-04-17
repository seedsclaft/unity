using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyModel : BaseModel
{
    private bool _needUseSpCommand = false;
    public bool NeedUseSpCommand { get {return _needUseSpCommand;}}
    public void SetNeedUseSpCommand(bool isNeed)
    {
        _needUseSpCommand = isNeed;
    }

    public bool CheckUseSp()
    {
        ActorInfo actorInfo = StrategyActors()[0];
        return (actorInfo.Sp < 10);
    }

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
                int levelBonus = PartyInfo.GetTrainLevelBonusValue();
                actorInfos[i].LevelUp(levelBonus);
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",actorInfos[i].Master.Name));
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3001).Text.Replace("\\d",actorInfos[i].Level.ToString()));
                actorInfos[i].ClearTacticsCommand();
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Train))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.SetTitleData("");
                    partyGetItemInfo.SetResultData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(1).Text));
                    PartyInfo.AddCommandRank(TacticsComandType.Train);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Alchemy)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",actorInfos[i].Master.Name));
                //actorInfos[i].LearnSkill(actorInfos[i].NextLearnSkillId);
                AttributeType attributeType = actorInfos[i].NextLearnAttribute;
                getItemInfo.SetSkillElementId((int)attributeType);
                //int hintLv = PartyInfo.SkillHintLevel(skillData.Id);
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3002).Text);
                //PartyInfo.PlusSkillHintLv(skillData.Id);
                actorInfos[i].LearnSkillAttribute((int)attributeType + 2000,actorInfos[i].NextLearnCost,attributeType);
                actorInfos[i].ClearTacticsCommand();
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Alchemy))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.SetTitleData("");
                    partyGetItemInfo.SetResultData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(2).Text));
                    PartyInfo.AddCommandRank(TacticsComandType.Alchemy);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Recovery)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3010).Text.Replace("\\d",actorInfos[i].Master.Name));
                int changeValue = 10 + PartyInfo.GetRecoveryBonusValue();
                int Hp = Mathf.Min(actorInfos[i].CurrentHp + actorInfos[i].TacticsCost * changeValue,actorInfos[i].MaxHp);
                int Mp = Mathf.Min(actorInfos[i].CurrentMp + actorInfos[i].TacticsCost * changeValue,actorInfos[i].MaxMp);
                actorInfos[i].ChangeHp(Hp);
                actorInfos[i].ChangeMp(Mp);
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3011).Text);
                actorInfos[i].ClearTacticsCommand();
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Recovery))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.SetTitleData("");
                    partyGetItemInfo.SetResultData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(3).Text));
                    PartyInfo.AddCommandRank(TacticsComandType.Recovery);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Resource)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3020).Text.Replace("\\d",actorInfos[i].Master.Name));
                PartyInfo.ChangeCurrency(Currency + TacticsUtility.ResourceCost(actorInfos[i]));
                getItemInfo.SetResultData(DataSystem.System.GetTextData(3021).Text.Replace("\\d",TacticsUtility.ResourceCost(actorInfos[i]).ToString()));
                actorInfos[i].ClearTacticsCommand();
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Resource))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.SetTitleData("");
                    partyGetItemInfo.SetResultData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(5).Text));
                    PartyInfo.AddCommandRank(TacticsComandType.Resource);
                    getItemInfos.Add(partyGetItemInfo);
                }
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
                int rand = UnityEngine.Random.Range(0,100);
                if (getItemInfo.Param2 >= rand)
                {
                    PartyInfo.AddAlchemy(getItemInfo.Param1);
                    getItemInfo.SetTitleData(DataSystem.System.GetTextData(14040).Text);
                    getItemInfos.Add(getItemInfo);
                }
            }
            if (getItemInfo.GetItemType == GetItemType.Numinous)
            {
                int bonus = PartyInfo.GetBattleBonusValue();
                PartyInfo.ChangeCurrency(PartyInfo.Currency + getItemInfo.Param1 + bonus);
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(14041).Text);
                getItemInfo.SetResultData("+" + (getItemInfo.Param1+bonus).ToString() + DataSystem.System.GetTextData(1000).Text);
                getItemInfos.Add(getItemInfo);
            }
        }
        CurrentStage.AddClearTroopId(CurrentTroopInfo().TroopId);
        CurrentStage.GainClearCount(1);
        CurrentStage.ChangeSubordinate(15);

        foreach (var actorInfo in CheckInBattleActors())
        {
            if (actorInfo.InBattle == true)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Battle))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.SetTitleData("");
                    partyGetItemInfo.SetResultData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(4).Text));
                    PartyInfo.AddCommandRank(TacticsComandType.Battle);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
        }
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
        foreach (var actorInfo in actorInfos)
        {
            actorInfo.SetNextBattleEnemyIndex(-1,0);
            if (actorInfo.InBattle == true)
            {
                actorInfo.InBattle = false;
            }
            actorInfo.ClearTacticsCommand();
        }
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
        CurrentStage.SeekStage();
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
