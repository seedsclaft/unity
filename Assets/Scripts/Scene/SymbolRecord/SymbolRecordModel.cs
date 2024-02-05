using System.Collections.Generic;
using UnityEngine;

public class SymbolRecordModel : BaseModel
{
    private List<TacticsResultInfo> _resultInfos = new();
    

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
        return TempData.TempResultActorInfos.FindAll(a => a.InBattle == false);
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
        var getItemInfos = TempData.TempGetItemInfos;
        // 魔法習熟度進行
        foreach (var actorInfo in StageMembers())
        {
            var learningSkills = actorInfo.SeekAlchemy();
            foreach (var learningSkill in learningSkills)
            {
                var getItemData = new GetItemData();
                getItemData.Type = GetItemType.LearnSkill;
                getItemData.Param1 = actorInfo.ActorId;
                getItemData.Param2 = learningSkill.Id;
                getItemInfos.Add(new GetItemInfo(getItemData));
            }
        }
        foreach (var getItemInfo in getItemInfos)
        {
            switch (getItemInfo.GetItemType)
            {
                case GetItemType.Numinous:
                    PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                    break;
                case GetItemType.Skill:
                    PartyInfo.AddAlchemy(getItemInfo.Param1);
                    break;
                case GetItemType.Regeneration:
                    foreach (var stageMember in StageMembers())
                    {
                        if (stageMember.Lost == false)
                        {
                            stageMember.ChangeHp(stageMember.CurrentHp + getItemInfo.Param1);
                            stageMember.ChangeMp(stageMember.CurrentMp + getItemInfo.Param1);
                        }
                    }
                    break;
                case GetItemType.Demigod:
                    foreach (var stageMember in StageMembers())
                    {
                        if (stageMember.Lost == false)
                        {
                            stageMember.GainDemigod(getItemInfo.Param1);
                        }
                    }
                    break;
                case GetItemType.StatusUp:
                    foreach (var stageMember in StageMembers())
                    {
                        if (stageMember.Lost == false)
                        {
                            stageMember.TempStatus.AddParameterAll(getItemInfo.Param1);
                            stageMember.DecideStrength(0);
                        }
                    }
                    break;
            }
        }
        _resultItemInfos = ListData.MakeListData(getItemInfos);
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
        return MakeListData(TempData.TempGetItemInfos);
    }

    public List<ActorInfo> BattleResultActors()
    {
        return TempData.TempResultActorInfos.FindAll(a => a.InBattle);
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
        return BattleResultActors().FindAll(a => a.InBattle && a.CurrentHp == 0);
    }

    public List<ListData> ResultCommand()
    {
        return MakeListData(BaseConfirmCommand(3040,5));
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
    
    public List<ListData> Symbols(int seek)
    {
        var symbolInfos = new List<SymbolInfo>();
        
        var stageData = DataSystem.FindStage(CurrentStage.Id);
        var symbols = stageData.StageSymbols.FindAll(a => a.Seek == seek+1);
        var symbolRecords = CurrentStageData.Party.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        
        for (int i = 0;i < symbols.Count;i++)
        {
            var symbol = symbols[i];
            var saveRecord = symbolRecords.Find(a => a.IsSameSymbol(stageData.Id,symbol.Seek,i));
            var getItemInfos = new List<GetItemInfo>();
            var symbolInfo = new SymbolInfo(symbol);
            if (symbol.BattleSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }                
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            } else
            if (symbol.BossSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }                
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            }
            if (symbol.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            }
            symbolInfo.SetSelected(saveRecord != null);
            symbolInfo.MakeGetItemInfos(getItemInfos);
            symbolInfos.Add(symbolInfo);
        }
        return MakeListData(symbolInfos);
    }

    public List<ListData> SymbolRecords()
    {
        var symbolRecords = CurrentStageData.Party.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        var symbolInfos = new List<SymbolInfo>();
        
        var stageData = DataSystem.FindStage(CurrentStage.Id);
        foreach (var symbolRecord in symbolRecords)
        {
            var symbols = stageData.StageSymbols.FindAll(a => a.Seek == symbolRecord.Seek);
            var symbol = symbols[symbolRecord.SeekIndex];
            var saveRecord = symbolRecords.Find(a => a.IsSameSymbol(stageData.Id,symbolRecord.Seek,symbolRecord.SeekIndex));
            var symbolInfo = new SymbolInfo(symbol);
            var getItemInfos = new List<GetItemInfo>();
            if (symbol.BattleSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            } else
            if (symbol.BossSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            }
            if (symbol.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                MakePrizeData(saveRecord,prizeSets,getItemInfos);
            }
            symbolInfo.MakeGetItemInfos(getItemInfos);
            symbolInfos.Add(symbolInfo);
        }
        return MakeListData(symbolInfos);
    }

    public List<ActorInfo> SymbolActors(int seek)
    {
        var symbolRecord = CurrentStageData.Party.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.ActorsData;
    }

    public TroopInfo BattleTroops(int troopId,int enemyCount)
    {
        var troopInfo = new TroopInfo(troopId,false);
        troopInfo.MakeEnemyTroopDates(CurrentStage.ClearCount);
        for (int j = 0;j < enemyCount;j++)
        {
            int rand = new System.Random().Next(1, CurrentStage.Master.RandomTroopCount);
            var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
            var enemy = new BattlerInfo(enemyData,CurrentStage.ClearCount + 1,j,0,false);
            troopInfo.AddEnemy(enemy);
        }
        troopInfo.MakeGetItemInfos();
        return troopInfo;
    }

    private void MakePrizeData(SymbolResultInfo saveRecord,List<PrizeSetData> prizeSets,List<GetItemInfo> getItemInfos)
    {
        foreach (var prizeSet in prizeSets)
        {
            var getItemInfo = new GetItemInfo(prizeSet.GetItem);
            if (saveRecord != null && prizeSet.GetItem.Type == GetItemType.SaveHuman)
            {
                var rate = (float)saveRecord.BattleScore * 0.01f;
                rate *= getItemInfo.Param1;
                getItemInfo.SetParam2((int)rate);
                getItemInfo.MakeTextData();
            }
            getItemInfos.Add(getItemInfo);
        }
    }
    
    public bool EnableBattleSkip()
    {
        // スキップ廃止
        return false;
        //return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
    }



    public void ReturnTempBattleMembers()
    {
        foreach (var tempActorInfo in TempData.TempActorInfos)
        {
            tempActorInfo.SetInBattle(false);
            CurrentStageData.UpdateActorInfo(tempActorInfo);
        }
        TempData.ClearBattleActors();
    }
}
