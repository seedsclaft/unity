using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyModel : BaseModel
{
    private List<TacticsResultInfo> _resultInfos = new();

    private bool _needUseSpCommand = false;
    public bool NeedUseSpCommand => _needUseSpCommand;
    public void SetNeedUseSpCommand(bool isNeed)
    {
        _needUseSpCommand = isNeed;
    }
    
    private bool _battleSkip = false;
    public bool BattleSkip => _battleSkip;

    private List<ActorInfo> _levelUpData = new();
    private List<ActorInfo> _levelUpBonusData = new();
    public List<ActorInfo> LevelUpData => _levelUpData;
    public List<ListData> LevelUpActorStatus(int index)
    {
        var list = new List<ListData>();
        var listData = new ListData(_levelUpData[0]);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        return list;
    }

    private List<GetItemInfo> _resultItemInfos = new();
    public List<GetItemInfo> ResultGetItemInfos => _resultItemInfos;

    public List<ActorInfo> TacticsActors()
    {
        List<ActorInfo> actorInfos = StageMembers().FindAll(a => a.TacticsCommandType != TacticsCommandType.None && a.TacticsCommandType != TacticsCommandType.Battle);
        return actorInfos;
    }

    public List<ActorInfo> TacticsBattleActors()
    {
        List<ActorInfo> actorInfos = StageMembers().FindAll(a => a.TacticsCommandType == TacticsCommandType.Battle);
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
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Train)
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

    public void MakeResult()
    {
        // Party初期化
        PartyInfo.InitActors();
        var getItemInfos = new List<GetItemInfo>();
        var actorInfos = TacticsActors();
        // 結果出力
        for (int i = 0;i < actorInfos.Count;i++)
        {
            var actorName = actorInfos[i].Master.Name;
            var getItemInfo = new GetItemInfo(null);
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Train)
            {
                var trainBonus = _levelUpBonusData.Find(a => a == actorInfos[i]) != null;
                getItemInfo.MakeTrainResult(actorName,actorInfos[i].Level,trainBonus);
                _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsCommandType.Train,trainBonus));
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Alchemy)
            {
                bool alchemyBonus = PartyInfo.GetAlchemyNuminosValue();
                SkillData skillData = DataSystem.Skills.Find(a => a.Id == actorInfos[i].NextLearnSkillId);
                getItemInfo.MakeAlchemyResult(actorName,skillData);
                actorInfos[i].LearnSkill(actorInfos[i].NextLearnSkillId);
                if (alchemyBonus)
                {
                    List<SkillData> getSkillDatas = DataSystem.Skills.FindAll(a => a.Rank == 1 && (int)a.Attribute == (int)skillData.Attribute && !PartyInfo.AlchemyIdList.Contains(a.Id));
                    if (getSkillDatas.Count > 0)
                    {
                        GetItemInfo bonusGetItemInfo = new GetItemInfo(null);
                        int rand2 = UnityEngine.Random.Range(0,getSkillDatas.Count);
                        PartyInfo.AddAlchemy(getSkillDatas[rand2].Id);
                        SkillData randSkillData = DataSystem.Skills.Find(a => a.Id == getSkillDatas[rand2].Id);
                        bonusGetItemInfo.MakeAlchemyBonusResult(randSkillData);
                        getItemInfos.Add(bonusGetItemInfo);
                    }
                }
               _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsCommandType.Alchemy,alchemyBonus));    
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Recovery)
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
               _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsCommandType.Recovery,isRecoveryBonus));
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Resource)
            {
                bool resourceBonus = PartyInfo.GetResourceBonusValue();
                getItemInfo.SetTitleData(DataSystem.System.GetReplaceText(3020,actorInfos[i].Master.Name));
                int resource = TacticsUtility.ResourceGain(actorInfos[i]);
                if (resourceBonus)
                {
                    resource *= 2;
                }
                PartyInfo.ChangeCurrency(Currency + resource);
                var resourceResult = DataSystem.System.GetReplaceText(3021,resource.ToString());
                if (resourceBonus)
                {
                    resourceResult += " " + DataSystem.System.GetTextData(3031).Text;
                }
                getItemInfo.SetResultData(resourceResult);
                _resultInfos.Add(new TacticsResultInfo(actorInfos[i].ActorId,TacticsCommandType.Resource,resourceBonus));
            }
            PartyInfo.AddActor(actorInfos[i].ActorId);
            getItemInfos.Add(getItemInfo);
        }
        
        // コマンドカウント出力
        for (int i = 0;i < actorInfos.Count;i++)
        {
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Train)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Train))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeTrainCommandResult(PartyInfo.CommandRankInfo[TacticsCommandType.Train]);
                    PartyInfo.AddCommandRank(TacticsCommandType.Train);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Alchemy)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Alchemy))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeAlchemyCommandResult(PartyInfo.CommandRankInfo[TacticsCommandType.Alchemy]);
                    PartyInfo.AddCommandRank(TacticsCommandType.Alchemy);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Recovery)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Recovery))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeRecoveryCommandResult(PartyInfo.CommandRankInfo[TacticsCommandType.Recovery]);
                    PartyInfo.AddCommandRank(TacticsCommandType.Recovery);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
            if (actorInfos[i].TacticsCommandType == TacticsCommandType.Resource)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Resource))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeResourceCommandResult(PartyInfo.CommandRankInfo[TacticsCommandType.Resource]);
                    PartyInfo.AddCommandRank(TacticsCommandType.Resource);
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
        // Party初期化
        PartyInfo.InitActors();
        List<ActorInfo> actorInfos = TacticsBattleActors();
        foreach (var actorInfo in actorInfos)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
        }
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
            
                foreach (var actorInfo in BattleResultActors())
                {
                    actorInfo.GainDemigod(getItemInfo.Param1);
                }
            }
            if ((int)getItemInfo.GetItemType >= (int)GetItemType.AttributeFire && (int)getItemInfo.GetItemType <= (int)GetItemType.AttributeDark)
            {
                int rand = UnityEngine.Random.Range(0,100);
                if (getItemInfo.Param2 >= rand)
                {
                    List< SkillData> getSkillDatas = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && (int)a.Attribute == (int)getItemInfo.GetItemType - 10 && !PartyInfo.AlchemyIdList.Contains(a.Id)); 
                    if (getSkillDatas.Count > 0)
                    {
                        int rand2 = UnityEngine.Random.Range(0,getSkillDatas.Count);
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
                CurrentStage.SetStageClear(true);
            }
            if (getItemInfo.GetItemType == GetItemType.StatusUp)
            {
                getItemInfo.SetTitleData(DataSystem.System.GetTextData(14071).Text);
                getItemInfos.Add(getItemInfo);
                
                foreach (var actorInfo in BattleResultActors())
                {
                    actorInfo.TempStatus.AddParameterAll(getItemInfo.Param1);
                    actorInfo.DecideStrength(0);
                }
            }

        }
        CurrentStage.AddClearTroopId(CurrentTroopInfo().TroopId);
        CurrentData.PlayerInfo.AddClearedTroopId(CurrentTroopInfo().TroopId);
        CurrentStage.GainTroopClearCount(1);
        CurrentStage.ChangeSubordinate(15);

        foreach (var actorInfo in BattleResultActors())
        {
            if (actorInfo.InBattle == true)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Battle))
                {
                    GetItemInfo partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeBattleCommandResult(PartyInfo.CommandRankInfo[TacticsCommandType.Battle]);
                    PartyInfo.AddCommandRank(TacticsCommandType.Battle);
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
                if (actorInfos[i].TacticsCommandType == TacticsCommandType.Battle)
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

    public List<ActorInfo> BattleResultActors()
    {
        return TacticsBattleActors().FindAll(a => a.InBattle);
    }

    public void SetBattleMembers(List<ActorInfo> actorInfos)
    {
        PartyInfo.InitActors();
        actorInfos.ForEach(a => PartyInfo.AddActor(a.ActorId));
        actorInfos.ForEach(a => a.SetInBattle(true));
    }

    public void ClearBattleData(List<ActorInfo> actorInfos)
    {
        foreach (var actorInfo in actorInfos)
        {
            actorInfo.SetNextBattleEnemyIndex(-1,0);
            if (actorInfo.InBattle == true)
            {
                actorInfo.SetInBattle(false);
            }
            actorInfo.ClearTacticsCommand();
        }
    }

    public List<ActorInfo> LostMembers()
    {
        return TacticsBattleActors().FindAll(a => a.InBattle && a.CurrentHp == 0);
    }

    public List<ListData> ResultCommand()
    {
        List<ListData> list = new List<ListData>();
        foreach (var commandData in BaseConfirmCommand(3040,6))
        {
            var listData = new ListData(commandData);
            list.Add(listData);
        }
        return list;
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

    public bool EnableBattleSkip()
    {
        return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
    }

    public void ChangeBattleSkip(bool battleSkip)
    {
        _battleSkip = battleSkip;
    }
}
