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
    public List<ActorInfo> LevelUpData => _levelUpData;
    private List<int> _levelUpBonusActorIds = new();
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

    private List<ListData> _resultItemInfos = new();
    public List<ListData> ResultGetItemInfos => _resultItemInfos;

    public List<ActorInfo> TacticsActors()
    {
        var actorInfos = StageMembers().FindAll(a => a.TacticsCommandType != TacticsCommandType.None && a.TacticsCommandType != TacticsCommandType.Battle);
        return actorInfos;
    }

    public List<ActorInfo> TacticsBattleActors()
    {
        var actorInfos = StageMembers().FindAll(a => a.TacticsCommandType == TacticsCommandType.Battle);
        return actorInfos;
    }

    public void SetLvUp()
    {
        if (_levelUpData.Count > 0) return;
        var lvUpActorInfos = TacticsActors().FindAll(a => a.TacticsCommandType == TacticsCommandType.Train);
        var lvUpList = new List<ActorInfo>();
        // 結果出力
        foreach (var lvUpActorInfo in lvUpActorInfos)
        {
            var levelBonus = PartyInfo.GetTrainLevelBonusValue();
            var statusInfo = lvUpActorInfo.LevelUp(levelBonus);
            lvUpActorInfo.TempStatus.SetParameter(
                statusInfo.Hp,
                statusInfo.Mp,
                statusInfo.Atk,
                statusInfo.Def,
                statusInfo.Spd
            );
            lvUpList.Add(lvUpActorInfo);
            if (levelBonus > 0)
            {
                _levelUpBonusActorIds.Add(lvUpActorInfo.ActorId);
            }
        }
        _levelUpData = lvUpList;
    }

    public void MakeResult()
    {
        // Party初期化
        PartyInfo.InitActors();
        var getItemInfos = new List<GetItemInfo>();
        var actorInfos = TacticsActors();
        // 結果出力
        foreach (var actorInfo in actorInfos)
        {
            var actorName = actorInfo.Master.Name;
            var getItemInfo = new GetItemInfo(null);
            switch (actorInfo.TacticsCommandType)
            {
                case TacticsCommandType.Train:
                    var trainBonus = _levelUpBonusActorIds.Contains(actorInfo.ActorId);
                    getItemInfo.MakeTrainResult(actorName,actorInfo.Level,trainBonus);
                    _resultInfos.Add(new TacticsResultInfo(actorInfo.ActorId,TacticsCommandType.Train,trainBonus));
                    break;
                case TacticsCommandType.Alchemy:
                    var skillData = DataSystem.Skills.Find(a => a.Id == actorInfo.NextLearnSkillId);
                    getItemInfo.MakeAlchemyResult(actorName,skillData);
                    actorInfo.LearnSkill(actorInfo.NextLearnSkillId);
                    var alchemyBonus = PartyInfo.GetAlchemyBonusValue();
                    if (alchemyBonus)
                    {
                        var getSkillDates = DataSystem.Skills.FindAll(a => a.Rank == 1 && (int)a.Attribute == (int)skillData.Attribute && !PartyInfo.AlchemyIdList.Contains(a.Id));
                        if (getSkillDates.Count > 0)
                        {
                            var bonusGetItemInfo = new GetItemInfo(null);
                            var rand2 = Random.Range(0,getSkillDates.Count);
                            PartyInfo.AddAlchemy(getSkillDates[rand2].Id);
                            var randSkillData = DataSystem.Skills.Find(a => a.Id == getSkillDates[rand2].Id);
                            bonusGetItemInfo.MakeAlchemyBonusResult(randSkillData);
                            getItemInfos.Add(bonusGetItemInfo);
                        }
                    }
                     _resultInfos.Add(new TacticsResultInfo(actorInfo.ActorId,TacticsCommandType.Alchemy,alchemyBonus));    
                    break;
                case TacticsCommandType.Recovery:
                    var recovery = actorInfo.TacticsCost * 10;
                    var Hp = Mathf.Min(actorInfo.CurrentHp + recovery,actorInfo.MaxHp);
                    actorInfo.ChangeHp(Hp);
                    var Mp = Mathf.Min(actorInfo.CurrentMp + recovery,actorInfo.MaxMp);
                    actorInfo.ChangeMp(Mp);
                    getItemInfo.MakeRecoveryResult(actorName);
                    var isRecoveryBonus = PartyInfo.GetRecoveryBonusValue();
                    if (isRecoveryBonus)
                    {
                        actorInfo.TempStatus.AddParameterAll(1);
                        actorInfo.DecideStrength(0);
                        var bonusGetItemInfo = new GetItemInfo(null);
                        bonusGetItemInfo.MakeRecoveryBonusResult(actorName);
                        getItemInfos.Add(bonusGetItemInfo);
                    }
                    _resultInfos.Add(new TacticsResultInfo(actorInfo.ActorId,TacticsCommandType.Recovery,isRecoveryBonus));
                    break;
                case TacticsCommandType.Resource:
                    getItemInfo.SetTitleData(DataSystem.System.GetReplaceText(3020,actorName));
                    var resource = TacticsUtility.ResourceGain(actorInfo);
                    var resourceBonus = PartyInfo.GetResourceBonusValue();
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
                    _resultInfos.Add(new TacticsResultInfo(actorInfo.ActorId,TacticsCommandType.Resource,resourceBonus));
                    break;
            }
            PartyInfo.AddActor(actorInfo.ActorId);
            getItemInfos.Add(getItemInfo);
        }
        
        // コマンドカウント出力
        getItemInfos.AddRange(CommandCountResults(actorInfos));

        // コマンド初期化
        foreach (var actorInfo in actorInfos)
        {
            actorInfo.ClearTacticsCommand();
        }

        _resultItemInfos = ListData.MakeListData(getItemInfos);
    }

    public List<GetItemInfo> CommandCountResults(List<ActorInfo> actorInfos)
    {
        var getItemInfos = new List<GetItemInfo>();
        foreach (var actorInfo in actorInfos)
        {
            switch (actorInfo.TacticsCommandType)
            {
                case TacticsCommandType.Train:
                case TacticsCommandType.Alchemy:
                case TacticsCommandType.Recovery:
                case TacticsCommandType.Resource:
                    if (PartyInfo.AddCommandCountInfo(actorInfo.TacticsCommandType))
                    {
                        var partyGetItemInfo = new GetItemInfo(null);
                        partyGetItemInfo.MakeCommandCountResult(PartyInfo.CommandRankInfo[actorInfo.TacticsCommandType],actorInfo.TacticsCommandType);
                        PartyInfo.AddCommandRank(actorInfo.TacticsCommandType);
                        getItemInfos.Add(partyGetItemInfo);
                    }
                    break;
            }
        }
        return getItemInfos;
    }

    public void SetLevelUpStatus()
    {
        var actorInfo = _levelUpData[0];
        actorInfo.DecideStrength(0);
        _levelUpData.RemoveAt(0);
    }

    public bool BattleResultVictory()
    {
        return PartyInfo.BattleResultVictory;
    }

    public List<ListData> BattleResultInfos()
    {
        // Party初期化
        PartyInfo.InitActors();
        var actorInfos = TacticsBattleActors();
        foreach (var actorInfo in actorInfos)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
        }
        var getItemInfos = new List<GetItemInfo>();
        if (PartyInfo.BattleResultVictory == false)
        {
            return new List<ListData>();
        }
        foreach (var getItemInfo in CurrentTroopInfo().GetItemInfos)
        {
            if (getItemInfo.GetItemType == GetItemType.Skill)
            {
                var rand = Random.Range(0,100);
                if (getItemInfo.Param2 >= rand)
                {
                    PartyInfo.AddAlchemy(getItemInfo.Param1);
                    getItemInfo.SetTitleData(DataSystem.System.GetTextData(14040).Text);
                    getItemInfos.Add(getItemInfo);
                }
            }
            if (getItemInfo.GetItemType == GetItemType.Numinous)
            {
                var getCurrency = PartyInfo.GetBattleBonusValue(getItemInfo.Param1);
                var alcanaBonus = CurrentAlcana.VictoryGainSpValue();
                PartyInfo.ChangeCurrency(PartyInfo.Currency + getCurrency + alcanaBonus);
                getItemInfo.MakeCurrencyResult(getCurrency + alcanaBonus);
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
                var rand = Random.Range(0,100);
                if (getItemInfo.Param2 >= rand)
                {
                    var getSkillDates = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && (int)a.Attribute == (int)getItemInfo.GetItemType - 10 && !PartyInfo.AlchemyIdList.Contains(a.Id)); 
                    if (getSkillDates.Count > 0)
                    {
                        int rand2 = Random.Range(0,getSkillDates.Count);
                        PartyInfo.AddAlchemy(getSkillDates[rand2].Id);
                        getItemInfo.SetTitleData(DataSystem.System.GetTextData(14040).Text);
                        
                        //string text = DataSystem.System.GetTextData(14051).Text.Replace("\\d", DataSystem.System.GetTextData(330 + (int)getItemInfo.GetItemType - 11).Text);
                        getItemInfo.SetResultData(getSkillDates[rand2].Name);
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
        CurrentStage.ChangeSubordinateValue(10);

        foreach (var actorInfo in BattleResultActors())
        {
            if (actorInfo.InBattle == true)
            {
                if (PartyInfo.AddCommandCountInfo(TacticsCommandType.Battle))
                {
                    var partyGetItemInfo = new GetItemInfo(null);
                    partyGetItemInfo.MakeCommandCountResult(PartyInfo.CommandRankInfo[TacticsCommandType.Battle],TacticsCommandType.Battle);
                    PartyInfo.AddCommandRank(TacticsCommandType.Battle);
                    getItemInfos.Add(partyGetItemInfo);
                }
            }
        }
        return ListData.MakeListData(getItemInfos);
    }

    public int BattleEnemyIndex(bool inBattle)
    {
        var enemyIndex = -1;
        var actorInfos = TacticsBattleActors();
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
        var enemyIndex = BattleEnemyIndex(false);
        var actorInfos = TacticsBattleActors();
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
        return MakeListData(BaseConfirmCommand(3040,6));
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
        //CurrentStage.ChangeSubordinateValue(-5);
        CurrentAlcana.SetAlacanaState(null);
    }

    public bool EnableBattleSkip()
    {
        // スキップ廃止
        return false;
        //return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
    }

    public void ChangeBattleSkip(bool battleSkip)
    {
        _battleSkip = battleSkip;
    }

    public void SaveTempBattleMembers()
    {
        TempData.CashBattleActors(BattleResultActors());
    }

    public void ReturnTempBattleMembers()
    {
        foreach (var tempActorInfo in TempData.TempActorInfos)
        {
            tempActorInfo.SetInBattle(false);
            CurrentData.UpdateActorInfo(tempActorInfo);
        }
        TempData.ClearBattleActors();
    }
}
