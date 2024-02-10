using System.Collections.Generic;
using UnityEngine;

public class SymbolRecordModel : BaseModel
{
    public List<ListData> Symbols(int seek)
    {
        var symbolInfos = new List<SymbolInfo>();
        
        var stageData = DataSystem.FindStage(CurrentStage.Id);
        var symbols = stageData.StageSymbols.FindAll(a => a.Seek == seek+1);
        var symbolRecords = CurrentSaveData.Party.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        
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
        var symbolRecords = CurrentSaveData.Party.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
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
        var symbolRecord = CurrentSaveData.Party.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.ActorsData;
    }

    public List<int> SymbolAlchemyList(int seek)
    {
        var symbolRecord = CurrentSaveData.Party.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.AlchemyIdList;
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
    
    public void MakeSymbolRecordStage(int seek)
    {
        CurrentSaveData.MakeCurrentStage(CurrentStage.Master.Id);
        CurrentStage.SetRecordStage(true);
        TempData.SetRecordActors(PartyInfo.ActorInfos);
        TempData.SetRecordAlchemyList(CurrentSaveData.Party.AlchemyIdList);
        
        CurrentSaveData.ClearActors();
        CurrentStage.ClearSelectActorId();
        foreach (var symbolActor in SymbolActors(seek))
        {
            CurrentSaveData.AddActor(symbolActor.ActorId);
            CurrentStage.AddSelectActorId(symbolActor.ActorId);
        }

        CurrentSaveData.Party.ClearAlchemy();
        foreach (var alchemyId in SymbolAlchemyList(seek))
        {
            CurrentSaveData.Party.AddAlchemy(alchemyId);
        }

		CurrentStage.MakeTurnSymbol();
        for (int i = 0;i < seek;i++)
        {
            CurrentStage.SeekStage();
            MakeSymbolResultInfos();
        }
    }
}
