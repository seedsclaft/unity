using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyModel : BaseModel
{
    private List<TacticsResultInfo> _resultInfos = new();

    private bool _needUseSpCommand = false;
    public bool NeedUseSpCommand { get {return _needUseSpCommand;}}
    public void SetNeedUseSpCommand(bool isNeed)
    {
        _needUseSpCommand = isNeed;
    }
    
    private List<ActorInfo> _levelUpData = new List<ActorInfo>();
    private List<ActorInfo> _levelUpBonusData = new List<ActorInfo>();
    public List<ActorInfo> LevelUpData { get {return _levelUpData;}}

    private List<GetItemInfo> _resultItemInfos = new ();
    public List<GetItemInfo> ResultGetItemInfos { get {return _resultItemInfos;}}

    public bool CheckUseSp()
    {
        ActorInfo actorInfo = StageMembers()[0];
        return (actorInfo.Sp < 10);
    }

    public List<ActorInfo> TacticsActors()
    {
        List<ActorInfo> actorInfos = StageMembers().FindAll(a => a.TacticsComandType != TacticsComandType.None && a.TacticsComandType != TacticsComandType.Battle);
        return actorInfos;
    }

    public List<ActorInfo> TacticsBattleActors()
    {
        List<ActorInfo> actorInfos = StageMembers().FindAll(a => a.TacticsComandType == TacticsComandType.Battle);
        return actorInfos;
    }

    public void SetLvup()
    {
        if (_levelUpData.Count > 0) return;
        List<ActorInfo> actorInfos = TacticsActors();
        var lvupList = new List<ActorInfo>();
        // 結果出力
        for (int i = 0;i < actorInfos.Count;i++)
        {
            if (actorInfos[i].TacticsComandType == TacticsComandType.Train)
            {
                int levelBonus = PartyInfo.GetTrainLevelBonusValue();
                var statusInfo = actorInfos[i].LevelUp(levelBonus);
                actorInfos[i].TempStatus.SetParameter(
                    statusInfo.Hp,
                    statusInfo.Mp,
                    statusInfo.Atk,
                    statusInfo.Def,
                    statusInfo.Spd
                );
                lvupList.Add(actorInfos[i]);
                if (levelBonus > 0)
                {
                    _levelUpBonusData.Add(actorInfos[i]);
                }
            }
        }
        _levelUpData = lvupList;
    }

    public void SetResult()
    {
        // Party初期化
        PartyInfo.InitActors();
        List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
        List<ActorInfo> actorInfos = TacticsActors();
        // 結果出力
        for (int i = 0;i < actorInfos.Count;i++)
        {
            string actorName = actorInfos[i].Master.Name;
            GetItemInfo getItemInfo = new GetItemInfo(null);
            if (actorInfos[i].TacticsComandType == TacticsComandType.Train)
            {
                var trainBonus = _levelUpBonusData.Find(a => a == actorInfos[i]) != null;
                getItemInfo.MakeTrainResult(actorName,actorInfos[i].Level,trainBonus);
                _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsComandType.Train,trainBonus));
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Alchemy)
            {
                bool alchemyBonus = PartyInfo.GetAlchemyNuminosValue();
                SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == actorInfos[i].NextLearnSkillId);
                getItemInfo.MakeAlchemyResult(actorName,skillData);
                actorInfos[i].LearnSkill(actorInfos[i].NextLearnSkillId);
                if (alchemyBonus)
                {
                    List<SkillsData.SkillData> getSkillDatas = DataSystem.Skills.FindAll(a => a.Rank == skillData.Rank && (int)a.Attribute == (int)skillData.Attribute && !PartyInfo.AlchemyIdList.Contains(a.Id));
                    if (getSkillDatas.Count > 0)
                    {
                        GetItemInfo bonusGetItemInfo = new GetItemInfo(null);
                        int rand2 = UnityEngine.Random.Range(0,getSkillDatas.Count-1);
                        PartyInfo.AddAlchemy(getSkillDatas[rand2].Id);
                        SkillsData.SkillData randSkillData = DataSystem.Skills.Find(a => a.Id == getSkillDatas[rand2].Id);
                        bonusGetItemInfo.MakeAlchemyBonusResult(randSkillData);
                        getItemInfos.Add(bonusGetItemInfo);
                    }
                }
               _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsComandType.Alchemy,alchemyBonus));    
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Recovery)
            {
                int Hp = Mathf.Min(actorInfos[i].CurrentHp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxHp);
                int Mp = Mathf.Min(actorInfos[i].CurrentMp + actorInfos[i].TacticsCost * 10,actorInfos[i].MaxMp);
                actorInfos[i].ChangeHp(Hp);
                actorInfos[i].ChangeMp(Mp);
                getItemInfo.MakeRecoveryResult(actorName);
                var isRecoveryBonus = PartyInfo.GetRecoveryBonusValue();
                if (isRecoveryBonus)
                {
                    actorInfos[i].TempStatus.AddParameterAll(1);
                    actorInfos[i].DecideStrength(0);
                    GetItemInfo bonusGetItemInfo = new GetItemInfo(null);
                    bonusGetItemInfo.MakeRecoveryBonusResult(actorName);
                    getItemInfos.Add(bonusGetItemInfo);
                }
               _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsComandType.Recovery,isRecoveryBonus));
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Resource)
            {
                bool resourceBonus = PartyInfo.GetResourceBonusValue();
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(3020).Text.Replace("\\d",actorInfos[i].Master.Name));
                int resource = TacticsUtility.ResourceCost(actorInfos[i]);
                if (resourceBonus)
                {
                    resource *= 2;
                }
                PartyInfo.ChangeCurrency(Currency + resource);
                var resourceResult = DataSystem.System.GetTextData(3021).Text.Replace("\\d",resource.ToString());
                if (resourceBonus)
                {
                    resourceResult += " " + DataSystem.System.GetTextData(3031).Text;
                }
                getItemInfo.SetResultData(resourceResult);
                _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsComandType.Resource,resourceBonus));
            }
            PartyInfo.AddActor(actorInfos[i].ActorId);
            getItemInfos.Add(getItemInfo);
        }
        
        // コマンドカウント出力
        for (int i = 0;i < actorInfos.Count;i++)
        {
            if (actorInfos[i].TacticsComandType == TacticsComandType.Train)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Train))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeTrainCommandResult(PartyInfo.CommandRankInfo[TacticsComandType.Train]);
                    PartyInfo.AddCommandRank(TacticsComandType.Train);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Alchemy)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Alchemy))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeAlchemyCommandResult(PartyInfo.CommandRankInfo[TacticsComandType.Alchemy]);
                    PartyInfo.AddCommandRank(TacticsComandType.Alchemy);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Recovery)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Recovery))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeRecoveryCommandResult(PartyInfo.CommandRankInfo[TacticsComandType.Recovery]);
                    PartyInfo.AddCommandRank(TacticsComandType.Recovery);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsComandType == TacticsComandType.Resource)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Resource))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeResourceCommandResult(PartyInfo.CommandRankInfo[TacticsComandType.Resource]);
                    PartyInfo.AddCommandRank(TacticsComandType.Resource);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
        }

        // コマンド初期化
        for (int i = 0;i < actorInfos.Count;i++)
        {
            actorInfos[i].ClearTacticsCommand();
        }

        _resultItemInfos = getItemInfos;
    }

    public void SetLevelUpStatus()
    {
        var actorInfo = _levelUpData[0];
        actorInfo.DecideStrength(0);
        _levelUpData.RemoveAt(0);
    }

    public List<GetItemInfo> SetBattleResult()
    {
        List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
        if (PartyInfo.BattleResult == false)
        {
            return getItemInfos;
        }
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
                int getNuminos = PartyInfo.GetBattleBonusValue(getItemInfo.Param1);
                int alcanaBonus = CurrentAlcana.VictoryGainSpValue();
                PartyInfo.ChangeCurrency(PartyInfo.Currency + getNuminos + alcanaBonus);
                getItemInfo.MakeNuminosResult(getNuminos + alcanaBonus);
                getItemInfos.Add(getItemInfo);
            }
            if (getItemInfo.GetItemType == GetItemType.Demigod)
            {
                getItemInfo.MakeDemigodResult(getItemInfo.Param1);
                getItemInfos.Add(getItemInfo);
            
                foreach (var actorInfo in CheckInBattleActors())
                {
                    actorInfo.GainDemigod(getItemInfo.Param1);
                }
            }
            if ((int)getItemInfo.GetItemType >= (int)GetItemType.AttributeFire && (int)getItemInfo.GetItemType <= (int)GetItemType.AttributeDark)
            {
                int rand = UnityEngine.Random.Range(0,100);
                if (getItemInfo.Param2 >= rand)
                {
                    List< SkillsData.SkillData> getSkillDatas = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && (int)a.Attribute == (int)getItemInfo.GetItemType - 10 && !PartyInfo.AlchemyIdList.Contains(a.Id)); 
                    if (getSkillDatas.Count > 0)
                    {
                        int rand2 = UnityEngine.Random.Range(0,getSkillDatas.Count-1);
                        PartyInfo.AddAlchemy(getSkillDatas[rand2].Id);
                        getItemInfo.SetTitleData(DataSystem.System.GetTextData(14040).Text);
                        
                        //string text = DataSystem.System.GetTextData(14051).Text.Replace("\\d", DataSystem.System.GetTextData(330 + (int)getItemInfo.GetItemType - 11).Text);
                        getItemInfo.SetResultData(getSkillDatas[rand2].Name);
                        getItemInfo.SetSkillElementId((int)getItemInfo.GetItemType - 10);
                        getItemInfos.Add(getItemInfo);
                    }
                }
            }
            if (getItemInfo.GetItemType == GetItemType.Ending)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(14060).Text);
                getItemInfos.Add(getItemInfo);
                CurrentStage.SetStageClaer(true);
            }
            if (getItemInfo.GetItemType == GetItemType.StatusUp)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(14070).Text.Replace("\\d", getItemInfo.Param1.ToString()));
                getItemInfos.Add(getItemInfo);
                
                foreach (var actorInfo in CheckInBattleActors())
                {
                    actorInfo.TempStatus.AddParameterAll(getItemInfo.Param1);
                    actorInfo.DecideStrength(0);
                }
            }

        }
        CurrentStage.AddClearTroopId(CurrentTroopInfo().TroopId);
        CurrentStage.GainTroopClearCount(1);
        CurrentStage.ChangeSubordinate(15);

        foreach (var actorInfo in CheckInBattleActors())
        {
            if (actorInfo.InBattle == true)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsComandType.Battle))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeBattleCommandResult(PartyInfo.CommandRankInfo[TacticsComandType.Battle]);
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

    public List<ActorInfo> LostMembers()
    {
        return TacticsBattleActors().FindAll(a => a.InBattle && a.CurrentHp == 0);
    }

    public List<SystemData.MenuCommandData> ResultCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(6).Text;
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = DataSystem.System.GetTextData(3040).Text;
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }

    public bool IsBonusTactics(int actorId)
    {
        var result = _resultInfos.Find(a => a.ActorId == actorId);
        if (result != null)
        {
            return result.IsBonus;
        }
        return false;
    }

    public void LoadActorResources()
    {
        var filePaths = BattleUtility.AnimationResourcePaths(TacticsBattleActors());
        int count = filePaths.Count;
        foreach (var filePath in filePaths)
        {
            Resources.LoadAsync<Sprite>( filePath );
        }
    }

    public void LoadEnemyResources()
    {
        var filePaths = BattleUtility.AnimationResourcePaths(TacticsBattleActors());
        int count = filePaths.Count;
        foreach (var filePath in filePaths)
        {
            Resources.LoadAsync<Sprite>( filePath );
        }
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
